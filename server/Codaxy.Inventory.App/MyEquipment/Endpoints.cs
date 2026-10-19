namespace Codaxy.Inventory.App.MyEquipment;

/// <summary>An employee's own equipment: in no section, and behind no permission but being them.</summary>
public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var mine = api.MapGroup("/my-equipment");

        Get.Endpoint.Map(mine);
        Details.Endpoint.Map(mine);
    }
}
