using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Vendors.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A vendor nothing names goes. One an asset or a maintenance contract names is refused, not only
    /// where the database would refuse: both foreign keys cascade, so an unguarded delete takes every
    /// asset bought from the vendor with it.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (vendor is null)
            return Results.NotFound();

        var assets = await context.Assets.CountAsync(a => a.VendorId == id, cancellationToken);
        var contracts = await context.MaintenanceContracts.CountAsync(
            c => c.VendorId == id,
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
            contracts switch
            {
                0 => null,
                1 => "1 maintenance contract",
                _ => $"{contracts} maintenance contracts",
            },
        }.OfType<string>().ToList();

        if (named.Count > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"{string.Join(" and ", named)} {(assets + contracts == 1 ? "names" : "name")} this vendor, so it cannot be deleted."
            );

        context.Vendors.Remove(vendor);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
