using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Items.Options;

/// <summary>Every list the information form and the list's filters pick from, in one call.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) => information.MapGet("/options", Handle);

    public sealed record Response(
        IReadOnlyList<AssetOption> Types,
        IReadOnlyList<AssetOption> People,
        IReadOnlyList<AssetOption> Projects,
        IReadOnlyList<AssetOption> Tags,
        IReadOnlyList<WeightedOption> Confidentialities,
        IReadOnlyList<WeightedOption> Integrities,
        IReadOnlyList<WeightedOption> Availabilities,
        IReadOnlyList<NumberedOption> Devices,
        IReadOnlyList<AssetOption> VirtualMachines,
        IReadOnlyList<AssetOption> Software,
        IReadOnlyList<AssetOption> CloudSubscriptions,
        IReadOnlyList<AssetOption> Locations
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
                    .InformationTypes.AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new AssetOption(t.Id, t.Name))
                    .ToListAsync(cancellationToken),
                asset.People,
                await context
                    .Projects.AsNoTracking()
                    .OrderBy(p => p.Name)
                    .Select(p => new AssetOption(p.Id, p.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .InformationTags.AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => new AssetOption(t.Id, t.Name))
                    .ToListAsync(cancellationToken),
                asset.Confidentialities,
                asset.Integrities,
                asset.Availabilities,
                // A device is found by name, told apart by number.
                await context
                    .ElectronicDevices.AsNoTracking()
                    .OrderBy(d => d.Asset.Name)
                    .Select(d => new NumberedOption(
                        d.AssetId,
                        d.Asset.Name,
                        d.Asset.InventoryNumber
                    ))
                    .ToListAsync(cancellationToken),
                await context
                    .VirtualMachines.AsNoTracking()
                    .OrderBy(v => v.Name)
                    .Select(v => new AssetOption(v.Id, v.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .Softwares.AsNoTracking()
                    .OrderBy(s => s.Name)
                    .Select(s => new AssetOption(s.Id, s.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .Clouds.AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new AssetOption(c.Id, c.Name))
                    .ToListAsync(cancellationToken),
                asset.Locations
            )
        );
    }
}
