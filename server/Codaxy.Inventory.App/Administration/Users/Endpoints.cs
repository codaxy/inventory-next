using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.Users;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        // Reading who has access is as much the job as changing it.
        var users = api.MapGroup("/administration/users")
            .RequirePermissions(Permissions.UsersManage, Permissions.UsersManage);

        List.Endpoint.Map(users);
        Options.Endpoint.Map(users);
        Get.Endpoint.Map(users);
        Create.Endpoint.Map(users);
        Update.Endpoint.Map(users);
        Deactivate.Endpoint.Map(users);
        Reactivate.Endpoint.Map(users);
    }
}
