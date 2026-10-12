using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.App.Administration.Roles.Catalogue;

/// <summary>
/// Every permission a role can grant, in the catalogue's order, and whether the reader may grant it:
/// nobody grants what they do not hold.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapGet("/catalogue", Handle);

    /// <param name="Implies">The view an edit cannot be held without.</param>
    public sealed record Entry(
        string Key,
        string Section,
        string Name,
        string Description,
        string? Implies,
        bool Grantable
    );

    private static async Task<IReadOnlyList<Entry>> Handle(
        UserAccess access,
        CancellationToken cancellationToken
    )
    {
        var reader = (await access.CurrentAsync(cancellationToken))!;
        return Permissions
            .All.Select(p => new Entry(
                p.Key,
                p.Section,
                p.Name,
                p.Description,
                Permissions.Implied(p.Key),
                reader.Has(p.Key)
            ))
            .ToList();
    }
}
