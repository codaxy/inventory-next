using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Locations.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) =>
        locations.MapDelete("/{id:guid}", Handle);

    /// <summary>A location nothing is at goes; the assets' and the information's foreign keys refuse otherwise.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var location = await context.Locations.FirstOrDefaultAsync(
            l => l.Id == id,
            cancellationToken
        );
        if (location is null)
            return Results.NotFound();

        var assets = await context.Assets.CountAsync(a => a.LocationId == id, cancellationToken);
        var information = await context.InformationLocations.CountAsync(
            l => l.PhysicalLocationId == id,
            cancellationToken
        );
        var named = new[]
        {
            assets switch
            {
                0 => null,
                1 => "1 asset",
                _ => $"{assets} assets",
            },
            information switch
            {
                0 => null,
                1 => "1 piece of information",
                _ => $"{information} pieces of information",
            },
        }.OfType<string>().ToList();

        if (named.Count > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"{string.Join(" and ", named)} {(assets + information == 1 ? "is" : "are")} at this location, so it cannot be deleted."
            );

        context.Locations.Remove(location);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
