namespace Codaxy.Inventory.App.Company.Locations;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var locations = api.MapGroup("/company/locations");

        List.Endpoint.Map(locations);
        Options.Endpoint.Map(locations);
        Get.Endpoint.Map(locations);
        Create.Endpoint.Map(locations);
        Update.Endpoint.Map(locations);
        Delete.Endpoint.Map(locations);
    }
}
