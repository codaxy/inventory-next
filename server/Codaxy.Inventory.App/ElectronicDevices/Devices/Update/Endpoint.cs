using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapPut("/{id:guid}", Handle);

    /// <summary>Every field but the number, against the last-modified time the form was loaded with.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] DeviceForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (AssetWrites.Validate(form, MiniValidator.Errors(form)) is { Count: > 0 } errors)
            return Results.ValidationProblem(errors);

        var device = await context
            .ElectronicDevices.Include(d => d.Asset)
            .FirstOrDefaultAsync(d => d.AssetId == id, cancellationToken);
        if (device is null)
            return Results.NotFound();

        if (AssetWrites.CheckUnchanged(device.Asset, form, "device") is { } changed)
            return changed;

        if (await DeviceWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        await DeviceWrites.ApplyAsync(
            context,
            device.Asset,
            device,
            form,
            clock,
            cancellationToken
        );
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await DeviceReads.DetailAsync(context, id, cancellationToken));
    }
}
