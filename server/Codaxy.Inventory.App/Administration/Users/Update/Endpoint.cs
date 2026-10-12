using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Administration.Users.Update;

/// <summary>
/// A user's name and role. Nobody changes their own role, nobody changes a user whose role grants
/// more than theirs, and the last active System Admin keeps the role.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] UserForm form,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var manager = (await access.CurrentAsync(cancellationToken))!;
        var user = await Users.FindAsync(context, id, cancellationToken);
        if (user is null)
            return Results.NotFound();

        if (!manager.Covers(user.Role))
            return Users.Forbidden(
                "This user's role grants more than yours, so you cannot change it."
            );

        if (form.RoleId != user.RoleId)
        {
            if (user.Id == manager.UserId)
                return Users.Conflict("You cannot change your own role.");

            var (role, refused) = await Users.AssignableAsync(
                context,
                manager,
                form.RoleId!.Value,
                cancellationToken
            );
            if (refused is not null)
                return refused;

            if (
                !role!.IsSystemAdmin
                && await Users.IsLastSystemAdminAsync(context, user, cancellationToken)
            )
                return Users.Conflict("This is the last active System Admin, so the role stays.");

            user.RoleId = role.Id;
        }

        user.Name = form.Name!.Trim();
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Users.DetailAsync(context, manager, id, cancellationToken));
    }
}
