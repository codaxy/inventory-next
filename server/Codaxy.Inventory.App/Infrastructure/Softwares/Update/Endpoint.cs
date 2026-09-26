using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] SoftwareForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entity = await context.Softwares.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        if (await SoftwareWrites.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        SoftwareWrites.Apply(entity, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await SoftwareWrites.DetailAsync(context, id, cancellationToken));
    }
}
