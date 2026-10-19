using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Email;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

/// <summary>
/// What is due — a subscription or a warranty ending within the lead, today or later, not yet
/// alerted for that date — sent in one email to every recipient and recorded once it is sent. Read as
/// the settings and dates are now, so a moved date or a changed lead needs nothing kept in step.
/// </summary>
public sealed class ExpiryAlertCheck(
    InventoryContext context,
    SettingStore settings,
    IEmailSender email,
    TimeProvider clock,
    IOptions<ExpiryAlertOptions> options,
    ILogger<ExpiryAlertCheck> logger
)
{
    /// <summary>How many records were alerted: none without recipients or with nothing due.</summary>
    public async Task<int> SendDueAsync(CancellationToken cancellationToken)
    {
        var values = await settings.LoadAsync(cancellationToken);
        var recipients = values.Get(ExpiryAlertSettings.Recipients);
        // Nothing recorded either, so whatever is due goes out once someone is named.
        if (recipients.Length == 0)
            return 0;

        var lead = values.Get(ExpiryAlertSettings.Lead);
        var today = Expiry.Today(clock);
        var due = await DueAsync(today, lead.Until(today), cancellationToken);
        if (due.Count == 0)
            return 0;

        var message = ExpiryAlertMessage.Compose(due, today, lead, options.Value.ApplicationUrl);
        await email.SendAsync(
            recipients,
            message.Subject,
            message.Text,
            message.Html,
            cancellationToken
        );

        var now = clock.GetUtcNow();
        context.ExpiryAlerts.AddRange(
            due.Select(d => new ExpiryAlert
            {
                Id = Guid.CreateVersion7(),
                AssetId = d.AssetId,
                ExpirationDate = d.Date,
                SentAt = now,
            })
        );
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Expiry alert sent for {Count} records to {Recipients}",
            due.Count,
            string.Join(", ", recipients)
        );
        return due.Count;
    }

    private async Task<List<DueAlert>> DueAsync(
        DateOnly today,
        DateOnly until,
        CancellationToken cancellationToken
    )
    {
        var alerted = context.ExpiryAlerts;

        var subscriptions = await context
            .Licenses.AsNoTracking()
            .Where(l =>
                l.SubscriptionExpirationDate >= today
                && l.SubscriptionExpirationDate <= until
                && !alerted.Any(a =>
                    a.AssetId == l.AssetId && a.ExpirationDate == l.SubscriptionExpirationDate
                )
            )
            .Select(l => new DueAlert(
                l.AssetId,
                DueKind.Subscription,
                l.Asset.Name,
                l.Asset.InventoryNumber,
                l.SubscriptionExpirationDate!.Value,
                l.AutoRenew == true,
                l.Asset.Location != null && l.Asset.Location.IsDisposal
                    ? new Disposal(l.Asset.Location.Id, l.Asset.Location.Name)
                    : null
            ))
            .ToListAsync(cancellationToken);

        var warranties = await context
            .ElectronicDevices.AsNoTracking()
            .Where(d =>
                d.WarrantyExpirationDate >= today
                && d.WarrantyExpirationDate <= until
                && !alerted.Any(a =>
                    a.AssetId == d.AssetId && a.ExpirationDate == d.WarrantyExpirationDate
                )
            )
            .Select(d => new DueAlert(
                d.AssetId,
                DueKind.Warranty,
                d.Asset.Name,
                d.Asset.InventoryNumber,
                d.WarrantyExpirationDate!.Value,
                false,
                d.Asset.Location != null && d.Asset.Location.IsDisposal
                    ? new Disposal(d.Asset.Location.Id, d.Asset.Location.Name)
                    : null
            ))
            .ToListAsync(cancellationToken);

        return [.. subscriptions.Concat(warranties).OrderBy(d => d.Date).ThenBy(d => d.Name)];
    }
}
