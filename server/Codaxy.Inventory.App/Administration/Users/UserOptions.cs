namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>The users a deployment states in configuration, under <c>Auth</c> beside how they sign in.</summary>
public sealed class UserOptions
{
    public const string Section = "Auth";

    /// <summary>Created at startup when missing; a user that exists is left as it is.</summary>
    public SeedUser[] SeedUsers { get; init; } = [];

    public sealed class SeedUser
    {
        public string Email { get; init; } = "";
        public string Name { get; init; } = "";

        /// <summary>A role's name, since ids differ between databases.</summary>
        public string Role { get; init; } = "";
    }

    public string? Problem =>
        SeedUsers.FirstOrDefault(u =>
            string.IsNullOrWhiteSpace(u.Email)
            || string.IsNullOrWhiteSpace(u.Name)
            || string.IsNullOrWhiteSpace(u.Role)
        )
            is { } incomplete
            ? $"{Section}:SeedUsers has an entry without an email, a name or a role ({incomplete.Email}): give each all three."
            : null;
}
