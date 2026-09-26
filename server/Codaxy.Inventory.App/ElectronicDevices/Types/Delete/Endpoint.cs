using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A type no device uses goes, and the database drops its tag links. One in use is refused: the
    /// device's foreign key does not cascade, so the database would refuse it anyway, as a 500.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var type = await context.ElectronicDeviceTypes.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );

        if (type is null)
            return Results.NotFound();

        var devices = await context.ElectronicDevices.CountAsync(
            d => d.ElectronicDeviceTypeId == id,
            cancellationToken
        );

        if (devices > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: devices == 1
                    ? "A device is of this type, so it cannot be deleted."
                    : $"{devices} devices are of this type, so it cannot be deleted."
            );

        context.ElectronicDeviceTypes.Remove(type);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
