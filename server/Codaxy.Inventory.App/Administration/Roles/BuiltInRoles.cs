using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Microsoft.EntityFrameworkCore;
using P = Codaxy.Inventory.App.Shared.Access.Permissions;

namespace Codaxy.Inventory.App.Administration.Roles;

/// <summary>The roles every database has, found by name, in authority order.</summary>
public static class BuiltInRoles
{
    public const string SystemAdmin = "System Admin";
    public const string Admin = "Admin";
    public const string Supervisor = "Supervisor";

    private sealed record Definition(
        string Name,
        string Description,
        bool IsSystemAdmin,
        string[] Permissions
    );

    private static readonly Definition[] Definitions =
    [
        new(SystemAdmin, "Everything, including permissions added later.", true, []),
        new(
            Admin,
            "Views and edits the inventory, reads the audit log and manages users.",
            false,
            [P.InventoryView, P.InventoryEdit, P.AuditLogView, P.UsersManage]
        ),
        new(Supervisor, "Views the inventory.", false, [P.InventoryView]),
    ];

    /// <summary>
    /// What Reset to default puts back on a built-in role — what a new database seeds it with — or
    /// none for System Admin, which has no editor, and for a custom role.
    /// </summary>
    public static (string Description, IReadOnlyList<string> Permissions)? DefaultsOf(
        string name
    ) =>
        Definitions.FirstOrDefault(d => d.Name == name && !d.IsSystemAdmin) is { } d
            ? (d.Description, d.Permissions)
            : null;

    /// <summary>
    /// A role's place in authority order: System Admin, the other built-in roles as listed, then
    /// every custom role.
    /// </summary>
    public static int Rank(string name) =>
        Array.FindIndex(Definitions, d => d.Name == name) is var i and >= 0
            ? i
            : Definitions.Length;

    /// <summary>
    /// Creates a built-in role that is missing, with its default permissions; leaves one that exists
    /// as it is, but for System Admin, whose rows follow the catalogue so a screen shows what it
    /// grants. A per-section key from before the catalogue was coarse (<see cref="P.Legacy"/>) puts a
    /// built-in role back to its defaults — nobody could edit one then — and becomes what it now is
    /// on a custom role; any other stored permission the catalogue no longer has is dropped.
    /// </summary>
    public static async Task SeedAsync(InventoryContext context, CancellationToken cancellation)
    {
        var roles = await context.Roles.Include(r => r.Permissions).ToListAsync(cancellation);

        foreach (
            var role in roles.Where(r => r.Permissions.Any(p => P.Legacy.ContainsKey(p.Permission)))
        )
        {
            if (role.IsBuiltIn && DefaultsOf(role.Name) is { } defaults)
            {
                role.Description = defaults.Description;
                Roles.Grant(context, role, defaults.Permissions);
            }
            else
                Grant(
                    context,
                    role,
                    role.Permissions.Select(p => P.Legacy.GetValueOrDefault(p.Permission))
                        .OfType<string>()
                        .Distinct()
                        .ToList()
                );
        }

        foreach (var definition in Definitions)
        {
            var role = roles.FirstOrDefault(r => r.Name == definition.Name);
            if (role is null)
            {
                role = new Role
                {
                    Id = Guid.CreateVersion7(),
                    Name = definition.Name,
                    Description = definition.Description,
                    IsSystemAdmin = definition.IsSystemAdmin,
                    IsBuiltIn = true,
                };
                context.Roles.Add(role);
                roles.Add(role);
                Grant(context, role, definition.Permissions);
            }

            if (role.IsSystemAdmin)
                Grant(context, role, P.All.Select(p => p.Key));
        }

        foreach (var role in roles)
        foreach (var stale in role.Permissions.Where(p => !P.Keys.Contains(p.Permission)).ToList())
        {
            role.Permissions.Remove(stale);
            context.Remove(stale);
        }

        await context.SaveChangesAsync(cancellation);
    }

    private static void Grant(InventoryContext context, Role role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions.Except(role.Permissions.Select(p => p.Permission)))
        {
            var row = new RolePermission
            {
                Id = Guid.CreateVersion7(),
                RoleId = role.Id,
                Permission = permission,
            };

            // Added to the context as well: a child with its key set, added only to a tracked
            // parent's collection, is taken for an existing row.
            role.Permissions.Add(row);
            context.Add(row);
        }
    }
}
