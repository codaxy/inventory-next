using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Manufacturers.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) =>
        manufacturers.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] ManufacturerForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var manufacturer = await context.Manufacturers.FirstOrDefaultAsync(
            m => m.Id == id,
            cancellationToken
        );
        if (manufacturer is null)
            return Results.NotFound();

        if (
            await ManufacturerWrites.CheckAsync(context, form, id, cancellationToken) is { } refused
        )
            return refused;

        ManufacturerWrites.Apply(manufacturer, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await ManufacturerWrites.DetailAsync(context, id, cancellationToken));
    }
}
