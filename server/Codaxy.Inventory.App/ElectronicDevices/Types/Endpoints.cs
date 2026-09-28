namespace Codaxy.Inventory.App.ElectronicDevices.Types;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var types = api.MapGroup("/electronic-devices/types");

        List.Endpoint.Map(types);
        Options.Endpoint.Map(types);
        Get.Endpoint.Map(types);
        Create.Endpoint.Map(types);
        Update.Endpoint.Map(types);
        Delete.Endpoint.Map(types);
    }
}
