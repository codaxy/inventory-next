using System.Globalization;
using System.Text;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

public enum DueKind
{
    Subscription,
    Warranty,
}

/// <param name="DisposedAt">The disposal location the asset is at, or null.</param>
public sealed record DueAlert(
    Guid AssetId,
    DueKind Kind,
    string Name,
    int? Number,
    DateOnly Date,
    bool RenewsAutomatically,
    string? DisposedAt
);

/// <summary>One email for everything due: what ends, when, and what changes the reading.</summary>
public static class ExpiryAlertMessage
{
    public static (string Subject, string Body) Compose(
        IReadOnlyList<DueAlert> due,
        DateOnly today,
        AlertLead lead,
        string applicationUrl
    )
    {
        var body = new StringBuilder();
        body.Append($"Ending within {lead}:\n");

        foreach (var alert in due)
        {
            body.Append('\n');
            body.Append(Title(alert)).Append('\n');
            body.Append(
                    $"{(alert.Kind == DueKind.Subscription ? "Subscription expires" : "Warranty ends")} "
                        + $"on {Date(alert.Date)}, {When(alert.Date, today)}."
                )
                .Append('\n');
            if (alert.RenewsAutomatically)
                body.Append("Renews automatically on that date unless it is cancelled.\n");
            if (alert.DisposedAt is not null)
                body.Append($"Disposed of, at {alert.DisposedAt}.\n");
            if (applicationUrl != "")
                body.Append(Link(applicationUrl, alert)).Append('\n');
        }

        body.Append(
            "\nSent by Inventory to the addresses under Administration → Settings → Expiry alerts.\n"
        );

        return (Subject(due), body.ToString());
    }

    private static string Subject(IReadOnlyList<DueAlert> due)
    {
        if (due is [var one])
            return one.Kind == DueKind.Subscription
                ? $"{Title(one)}: subscription expires on {Date(one.Date)}"
                : $"{Title(one)}: warranty ends on {Date(one.Date)}";

        var subscriptions = due.Count(d => d.Kind == DueKind.Subscription);
        var warranties = due.Count - subscriptions;
        var parts = new[]
        {
            Count(subscriptions, "subscription", "subscriptions"),
            Count(warranties, "warranty", "warranties"),
        }.Where(p => p is not null);

        return $"{string.Join(" and ", parts)} ending";
    }

    private static string? Count(int n, string one, string many) =>
        n switch
        {
            0 => null,
            1 => $"1 {one}",
            _ => $"{n} {many}",
        };

    private static string Title(DueAlert alert) =>
        alert.Number is { } number ? $"{alert.Name} #{number}" : alert.Name;

    private static string Date(DateOnly date) =>
        date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    private static string When(DateOnly date, DateOnly today) =>
        (date.DayNumber - today.DayNumber) switch
        {
            0 => "today",
            1 => "tomorrow",
            var days => $"in {days} days",
        };

    private static string Link(string applicationUrl, DueAlert alert) =>
        $"{applicationUrl.TrimEnd('/')}/{(alert.Kind == DueKind.Subscription ? "licenses" : "electronic-devices")}/{alert.AssetId}";
}
