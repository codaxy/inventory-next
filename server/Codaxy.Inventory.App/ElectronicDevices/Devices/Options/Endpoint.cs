using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Options;

/// <summary>The pickers the device list filters by.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapGet("/options", Handle);

    public sealed record Response(
        IReadOnlyList<AssetOption> Types,
        IReadOnlyList<AssetOption> Tags,
        IReadOnlyList<AssetOption> People,
        IReadOnlyList<AssetOption> Vendors,
        IReadOnlyList<AssetOption> Locations,
        IReadOnlyList<AssetOption> Manufacturers
    );

    private static async Task<IResult> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var asset = await AssetWrites.OptionsAsync(context, cancellationToken);
        return Results.Ok(
            new Response(
                await context
                    .ElectronicDeviceTypes.AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new AssetOption(t.Id, t.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .ElectronicDeviceTags.AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new AssetOption(t.Id, t.Name))
                    .ToListAsync(cancellationToken),
                asset.People,
                asset.Vendors,
                asset.Locations,
                await context
                    .Manufacturers.AsNoTracking()
                    .OrderBy(m => m.Name)
                    .Select(m => new AssetOption(m.Id, m.Name))
                    .ToListAsync(cancellationToken)
            )
        );
    }
}
