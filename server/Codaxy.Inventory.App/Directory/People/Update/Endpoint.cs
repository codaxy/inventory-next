using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] PersonForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var person = await context.Persons.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (person is null)
            return Results.NotFound();

        if (await People.CheckUniqueAsync(context, form, id, cancellationToken) is { } taken)
            return taken;

        People.Apply(person, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await People.DetailAsync(context, id, cancellationToken));
    }
}
