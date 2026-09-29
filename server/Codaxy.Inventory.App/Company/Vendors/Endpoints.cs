namespace Codaxy.Inventory.App.Company.Vendors;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var vendors = api.MapGroup("/company/vendors");

        List.Endpoint.Map(vendors);
        Export.Endpoint.Map(vendors);
        Get.Endpoint.Map(vendors);
        Create.Endpoint.Map(vendors);
        Update.Endpoint.Map(vendors);
        Delete.Endpoint.Map(vendors);
    }
}
