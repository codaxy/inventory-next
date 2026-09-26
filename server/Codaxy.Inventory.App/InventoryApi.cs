namespace Codaxy.Inventory.App;

public static class InventoryApi
{
    /// <summary>The menu's endpoints, under <c>/api</c>, every one behind a session.</summary>
    public static void MapInventoryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        var tags = api.MapGroup("/electronic-devices/tags");

        ElectronicDevices.Tags.List.Endpoint.Map(tags);
        ElectronicDevices.Tags.Options.Endpoint.Map(tags);
        ElectronicDevices.Tags.Get.Endpoint.Map(tags);
        ElectronicDevices.Tags.Create.Endpoint.Map(tags);
        ElectronicDevices.Tags.Update.Endpoint.Map(tags);
        ElectronicDevices.Tags.Delete.Endpoint.Map(tags);

        var types = api.MapGroup("/electronic-devices/types");

        ElectronicDevices.Types.List.Endpoint.Map(types);
        ElectronicDevices.Types.Options.Endpoint.Map(types);
        ElectronicDevices.Types.Get.Endpoint.Map(types);
        ElectronicDevices.Types.Create.Endpoint.Map(types);
        ElectronicDevices.Types.Update.Endpoint.Map(types);
        ElectronicDevices.Types.Delete.Endpoint.Map(types);

        var licenses = api.MapGroup("/licenses");

        Licenses.Licenses.List.Endpoint.Map(licenses);
        Licenses.Licenses.Options.Endpoint.Map(licenses);
        Licenses.Licenses.Export.Endpoint.Map(licenses);
        Licenses.Licenses.Get.Endpoint.Map(licenses);
        Licenses.Licenses.Create.Endpoint.Map(licenses);
        Licenses.Licenses.Update.Endpoint.Map(licenses);
        Licenses.Licenses.Delete.Endpoint.Map(licenses);

        var activations = api.MapGroup("/licenses/activations");

        Licenses.Activations.List.Endpoint.Map(activations);
        Licenses.Activations.Options.Endpoint.Map(activations);
        Licenses.Activations.Export.Endpoint.Map(activations);
        Licenses.Activations.Volumes.Endpoint.Map(activations);
        Licenses.Activations.Get.Endpoint.Map(activations);
        Licenses.Activations.Create.Endpoint.Map(activations);
        Licenses.Activations.Deactivate.Endpoint.Map(activations);
        Licenses.Activations.Reactivate.Endpoint.Map(activations);
        Licenses.Activations.Delete.Endpoint.Map(activations);

        var softwareServices = api.MapGroup("/licenses/software-services");

        Licenses.SoftwareServices.List.Endpoint.Map(softwareServices);
        Licenses.SoftwareServices.Options.Endpoint.Map(softwareServices);
        Licenses.SoftwareServices.Get.Endpoint.Map(softwareServices);
        Licenses.SoftwareServices.Create.Endpoint.Map(softwareServices);
        Licenses.SoftwareServices.Update.Endpoint.Map(softwareServices);
        Licenses.SoftwareServices.Delete.Endpoint.Map(softwareServices);

        var furnitureTypes = api.MapGroup("/furniture/types");

        Furnitures.Types.List.Endpoint.Map(furnitureTypes);
        Furnitures.Types.Get.Endpoint.Map(furnitureTypes);
        Furnitures.Types.Create.Endpoint.Map(furnitureTypes);
        Furnitures.Types.Update.Endpoint.Map(furnitureTypes);
        Furnitures.Types.Delete.Endpoint.Map(furnitureTypes);

        var furniture = api.MapGroup("/furniture");

        Furnitures.Items.List.Endpoint.Map(furniture);
        Furnitures.Items.Options.Endpoint.Map(furniture);
        Furnitures.Items.Export.Endpoint.Map(furniture);
        Furnitures.Items.Get.Endpoint.Map(furniture);
        Furnitures.Items.Create.Endpoint.Map(furniture);
        Furnitures.Items.Update.Endpoint.Map(furniture);
        Furnitures.Items.Delete.Endpoint.Map(furniture);

        var auditLog = api.MapGroup("/administration/audit-log");

        Administration.AuditLogs.List.Endpoint.Map(auditLog);
        Administration.AuditLogs.Facets.Endpoint.Map(auditLog);
        Administration.AuditLogs.Get.Endpoint.Map(auditLog);

        // A policy of its own, so that restricting the log to a role later is one line in the host.
        var serverLog = api.MapGroup("/administration/server-log")
            .RequireAuthorization(Administration.ServerLogs.ServerLogOptions.ReadPolicy);

        Administration.ServerLogs.Days.Endpoint.Map(serverLog);
        Administration.ServerLogs.List.Endpoint.Map(serverLog);
    }
}
