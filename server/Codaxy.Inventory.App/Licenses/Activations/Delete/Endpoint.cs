using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) =>
        activations.MapDelete("/{id:guid}", Handle);

    /// <summary>A wrong activation is deleted and made again; nothing points at one.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var activation = await context.Activations.FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken
        );

        if (activation is null)
            return Results.NotFound();

        context.Activations.Remove(activation);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
