using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.Options;

/// <summary>The volumes a cloud subscription can be bought under.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapGet("/options", Handle);

    public sealed record Response(IReadOnlyList<VolumeOption> Volumes);

    private static async Task<IResult> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) => Results.Ok(new Response(await VolumeOptions.AllAsync(context, cancellationToken)));
}
