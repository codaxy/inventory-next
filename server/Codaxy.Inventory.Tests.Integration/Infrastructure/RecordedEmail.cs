using System.Collections.Concurrent;
using Codaxy.Inventory.App.Shared.Email;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

public sealed record SentEmail(IReadOnlyList<string> To, string Subject, string Body);

/// <summary>Holds what would have been sent, so a test can read the code out of it.</summary>
public sealed class RecordingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<SentEmail> sent = new();

    /// <summary>Makes every send fail, as a relay that is down does.</summary>
    public bool Failing { get; set; }

    public Task SendAsync(
        IReadOnlyList<string> to,
        string subject,
        string body,
        CancellationToken cancellation = default
    )
    {
        if (Failing)
            throw new IOException("The relay is down.");

        sent.Enqueue(new(to, subject, body));
        return Task.CompletedTask;
    }

    public IReadOnlyList<SentEmail> All => [.. sent];

    public void Clear() => sent.Clear();

    public string? LastCodeFor(string email) =>
        sent.Where(m => m.To.Contains(email, StringComparer.OrdinalIgnoreCase))
            .Select(m => System.Text.RegularExpressions.Regex.Match(m.Body, "[0-9]{6}"))
            .LastOrDefault(m => m.Success)
            ?.Value;
}
