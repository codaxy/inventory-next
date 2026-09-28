namespace Codaxy.Inventory.App.ElectronicDevices.Devices;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var devices = api.MapGroup("/electronic-devices");

        List.Endpoint.Map(devices);
        Options.Endpoint.Map(devices);
        Export.Endpoint.Map(devices);
        Get.Endpoint.Map(devices);
        Create.Endpoint.Map(devices);
        Update.Endpoint.Map(devices);
        Delete.Endpoint.Map(devices);
    }
}
