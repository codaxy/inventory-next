using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await SoftwareServices.DetailAsync(context, id, cancellationToken) is { } entry
            ? Results.Ok(entry)
            : Results.NotFound();
}
