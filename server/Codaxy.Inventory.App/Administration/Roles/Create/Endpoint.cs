using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Administration.Roles.Create;

/// <summary>A custom role, granting only what its maker holds.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] RoleForm form,
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var manager = (await access.CurrentAsync(cancellationToken))!;
        var (permissions, refused) = Roles.Read(form, manager);
        if (refused is not null)
            return refused;

        if (await Roles.CheckNameAsync(context, form.Name!, null, cancellationToken) is { } taken)
            return taken;

        var role = new Role
        {
            Id = Guid.CreateVersion7(),
            Name = form.Name!.Trim(),
            Description = form.Description?.Trim() ?? "",
        };
        context.Roles.Add(role);
        Roles.Grant(context, role, permissions!);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/administration/roles/{role.Id}",
            await Roles.DetailAsync(context, manager, role.Id, cancellationToken)
        );
    }
}
