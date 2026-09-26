using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await Tags.DetailAsync(context, id, cancellationToken) is { } tag
            ? Results.Ok(tag)
            : Results.NotFound();
}
