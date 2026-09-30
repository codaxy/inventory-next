using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.People.Holdings;

/// <summary>
/// Everything attached to a person, for their page: per kind the total and the first rows. Virtual
/// machines, cloud subscriptions and software have no owner in the schema, so they are not here.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) =>
        people.MapGet("/{id:guid}/holdings", Handle);

    /// <param name="Device">The device the seat is on, where it is on one the person holds rather than theirs by name.</param>
    public sealed record SeatRow(
        Guid Id,
        string Software,
        Guid LicenseId,
        string License,
        int? LicenseNumber,
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

        var assets = await AssetHoldings.SectionsAsync(
            context,
            a => a.PersonId == id,
            Expiry.Today(clock),
            cancellationToken
        );

        // Theirs by name, or on a device they hold: either way the person answers for the seat.
        var seats = context
            .Activations.AsNoTracking()
            .Where(a => a.PersonId == id || a.Asset.PersonId == id);
        var seatRows = await AssetHoldings.SectionAsync(
            seats,
            q =>
                q.OrderBy(a => a.DeactivationDate != null)
                    .ThenByDescending(a => a.ActivationDate)
                    .ThenBy(a => a.Id)
                    .Select(a => new SeatRow(
                        a.Id,
                        a.Volume.SoftwareOrService.Name,
                        a.Volume.LicenseId,
                        a.Volume.License.Asset.Name,
                        a.Volume.License.Asset.InventoryNumber,
                        a.Quantity,
                        a.ActivationDate,
                        a.DeactivationDate,
                        a.PersonId == id ? null : a.Asset.Name,
                        a.PersonId == id ? null : a.Asset.InventoryNumber
                    )),
            cancellationToken
        );

        return Results.Ok(
            new Response(
                assets.Devices,
                assets.Furniture,
                assets.Licenses,
                new Seats(
                    seatRows.Total,
                    await seats.CountAsync(a => a.DeactivationDate == null, cancellationToken),
                    seatRows.Items
                ),
                await AssetHoldings.SectionAsync(
                    context.Informations.AsNoTracking().Where(i => i.PersonId == id),
                    q =>
                        q.OrderBy(i => i.Name)
                            .ThenBy(i => i.Id)
                            .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                    cancellationToken
                ),
                await AssetHoldings.SectionAsync(
                    context.Projects.AsNoTracking().Where(p => p.ProjectOwnerId == id),
                    q =>
                        q.OrderBy(p => p.Name)
                            .ThenBy(p => p.Id)
                            .Select(p => new ProjectRow(p.Id, p.Name, p.Client.Name)),
                    cancellationToken
                )
            )
        );
    }
}
