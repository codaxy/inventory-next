using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Types.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] InformationTypeForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entity = await context.InformationTypes.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        if (
            await InformationTypeWrites.CheckAsync(context, form, id, cancellationToken) is
            { } refused
        )
            return refused;

        InformationTypeWrites.Apply(entity, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await InformationTypeWrites.DetailAsync(context, id, cancellationToken));
    }
}
