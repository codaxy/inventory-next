using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder virtualMachines) =>
        virtualMachines.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await VirtualMachineWrites.DetailAsync(context, id, cancellationToken) is { } detail
            ? Results.Ok(detail)
            : Results.NotFound();
}
