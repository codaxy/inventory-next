using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Reactivate;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) =>
        activations.MapPost("/{id:guid}/reactivate", Handle);

    /// <summary>A deactivation undone: the date it held is discarded, as in the original.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        var activation = await context.Activations.FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken
        );

        if (activation is null)
            return Results.NotFound();

        if (activation.DeactivationDate is null)
            return Activations.Conflict("This activation is active.");

        activation.DeactivationDate = null;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            await Activations.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
        );
    }
}
