using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Locations.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] LocationForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var location = await context.Locations.FirstOrDefaultAsync(
            l => l.Id == id,
            cancellationToken
        );
        if (location is null)
            return Results.NotFound();

        if (await LocationWrites.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        LocationWrites.Apply(location, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await LocationWrites.DetailAsync(context, id, clock, cancellationToken));
    }
}
