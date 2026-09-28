namespace Codaxy.Inventory.App.ElectronicDevices.Tags;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var tags = api.MapGroup("/electronic-devices/tags");

        List.Endpoint.Map(tags);
        Options.Endpoint.Map(tags);
        Get.Endpoint.Map(tags);
        Create.Endpoint.Map(tags);
        Update.Endpoint.Map(tags);
        Delete.Endpoint.Map(tags);
    }
}
