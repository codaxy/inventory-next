namespace Codaxy.Inventory.App.Company;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        People.Endpoints.Map(api);
        Clients.Endpoints.Map(api);
        Projects.Endpoints.Map(api);
        Vendors.Endpoints.Map(api);
        Manufacturers.Endpoints.Map(api);
        Locations.Endpoints.Map(api);
    }
}
