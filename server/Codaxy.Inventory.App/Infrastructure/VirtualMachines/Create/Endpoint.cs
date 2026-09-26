using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder virtualMachines) =>
        virtualMachines.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] VirtualMachineForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await VirtualMachineWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var entity = new VirtualMachine { Id = Guid.CreateVersion7() };
        VirtualMachineWrites.Apply(entity, form);
        context.VirtualMachines.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/infrastructure/virtual-machines/{entity.Id}",
            await VirtualMachineWrites.DetailAsync(context, entity.Id, cancellationToken)
        );
    }
}
