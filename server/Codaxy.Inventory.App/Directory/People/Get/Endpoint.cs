using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Directory.People.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await People.DetailAsync(context, id, cancellationToken) is { } person
            ? Results.Ok(person)
            : Results.NotFound();
}
