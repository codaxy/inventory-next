using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Directory.People.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] PersonForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await People.CheckUniqueAsync(context, form, null, cancellationToken) is { } taken)
            return taken;

        var person = new Person { Id = Guid.CreateVersion7() };
        People.Apply(person, form);
        context.Persons.Add(person);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/directory/people/{person.Id}",
            await People.DetailAsync(context, person.Id, cancellationToken)
        );
    }
}
