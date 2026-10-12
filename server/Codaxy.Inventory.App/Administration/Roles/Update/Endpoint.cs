using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Administration.Roles.Update;

/// <summary>A role's name, description and permissions. A built-in role keeps its name.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] RoleForm form,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var manager = (await access.CurrentAsync(cancellationToken))!;
        var role = await Roles.FindAsync(context, id, cancellationToken);
        if (role is null)
            return Results.NotFound();

        if (Roles.Untouchable(manager, role) is { } untouchable)
            return untouchable;

        // The seeder and Reset to default find a built-in role by its name.
        if (role.IsBuiltIn && form.Name!.Trim() != role.Name)
            return Roles.Problem("name", "A built-in role keeps its name.");

        var (permissions, refused) = Roles.Read(form, manager);
        if (refused is not null)
            return refused;

        if (await Roles.CheckNameAsync(context, form.Name!, id, cancellationToken) is { } taken)
            return taken;

        role.Name = form.Name!.Trim();
        role.Description = form.Description?.Trim() ?? "";
        Roles.Grant(context, role, permissions!);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await Roles.DetailAsync(context, manager, id, cancellationToken));
    }
}
