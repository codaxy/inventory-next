using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] CloudSubscriptionForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entity = await context.Clouds.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return Results.NotFound();

        if (
            await CloudSubscriptionWrites.CheckAsync(context, form, id, cancellationToken) is
            { } refused
        )
            return refused;

        CloudSubscriptionWrites.Apply(entity, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            await CloudSubscriptionWrites.DetailAsync(context, id, cancellationToken)
        );
    }
}
