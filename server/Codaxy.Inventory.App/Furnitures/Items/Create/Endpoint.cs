using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Furnitures.Items.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapPost("/", Handle);

    /// <summary>The asset and its furniture row, numbered from the sequence, in one save.</summary>
    private static async Task<IResult> Handle(
        [FromBody] FurnitureForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (AssetWrites.Validate(form, MiniValidator.Errors(form)) is { Count: > 0 } errors)
            return Results.ValidationProblem(errors);

        if (await FurnitureWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        var id = Guid.CreateVersion7();
        var asset = new Asset
        {
            Id = id,
            InventoryNumber = await AssetWrites.TakeNumberAsync(context, cancellationToken),
            AssetTypeId = await AssetWrites.AssetTypeAsync(
                context,
                FurnitureWrites.AssetType,
                cancellationToken
            ),
        };
        var furniture = new Furniture { AssetId = id, Asset = asset };

        await FurnitureWrites.ApplyAsync(context, asset, furniture, form, clock, cancellationToken);
        context.Assets.Add(asset);
        context.Furnitures.Add(furniture);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/furniture/{id}",
            await FurnitureWrites.DetailAsync(context, id, cancellationToken)
        );
    }
}
