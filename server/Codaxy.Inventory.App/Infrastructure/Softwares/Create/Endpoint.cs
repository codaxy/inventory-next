using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] SoftwareForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await SoftwareWrites.CheckAsync(context, form, null, cancellationToken) is { } refused)
            return refused;

        var entity = new Software { Id = Guid.CreateVersion7() };
        SoftwareWrites.Apply(entity, form);
        context.Softwares.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/infrastructure/software/{entity.Id}",
            await SoftwareWrites.DetailAsync(context, entity.Id, cancellationToken)
        );
    }
}
