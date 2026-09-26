using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Types.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapDelete("/{id:guid}", Handle);

    /// <summary>A type no information is of goes. One in use is refused, not only where the database would refuse: the information's foreign key cascades, so an unguarded delete takes every piece of it.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var entity = await context.InformationTypes.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        var information = await context.Informations.CountAsync(
            i => i.InformationTypeId == id,
            cancellationToken
        );
        if (information > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: information == 1
                    ? "A piece of information is of this type, so it cannot be deleted."
                    : $"{information} pieces of information are of this type, so it cannot be deleted."
            );

        context.InformationTypes.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
