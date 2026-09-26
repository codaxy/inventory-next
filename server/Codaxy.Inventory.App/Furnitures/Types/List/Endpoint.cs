using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Types.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>-name</c>, <c>furniture</c> or <c>-furniture</c>.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(Guid Id, string Name, string? Description, int FurnitureCount);

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "name" or "-name" or "furniture" or "-furniture"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name or furniture."] }
            );

        var types = context.FurnitureTypes.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            types = types.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Description, pattern, FreeText.Escape)
            );
        }

        var furniture = context.Furnitures;

        // Ordered on the entity, then projected: EF cannot order by members of a record it built.
        var ordered = query.Sort switch
        {
            "-name" => types.OrderByDescending(t => t.Name).ThenBy(t => t.Id),
            "furniture" => types
                .OrderBy(t => furniture.Count(f => f.FurnitureTypeId == t.Id))
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            "-furniture" => types
                .OrderByDescending(t => furniture.Count(f => f.FurnitureTypeId == t.Id))
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            _ => types.OrderBy(t => t.Name).ThenBy(t => t.Id),
        };

        return Results.Ok(
            await ordered
                .Select(t => new Item(
                    t.Id,
                    t.Name,
                    t.Description,
                    furniture.Count(f => f.FurnitureTypeId == t.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
