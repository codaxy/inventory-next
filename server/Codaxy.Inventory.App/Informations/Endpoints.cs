namespace Codaxy.Inventory.App.Informations;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        Items.Endpoints.Map(api);
        Types.Endpoints.Map(api);
        Tags.Endpoints.Map(api);
    }
}
