using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Items.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) =>
        information.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] InformationForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var information = await context.Informations.FirstOrDefaultAsync(
            i => i.Id == id,
            cancellationToken
        );
        if (information is null)
            return Results.NotFound();

        if (await InformationWrites.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        await InformationWrites.ApplyAsync(context, information, form, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await InformationWrites.DetailAsync(context, id, cancellationToken));
    }
}
