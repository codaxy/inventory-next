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

        var projects = api.MapGroup("/company/projects");

        Company.Projects.List.Endpoint.Map(projects);
        Company.Projects.Options.Endpoint.Map(projects);
        Company.Projects.Get.Endpoint.Map(projects);
        Company.Projects.Create.Endpoint.Map(projects);
        Company.Projects.Update.Endpoint.Map(projects);
        Company.Projects.Delete.Endpoint.Map(projects);

        var vendors = api.MapGroup("/company/vendors");

        Company.Vendors.List.Endpoint.Map(vendors);
        Company.Vendors.Get.Endpoint.Map(vendors);
        Company.Vendors.Create.Endpoint.Map(vendors);
        Company.Vendors.Update.Endpoint.Map(vendors);
        Company.Vendors.Delete.Endpoint.Map(vendors);

        var manufacturers = api.MapGroup("/company/manufacturers");

        Company.Manufacturers.List.Endpoint.Map(manufacturers);
        Company.Manufacturers.Get.Endpoint.Map(manufacturers);
        Company.Manufacturers.Create.Endpoint.Map(manufacturers);
        Company.Manufacturers.Update.Endpoint.Map(manufacturers);
        Company.Manufacturers.Delete.Endpoint.Map(manufacturers);

        var locations = api.MapGroup("/company/locations");

        Company.Locations.List.Endpoint.Map(locations);
        Company.Locations.Options.Endpoint.Map(locations);
        Company.Locations.Get.Endpoint.Map(locations);
        Company.Locations.Create.Endpoint.Map(locations);
        Company.Locations.Update.Endpoint.Map(locations);
        Company.Locations.Delete.Endpoint.Map(locations);

        var informationTypes = api.MapGroup("/informations/types");

        Informations.Types.List.Endpoint.Map(informationTypes);
        Informations.Types.Get.Endpoint.Map(informationTypes);
        Informations.Types.Create.Endpoint.Map(informationTypes);
        Informations.Types.Update.Endpoint.Map(informationTypes);
        Informations.Types.Delete.Endpoint.Map(informationTypes);

        var informationTags = api.MapGroup("/informations/tags");

        Informations.Tags.List.Endpoint.Map(informationTags);
        Informations.Tags.Get.Endpoint.Map(informationTags);
        Informations.Tags.Create.Endpoint.Map(informationTags);
        Informations.Tags.Update.Endpoint.Map(informationTags);
        Informations.Tags.Delete.Endpoint.Map(informationTags);

        var information = api.MapGroup("/informations");

        Informations.Items.List.Endpoint.Map(information);
        Informations.Items.Options.Endpoint.Map(information);
        Informations.Items.Export.Endpoint.Map(information);
        Informations.Items.Get.Endpoint.Map(information);
        Informations.Items.Create.Endpoint.Map(information);
        Informations.Items.Update.Endpoint.Map(information);
        Informations.Items.Delete.Endpoint.Map(information);

        var clients = api.MapGroup("/company/clients");

        Company.Clients.List.Endpoint.Map(clients);
        Company.Clients.Get.Endpoint.Map(clients);
        Company.Clients.Create.Endpoint.Map(clients);
        Company.Clients.Update.Endpoint.Map(clients);
        Company.Clients.Delete.Endpoint.Map(clients);

        var people = api.MapGroup("/company/people");

        Company.People.List.Endpoint.Map(people);
        Company.People.Get.Endpoint.Map(people);
        Company.People.Holdings.Endpoint.Map(people);
        Company.People.Handover.Endpoint.Map(people);
        Company.People.Create.Endpoint.Map(people);
        Company.People.Update.Endpoint.Map(people);
        Company.People.Delete.Endpoint.Map(people);

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
