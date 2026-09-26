using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People.Handover;

/// <summary>
/// The handover sheet's rows: every asset the person holds — the equipment they sign for — in the
/// original's columns. Seats are not equipment, and the original's seat rows only repeated the device.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) =>
        people.MapGet("/{id:guid}/handover", Handle);

    public sealed record Row(int? Number, string Name, string? Description, string Type);

    public sealed record Response(string Name, IReadOnlyList<Row> Assets);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
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

        return Results.Ok(new Response(name, assets));
    }
}
