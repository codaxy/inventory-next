using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Administration.Roles.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await Roles.DetailAsync(
            context,
            (await access.CurrentAsync(cancellationToken))!,
            id,
            cancellationToken
        )
            is { } role
            ? Results.Ok(role)
            : Results.NotFound();
}
