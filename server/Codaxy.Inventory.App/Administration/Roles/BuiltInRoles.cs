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

    private static readonly string[] Views =
    [
        P.DashboardView,
        P.CompanyView,
        P.ElectronicDevicesView,
        P.LicensesView,
        P.FurnitureView,
        P.InformationView,
        P.InfrastructureView,
    ];

    private static readonly string[] Edits =
    [
        P.CompanyEdit,
        P.ElectronicDevicesEdit,
        P.LicensesEdit,
        P.FurnitureEdit,
        P.InformationEdit,
        P.InfrastructureEdit,
    ];

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
            "Views and edits every section, reads the audit log and manages users.",
            false,
            [.. Views, .. Edits, P.AuditLogView, P.UsersManage]
        ),
        new(
            Supervisor,
            "Views every section and the audit log.",
            false,
            [.. Views, P.AuditLogView]
        ),
    ];

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
    /// grants. A stored permission the catalogue no longer has is dropped from every role.
    /// </summary>
    public static async Task SeedAsync(InventoryContext context, CancellationToken cancellation)
    {
        var roles = await context.Roles.Include(r => r.Permissions).ToListAsync(cancellation);

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
