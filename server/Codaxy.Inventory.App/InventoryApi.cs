namespace Codaxy.Inventory.App;

public static class InventoryApi
{
    /// <summary>
    /// The menu's endpoints, under <c>/api</c>, every one behind a session: an item's group in the
    /// menu's order, each mapping its own use cases.
    /// </summary>
    public static void MapInventoryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        Dashboard.Endpoints.Map(api);
        Company.People.Endpoints.Map(api);
        Company.Clients.Endpoints.Map(api);
        Company.Projects.Endpoints.Map(api);
        Company.Vendors.Endpoints.Map(api);
        Company.Manufacturers.Endpoints.Map(api);
        Company.Locations.Endpoints.Map(api);
        ElectronicDevices.Devices.Endpoints.Map(api);
        ElectronicDevices.Types.Endpoints.Map(api);
        ElectronicDevices.Tags.Endpoints.Map(api);
        Licenses.Licenses.Endpoints.Map(api);
        Licenses.Activations.Endpoints.Map(api);
        Licenses.SoftwareServices.Endpoints.Map(api);
        Furnitures.Items.Endpoints.Map(api);
        Furnitures.Types.Endpoints.Map(api);
        Informations.Items.Endpoints.Map(api);
        Informations.Types.Endpoints.Map(api);
        Informations.Tags.Endpoints.Map(api);
        Infrastructure.VirtualMachines.Endpoints.Map(api);
        Infrastructure.CloudSubscriptions.Endpoints.Map(api);
        Infrastructure.Softwares.Endpoints.Map(api);
        Administration.AuditLogs.Endpoints.Map(api);
        Administration.ServerLogs.Endpoints.Map(api);
    }
}
