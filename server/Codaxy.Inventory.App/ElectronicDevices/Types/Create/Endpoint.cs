using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] TypeForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var tagIds = (form.TagIds ?? []).Distinct().ToList();

        if (await DeviceTypes.CheckTagsAsync(context, tagIds, cancellationToken) is { } missing)
            return missing;

        if (
            await DeviceTypes.CheckNameAsync(context, form.Name!, null, cancellationToken) is
            { } taken
        )
            return taken;

        var type = new ElectronicDeviceType { Id = Guid.CreateVersion7() };
        DeviceTypes.Apply(context, type, form, tagIds);
        context.ElectronicDeviceTypes.Add(type);

        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/electronic-devices/types/{type.Id}",
            await DeviceTypes.DetailAsync(context, type.Id, cancellationToken)
        );
    }
}
