using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Company.People.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        SettingStore settings,
        CancellationToken cancellationToken
    ) =>
        await People.DetailAsync(context, settings, id, cancellationToken) is { } person
            ? Results.Ok(person)
            : Results.NotFound();
}
