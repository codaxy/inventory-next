using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Locations.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapGet("/", Handle);

    /// <param name="Q">Free text over name, description, street and city.</param>
    /// <param name="Sort"><c>name</c> (default), <c>city</c>, <c>assets</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(
        Guid Id,
        string Name,
        string? Street,
        int? HouseNumber,
        string City,
        string? Room,
        int Assets
    );

    private static readonly string[] Keys = ["name", "city", "assets"];

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-')))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name, city or assets."] }
            );

        var locations = context.Locations.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                locations = locations.Where(l => l.Id == id || l.CityId == id || l.StateId == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            locations = locations.Where(l =>
                EF.Functions.ILike(l.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.Description, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.Street, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.City.Name, pattern, FreeText.Escape)
            );
        }

        var assets = context.Assets;
        var descending = query.Sort?.StartsWith('-') == true;
        var ordered = query.Sort?.TrimStart('-') switch
        {
            "city" => descending
                ? locations.OrderByDescending(l => l.City.Name)
                : locations.OrderBy(l => l.City.Name),
            "assets" => descending
                ? locations.OrderByDescending(l => assets.Count(a => a.LocationId == l.Id))
                : locations.OrderBy(l => assets.Count(a => a.LocationId == l.Id)),
            _ => descending
                ? locations.OrderByDescending(l => l.Name)
                : locations.OrderBy(l => l.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(l => l.Name)
                .ThenBy(l => l.Id)
                .Select(l => new Item(
                    l.Id,
                    l.Name,
                    l.Street,
                    l.HouseNumber,
                    l.City.Name,
                    l.Room,
                    assets.Count(a => a.LocationId == l.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
