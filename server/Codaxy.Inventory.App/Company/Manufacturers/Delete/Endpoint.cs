using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Manufacturers.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) =>
        manufacturers.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A manufacturer nothing names goes. One a device or a software names is refused, not only where
    /// the database would refuse: the software's foreign key cascades, so an unguarded delete takes the
    /// software — and its volumes — with it.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var manufacturer = await context.Manufacturers.FirstOrDefaultAsync(
            m => m.Id == id,
            cancellationToken
        );
        if (manufacturer is null)
            return Results.NotFound();

        var devices = await context.ElectronicDevices.CountAsync(
            d => d.ManufacturerId == id,
            cancellationToken
        );
        var software = await context.SoftwareOrServices.CountAsync(
            s => s.ManufacturerId == id,
            cancellationToken
        );
        var named = new[]
        {
            devices switch
            {
                0 => null,
                1 => "1 device",
                _ => $"{devices} devices",
            },
            software switch
            {
                0 => null,
                1 => "1 software or service",
                _ => $"{software} software and services",
            },
        }.OfType<string>().ToList();

        if (named.Count > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"{string.Join(" and ", named)} {(devices + software == 1 ? "names" : "name")} this manufacturer, so it cannot be deleted."
            );

        context.Manufacturers.Remove(manufacturer);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
