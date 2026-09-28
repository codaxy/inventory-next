namespace Codaxy.Inventory.App.Furnitures.Items;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var furniture = api.MapGroup("/furniture");

        List.Endpoint.Map(furniture);
        Options.Endpoint.Map(furniture);
        Export.Endpoint.Map(furniture);
        Get.Endpoint.Map(furniture);
        Create.Endpoint.Map(furniture);
        Update.Endpoint.Map(furniture);
        Delete.Endpoint.Map(furniture);
    }
}
