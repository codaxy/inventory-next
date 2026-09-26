using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Clients.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] ClientForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null)
            return Results.NotFound();

        if (await Clients.CheckNameAsync(context, form.Name!, id, cancellationToken) is { } taken)
            return taken;

        Clients.Apply(client, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Clients.DetailAsync(context, id, cancellationToken));
    }
}
