using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Company.Locations.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] LocationForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await LocationWrites.CheckAsync(context, form, null, cancellationToken) is { } refused)
            return refused;

        var location = new Location { Id = Guid.CreateVersion7() };
        LocationWrites.Apply(location, form);
        context.Locations.Add(location);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/company/locations/{location.Id}",
            await LocationWrites.DetailAsync(context, location.Id, clock, cancellationToken)
        );
    }
}
