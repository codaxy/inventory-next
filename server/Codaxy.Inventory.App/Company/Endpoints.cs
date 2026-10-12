using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Company;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder inventory)
    {
        var api = inventory
            .MapGroup("")
            .RequirePermissions(Permissions.CompanyView, Permissions.CompanyEdit);

        People.Endpoints.Map(api);
        Clients.Endpoints.Map(api);
        Projects.Endpoints.Map(api);
        Vendors.Endpoints.Map(api);
        Manufacturers.Endpoints.Map(api);
        Locations.Endpoints.Map(api);
    }
}
