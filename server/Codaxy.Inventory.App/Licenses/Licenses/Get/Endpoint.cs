using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Licenses.Licenses.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    ) =>
        await LicenseWrites.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
            is { } license
            ? Results.Ok(license)
            : Results.NotFound();
}
