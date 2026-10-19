using System.Globalization;
using System.Net;
using System.Text;

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
    Disposal? DisposedAt
);

public sealed record AlertEmail(string Subject, string Text, string Html);

/// <summary>
/// One email for everything due: what ends, when, and what changes the reading — as HTML, with a
/// plain-text copy for a client that shows no HTML. A notification, not a newsletter: one white card
/// on a grey page under a thin violet bar, the mark and the heading, the records, the sender beneath —
/// no banner, no imagery, no call to action. A name with a page links to it where the application's
/// address is known.
/// </summary>
public static class ExpiryAlertMessage
{
    private const string Font = "-apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";

    /// <summary>
    /// The page, the card and their text always set together, light or dark as the reader's theme is: a
    /// client that follows a dark theme for the text but keeps its own white behind it would otherwise
    /// show white on white. Never inline, where a dark theme could not undo it; a client that reads no
    /// stylesheet keeps its own colours.
    /// </summary>
    private const string Style = """
        :root { color-scheme: light dark; supported-color-schemes: light dark; }
        body, .page { background: #f3f5f8; color: #101a2b; }
        .card { background: #ffffff; color: #101a2b; }
        a { color: #2a60cf; }
        .muted, .muted a { color: #5b6779; }
        .rule { border-top: 1px solid #e4e8ef; }
        @media (prefers-color-scheme: dark) {
          body, .page { background: #0f1217 !important; color: #e7eaf0 !important; }
          .card { background: #1b1f27 !important; color: #e7eaf0 !important; }
          a { color: #8fb1ff !important; }
          .muted, .muted a { color: #a3acbb !important; }
          .rule { border-top-color: #2f3644 !important; }
        }
        """;

    /// <summary>Inventory's mark: the violet of its logo tile, in either theme.</summary>
    private const string Violet = "#8455e0";

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

        return text.Append("\nSent by Inventory\n").ToString();
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

        var title = E(Subject(due));
        var heading = E(Heading(due));
        var within = E(Within(due, lead));
        var inventory = Link(links.Home, "Inventory");

        return $"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="color-scheme" content="light dark">
            <meta name="supported-color-schemes" content="light dark">
            <title>{title}</title>
            <style>{Style}</style>
            </head>
            <body style="margin:0;padding:0;">
            <table role="presentation" class="page" width="100%" cellpadding="0" cellspacing="0" style="font-family:{Font};font-size:15px;line-height:1.5;">
            <tr><td align="center" style="padding:32px 12px;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;">
            <tr><td style="height:4px;line-height:4px;font-size:0;background:{Violet};">&nbsp;</td></tr>
            <tr><td class="card" style="padding:32px 32px 16px;text-align:left;">
            <table role="presentation" cellpadding="0" cellspacing="0"><tr>
            <td style="width:26px;height:26px;border-radius:7px;background:{Violet};color:#ffffff;font-size:14px;font-weight:700;text-align:center;vertical-align:middle;">I</td>
            <td style="padding-left:10px;font-size:18px;font-weight:700;vertical-align:middle;">Inventory</td>
            </tr></table>
            <h1 style="margin:32px 0 4px;font-size:22px;line-height:1.3;font-weight:700;">{heading}</h1>
            <p style="margin:0 0 24px;">{within}</p>
            {items}</td></tr>
            <tr><td class="muted" align="center" style="padding:20px 0 0;font-size:12px;">Sent by {inventory}</td></tr>
            </table>
            </td></tr>
            </table>
            </body>
            </html>
            """;
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
        $"{Ends(alert)} on {Date(alert.Date)} ({When(alert.Date, today)}).";

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

    private static string E(string text) => WebUtility.HtmlEncode(text);

    /// <summary>An anchor where there is an address, the text alone where there is none.</summary>
    private static string Link(string? url, string text) =>
        url is null ? E(text) : $"""<a href="{E(url)}">{E(text)}</a>""";

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
