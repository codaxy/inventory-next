using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Administration.Users.Reactivate;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) =>
        users.MapPost("/{id:guid}/reactivate", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var manager = (await access.CurrentAsync(cancellationToken))!;
        var user = await Users.FindAsync(context, id, cancellationToken);
        if (user is null)
            return Results.NotFound();

        if (user.IsActive)
            return Users.Conflict("This user is already active.");

        if (!manager.Covers(user.Role))
            return Users.Forbidden(
                "This user's role grants more than yours, so you cannot change it."
            );

        user.IsActive = true;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Users.DetailAsync(context, manager, id, cancellationToken));
    }
}
