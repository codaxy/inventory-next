namespace Codaxy.Inventory.App.Company.People;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var people = api.MapGroup("/company/people");

        List.Endpoint.Map(people);
        Export.Endpoint.Map(people);
        Get.Endpoint.Map(people);
        Holdings.Endpoint.Map(people);
        Handover.Endpoint.Map(people);
        HandoverPdf.Endpoint.Map(people);
        Create.Endpoint.Map(people);
        Update.Endpoint.Map(people);
        Delete.Endpoint.Map(people);
    }
}
