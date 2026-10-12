using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Auth;

/// <summary>
/// The deployment's outer limit on who may sign in — the domain — applied to whichever provider the
/// person came through. Inside it, being a user decides; see <c>UserAccess</c>.
/// </summary>
public sealed class SignInPolicy(IOptions<AuthOptions> options)
{
    private readonly AuthOptions options = options.Value;

    public SignInResult Evaluate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return SignInResult.Refused("The account has no email address.");

        email = email.Trim();

        if (options.AllowedDomains.Length > 0 && !IsInAnAllowedDomain(email))
            return SignInResult.Refused(
                "That email domain is not allowed to sign in.",
                IsSayable: true
            );

        return SignInResult.Allowed;
    }

    private bool IsInAnAllowedDomain(string email)
    {
        var at = email.LastIndexOf('@');

        if (at < 0)
            return false;

        var domain = email[(at + 1)..];

        // Configured domains are written as people write them, so "@Codaxy.com " matches.
        return options.AllowedDomains.Any(allowed =>
            Normalize(allowed).Equals(domain, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static string Normalize(string domain) => domain.Trim().TrimStart('@');

    /// <param name="IsSayable">
    /// Whether the reason may be told to whoever asked. That a domain is refused may be: it leaves
    /// someone who mistyped their own address with something to act on, and names nothing. Which
    /// domain is accepted may not.
    /// </param>
    public readonly record struct SignInResult(bool IsAllowed, string? Reason, bool IsSayable)
    {
        public static SignInResult Allowed { get; } = new(true, null, false);

        public static SignInResult Refused(string reason, bool IsSayable = false) =>
            new(false, reason, IsSayable);
    }
}
