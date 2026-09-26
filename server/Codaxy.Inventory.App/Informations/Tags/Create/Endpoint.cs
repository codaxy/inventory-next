using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Informations.Tags.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] InformationTagForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await InformationTagWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var entity = new InformationTag { Id = Guid.CreateVersion7() };
        InformationTagWrites.Apply(entity, form);
        context.InformationTags.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/informations/tags/{entity.Id}",
            await InformationTagWrites.DetailAsync(context, entity.Id, cancellationToken)
        );
    }
}
