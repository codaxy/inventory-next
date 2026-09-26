using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] TagForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var tag = await context
            .ElectronicDeviceTags.Include(t => t.Types)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag is null)
            return Results.NotFound();

        var typeIds = (form.TypeIds ?? []).Distinct().ToList();

        if (await Tags.CheckTypesAsync(context, typeIds, cancellationToken) is { } missing)
            return missing;

        Tags.Apply(context, tag, form, typeIds);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Tags.DetailAsync(context, id, cancellationToken));
    }
}
