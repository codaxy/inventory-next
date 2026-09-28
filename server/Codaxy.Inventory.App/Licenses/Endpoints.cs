namespace Codaxy.Inventory.App.Licenses;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        Licenses.Endpoints.Map(api);
        Activations.Endpoints.Map(api);
        SoftwareServices.Endpoints.Map(api);
    }
}
