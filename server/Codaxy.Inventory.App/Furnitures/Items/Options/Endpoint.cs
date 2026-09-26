using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Items.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapGet("/options", Handle);

    /// <summary>The asset's pickers, shared by every asset form, and the furniture types.</summary>
    public sealed record Response(
        IReadOnlyList<AssetOption> Vendors,
        IReadOnlyList<AssetOption> People,
        IReadOnlyList<WeightedOption> Confidentialities,
        IReadOnlyList<WeightedOption> Integrities,
        IReadOnlyList<WeightedOption> Availabilities,
        IReadOnlyList<AssetOption> BusinessEntities,
        IReadOnlyList<AssetOption> Locations,
        IReadOnlyList<AssetOption> Types
    );

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var asset = await AssetWrites.OptionsAsync(context, cancellationToken);
        return new(
            asset.Vendors,
            asset.People,
            asset.Confidentialities,
            asset.Integrities,
            asset.Availabilities,
            asset.BusinessEntities,
            asset.Locations,
            await context
                .FurnitureTypes.AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new AssetOption(t.Id, t.Name))
                .ToListAsync(cancellationToken)
        );
    }
}
