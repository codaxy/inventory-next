using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.AuditLogs;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var auditLog = api.MapGroup("/administration/audit-log")
            .RequirePermissions(Permissions.AuditLogView);

        List.Endpoint.Map(auditLog);
        Facets.Endpoint.Map(auditLog);
        Get.Endpoint.Map(auditLog);
    }
}
