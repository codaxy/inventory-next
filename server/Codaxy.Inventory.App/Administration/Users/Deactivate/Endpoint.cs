using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Administration.Users.Deactivate;

/// <summary>Ends a user's access on their next request; their sessions stop there.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) =>
        users.MapPost("/{id:guid}/deactivate", Handle);

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

        if (!user.IsActive)
            return Users.Conflict("This user is already deactivated.");

        if (user.Id == manager.UserId)
            return Users.Conflict("You cannot deactivate yourself.");

        if (!manager.Covers(user.Role))
            return Users.Forbidden(
                "This user's role grants more than yours, so you cannot change it."
            );

        if (await Users.IsLastSystemAdminAsync(context, user, cancellationToken))
            return Users.Conflict("This is the last active System Admin, so they stay active.");

        user.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Users.DetailAsync(context, manager, id, cancellationToken));
    }
}
