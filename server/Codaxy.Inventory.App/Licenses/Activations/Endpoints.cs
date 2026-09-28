namespace Codaxy.Inventory.App.Licenses.Activations;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var activations = api.MapGroup("/licenses/activations");

        List.Endpoint.Map(activations);
        Options.Endpoint.Map(activations);
        Export.Endpoint.Map(activations);
        Volumes.Endpoint.Map(activations);
        Get.Endpoint.Map(activations);
        Create.Endpoint.Map(activations);
        Deactivate.Endpoint.Map(activations);
        Reactivate.Endpoint.Map(activations);
        Delete.Endpoint.Map(activations);
    }
}
