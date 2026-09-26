using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Codaxy.Inventory.Web.Auth;

public interface IEmailSender
{
    Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellation = default
    );
}

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions options = options.Value;

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellation = default
    )
    {
        var message = new MimeMessage
        {
            Subject = subject,
            Body = new TextPart("plain") { Text = body },
        };

        message.From.Add(MailboxAddress.Parse(options.From));
        message.To.Add(MailboxAddress.Parse(to));

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
