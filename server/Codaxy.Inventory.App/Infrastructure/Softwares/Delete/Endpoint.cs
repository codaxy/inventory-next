using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapDelete("/{id:guid}", Handle);

    /// <summary>A software no information is kept on goes; the information's foreign key refuses otherwise.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var entity = await context.Softwares.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        var information = await context.Informations.CountAsync(
            i => i.InformationLocations.Any(l => l.SoftwareId == id),
            cancellationToken
        );
        if (information > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: information == 1
                    ? "A piece of information is kept on it, so it cannot be deleted."
                    : $"{information} pieces of information are kept on it, so it cannot be deleted."
            );

        context.Softwares.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
