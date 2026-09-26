using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// The volumes, the licence and the asset, in one save — unless something stands on them: a
    /// volume's activations, clouds or software cascade with it, and a maintenance contract restricts
    /// the asset. Those are named in a 409 rather than taken along or left to fail as a 500.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var license = await context
            .Licenses.Include(l => l.Asset)
            .Include(l => l.Volumes)
            .FirstOrDefaultAsync(l => l.AssetId == id, cancellationToken);

        if (license is null)
            return Results.NotFound();

        var held = await LicenseWrites.HeldAsync(
            context,
            license.Volumes.Select(v => v.Id).ToList(),
            cancellationToken
        );

        if (held.Count > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"{held.Values.First()}, so the licence cannot be deleted."
            );

        var contracts = await context.MaintenanceContracts.CountAsync(
            m => m.AssetId == id,
            cancellationToken
        );
        if (contracts > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: contracts == 1
                    ? "A maintenance contract is on this licence, so it cannot be deleted."
                    : $"{contracts} maintenance contracts are on this licence, so it cannot be deleted."
            );

        context.Volumes.RemoveRange(license.Volumes);
        context.Licenses.Remove(license);
        context.Assets.Remove(license.Asset);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
