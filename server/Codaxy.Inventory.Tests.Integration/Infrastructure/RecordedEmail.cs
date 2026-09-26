using System.Collections.Concurrent;
using Codaxy.Inventory.Web.Auth;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

/// <summary>Holds what would have been sent, so a test can read the code out of it.</summary>
public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<(string To, string Subject, string Body)> sent = new();

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellation = default
    )
    {
        sent.Enqueue((to, subject, body));
        return Task.CompletedTask;
    }

    public (string To, string Subject, string Body)? Last =>
        sent.TryPeek(out _) ? sent.Last() : null;

    public string? LastCodeFor(string email) =>
        sent.Where(m => m.To.Equals(email, StringComparison.OrdinalIgnoreCase))
            .Select(m => System.Text.RegularExpressions.Regex.Match(m.Body, "[0-9]{6}"))
            .LastOrDefault(m => m.Success)
            ?.Value;
}
