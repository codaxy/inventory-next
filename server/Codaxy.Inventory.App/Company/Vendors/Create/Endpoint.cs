using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Company.Vendors.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] VendorForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await VendorWrites.CheckAsync(context, form, null, cancellationToken) is { } refused)
            return refused;

        var vendor = new Vendor { Id = Guid.CreateVersion7() };
        VendorWrites.Apply(vendor, form);
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/company/vendors/{vendor.Id}",
            await VendorWrites.DetailAsync(
                context,
                vendor.Id,
                VendorWrites.Today(clock),
                cancellationToken
            )
        );
    }
}
