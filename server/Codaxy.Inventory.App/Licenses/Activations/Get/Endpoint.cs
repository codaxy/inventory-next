using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Licenses.Activations.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) =>
        activations.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    ) =>
        await Activations.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
            is { } activation
            ? Results.Ok(activation)
            : Results.NotFound();
}
