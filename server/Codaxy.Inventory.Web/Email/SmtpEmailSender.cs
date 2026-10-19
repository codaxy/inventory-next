using Codaxy.Inventory.App.Shared.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Codaxy.Inventory.Web.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions options = options.Value;

    public async Task SendAsync(
        IReadOnlyList<string> to,
        string subject,
        string text,
        string? html = null,
        CancellationToken cancellation = default
    )
    {
        var message = new MimeMessage
        {
            Subject = subject,
            Body = new BodyBuilder { TextBody = text, HtmlBody = html }.ToMessageBody(),
        };

        message.From.Add(MailboxAddress.Parse(options.From));
        foreach (var address in to)
            message.To.Add(MailboxAddress.Parse(address));

        using var client = new SmtpClient();

        // Mailpit and every other local relay speak plaintext on 1025; requiring TLS here would make
        // the development loop need certificates it has no reason to have.
        await client.ConnectAsync(
            options.Host,
            options.Port,
            SecureSocketOptions.StartTlsWhenAvailable,
            cancellation
        );
        await client.SendAsync(message, cancellation);
        await client.DisconnectAsync(true, cancellation);
    }
}
