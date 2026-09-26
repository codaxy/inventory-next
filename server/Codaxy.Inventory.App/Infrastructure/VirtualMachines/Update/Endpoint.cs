using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder virtualMachines) =>
        virtualMachines.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] VirtualMachineForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var entity = await context.VirtualMachines.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        if (
            await VirtualMachineWrites.CheckAsync(context, form, id, cancellationToken) is
            { } refused
        )
            return refused;

        VirtualMachineWrites.Apply(entity, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await VirtualMachineWrites.DetailAsync(context, id, cancellationToken));
    }
}
