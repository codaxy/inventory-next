using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] SoftwareOrServiceForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entry = await context.SoftwareOrServices.FirstOrDefaultAsync(
            s => s.Id == id,
            cancellationToken
        );

        if (entry is null)
            return Results.NotFound();

        if (await SoftwareServices.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        SoftwareServices.Apply(entry, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await SoftwareServices.DetailAsync(context, id, cancellationToken));
    }
}
