using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Vendors.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    ) =>
        await VendorWrites.DetailAsync(context, id, VendorWrites.Today(clock), cancellationToken)
            is { } vendor
            ? Results.Ok(vendor)
            : Results.NotFound();
}
