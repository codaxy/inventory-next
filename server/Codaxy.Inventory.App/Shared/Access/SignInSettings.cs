using System.Text.RegularExpressions;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Shared.Access;

/// <summary>Who may sign in, beyond being a user.</summary>
public static partial class SignInSettings
{
    /// <summary>
    /// The domains an account may belong to, inside which being a user decides; empty, any domain.
    /// Stored as a domain is written in an address — lower-case, no <c>@</c>, once each.
    /// </summary>
    public static readonly SettingDefinition<string[]> AllowedDomains = new(
        "sign_in.allowed_domains",
        [],
        domains =>
            domains.FirstOrDefault(d => !Domain().IsMatch(d)) is { } wrong
                ? $"\"{wrong}\" is not a domain."
                : null,
        domains =>
            domains
                .Select(d => d.Trim().TrimStart('@').ToLowerInvariant())
                .Where(d => d != "")
                .Distinct()
                .ToArray()
    );

    /// <summary>
    /// Whether employees may sign in as the person they are, to see their own inventory: the master
    /// switch over every person's <c>CanSignIn</c>. Off by default — nothing opens until someone opens it.
    /// </summary>
    public static readonly SettingDefinition<bool> People = new("sign_in.people", false);

    /// <summary>Whether an address belongs to one of the domains, or the list is empty.</summary>
    public static bool Allows(IReadOnlyCollection<string> domains, string email)
    {
        if (domains.Count == 0)
            return true;

        var at = email.LastIndexOf('@');
        return at >= 0
            && domains.Contains(email[(at + 1)..].Trim(), StringComparer.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)+$")]
    private static partial Regex Domain();
}
