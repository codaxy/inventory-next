using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Administration.Roles.Reset;

/// <summary>A built-in role back to what a new database seeds it with.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapPost("/{id:guid}/reset", Handle);

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

        if (BuiltInRoles.DefaultsOf(role.Name) is not { } defaults || !role.IsBuiltIn)
            return Roles.Conflict("Only a built-in role has defaults to go back to.");

        // The defaults are granted like any edit: someone lacking one of them cannot reset to it.
        if (defaults.Permissions.FirstOrDefault(p => !manager.Has(p)) is { } lacking)
            return Roles.Forbidden($"The defaults grant {lacking}, which you do not hold.");

        role.Description = defaults.Description;
        Roles.Grant(context, role, defaults.Permissions);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Roles.DetailAsync(context, manager, id, cancellationToken));
    }
}
