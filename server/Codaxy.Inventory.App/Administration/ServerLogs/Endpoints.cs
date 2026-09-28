namespace Codaxy.Inventory.App.Administration.ServerLogs;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        // A policy of its own, so that restricting the log to a role later is one line in the host.
        var serverLog = api.MapGroup("/administration/server-log")
            .RequireAuthorization(ServerLogOptions.ReadPolicy);

        Days.Endpoint.Map(serverLog);
        List.Endpoint.Map(serverLog);
    }
}
