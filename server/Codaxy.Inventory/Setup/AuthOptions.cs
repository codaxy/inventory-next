namespace Codaxy.Inventory.Setup;

/// <summary>
/// Who may sign in, and how. Google is enabled by the presence of its credentials rather than by a
/// flag, so a checkout with no secrets has no provider that could half-work; one-time codes carry
/// their own flag because they need no secret and would otherwise always be on.
/// </summary>
public sealed class AuthOptions
{
    public const string Section = "Auth";

    public GoogleOptions Google { get; init; } = new();

    public OneTimeCodeOptions OneTimeCode { get; init; } = new();

    /// <summary>
    /// The domains an account may belong to. Empty allows any, which is what an internal deployment
    /// behind an allow list may want and what a public one must not.
    /// </summary>
    public string[] AllowedDomains { get; init; } = [];

    /// <summary>When non-empty, only these addresses may sign in.</summary>
    public string[] AllowedUsers { get; init; } = [];

    /// <summary>Addresses refused regardless of the two rules above.</summary>
    public string[] DeniedUsers { get; init; } = [];

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
