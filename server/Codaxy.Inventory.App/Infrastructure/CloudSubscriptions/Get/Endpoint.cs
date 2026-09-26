using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await CloudSubscriptionWrites.DetailAsync(context, id, cancellationToken) is { } detail
            ? Results.Ok(detail)
            : Results.NotFound();
}
