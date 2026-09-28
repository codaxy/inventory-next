namespace Codaxy.Inventory.App.Informations.Tags;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var informationTags = api.MapGroup("/informations/tags");

        List.Endpoint.Map(informationTags);
        Get.Endpoint.Map(informationTags);
        Create.Endpoint.Map(informationTags);
        Update.Endpoint.Map(informationTags);
        Delete.Endpoint.Map(informationTags);
    }
}
