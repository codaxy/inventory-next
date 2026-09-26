using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Informations.Items.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) => information.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] InformationForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await InformationWrites.CheckAsync(context, form, null, cancellationToken) is
            { } refused
        )
            return refused;

        var information = new Information { Id = Guid.CreateVersion7() };
        context.Informations.Add(information);
        await InformationWrites.ApplyAsync(context, information, form, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/informations/{information.Id}",
            await InformationWrites.DetailAsync(context, information.Id, cancellationToken)
        );
    }
}
