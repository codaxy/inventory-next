namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>
/// The users a deployment states in configuration, under <c>Auth</c> beside how they sign in.
/// </summary>
public sealed class UserOptions
{
    public const string Section = "Auth";

    /// <summary>Created at startup when missing; a user that exists is left as it is.</summary>
    public SeedUser[] SeedUsers { get; init; } = [];

    /// <summary>
    /// Addresses that sign in as Admin until they are added as users. A user, once added, decides
    /// for itself and the list no longer matters for that address. Deleted once every listed address
    /// is a user on every deployment.
    /// </summary>
    public string[] AllowedUsers { get; init; } = [];

    /// <summary>Addresses on <see cref="AllowedUsers"/> it does not admit after all.</summary>
    public string[] DeniedUsers { get; init; } = [];

    public sealed class SeedUser
    {
        public string Email { get; init; } = "";
        public string Name { get; init; } = "";

        /// <summary>A role's name, since ids differ between databases.</summary>
        public string Role { get; init; } = "";
    }

    /// <summary>Whether <paramref name="email"/> signs in without being a user.</summary>
    public bool Admits(string email) =>
        Contains(AllowedUsers, email) && !Contains(DeniedUsers, email);

    /// <summary>The addresses admitted without being a user, normalised.</summary>
    public IEnumerable<string> Admitted() =>
        AllowedUsers
            .Select(AppUser.Normalize)
            .Where(e => e.Length > 0 && !Contains(DeniedUsers, e))
            .Distinct();

    public string? Problem =>
        SeedUsers.FirstOrDefault(u =>
            string.IsNullOrWhiteSpace(u.Email)
            || string.IsNullOrWhiteSpace(u.Name)
            || string.IsNullOrWhiteSpace(u.Role)
        )
            is { } incomplete
            ? $"{Section}:SeedUsers has an entry without an email, a name or a role ({incomplete.Email}): give each all three."
            : null;

    private static bool Contains(string[] list, string email) =>
        list.Any(e => AppUser.Normalize(e) == AppUser.Normalize(email));
}
