using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.Web.Auth;

/// <summary>
/// The outer limit on who may sign in — the allowed domains, a setting — applied to whichever
/// provider the person came through. Inside it, being a user decides; see <c>UserAccess</c>.
/// </summary>
public sealed class SignInPolicy(SettingStore settings)
{
    public async Task<SignInResult> EvaluateAsync(
        string? email,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            return SignInResult.Refused("The account has no email address.");

        var domains = (await settings.LoadAsync(cancellationToken)).Get(
            SignInSettings.AllowedDomains
        );

        if (!SignInSettings.Allows(domains, email.Trim()))
            return SignInResult.Refused(
                "That email domain is not allowed to sign in.",
                IsSayable: true
            );

        return SignInResult.Allowed;
    }

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
