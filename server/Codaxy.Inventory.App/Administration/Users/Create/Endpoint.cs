using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] NewUserForm form,
        UserAccess access,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var manager = (await access.CurrentAsync(cancellationToken))!;
        var (role, refused) = await Users.AssignableAsync(
            context,
            manager,
            form.RoleId!.Value,
            cancellationToken
        );
        if (refused is not null)
            return refused;

        var email = AppUser.Normalize(form.Email!);
        if (await context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken))
            return Users.Problem("email", "A user with this email already exists.");

        var user = new AppUser
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            Name = form.Name!.Trim(),
            RoleId = role!.Id,
            IsActive = true,
            CreatedAt = clock.GetUtcNow(),
        };
        context.AppUsers.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/administration/users/{user.Id}",
            await Users.DetailAsync(context, manager, user.Id, cancellationToken)
        );
    }
}
