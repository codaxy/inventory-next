using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    /// <param name="Weight">What the importance is computed from.</param>
    public sealed record Weighted(Guid Id, string Text, int Weight);

    public sealed record VolumeType(int Id, string Text);

    /// <summary>Every picker the editor and the list's filters show, in one call: all small lists.</summary>
    public sealed record Response(
        IReadOnlyList<Option> Vendors,
        IReadOnlyList<Option> People,
        IReadOnlyList<Weighted> Confidentialities,
        IReadOnlyList<Weighted> Integrities,
        IReadOnlyList<Weighted> Availabilities,
        IReadOnlyList<Option> Importances,
        IReadOnlyList<Option> LicenseTypes,
        IReadOnlyList<Option> LicenseModels,
        IReadOnlyList<Option> ExpirationModels,
        IReadOnlyList<Option> Currencies,
        IReadOnlyList<Option> Periods,
        IReadOnlyList<Option> BusinessEntities,
        IReadOnlyList<Option> Locations,
        IReadOnlyList<Option> Software,
        IReadOnlyList<VolumeType> VolumeTypes
    );

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .Vendors.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new Option(x.Id, x.Name))
                .ToListAsync(cancellationToken),
            await context
                .Persons.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new Option(x.Id, x.Name))
                .ToListAsync(cancellationToken),
            await context
                .Confidentialities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new Weighted(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
            await context
                .Integrities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new Weighted(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
            await context
                .Availabilities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new Weighted(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
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
            await context
                .BusinessEntities.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new Option(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .Locations.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new Option(x.Id, x.Name))
                .ToListAsync(cancellationToken),
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
