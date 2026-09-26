using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Locations.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    ) =>
        await LocationWrites.DetailAsync(context, id, clock, cancellationToken) is { } location
            ? Results.Ok(location)
            : Results.NotFound();
}
