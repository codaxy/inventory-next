using System.Collections.Concurrent;
using System.Security.Cryptography;
using Codaxy.Inventory.Setup;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Auth;

/// <summary>
/// Codes live in memory, so restarting the process invalidates every outstanding one and a second
/// instance cannot verify a code the first issued. That is the simplest thing that works for a
/// single container and is deliberately temporary — the store is an interface so the decision can be
/// taken again without touching the endpoints.
/// </summary>
public interface IOneTimeCodeStore
{
    /// <summary>The code to send, or null while the address is within its cooldown.</summary>
    string? Issue(string email);

    bool Consume(string email, string code);
}

public sealed class InMemoryOneTimeCodeStore(
    IOptions<AuthOptions> options,
    TimeProvider time,
    ILogger<InMemoryOneTimeCodeStore> log
) : IOneTimeCodeStore
{
    private readonly ConcurrentDictionary<string, Entry> entries = new(
        StringComparer.OrdinalIgnoreCase
    );
    private readonly AuthOptions options = options.Value;

    public string? Issue(string email)
    {
        var now = time.GetUtcNow();

        if (entries.TryGetValue(email, out var outstanding) && now < outstanding.NextIssueAt)
            return null;

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        entries[email] = new Entry(
            code,
            now + options.OneTimeCode.Validity,
            now + options.OneTimeCode.Cooldown
        );
        Prune();

        return code;
    }

    public bool Consume(string email, string code)
    {
        if (!entries.TryGetValue(email, out var entry))
            return false;

        // A code is single-use whether or not it matched: removing it on failure as well is what
        // stops six digits being guessed a thousand times.
        entries.TryRemove(email, out _);

        if (entry.ExpiresAt <= time.GetUtcNow())
        {
            log.LogInformation("One-time code for {Email} had expired.", email);
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(entry.Code),
            System.Text.Encoding.UTF8.GetBytes(code)
        );
    }

    private void Prune()
    {
        var now = time.GetUtcNow();

        foreach (var (email, entry) in entries)
            if (entry.ExpiresAt <= now)
                entries.TryRemove(email, out _);
    }

    private readonly record struct Entry(
        string Code,
        DateTimeOffset ExpiresAt,
        DateTimeOffset NextIssueAt
    );
}
