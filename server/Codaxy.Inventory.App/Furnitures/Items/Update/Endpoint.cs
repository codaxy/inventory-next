using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Items.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapPut("/{id:guid}", Handle);

    /// <summary>Every field but the number, against the last-modified time the form was loaded with.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] FurnitureForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (AssetWrites.Validate(form, MiniValidator.Errors(form)) is { Count: > 0 } errors)
            return Results.ValidationProblem(errors);

        var furniture = await context
            .Furnitures.Include(f => f.Asset)
            .FirstOrDefaultAsync(f => f.AssetId == id, cancellationToken);
        if (furniture is null)
            return Results.NotFound();

        if (AssetWrites.CheckUnchanged(furniture.Asset, form, "furniture") is { } changed)
            return changed;

        if (await FurnitureWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        await FurnitureWrites.ApplyAsync(
            context,
            furniture.Asset,
            furniture,
            form,
            clock,
            cancellationToken
        );
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await FurnitureWrites.DetailAsync(context, id, cancellationToken));
    }
}
