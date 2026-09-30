using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Options;

/// <summary>The volumes a software can be bought under.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapGet("/options", Handle);

    public sealed record Response(IReadOnlyList<VolumeOption> Volumes);

    private static async Task<IResult> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) => Results.Ok(new Response(await VolumeOptions.AllAsync(context, cancellationToken)));
}
