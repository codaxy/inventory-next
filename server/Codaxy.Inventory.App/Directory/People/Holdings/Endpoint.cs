using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People.Holdings;

/// <summary>
/// Everything attached to a person, for their page: per kind the total and the first rows. Virtual
/// machines, clouds and software have no owner in the schema, so they are not here.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) =>
        people.MapGet("/{id:guid}/holdings", Handle);

    /// <summary>What a section shows before "see all".</summary>
    public const int First = 10;

    public sealed record Section<T>(int Total, IReadOnlyList<T> Items);

    /// <param name="Type">The device's or the furniture's type.</param>
    /// <param name="Model">The model it is.</param>
    public sealed record AssetRow(Guid Id, int? Number, string Name, string? Type, string? Model);

    public sealed record LicenseRow(
        Guid Id,
        int? Number,
        string Name,
        string Vendor,
        DateOnly? ExpirationDate,
        string? Expiry
    );

    /// <param name="Device">The device the seat is on, where it is on one the person holds rather than theirs by name.</param>
    public sealed record SeatRow(
        Guid Id,
        string Software,
        Guid LicenseId,
        string License,
        int Quantity,
        DateOnly ActivationDate,
        DateOnly? DeactivationDate,
        string? Device,
        int? DeviceNumber
    );

    /// <param name="Active">The seats not deactivated; `Total` counts the deactivated too.</param>
    public sealed record Seats(int Total, int Active, IReadOnlyList<SeatRow> Items);

    public sealed record InformationRow(Guid Id, string Name, string? Type);

    public sealed record ProjectRow(Guid Id, string Name, string? Client);

    public sealed record Response(
        Section<AssetRow> Devices,
        Section<AssetRow> Furniture,
        Section<LicenseRow> Licenses,
        Seats Seats,
        Section<InformationRow> Information,
        Section<ProjectRow> Projects
    );

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!await context.Persons.AnyAsync(p => p.Id == id, cancellationToken))
            return Results.NotFound();

        var today = Expiry.Today(clock);

        var devices = context.ElectronicDevices.AsNoTracking().Where(d => d.Asset.PersonId == id);
        var furniture = context.Furnitures.AsNoTracking().Where(f => f.Asset.PersonId == id);
        var licenses = context.Licenses.AsNoTracking().Where(l => l.Asset.PersonId == id);
        // Theirs by name, or on a device they hold: either way the person answers for the seat.
        var seats = context
            .Activations.AsNoTracking()
            .Where(a => a.PersonId == id || a.Asset.PersonId == id);
        var information = context.Informations.AsNoTracking().Where(i => i.PersonId == id);
        var projects = context.Projects.AsNoTracking().Where(p => p.ProjectOwnerId == id);

        var licenseRows = await licenses
            .OrderBy(l => l.Asset.Name)
            .ThenBy(l => l.AssetId)
            .Take(First)
            .Select(l => new
            {
                l.AssetId,
                l.Asset.InventoryNumber,
                l.Asset.Name,
                Vendor = l.Asset.Vendor.Name,
                l.SubscriptionExpirationDate,
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(
            new Response(
                new Section<AssetRow>(
                    await devices.CountAsync(cancellationToken),
                    await devices
                        .OrderBy(d => d.Asset.Name)
                        .ThenBy(d => d.AssetId)
                        .Take(First)
                        .Select(d => new AssetRow(
                            d.AssetId,
                            d.Asset.InventoryNumber,
                            d.Asset.Name,
                            d.ElectronicDeviceType.Name,
                            d.ModelName
                        ))
                        .ToListAsync(cancellationToken)
                ),
                new Section<AssetRow>(
                    await furniture.CountAsync(cancellationToken),
                    await furniture
                        .OrderBy(f => f.Asset.Name)
                        .ThenBy(f => f.AssetId)
                        .Take(First)
                        .Select(f => new AssetRow(
                            f.AssetId,
                            f.Asset.InventoryNumber,
                            f.Asset.Name,
                            f.FurnitureType.Name,
                            f.Model
                        ))
                        .ToListAsync(cancellationToken)
                ),
                new Section<LicenseRow>(
                    await licenses.CountAsync(cancellationToken),
                    licenseRows
                        .Select(l => new LicenseRow(
                            l.AssetId,
                            l.InventoryNumber,
                            l.Name,
                            l.Vendor,
                            l.SubscriptionExpirationDate,
                            Expiry.Status(l.SubscriptionExpirationDate, today)
                        ))
                        .ToList()
                ),
                new Seats(
                    await seats.CountAsync(cancellationToken),
                    await seats.CountAsync(a => a.DeactivationDate == null, cancellationToken),
                    await seats
                        .OrderBy(a => a.DeactivationDate != null)
                        .ThenByDescending(a => a.ActivationDate)
                        .ThenBy(a => a.Id)
                        .Take(First)
                        .Select(a => new SeatRow(
                            a.Id,
                            a.Volume.SoftwareOrService.Name,
                            a.Volume.LicenseId,
                            a.Volume.License.Asset.Name,
                            a.Quantity,
                            a.ActivationDate,
                            a.DeactivationDate,
                            a.PersonId == id ? null : a.Asset.Name,
                            a.PersonId == id ? null : a.Asset.InventoryNumber
                        ))
                        .ToListAsync(cancellationToken)
                ),
                new Section<InformationRow>(
                    await information.CountAsync(cancellationToken),
                    await information
                        .OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Take(First)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name))
                        .ToListAsync(cancellationToken)
                ),
                new Section<ProjectRow>(
                    await projects.CountAsync(cancellationToken),
                    await projects
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Id)
                        .Take(First)
                        .Select(p => new ProjectRow(p.Id, p.Name, p.Client.Name))
                        .ToListAsync(cancellationToken)
                )
            )
        );
    }
}
