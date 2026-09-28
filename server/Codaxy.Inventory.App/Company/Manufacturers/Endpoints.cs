namespace Codaxy.Inventory.App.Company.Manufacturers;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var manufacturers = api.MapGroup("/company/manufacturers");

        List.Endpoint.Map(manufacturers);
        Get.Endpoint.Map(manufacturers);
        Create.Endpoint.Map(manufacturers);
        Update.Endpoint.Map(manufacturers);
        Delete.Endpoint.Map(manufacturers);
    }
}
