namespace Codaxy.Inventory.App;

public static class InventoryApi
{
    /// <summary>The menu's endpoints, under <c>/api</c>, every one behind a session.</summary>
    public static void MapInventoryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        var auditLog = api.MapGroup("/administration/audit-log");

        Administration.AuditLogs.List.Endpoint.Map(auditLog);
        Administration.AuditLogs.Facets.Endpoint.Map(auditLog);
        Administration.AuditLogs.Get.Endpoint.Map(auditLog);

        // A policy of its own, so that restricting the log to a role later is one line in the host.
        var serverLog = api.MapGroup("/administration/server-log")
            .RequireAuthorization(Administration.ServerLogs.ServerLogOptions.ReadPolicy);

        Administration.ServerLogs.Days.Endpoint.Map(serverLog);
        Administration.ServerLogs.List.Endpoint.Map(serverLog);
    }
}
