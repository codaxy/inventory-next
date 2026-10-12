using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Administration.Users.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await Users.DetailAsync(
            context,
            (await access.CurrentAsync(cancellationToken))!,
            id,
            cancellationToken
        )
            is { } user
            ? Results.Ok(user)
            : Results.NotFound();
}
