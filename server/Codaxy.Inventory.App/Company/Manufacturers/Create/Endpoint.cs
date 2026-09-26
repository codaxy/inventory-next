using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Company.Manufacturers.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) => manufacturers.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] ManufacturerForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await ManufacturerWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var manufacturer = new Manufacturer { Id = Guid.CreateVersion7() };
        ManufacturerWrites.Apply(manufacturer, form);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/company/manufacturers/{manufacturer.Id}",
            await ManufacturerWrites.DetailAsync(context, manufacturer.Id, cancellationToken)
        );
    }
}
