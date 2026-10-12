namespace Codaxy.Inventory.App.Shared.Access;

/// <param name="Key">What a role stores and an endpoint requires: <c>section.action</c>.</param>
public sealed record Permission(string Key, string Name, string Description);

/// <summary>
/// The catalogue: every permission the application checks, and nothing it does not. Code checks a
/// permission, never a role; a role is a named set of these.
/// </summary>
public static class Permissions
{
    public const string DashboardView = "dashboard.view";
    public const string CompanyView = "company.view";
    public const string CompanyEdit = "company.edit";
    public const string ElectronicDevicesView = "electronic_devices.view";
    public const string ElectronicDevicesEdit = "electronic_devices.edit";
    public const string LicensesView = "licenses.view";
    public const string LicensesEdit = "licenses.edit";
    public const string FurnitureView = "furniture.view";
    public const string FurnitureEdit = "furniture.edit";
    public const string InformationView = "information.view";
    public const string InformationEdit = "information.edit";
    public const string InfrastructureView = "infrastructure.view";
    public const string InfrastructureEdit = "infrastructure.edit";

    /// <summary>All or nothing: the log holds every field of every section.</summary>
    public const string AuditLogView = "audit_log.view";
    public const string ServerLogView = "server_log.view";
    public const string UsersManage = "users.manage";

    public static IReadOnlyList<Permission> All { get; } =
    [
        new(DashboardView, "View the dashboard", "What is worth a look, across every section."),
        new(
            CompanyView,
            "View the company",
            "People, clients, projects, vendors, manufacturers, locations."
        ),
        new(CompanyEdit, "Edit the company", "Add, change and delete the company's records."),
        new(ElectronicDevicesView, "View electronic devices", "Devices, their types and tags."),
        new(
            ElectronicDevicesEdit,
            "Edit electronic devices",
            "Add, change and delete devices, types and tags."
        ),
        new(LicensesView, "View licenses", "Licenses, activations, software and services."),
        new(
            LicensesEdit,
            "Edit licenses",
            "Add, change and delete licenses; activate and deactivate seats."
        ),
        new(FurnitureView, "View furniture", "Furniture and its types."),
        new(FurnitureEdit, "Edit furniture", "Add, change and delete furniture and its types."),
        new(InformationView, "View information", "Information, its types and tags."),
        new(
            InformationEdit,
            "Edit information",
            "Add, change and delete information, its types and tags."
        ),
        new(
            InfrastructureView,
            "View infrastructure",
            "Virtual machines, cloud subscriptions, software."
        ),
        new(
            InfrastructureEdit,
            "Edit infrastructure",
            "Add, change and delete the infrastructure."
        ),
        new(AuditLogView, "View the audit log", "Every save, with every field, in every section."),
        new(ServerLogView, "View the server log", "What the server wrote, day by day."),
        new(UsersManage, "Manage users", "Add users, give them roles, deactivate them."),
    ];

    public static IReadOnlySet<string> Keys { get; } = All.Select(p => p.Key).ToHashSet();
}
