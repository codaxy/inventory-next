using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.Roles;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var roles = api.MapGroup("/administration/roles")
            .RequirePermissions(Permissions.RolesManage, Permissions.RolesManage);

        List.Endpoint.Map(roles);
        Catalogue.Endpoint.Map(roles);
        Get.Endpoint.Map(roles);
        Create.Endpoint.Map(roles);
        Update.Endpoint.Map(roles);
        Reset.Endpoint.Map(roles);
        Delete.Endpoint.Map(roles);
    }
}
