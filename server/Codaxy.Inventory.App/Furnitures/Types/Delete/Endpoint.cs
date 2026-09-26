using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Types.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A type no furniture is of goes. One in use is refused: the furniture's foreign key does not
    /// cascade, so the database would refuse it anyway, as a 500.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var type = await context.FurnitureTypes.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (type is null)
            return Results.NotFound();

        var furniture = await context.Furnitures.CountAsync(
            f => f.FurnitureTypeId == id,
            cancellationToken
        );
        if (furniture > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: furniture == 1
                    ? "A piece of furniture is of this type, so it cannot be deleted."
                    : $"{furniture} pieces of furniture are of this type, so it cannot be deleted."
            );

        context.FurnitureTypes.Remove(type);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
