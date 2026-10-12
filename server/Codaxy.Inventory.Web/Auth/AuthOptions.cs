namespace Codaxy.Inventory.Web.Auth;

/// <summary>
/// How people sign in. Google is enabled by the presence of its credentials rather than by a
/// flag, so a checkout with no secrets has no provider that could half-work; one-time codes carry
/// their own flag because they need no secret and would otherwise always be on.
/// </summary>
public sealed class AuthOptions
{
    public const string Section = "Auth";

    public GoogleOptions Google { get; init; } = new();

    public OneTimeCodeOptions OneTimeCode { get; init; } = new();

    public sealed class GoogleOptions
    {
        public string? ClientId { get; init; }

        public string? ClientSecret { get; init; }

        public bool Enabled =>
            !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
    }

    public sealed class OneTimeCodeOptions
    {
        public bool Enabled { get; init; }

        public TimeSpan Validity { get; init; } = TimeSpan.FromMinutes(10);

        /// <summary>How long one address waits between codes, whoever asks for them.</summary>
        public TimeSpan Cooldown { get; init; } = TimeSpan.FromMinutes(1);
    }
}
