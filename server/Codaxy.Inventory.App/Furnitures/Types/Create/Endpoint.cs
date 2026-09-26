using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Furnitures.Types.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] TypeForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await FurnitureTypes.CheckNameAsync(context, form.Name!, null, cancellationToken) is
            { } taken
        )
            return taken;

        var type = new FurnitureType { Id = Guid.CreateVersion7() };
        FurnitureTypes.Apply(type, form);
        context.FurnitureTypes.Add(type);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/furniture/types/{type.Id}",
            await FurnitureTypes.DetailAsync(context, type.Id, cancellationToken)
        );
    }
}
