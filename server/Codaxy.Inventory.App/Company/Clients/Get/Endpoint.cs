using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Clients.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await Clients.DetailAsync(context, id, cancellationToken) is { } client
            ? Results.Ok(client)
            : Results.NotFound();
}
