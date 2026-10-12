using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder users) => users.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>email</c>, <c>role</c>, <c>created</c>; <c>-</c> for descending.</param>
    /// <param name="Active">Only the active users, or only the deactivated.</param>
    public sealed record Query(string? Q, bool? Active, string? Sort, int? Page, int? PageSize);

    public sealed record Item(
        Guid Id,
        string Name,
        string Email,
        string Role,
        bool IsActive,
        DateTimeOffset CreatedAt
    );

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (
            query.Sort
            is not (
                null
                or "name"
                or "-name"
                or "email"
                or "-email"
                or "role"
                or "-role"
                or "created"
                or "-created"
            )
        )
            return Users.Problem("sort", "Sort by name, email, role or created.");

        return Results.Ok(
            await Rows(context, query)
                .Select(u => new Item(u.Id, u.Name, u.Email, u.Role.Name, u.IsActive, u.CreatedAt))
                .ToPageAsync(window, cancellationToken)
        );
    }

    private static IOrderedQueryable<AppUser> Rows(InventoryContext context, Query query)
    {
        var users = context.AppUsers.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                users = users.Where(u => u.Id == id || u.RoleId == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            users = users.Where(u =>
                EF.Functions.ILike(u.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(u.Email, pattern, FreeText.Escape)
                || EF.Functions.ILike(u.Role.Name, pattern, FreeText.Escape)
            );
        }

        if (query.Active is { } active)
            users = users.Where(u => u.IsActive == active);

        // Authority order: System Admin, the built-in roles as listed, then the rest by name.
        var ordered = query.Sort switch
        {
            "-name" => users.OrderByDescending(u => u.Name),
            "email" => users.OrderBy(u => u.Email),
            "-email" => users.OrderByDescending(u => u.Email),
            "role" => users
                .OrderByDescending(u => u.Role.IsSystemAdmin)
                .ThenByDescending(u => u.Role.IsBuiltIn)
                .ThenBy(u => u.Role.Name == BuiltInRoles.Admin ? 0 : 1)
                .ThenBy(u => u.Role.Name)
                .ThenBy(u => u.Name),
            "-role" => users
                .OrderBy(u => u.Role.IsSystemAdmin)
                .ThenBy(u => u.Role.IsBuiltIn)
                .ThenBy(u => u.Role.Name == BuiltInRoles.Admin ? 1 : 0)
                .ThenByDescending(u => u.Role.Name)
                .ThenBy(u => u.Name),
            "created" => users.OrderBy(u => u.CreatedAt),
            "-created" => users.OrderByDescending(u => u.CreatedAt),
            _ => users.OrderBy(u => u.Name),
        };
        return ordered.ThenBy(u => u.Id);
    }
}
