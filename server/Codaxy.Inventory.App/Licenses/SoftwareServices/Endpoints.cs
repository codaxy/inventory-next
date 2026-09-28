namespace Codaxy.Inventory.App.Licenses.SoftwareServices;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var softwareServices = api.MapGroup("/licenses/software-services");

        List.Endpoint.Map(softwareServices);
        Options.Endpoint.Map(softwareServices);
        Get.Endpoint.Map(softwareServices);
        Create.Endpoint.Map(softwareServices);
        Update.Endpoint.Map(softwareServices);
        Delete.Endpoint.Map(softwareServices);
    }
}
