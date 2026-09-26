using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Company.Clients.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] ClientForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await Clients.CheckNameAsync(context, form.Name!, null, cancellationToken) is { } taken)
            return taken;

        var client = new Client { Id = Guid.CreateVersion7() };
        Clients.Apply(client, form);
        context.Clients.Add(client);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/company/clients/{client.Id}",
            await Clients.DetailAsync(context, client.Id, cancellationToken)
        );
    }
}
