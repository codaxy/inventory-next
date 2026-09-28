namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var cloudSubscriptions = api.MapGroup("/infrastructure/cloud-subscriptions");

        List.Endpoint.Map(cloudSubscriptions);
        Options.Endpoint.Map(cloudSubscriptions);
        Get.Endpoint.Map(cloudSubscriptions);
        Create.Endpoint.Map(cloudSubscriptions);
        Update.Endpoint.Map(cloudSubscriptions);
        Delete.Endpoint.Map(cloudSubscriptions);
    }
}
