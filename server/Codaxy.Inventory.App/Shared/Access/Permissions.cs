namespace Codaxy.Inventory.App.Shared.Access;

/// <param name="Key">What a role stores and an endpoint requires: <c>area.action</c>.</param>
/// <param name="Section">What a screen groups it under: a view and its edit share one.</param>
public sealed record Permission(string Key, string Section, string Name, string Description);

/// <summary>
/// The catalogue: every permission the application checks, and nothing it does not. Code checks a
/// permission, never a role; a role is a named set of these. Coarse, as the application is used:
/// nobody views or edits part of the inventory and not the rest.
/// </summary>
public static class Permissions
{
    /// <summary>Every section of the inventory and the dashboard.</summary>
    public const string InventoryView = "inventory.view";
    public const string InventoryEdit = "inventory.edit";

    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";

    /// <summary>The application's settings, which apply to everyone.</summary>
    public const string SettingsManage = "settings.manage";

    /// <summary>
    /// Apart from the inventory: the log holds who changed what, and the whole of deleted records.
    /// </summary>
    public const string AuditLogView = "audit_log.view";
    public const string ServerLogView = "server_log.view";

    /// <summary>In the menu's order, which the role screen follows: the inventory, then Administration.</summary>
    public static IReadOnlyList<Permission> All { get; } =
    [
        new(InventoryView, "Inventory", "View", "Every section and the dashboard."),
        new(
            InventoryEdit,
            "Inventory",
            "Edit",
            "Add, change and delete anything in the inventory."
        ),
        new(UsersManage, "Users", "Manage", "Add users, give them roles, deactivate them."),
        new(RolesManage, "Roles", "Manage", "Define roles and what they grant."),
        new(
            SettingsManage,
            "Settings",
            "Manage",
            "Change how the application behaves for everyone."
        ),
        new(AuditLogView, "Audit log", "View", "Who changed what, and deleted records whole."),
        new(ServerLogView, "Server log", "View", "What the server wrote, day by day."),
    ];

    public static IReadOnlySet<string> Keys { get; } = All.Select(p => p.Key).ToHashSet();

    private const string EditSuffix = ".edit";

    /// <summary>
    /// The permission <paramref name="key"/> cannot be held without: an <c>edit</c> its
    /// <c>view</c>, since an editor cannot load the record it edits.
    /// </summary>
    public static string? Implied(string key) =>
        key.EndsWith(EditSuffix, StringComparison.Ordinal)
            ? key[..^EditSuffix.Length] + ".view"
            : null;

    /// <summary>
    /// The per-section keys roles stored before the catalogue was coarse, and what each became.
    /// The seeder translates them; deletable once every database has started with it.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Legacy { get; } =
        new Dictionary<string, string>
        {
            ["dashboard.view"] = InventoryView,
            ["company.view"] = InventoryView,
            ["electronic_devices.view"] = InventoryView,
            ["licenses.view"] = InventoryView,
            ["furniture.view"] = InventoryView,
            ["information.view"] = InventoryView,
            ["infrastructure.view"] = InventoryView,
            ["company.edit"] = InventoryEdit,
            ["electronic_devices.edit"] = InventoryEdit,
            ["licenses.edit"] = InventoryEdit,
            ["furniture.edit"] = InventoryEdit,
            ["information.edit"] = InventoryEdit,
            ["infrastructure.edit"] = InventoryEdit,
        };
}
