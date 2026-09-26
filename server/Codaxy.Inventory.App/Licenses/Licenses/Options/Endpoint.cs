using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    public sealed record VolumeType(int Id, string Text);

    /// <summary>
    /// Every picker the editor and the list's filters show, in one call — the asset's, shared by every
    /// asset form, and the licence's own. All small lists.
    /// </summary>
    public sealed record Response(
        IReadOnlyList<AssetOption> Vendors,
        IReadOnlyList<AssetOption> People,
        IReadOnlyList<WeightedOption> Confidentialities,
        IReadOnlyList<WeightedOption> Integrities,
        IReadOnlyList<WeightedOption> Availabilities,
        IReadOnlyList<Option> Importances,
        IReadOnlyList<Option> LicenseTypes,
        IReadOnlyList<Option> LicenseModels,
        IReadOnlyList<Option> ExpirationModels,
        IReadOnlyList<Option> Currencies,
        IReadOnlyList<Option> Periods,
        IReadOnlyList<AssetOption> BusinessEntities,
        IReadOnlyList<AssetOption> Locations,
        IReadOnlyList<Option> Software,
        IReadOnlyList<VolumeType> VolumeTypes
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
            await context
                .Importances.AsNoTracking()
                .Select(x => new Option(x.Id, x.Level))
                .ToListAsync(cancellationToken),
            await context
                .LicenseTypes.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .LicenseModels.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .LicenseExpirationModels.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .Currencies.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .Periods.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            asset.BusinessEntities,
            asset.Locations,
            await context
                .SoftwareOrServices.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new Option(x.Id, x.Name))
                .ToListAsync(cancellationToken),
            await context
                .VolumeTypes.AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new VolumeType(x.Id, x.Text))
                .ToListAsync(cancellationToken)
        );
    }
}
