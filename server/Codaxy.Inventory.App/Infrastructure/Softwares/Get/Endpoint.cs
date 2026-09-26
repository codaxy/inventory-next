using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await SoftwareWrites.DetailAsync(context, id, cancellationToken) is { } detail
            ? Results.Ok(detail)
            : Results.NotFound();
}
