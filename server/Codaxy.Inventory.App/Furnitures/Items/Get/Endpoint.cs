using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Furnitures.Items.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await FurnitureWrites.DetailAsync(context, id, cancellationToken) is { } item
            ? Results.Ok(item)
            : Results.NotFound();
}
