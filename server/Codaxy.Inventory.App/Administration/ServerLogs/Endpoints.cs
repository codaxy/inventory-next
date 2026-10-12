using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.ServerLogs;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var serverLog = api.MapGroup("/administration/server-log")
            .RequirePermissions(Permissions.ServerLogView);

        Days.Endpoint.Map(serverLog);
        List.Endpoint.Map(serverLog);
    }
}
