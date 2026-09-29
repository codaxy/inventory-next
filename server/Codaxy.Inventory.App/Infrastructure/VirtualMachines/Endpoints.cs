namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines;

public static class Endpoints
{
    public static void Map(RouteGroupBuilder api)
    {
        var virtualMachines = api.MapGroup("/infrastructure/virtual-machines");

        List.Endpoint.Map(virtualMachines);
        Export.Endpoint.Map(virtualMachines);
        Get.Endpoint.Map(virtualMachines);
        Create.Endpoint.Map(virtualMachines);
        Update.Endpoint.Map(virtualMachines);
        Delete.Endpoint.Map(virtualMachines);
    }
}
