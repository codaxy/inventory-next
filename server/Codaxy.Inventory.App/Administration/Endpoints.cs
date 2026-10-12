namespace Codaxy.Inventory.App.Administration;

public static class Endpoints
{
    /// <summary>The section's items, in the menu's order, each behind a permission of its own.</summary>
    public static void Map(RouteGroupBuilder api)
    {
        Users.Endpoints.Map(api);
        AuditLogs.Endpoints.Map(api);
        ServerLogs.Endpoints.Map(api);
    }
}
