using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Types.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] TypeForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var type = await context.FurnitureTypes.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (type is null)
            return Results.NotFound();

        if (
            await FurnitureTypes.CheckNameAsync(context, form.Name!, id, cancellationToken) is
            { } taken
        )
            return taken;

        FurnitureTypes.Apply(type, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await FurnitureTypes.DetailAsync(context, id, cancellationToken));
    }
}
