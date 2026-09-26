using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Informations.Tags.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await InformationTagWrites.DetailAsync(context, id, cancellationToken) is { } detail
            ? Results.Ok(detail)
            : Results.NotFound();
}
