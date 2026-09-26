using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Vendors.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] VendorForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var vendor = await context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (vendor is null)
            return Results.NotFound();

        if (await VendorWrites.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        VendorWrites.Apply(vendor, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            await VendorWrites.DetailAsync(
                context,
                id,
                VendorWrites.Today(clock),
                cancellationToken
            )
        );
    }
}
