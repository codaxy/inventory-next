namespace Codaxy.Inventory.App.Shared.Email;

/// <summary>
/// Mail, one message to every recipient: a failure sends to none of them rather than to some, so
/// trying again reaches nobody twice. Plain text, with an HTML version where one is given.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(
        IReadOnlyList<string> to,
        string subject,
        string text,
        string? html = null,
        CancellationToken cancellation = default
    );
}
