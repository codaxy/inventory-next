using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await DeviceReads.DetailAsync(context, id, cancellationToken) is { } device
            ? Results.Ok(device)
            : Results.NotFound();
}
