using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>Who someone is to the application and what they may do.</summary>
public sealed record Access(
    Guid UserId,
    string Email,
    string Name,
    Guid RoleId,
    string RoleName,
    bool IsSystemAdmin,
    IReadOnlySet<string> Permissions
)
{
    public bool Has(string permission) => Permissions.Contains(permission);

    /// <summary>
    /// Whether everything <paramref name="role"/> grants is this access's own: the rule that keeps
    /// anyone from acting on someone more powerful, or handing out what they do not hold.
    /// </summary>
    public bool Covers(Role role) =>
        IsSystemAdmin || (!role.IsSystemAdmin && role.Grants().IsSubsetOf(Permissions));

    internal static Access Of(Guid userId, string email, string name, Role role) =>
        new(userId, email, name, role.Id, role.Name, role.IsSystemAdmin, role.Grants());
}

/// <summary>An employee signed in as the person they are: no role, no permission, their own equipment.</summary>
public sealed record PersonAccess(Guid PersonId, string Email, string Name);

/// <summary>
/// Reads someone's access from the database, every request and uncached beyond it: a deactivation or
/// a changed role applies on the next request. Scoped, so the cookie's check and the endpoint's share
/// one query.
/// </summary>
public sealed class UserAccess(
    InventoryContext context,
    ICurrentUser current,
    SettingStore settings
)
{
    private readonly Dictionary<string, Access?> read = [];

    /// <summary>The signed-in user's access, or none.</summary>
    public Task<Access?> CurrentAsync(CancellationToken cancellation = default) =>
        current.Email is { } email ? ForAsync(email, cancellation) : Task.FromResult<Access?>(null);

    /// <summary>
    /// The access of whoever holds <paramref name="email"/>: an active user's role, or none.
    /// </summary>
    public async Task<Access?> ForAsync(string email, CancellationToken cancellation = default)
    {
        email = AppUser.Normalize(email);
        if (read.TryGetValue(email, out var known))
            return known;

        var user = await context
            .AppUsers.AsNoTracking()
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, cancellation);

        var access = user is { IsActive: true }
            ? Access.Of(user.Id, user.Email, user.Name, user.Role)
            : null;

        read[email] = access;
        return access;
    }

    /// <summary>
    /// Whether <paramref name="email"/> may hold a session: an active user, or a person who may sign in
    /// while employees may. The one answer for a code, for Google and for the cookie on every request.
    /// </summary>
    public async Task<bool> MaySignInAsync(
        string email,
        CancellationToken cancellation = default
    ) =>
        await ForAsync(email, cancellation) is not null
        || await PersonAsync(email, cancellation) is not null;

    /// <summary>The signed-in person, where the session is one and not a user's.</summary>
    public Task<PersonAccess?> CurrentPersonAsync(CancellationToken cancellation = default) =>
        current.Email is { } email
            ? PersonAsync(email, cancellation)
            : Task.FromResult<PersonAccess?>(null);

    /// <summary>
    /// The person holding <paramref name="email"/> — trimmed, whatever the case, as people are kept —
    /// where employees may sign in and this one may; none otherwise.
    /// </summary>
    public async Task<PersonAccess?> PersonAsync(
        string email,
        CancellationToken cancellation = default
    )
    {
        if (!(await settings.LoadAsync(cancellation)).Get(SignInSettings.People))
            return null;

        email = AppUser.Normalize(email);
        return await context
            .Persons.AsNoTracking()
            .Where(p => p.CanSignIn && p.EmailKey == email)
            .Select(p => new PersonAccess(p.Id, p.Email.Trim(), p.Name))
            .FirstOrDefaultAsync(cancellation);
    }
}
