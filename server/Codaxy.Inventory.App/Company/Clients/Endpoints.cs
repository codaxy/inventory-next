namespace Codaxy.Inventory.App.Company.Clients;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var clients = api.MapGroup("/company/clients");

        List.Endpoint.Map(clients);
        Export.Endpoint.Map(clients);
        Get.Endpoint.Map(clients);
        Create.Endpoint.Map(clients);
        Update.Endpoint.Map(clients);
        Delete.Endpoint.Map(clients);
    }
}
