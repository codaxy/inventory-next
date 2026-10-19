using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Documents;
using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Settings;

/// <summary>Every setting, in the screen's order. A setting nothing lists is never seeded.</summary>
public static class SettingCatalogue
{
    public static readonly IReadOnlyList<SettingDefinition> All =
    [
        SignInSettings.AllowedDomains,
        DashboardSettings.SubscriptionsExpiredDays,
        DashboardSettings.SubscriptionsExpiringDays,
        DashboardSettings.WarrantiesEndedDays,
        DashboardSettings.WarrantiesEndingDays,
        DocumentSettings.Place,
        DocumentSettings.DefaultLanguage,
        ExpiryAlertSettings.Recipients,
        ExpiryAlertSettings.Lead,
    ];

    /// <summary>
    /// Inserts every listed setting a database lacks, with its default; one that exists is left as it
    /// is, and so is a row nothing lists any more.
    /// </summary>
    public static async Task SeedAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var stored = await context.Settings.Select(s => s.Key).ToListAsync(cancellationToken);

        foreach (var definition in All.Where(d => !stored.Contains(d.Key)))
            context.Settings.Add(
                new Setting
                {
                    Id = Guid.CreateVersion7(),
                    Key = definition.Key,
                    Value = definition.DefaultJson,
                }
            );

        await context.SaveChangesAsync(cancellationToken);
    }
}
