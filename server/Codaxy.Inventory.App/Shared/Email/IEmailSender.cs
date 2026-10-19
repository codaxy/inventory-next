namespace Codaxy.Inventory.App.Shared.Email;

/// <summary>
/// Plain-text mail, one message to every recipient: a failure sends to none of them rather than to
/// some, so trying again reaches nobody twice.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(
        IReadOnlyList<string> to,
        string subject,
        string body,
        CancellationToken cancellation = default
    );
}
