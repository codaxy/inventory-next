namespace Codaxy.Inventory.App.Informations.Items;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var information = api.MapGroup("/informations");

        List.Endpoint.Map(information);
        Options.Endpoint.Map(information);
        Export.Endpoint.Map(information);
        Get.Endpoint.Map(information);
        Create.Endpoint.Map(information);
        Update.Endpoint.Map(information);
        Delete.Endpoint.Map(information);
    }
}
