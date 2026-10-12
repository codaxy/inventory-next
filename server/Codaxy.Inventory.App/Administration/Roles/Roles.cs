using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Roles;

/// <summary>What creating and editing a role take.</summary>
/// <param name="Permissions">Keys from the catalogue; a section's edit only with its view.</param>
public sealed record RoleForm(
    [property:
        Required(ErrorMessage = "Give the role a name."),
        StringLength(100, ErrorMessage = "A name is at most 100 characters.")
    ]
        string? Name,
    [property: StringLength(500, ErrorMessage = "A description is at most 500 characters.")]
        string? Description,
    string[]? Permissions
);

/// <summary>A role as its page shows it.</summary>
/// <param name="Users">How many users hold it; one held by any is not deleted.</param>
/// <param name="IsOwn">The reader holds it, so may not change it.</param>
/// <param name="CanManage">The reader holds everything it grants, by the subset rule.</param>
public sealed record RoleDetail(
    Guid Id,
    string Name,
    string Description,
    bool IsBuiltIn,
    bool IsSystemAdmin,
    IReadOnlyList<string> Permissions,
    int Users,
    bool IsOwn,
    bool CanManage
);

internal static class Roles
{
    public static IResult Conflict(string title) =>
        Results.Problem(title: title, statusCode: StatusCodes.Status409Conflict);

    public static IResult Forbidden(string title) =>
        Results.Problem(title: title, statusCode: StatusCodes.Status403Forbidden);

    public static IResult Problem(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    public static Task<Role?> FindAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    /// <summary>
    /// Why <paramref name="manager"/> may not change <paramref name="role"/> at all, or none: System
    /// Admin changes for nobody, the role one holds is not one's own to change, and a role granting
    /// more than the manager's is out of reach.
    /// </summary>
    public static IResult? Untouchable(Access manager, Role role)
    {
        if (role.IsSystemAdmin)
            return Conflict("System Admin grants everything; nothing about it changes.");

        if (role.Id == manager.RoleId)
            return Conflict("You cannot change the role you hold.");

        if (!manager.Covers(role))
            return Forbidden("This role grants more than yours, so you cannot change it.");

        return null;
    }

    /// <summary>
    /// The form's permissions as a set, or the problem with them: each in the catalogue, an edit
    /// with its view, and every one held by the manager — nobody grants what they do not hold.
    /// </summary>
    public static (IReadOnlySet<string>? Permissions, IResult? Problem) Read(
        RoleForm form,
        Access manager
    )
    {
        var permissions = (form.Permissions ?? []).ToHashSet();

        if (
            permissions.FirstOrDefault(p => !Shared.Access.Permissions.Keys.Contains(p)) is
            { } unknown
        )
            return (null, Problem("permissions", $"There is no permission \"{unknown}\"."));

        if (
            permissions.FirstOrDefault(p =>
                Shared.Access.Permissions.Implied(p) is { } view && !permissions.Contains(view)
            ) is
            { } edit
        )
            return (
                null,
                Problem(
                    "permissions",
                    $"{edit} needs {Shared.Access.Permissions.Implied(edit)} too."
                )
            );

        if (permissions.FirstOrDefault(p => !manager.Has(p)) is { } lacking)
            return (
                null,
                Problem("permissions", $"You cannot grant {lacking}, which you do not hold.")
            );

        return (permissions, null);
    }

    public static async Task<IResult?> CheckNameAsync(
        InventoryContext context,
        string name,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var lower = name.Trim().ToLowerInvariant();
        return await context.Roles.AnyAsync(
            r => r.Id != except && r.Name.Trim().ToLower() == lower,
            cancellationToken
        )
            ? Problem("name", "A role with this name already exists.")
            : null;
    }

    /// <summary>
    /// The role's rows made to hold exactly <paramref name="permissions"/>: removed and added one by
    /// one, so the audit log records each grant and each revocation.
    /// </summary>
    public static void Grant(InventoryContext context, Role role, IEnumerable<string> permissions)
    {
        var wanted = permissions.ToHashSet();

        foreach (
            var revoked in role.Permissions.Where(p => !wanted.Contains(p.Permission)).ToList()
        )
        {
            role.Permissions.Remove(revoked);
            context.Remove(revoked);
        }

        foreach (var granted in wanted.Except(role.Permissions.Select(p => p.Permission)))
        {
            var row = new RolePermission
            {
                Id = Guid.CreateVersion7(),
                RoleId = role.Id,
                Permission = granted,
            };

            // Added to the context as well: a child with its key set, added only to a tracked
            // parent's collection, is taken for an existing row.
            role.Permissions.Add(row);
            context.Add(row);
        }
    }

    public static async Task<RoleDetail?> DetailAsync(
        InventoryContext context,
        Access reader,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var role = await context
            .Roles.AsNoTracking()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role is null)
            return null;

        return new RoleDetail(
            role.Id,
            role.Name,
            role.Description,
            role.IsBuiltIn,
            role.IsSystemAdmin,
            role.Grants().Order(StringComparer.Ordinal).ToList(),
            await context.AppUsers.CountAsync(u => u.RoleId == id, cancellationToken),
            role.Id == reader.RoleId,
            reader.Covers(role)
        );
    }
}
