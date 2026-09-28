namespace Codaxy.Inventory.App.ElectronicDevices;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        Devices.Endpoints.Map(api);
        Types.Endpoints.Map(api);
        Tags.Endpoints.Map(api);
    }
}
