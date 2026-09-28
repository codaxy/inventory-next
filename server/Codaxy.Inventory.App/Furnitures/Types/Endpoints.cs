namespace Codaxy.Inventory.App.Furnitures.Types;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var furnitureTypes = api.MapGroup("/furniture/types");

        List.Endpoint.Map(furnitureTypes);
        Get.Endpoint.Map(furnitureTypes);
        Create.Endpoint.Map(furnitureTypes);
        Update.Endpoint.Map(furnitureTypes);
        Delete.Endpoint.Map(furnitureTypes);
    }
}
