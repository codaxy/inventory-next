using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Roles.List;

/// <summary>The roles in authority order: System Admin, the other built-in roles, then the rest by name.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder roles) => roles.MapGet("/", Handle);

    public sealed record Query(string? Q, int? Page, int? PageSize);

    public sealed record Item(
        Guid Id,
        string Name,
        string Description,
        bool IsBuiltIn,
        bool IsSystemAdmin,
        int Users
    );

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        var roles = context.Roles.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                roles = roles.Where(r => r.Id == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            roles = roles.Where(r =>
                EF.Functions.ILike(r.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(r.Description, pattern, FreeText.Escape)
            );
        }

        var users = context.AppUsers;
        return Results.Ok(
            await roles
                .OrderByDescending(r => r.IsSystemAdmin)
                .ThenByDescending(r => r.IsBuiltIn)
                .ThenBy(r => r.Name == BuiltInRoles.Admin ? 0 : 1)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.Id)
                .Select(r => new Item(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsBuiltIn,
                    r.IsSystemAdmin,
                    users.Count(u => u.RoleId == r.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
