using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Roles.Delete;

/// <summary>A custom role no user holds.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapDelete("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var manager = (await access.CurrentAsync(cancellationToken))!;
        var role = await Roles.FindAsync(context, id, cancellationToken);
        if (role is null)
            return Results.NotFound();

        if (Roles.Untouchable(manager, role) is { } untouchable)
            return untouchable;

        if (role.IsBuiltIn)
            return Roles.Conflict("A built-in role is not deleted.");

        var holders = await context.AppUsers.CountAsync(u => u.RoleId == id, cancellationToken);
        if (holders > 0)
            return Roles.Conflict(
                holders == 1 ? "1 user holds this role." : $"{holders} users hold this role."
            );

        Roles.Grant(context, role, []);
        context.Roles.Remove(role);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
