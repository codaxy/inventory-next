namespace Codaxy.Inventory.App.Infrastructure;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        VirtualMachines.Endpoints.Map(api);
        CloudSubscriptions.Endpoints.Map(api);
        Softwares.Endpoints.Map(api);
    }
}
