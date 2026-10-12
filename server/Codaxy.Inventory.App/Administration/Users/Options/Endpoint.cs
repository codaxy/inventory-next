using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users.Options;

/// <summary>The roles the reader may give, in authority order: System Admin, the built-in, the rest.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapGet("/options", Handle);

    public sealed record RoleOption(Guid Id, string Name, string Description);

    public sealed record Response(IReadOnlyList<RoleOption> Roles);

    private static async Task<Response> Handle(
        UserAccess access,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var reader = (await access.CurrentAsync(cancellationToken))!;
        var roles = await context
            .Roles.AsNoTracking()
            .Include(r => r.Permissions)
            .ToListAsync(cancellationToken);

        return new(
            roles
                .Where(reader.Covers)
                .OrderBy(r => BuiltInRoles.Rank(r.Name))
                .ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
                .Select(r => new RoleOption(r.Id, r.Name, r.Description))
                .ToList()
        );
    }
}
