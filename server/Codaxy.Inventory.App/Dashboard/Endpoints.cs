namespace Codaxy.Inventory.App.Dashboard;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var dashboard = api.MapGroup("/dashboard");

        Get.Endpoint.Map(dashboard);
    }
}
