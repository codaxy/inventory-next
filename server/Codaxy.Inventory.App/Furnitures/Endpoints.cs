using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Furnitures;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder inventory)
    {
        var api = inventory
            .MapGroup("")
            .RequirePermissions(Permissions.FurnitureView, Permissions.FurnitureEdit);

        Items.Endpoints.Map(api);
        Types.Endpoints.Map(api);
    }
}
