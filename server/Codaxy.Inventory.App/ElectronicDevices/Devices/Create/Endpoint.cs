using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapPost("/", Handle);

    /// <summary>The asset and its device row, numbered from the sequence, in one save.</summary>
    private static async Task<IResult> Handle(
        [FromBody] DeviceForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (AssetWrites.Validate(form, MiniValidator.Errors(form)) is { Count: > 0 } errors)
            return Results.ValidationProblem(errors);

        if (await DeviceWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        var id = Guid.CreateVersion7();
        var asset = new Asset
        {
            Id = id,
            InventoryNumber = await AssetWrites.TakeNumberAsync(context, cancellationToken),
            AssetTypeId = await AssetWrites.AssetTypeAsync(
                context,
                DeviceWrites.AssetType,
                cancellationToken
            ),
        };
        var device = new ElectronicDevice { AssetId = id, Asset = asset };
        await DeviceWrites.ApplyAsync(context, asset, device, form, clock, cancellationToken);
        context.Assets.Add(asset);
        context.ElectronicDevices.Add(device);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/electronic-devices/{id}",
            await DeviceReads.DetailAsync(context, id, cancellationToken)
        );
    }
}
