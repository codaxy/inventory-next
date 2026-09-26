using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Manufacturers.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) =>
        manufacturers.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await ManufacturerWrites.DetailAsync(context, id, cancellationToken) is { } manufacturer
            ? Results.Ok(manufacturer)
            : Results.NotFound();
}
