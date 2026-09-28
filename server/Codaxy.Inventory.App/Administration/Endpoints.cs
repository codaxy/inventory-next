namespace Codaxy.Inventory.App.Administration;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        AuditLogs.Endpoints.Map(api);
        ServerLogs.Endpoints.Map(api);
    }
}
