namespace Codaxy.Inventory.App.Company.Projects;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var projects = api.MapGroup("/company/projects");

        List.Endpoint.Map(projects);
        Options.Endpoint.Map(projects);
        Get.Endpoint.Map(projects);
        Create.Endpoint.Map(projects);
        Update.Endpoint.Map(projects);
        Delete.Endpoint.Map(projects);
    }
}
