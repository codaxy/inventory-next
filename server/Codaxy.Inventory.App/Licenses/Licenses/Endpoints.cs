namespace Codaxy.Inventory.App.Licenses.Licenses;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var licenses = api.MapGroup("/licenses");

        List.Endpoint.Map(licenses);
        Options.Endpoint.Map(licenses);
        Export.Endpoint.Map(licenses);
        Get.Endpoint.Map(licenses);
        Create.Endpoint.Map(licenses);
        Update.Endpoint.Map(licenses);
        Delete.Endpoint.Map(licenses);
    }
}
