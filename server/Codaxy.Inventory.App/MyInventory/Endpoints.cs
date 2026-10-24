namespace Codaxy.Inventory.App.MyInventory;

/// <summary>An employee's own inventory: in no section, and behind no permission but being them.</summary>
public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var mine = api.MapGroup("/my-inventory");

        Get.Endpoint.Map(mine);
        Details.Endpoint.Map(mine);
    }
}
