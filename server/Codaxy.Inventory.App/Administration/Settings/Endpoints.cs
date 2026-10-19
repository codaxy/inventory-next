using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.Settings;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        // Who may sign in is among them, so reading them is the job too.
        var settings = api.MapGroup("/administration/settings")
            .RequirePermissions(Permissions.SettingsManage, Permissions.SettingsManage);

        List.Endpoint.Map(settings);
        Update.Endpoint.Map(settings);
        Mail.Endpoint.Map(settings);
        TestEmail.Endpoint.Map(settings);
    }
}
