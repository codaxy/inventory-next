namespace Codaxy.Inventory.App.Infrastructure.Softwares;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var software = api.MapGroup("/infrastructure/software");

        List.Endpoint.Map(software);
        Options.Endpoint.Map(software);
        Get.Endpoint.Map(software);
        Create.Endpoint.Map(software);
        Update.Endpoint.Map(software);
        Delete.Endpoint.Map(software);
    }
}
