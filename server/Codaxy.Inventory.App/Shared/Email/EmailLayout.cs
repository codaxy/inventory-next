using System.Net;

namespace Codaxy.Inventory.App.Shared.Email;

/// <summary>
/// Every email's HTML: a notification, not a newsletter — one white card on a grey page under a thin
/// violet bar, the mark and a heading that says what happened, the content, and the sender beneath. No
/// banner, no imagery, no call to action. Inline layout, since mail clients read little CSS; colours
/// only in the stylesheet, so a dark theme can set them.
/// </summary>
public static class EmailLayout
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

    /// <summary>The plain text's last line, after a blank one.</summary>
    public const string TextFooter = "\nSent by Inventory\n";

    /// <param name="title">The subject, as the document's title.</param>
    /// <param name="lede">A line under the heading, or null.</param>
    /// <param name="content">HTML, escaped by the caller.</param>
    /// <param name="home">The application's address, linking "Inventory" in the footer, or null.</param>
    public static string Html(
        string title,
        string heading,
        string? lede,
        string content,
        string? home
    )
    {
        var under = lede is null ? "" : $"""<p style="margin:0 0 24px;">{E(lede)}</p>""";
        var headingMargin = lede is null ? "32px 0 16px" : "32px 0 4px";
        var inventory = Link(home, "Inventory");

        return $"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="color-scheme" content="light dark">
            <meta name="supported-color-schemes" content="light dark">
            <title>{E(title)}</title>
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
            <h1 style="margin:{headingMargin};font-size:22px;line-height:1.3;font-weight:700;">{E(
                heading
            )}</h1>
            {under}
            {content}</td></tr>
            <tr><td class="muted" align="center" style="padding:20px 0 0;font-size:12px;">Sent by {inventory}</td></tr>
            </table>
            </td></tr>
            </table>
            </body>
            </html>
            """;
    }

    public static string E(string text) => WebUtility.HtmlEncode(text);

    /// <summary>An anchor where there is an address, the text alone where there is none.</summary>
    public static string Link(string? url, string text) =>
        url is null ? E(text) : $"""<a href="{E(url)}">{E(text)}</a>""";
}
