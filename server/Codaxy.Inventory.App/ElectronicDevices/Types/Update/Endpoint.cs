using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.Update;

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

        var type = await context
            .ElectronicDeviceTypes.Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (type is null)
            return Results.NotFound();

        var tagIds = (form.TagIds ?? []).Distinct().ToList();

        if (await DeviceTypes.CheckTagsAsync(context, tagIds, cancellationToken) is { } missing)
            return missing;

        if (
            await DeviceTypes.CheckNameAsync(context, form.Name!, id, cancellationToken) is
            { } taken
        )
            return taken;

        DeviceTypes.Apply(context, type, form, tagIds);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await DeviceTypes.DetailAsync(context, id, cancellationToken));
    }
}
