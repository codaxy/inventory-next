using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Options;

/// <summary>The pickers the device list filters by and its form picks from, in one call.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapGet("/options", Handle);

    /// <param name="TypeTags">Each type's tags, by the type's id: what a device of it shows, each a link to its tag.</param>
    public sealed record Response(
        IReadOnlyList<AssetOption> Types,
        IReadOnlyList<AssetOption> Tags,
        IReadOnlyList<AssetOption> People,
        IReadOnlyList<AssetOption> Vendors,
        IReadOnlyList<AssetOption> Locations,
        IReadOnlyList<AssetOption> Manufacturers,
        IReadOnlyList<WeightedOption> Confidentialities,
        IReadOnlyList<WeightedOption> Integrities,
        IReadOnlyList<WeightedOption> Availabilities,
        IReadOnlyList<AssetOption> BusinessEntities,
        IReadOnlyDictionary<Guid, List<AssetOption>> TypeTags
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
                    .ToListAsync(cancellationToken),
                asset.Confidentialities,
                asset.Integrities,
                asset.Availabilities,
                asset.BusinessEntities,
                (
                    await context
                        .ElectronicDeviceTypes.AsNoTracking()
                        .Select(t => new
                        {
                            t.Id,
                            Tags = t
                                .Tags.OrderBy(x => x.ElectronicDeviceTag.Name)
                                .Select(x => new AssetOption(
                                    x.ElectronicDeviceTagId,
                                    x.ElectronicDeviceTag.Name
                                ))
                                .ToList(),
                        })
                        .ToListAsync(cancellationToken)
                ).ToDictionary(t => t.Id, t => t.Tags)
            )
        );
    }
}
