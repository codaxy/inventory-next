using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Deactivate;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) =>
        activations.MapPost("/{id:guid}/deactivate", Handle);

    /// <summary>An active activation ends on a day, on or after the one it began.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] DeactivationForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var activation = await context.Activations.FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken
        );

        if (activation is null)
            return Results.NotFound();

        if (activation.DeactivationDate is not null)
            return Activations.Conflict("This activation is already deactivated.");

        if (form.Date < activation.ActivationDate)
            return Activations.Problem("date", "An activation cannot end before it began.");

        activation.DeactivationDate = form.Date;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            await Activations.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
        );
    }
}
