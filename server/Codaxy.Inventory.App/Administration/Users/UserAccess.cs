using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>Who someone is to the application and what they may do.</summary>
/// <param name="UserId">None for an address admitted by configuration and not yet added.</param>
public sealed record Access(
    Guid? UserId,
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

    internal static Access Of(Guid? userId, string email, string name, Role role) =>
        new(userId, email, name, role.Id, role.Name, role.IsSystemAdmin, role.Grants());
}

/// <summary>
/// Reads someone's access from the database, every request and uncached beyond it: a deactivation or
/// a changed role applies on the next request. Scoped, so the cookie's check and the endpoint's share
/// one query.
/// </summary>
public sealed class UserAccess(
    InventoryContext context,
    IOptions<UserOptions> options,
    ICurrentUser current
)
{
    private readonly Dictionary<string, Access?> read = [];

    /// <summary>The signed-in user's access, or none.</summary>
    public Task<Access?> CurrentAsync(CancellationToken cancellation = default) =>
        current.Email is { } email ? ForAsync(email, cancellation) : Task.FromResult<Access?>(null);

    /// <summary>
    /// The access of whoever holds <paramref name="email"/>: an active user's role; for an address
    /// admitted by configuration that is not a user yet, Admin's; for anyone else, none.
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

        Access? access;
        if (user is not null)
            access = user.IsActive ? Access.Of(user.Id, user.Email, user.Name, user.Role) : null;
        else if (options.Value.Admits(email))
            access = await context
                .Roles.AsNoTracking()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == BuiltInRoles.Admin, cancellation)
                is { } admin
                ? Access.Of(null, email, email, admin)
                : null;
        else
            access = null;

        read[email] = access;
        return access;
    }
}
