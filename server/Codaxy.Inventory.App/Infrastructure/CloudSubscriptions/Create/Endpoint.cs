using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] CloudSubscriptionForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await CloudSubscriptionWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var entity = new Cloud { Id = Guid.CreateVersion7() };
        CloudSubscriptionWrites.Apply(entity, form);
        context.Clouds.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/infrastructure/cloud-subscriptions/{entity.Id}",
            await CloudSubscriptionWrites.DetailAsync(context, entity.Id, cancellationToken)
        );
    }
}
