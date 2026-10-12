using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Dashboard;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var dashboard = api.MapGroup("/dashboard").RequirePermissions(Permissions.InventoryView);

        Get.Endpoint.Map(dashboard);
    }
}
