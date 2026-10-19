using System.Globalization;
using System.Text;
using Codaxy.Inventory.App.Shared.Email;
using static Codaxy.Inventory.App.Shared.Email.EmailLayout;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

public enum DueKind
{
    Subscription,
    Warranty,
}

/// <summary>The disposal location an asset is at.</summary>
public sealed record Disposal(Guid LocationId, string Name);

public sealed record DueAlert(
    Guid AssetId,
    DueKind Kind,
    string Name,
    int? Number,
    DateOnly Date,
    bool RenewsAutomatically,
    Disposal? DisposedAt,
    int Stage = 0
);

public sealed record AlertEmail(string Subject, string Text, string Html);

/// <summary>
/// One email for everything due, in <see cref="EmailLayout"/>: what ends, when, and what changes the
/// reading, with a plain-text copy for a client that shows no HTML. A name with a page links to it
/// where the application's address is known.
/// </summary>
public static class ExpiryAlertMessage
{
    public static AlertEmail Compose(
        IReadOnlyList<DueAlert> due,
        DateOnly today,
        AlertLead lead,
        string applicationUrl
    )
    {
        var links = new Links(applicationUrl);
        return new(Subject(due), Text(due, today, lead, links), Html(due, today, lead, links));
    }

    /// <summary>"Expiry alert: 1 subscription and 1 warranty ending".</summary>
    private static string Subject(IReadOnlyList<DueAlert> due) => $"Expiry alert: {Heading(due)}";

    /// <summary>"1 subscription and 1 warranty ending": the subject's, and the card's heading.</summary>
    private static string Heading(IReadOnlyList<DueAlert> due) => $"{Counted(due)} ending";

    private static string Within(IReadOnlyList<DueAlert> due, AlertLead lead) =>
        $"{(due.Count == 1 ? "It ends" : "These end")} within the next {lead}.";

    private static string Text(
        IReadOnlyList<DueAlert> due,
        DateOnly today,
        AlertLead lead,
        Links links
    )
    {
        var text = new StringBuilder($"{Heading(due)}\n\n{Within(due, lead)}\n");

        foreach (var alert in due)
        {
            text.Append($"\n{Title(alert)}\n{Line(alert, today)}\n");
            if (alert.RenewsAutomatically)
                text.Append($"{RenewsAutomatically}\n");
            if (alert.DisposedAt is { } disposal)
                text.Append($"Disposed of, at {disposal.Name}.\n");
            if (links.Record(alert) is { } url)
                text.Append($"{url}\n");
        }

        return text.Append(EmailLayout.TextFooter).ToString();
    }

    private static string Html(
        IReadOnlyList<DueAlert> due,
        DateOnly today,
        AlertLead lead,
        Links links
    )
    {
        var items = new StringBuilder();
        foreach (var alert in due)
            items.Append(Item(alert, today, links));

        return EmailLayout.Html(
            Subject(due),
            Heading(due),
            Within(due, lead),
            items.ToString(),
            links.Home
        );
    }

    /// <summary>A record under a hairline: its name, what ends and when, and any note.</summary>
    private static string Item(DueAlert alert, DateOnly today, Links links)
    {
        var name = Link(links.Record(alert), alert.Name);
        var number = alert.Number is { } n ? $""" <span class="muted">#{n}</span>""" : "";
        var line = E(Line(alert, today));
        var notes = new StringBuilder();
        if (alert.RenewsAutomatically)
            notes.Append($"""<br><span class="muted">{E(RenewsAutomatically)}</span>""");
        if (alert.DisposedAt is { } disposal)
        {
            var location = Link(links.Location(disposal), disposal.Name);
            notes.Append($"""<br><span class="muted">Disposed of, at {location}.</span>""");
        }

        return $"""<p class="rule" style="margin:0;padding:14px 0;"><strong>{name}</strong>{number}<br>{line}{notes}</p>"""
            + "\n";
    }

    /// <summary>"Subscription expires on 12 Dec 2026 (in 54 days)."</summary>
    private static string Line(DueAlert alert, DateOnly today) =>
        $"{What(alert)} on {Date(alert.Date)} ({When(alert.Date, today)}).";

    /// <summary>"Subscription expires", or for a reminder "Reminder: subscription expires".</summary>
    private static string What(DueAlert alert) =>
        alert.Stage == 0
            ? Ends(alert)
            : $"Reminder: {char.ToLowerInvariant(Ends(alert)[0])}{Ends(alert)[1..]}";

    private const string RenewsAutomatically =
        "Renews automatically on that date unless it is cancelled.";

    /// <summary>"2 subscriptions and 1 warranty".</summary>
    private static string Counted(IReadOnlyList<DueAlert> due)
    {
        var subscriptions = due.Count(d => d.Kind == DueKind.Subscription);
        var warranties = due.Count - subscriptions;
        return string.Join(
            " and ",
            new[]
            {
                Count(subscriptions, "subscription", "subscriptions"),
                Count(warranties, "warranty", "warranties"),
            }.Where(p => p is not null)
        );
    }

    private static string? Count(int n, string one, string many) =>
        n switch
        {
            0 => null,
            1 => $"1 {one}",
            _ => $"{n} {many}",
        };

    private static string Ends(DueAlert alert) =>
        alert.Kind == DueKind.Subscription ? "Subscription expires" : "Warranty ends";

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

    /// <summary>The application's pages, or none without its address.</summary>
    private sealed class Links(string applicationUrl)
    {
        private readonly string? root = applicationUrl == "" ? null : applicationUrl.TrimEnd('/');

        public string? Home => root is null ? null : $"{root}/";

        public string? Record(DueAlert alert) =>
            root is null
                ? null
                : $"{root}/{(alert.Kind == DueKind.Subscription ? "licenses" : "electronic-devices")}/{alert.AssetId}";

        public string? Location(Disposal disposal) =>
            root is null ? null : $"{root}/company/locations/{disposal.LocationId}";
    }
}
