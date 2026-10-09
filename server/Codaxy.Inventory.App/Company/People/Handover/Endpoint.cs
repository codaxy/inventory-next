using System.Security.Claims;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Printing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.People.Handover;

/// <summary>
/// The handover sheet: every asset the person holds — the equipment they sign for — in the original's
/// columns, their active seats apart from it, and what the sheet fills in for itself. A seat is not
/// equipment: the original's seat rows only repeated the device.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) =>
        people.MapGet("/{id:guid}/handover", Handle);

    public sealed record Row(int? Number, string Name, string? Description, string Type);

    /// <param name="Device">The device the seat is on, where it is on one the person holds rather than theirs by name.</param>
    /// <param name="Expires">The license's subscription end, where it has one.</param>
    public sealed record SeatRow(
        string Software,
        string License,
        int? LicenseNumber,
        string? Device,
        int? DeviceNumber,
        DateOnly? Expires
    );

    /// <param name="Controller">Who produced it: the signed-in person.</param>
    /// <param name="Pdf">Whether the server can print it.</param>
    public sealed record Response(
        string Name,
        string Controller,
        bool Pdf,
        IReadOnlyList<Row> Assets,
        IReadOnlyList<SeatRow> Seats
    );

    private static async Task<IResult> Handle(
        Guid id,
        ClaimsPrincipal user,
        InventoryContext context,
        [FromServices] IPagePrinter? printer,
        CancellationToken cancellationToken
    )
    {
        var name = await context
            .Persons.Where(p => p.Id == id)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null)
            return Results.NotFound();

        var assets = await context
            .Assets.AsNoTracking()
            .Where(a => a.PersonId == id)
            .OrderBy(a => a.AssetType.Name)
            .ThenBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Select(a => new Row(a.InventoryNumber, a.Name, a.Description, a.AssetType.Name))
            .ToListAsync(cancellationToken);

        // Theirs by name, or on a device they hold, as their page counts them; active only, since an
        // ended seat is history, not something signed for.
        var seats = await context
            .Activations.AsNoTracking()
            .Where(a => (a.PersonId == id || a.Asset.PersonId == id) && a.DeactivationDate == null)
            .OrderBy(a => a.Volume.SoftwareOrService.Name)
            .ThenBy(a => a.ActivationDate)
            .ThenBy(a => a.Id)
            .Select(a => new SeatRow(
                a.Volume.SoftwareOrService.Name,
                a.Volume.License.Asset.Name,
                a.Volume.License.Asset.InventoryNumber,
                a.PersonId == id ? null : a.Asset.Name,
                a.PersonId == id ? null : a.Asset.InventoryNumber,
                a.Volume.License.SubscriptionExpirationDate
            ))
            .ToListAsync(cancellationToken);

        return Results.Ok(
            new Response(
                name,
                await ControllerAsync(user, context, cancellationToken),
                printer is not null,
                assets,
                seats
            )
        );
    }

    /// <summary>
    /// The signed-in person's name: the person whose email the session's is, else the session's own
    /// name, which a one-time code's session holds as the email.
    /// </summary>
    private static async Task<string> ControllerAsync(
        ClaimsPrincipal user,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var email = user.FindFirstValue(ClaimTypes.Email) ?? "";
        var lower = email.Trim().ToLowerInvariant();
        var person =
            lower == ""
                ? null
                : await context
                    .Persons.Where(p => p.Email.Trim().ToLower() == lower)
                    .OrderBy(p => p.Name)
                    .ThenBy(p => p.Id)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync(cancellationToken);

        return !string.IsNullOrWhiteSpace(person) ? person.Trim()
            : !string.IsNullOrWhiteSpace(user.Identity?.Name) ? user.Identity.Name
            : email;
    }
}
