using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// An entry no licence volume names goes. One named is refused: the volume's foreign key
    /// cascades, so the database would take the volumes and every activation of them along with it.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var entry = await context.SoftwareOrServices.FirstOrDefaultAsync(
            s => s.Id == id,
            cancellationToken
        );

        if (entry is null)
            return Results.NotFound();

        var volumes = await context.Volumes.CountAsync(
            v => v.SoftwareOrServiceId == id,
            cancellationToken
        );

        if (volumes > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: volumes == 1
                    ? "A licence volume is of this software or service, so it cannot be deleted."
                    : $"{volumes} licence volumes are of this software or service, so it cannot be deleted."
            );

        context.SoftwareOrServices.Remove(entry);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
