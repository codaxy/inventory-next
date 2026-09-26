using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Informations.Types.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await InformationTypeWrites.DetailAsync(context, id, cancellationToken) is { } detail
            ? Results.Ok(detail)
            : Results.NotFound();
}
