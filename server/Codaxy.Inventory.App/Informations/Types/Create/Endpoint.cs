using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Informations.Types.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] InformationTypeForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await InformationTypeWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var entity = new InformationType { Id = Guid.CreateVersion7() };
        InformationTypeWrites.Apply(entity, form);
        context.InformationTypes.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/informations/types/{entity.Id}",
            await InformationTypeWrites.DetailAsync(context, entity.Id, cancellationToken)
        );
    }
}
