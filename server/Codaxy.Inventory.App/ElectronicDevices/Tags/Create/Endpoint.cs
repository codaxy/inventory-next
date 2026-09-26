using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] TagForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var typeIds = (form.TypeIds ?? []).Distinct().ToList();

        if (await Tags.CheckTypesAsync(context, typeIds, cancellationToken) is { } missing)
            return missing;

        if (await Tags.CheckNameAsync(context, form.Name!, null, cancellationToken) is { } taken)
            return taken;

        var tag = new ElectronicDeviceTag { Id = Guid.CreateVersion7() };
        Tags.Apply(context, tag, form, typeIds);
        context.ElectronicDeviceTags.Add(tag);

        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/electronic-devices/tags/{tag.Id}",
            await Tags.DetailAsync(context, tag.Id, cancellationToken)
        );
    }
}
