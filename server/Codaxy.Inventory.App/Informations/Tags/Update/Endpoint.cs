using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Tags.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] InformationTagForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entity = await context.InformationTags.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        if (
            await InformationTagWrites.CheckAsync(context, form, id, cancellationToken) is
            { } refused
        )
            return refused;

        InformationTagWrites.Apply(entity, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await InformationTagWrites.DetailAsync(context, id, cancellationToken));
    }
}
