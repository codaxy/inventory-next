using Codaxy.Inventory.App.Shared.Email;

namespace Codaxy.Inventory.Web.Auth.OneTimeCodes.Request;

public sealed record CodeEmail(string Subject, string Text, string Html);

/// <summary>A sign-in code in <see cref="EmailLayout"/>, set large enough to read and type.</summary>
public static class CodeEmails
{
    public const string Subject = "Your Inventory sign-in code";

    /// <param name="home">The application's address, for the footer's link, or null.</param>
    public static CodeEmail Compose(string code, TimeSpan validity, string? home)
    {
        var minutes = $"{validity.TotalMinutes:0} minutes";
        var ignore = "If you did not ask for it, you can ignore this email.";

        var text =
            $"Your sign-in code is {code}. It is valid for {minutes}.\n\n{ignore}\n"
            + EmailLayout.TextFooter;

        var html = EmailLayout.Html(
            Subject,
            "Your sign-in code",
            null,
            $"""
            <p style="margin:0 0 16px;font-size:30px;line-height:1.2;font-weight:700;letter-spacing:0.15em;font-family:ui-monospace,Menlo,Consolas,monospace;">{EmailLayout.E(
                code
            )}</p>
            <p style="margin:0 0 8px;">It is valid for {minutes}.</p>
            <p class="muted" style="margin:0 0 16px;">{EmailLayout.E(ignore)}</p>
            """,
            home
        );

        return new(Subject, text, html);
    }
}
