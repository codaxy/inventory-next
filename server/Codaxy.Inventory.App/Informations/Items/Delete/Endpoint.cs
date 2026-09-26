using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Items.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) =>
        information.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A piece of information goes with its locations and tag links, and nothing else points at it.
    /// The locations are removed through the context so the audit log records them.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var information = await context.Informations.FirstOrDefaultAsync(
            i => i.Id == id,
            cancellationToken
        );
        if (information is null)
            return Results.NotFound();

        context.InformationLocations.RemoveRange(
            await context
                .InformationLocations.Where(l => l.InformationId == id)
                .ToListAsync(cancellationToken)
        );
        context.Informations.Remove(information);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
