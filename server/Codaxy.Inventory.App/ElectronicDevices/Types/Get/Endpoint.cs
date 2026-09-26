using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await DeviceTypes.DetailAsync(context, id, cancellationToken) is { } type
            ? Results.Ok(type)
            : Results.NotFound();
}
