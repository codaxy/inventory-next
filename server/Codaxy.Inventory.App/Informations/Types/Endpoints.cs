namespace Codaxy.Inventory.App.Informations.Types;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var informationTypes = api.MapGroup("/informations/types");

        List.Endpoint.Map(informationTypes);
        Get.Endpoint.Map(informationTypes);
        Create.Endpoint.Map(informationTypes);
        Update.Endpoint.Map(informationTypes);
        Delete.Endpoint.Map(informationTypes);
    }
}
