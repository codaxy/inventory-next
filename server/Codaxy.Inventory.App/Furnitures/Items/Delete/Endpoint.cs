using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Items.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) =>
        furniture.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// The furniture and its asset, in one save — unless a maintenance contract is on it, which
    /// restricts the asset: named in a 409 rather than left to fail as a 500.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var furniture = await context
            .Furnitures.Include(f => f.Asset)
            .FirstOrDefaultAsync(f => f.AssetId == id, cancellationToken);
        if (furniture is null)
            return Results.NotFound();

        var contracts = await context.MaintenanceContracts.CountAsync(
            m => m.AssetId == id,
            cancellationToken
        );
        if (contracts > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: contracts == 1
                    ? "A maintenance contract is on this furniture, so it cannot be deleted."
                    : $"{contracts} maintenance contracts are on this furniture, so it cannot be deleted."
            );

        context.Furnitures.Remove(furniture);
        context.Assets.Remove(furniture.Asset);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
