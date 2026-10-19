using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Email;
using Codaxy.Inventory.App.Shared.Links;
using Microsoft.Extensions.Options;
using static Codaxy.Inventory.App.Shared.Email.EmailLayout;

namespace Codaxy.Inventory.App.Administration.Settings.TestEmail;

/// <summary>
/// A test email to the signed-in administrator — never to an address typed in, so the button mails
/// nobody else. Answers whether the relay took it and, if not, what the relay said: a failed test is
/// the answer asked for, not an error of the request.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder settings) => settings.MapPost("/mail/test", Handle);

    /// <param name="Problem">What the relay or the connection said, where it was not sent.</param>
    public sealed record Response(string To, bool Sent, string? Problem);

    public const string Subject = "Inventory test email";

    private static async Task<IResult> Handle(
        ICurrentUser user,
        IEmailSender email,
        IEmailRelay relay,
        IOptions<ApplicationOptions> application,
        ILogger<Response> log,
        CancellationToken cancellationToken
    )
    {
        // The group requires a session, so there is always one.
        var to = user.Email!;
        var smtp = relay.Describe();
        var through = $"{smtp.Host}:{smtp.Port}";
        const string Readable = "If you can read this, Inventory can send mail.";
        const string Page = "Administration → Settings";
        var home = application.Value.Home;
        var pageUrl = home is null ? null : $"{home}administration/settings";

        var sentFrom = $"Sent through {through} from {Page}";
        var text =
            $"{Readable}\n\n{sentFrom}{(pageUrl is null ? "" : $" ({pageUrl})")}.\n{TextFooter}";
        var html = Html(
            Subject,
            "Test email",
            null,
            $"""
            <p style="margin:0 0 8px;">{E(Readable)}</p>
            <p class="muted" style="margin:0 0 16px;">Sent through {E(through)} from {Link(
                pageUrl,
                Page
            )}.</p>
            """,
            home
        );

        try
        {
            await email.SendAsync([to], Subject, text, html, cancellationToken);
            return Results.Ok(new Response(to, true, null));
        }
        catch (Exception e) when (!cancellationToken.IsCancellationRequested)
        {
            log.LogWarning(e, "Test email to {Email} was not sent", to);
            return Results.Ok(new Response(to, false, e.Message));
        }
    }
}
