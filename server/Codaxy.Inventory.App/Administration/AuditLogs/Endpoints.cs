namespace Codaxy.Inventory.App.Administration.AuditLogs;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var auditLog = api.MapGroup("/administration/audit-log");

        List.Endpoint.Map(auditLog);
        Facets.Endpoint.Map(auditLog);
        Get.Endpoint.Map(auditLog);
    }
}
