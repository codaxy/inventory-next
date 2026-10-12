using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Administration.Users.Configured;

/// <summary>
/// The addresses <c>Auth:AllowedUsers</c> admits that are not users yet, so the screen shows everyone
/// who can get in. Gone with the setting.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapGet("/configured", Handle);

    private static async Task<IReadOnlyList<string>> Handle(
        IOptions<UserOptions> options,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var admitted = options.Value.Admitted().ToList();
        var added = await context
            .AppUsers.Where(u => admitted.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        return admitted.Except(added).Order(StringComparer.Ordinal).ToList();
    }
}
