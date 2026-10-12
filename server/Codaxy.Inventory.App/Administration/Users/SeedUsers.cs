using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>How a deployment gets its first users: <c>Auth:SeedUsers</c>, read at startup.</summary>
public static class SeedUsers
{
    /// <summary>
    /// Creates each configured user that does not exist yet, and never changes one that does. A role
    /// the database does not have stops the start, and so does a database left without an active
    /// System Admin: nobody could then manage the users at all.
    /// </summary>
    public static async Task ApplyAsync(
        InventoryContext context,
        UserOptions options,
        TimeProvider time,
        CancellationToken cancellation = default
    )
    {
        var roles = await context.Roles.ToListAsync(cancellation);

        foreach (var seed in options.SeedUsers)
        {
            var role =
                roles.FirstOrDefault(r => r.Name == seed.Role.Trim())
                ?? throw new InvalidOperationException(
                    $"{UserOptions.Section}:SeedUsers gives {seed.Email} the role \"{seed.Role}\", which "
                        + $"does not exist. The roles are: {string.Join(", ", roles.Select(r => r.Name))}."
                );

            var email = AppUser.Normalize(seed.Email);
            if (await context.AppUsers.AnyAsync(u => u.Email == email, cancellation))
                continue;

            context.AppUsers.Add(
                new AppUser
                {
                    Id = Guid.CreateVersion7(),
                    Email = email,
                    Name = seed.Name.Trim(),
                    RoleId = role.Id,
                    IsActive = true,
                    CreatedAt = time.GetUtcNow(),
                }
            );
            await context.SaveChangesAsync(cancellation);
        }

        if (!await context.AppUsers.AnyAsync(u => u.IsActive && u.Role.IsSystemAdmin, cancellation))
            throw new InvalidOperationException(
                "There is no active System Admin, so nobody could manage the users. Name one in "
                    + $"{UserOptions.Section}:SeedUsers — {UserOptions.Section}__SeedUsers__0__Email, "
                    + "__Name and __Role (\"System Admin\") in the environment."
            );
    }
}
