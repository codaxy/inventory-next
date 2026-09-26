using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapGet("/", Handle);

    /// <param name="Q">Free text over name, URL, category and manufacturer.</param>
    /// <param name="Sort"><c>name</c> (default), <c>category</c>, <c>manufacturer</c> or <c>volumes</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? CategoryId,
        Guid? ManufacturerId,
        string? Sort,
        int? Page,
        int? PageSize
    );

    public sealed record Item(
        Guid Id,
        string Name,
        string Category,
        string Manufacturer,
        string? Url,
        int VolumeCount
    );

    private static readonly string[] Sorts =
    [
        "name",
        "-name",
        "category",
        "-category",
        "manufacturer",
        "-manufacturer",
        "volumes",
        "-volumes",
    ];

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not null && !Sorts.Contains(query.Sort))
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Sort by name, category, manufacturer or volumes."],
                }
            );

        var entries = context.SoftwareOrServices.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            entries = entries.Where(s =>
                EF.Functions.ILike(s.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(s.Url, pattern, FreeText.Escape)
                || EF.Functions.ILike(s.SoftwareOrServiceCategory.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(s.Manufacturer.Name, pattern, FreeText.Escape)
            );
        }

        if (query.CategoryId is { } category)
            entries = entries.Where(s => s.SoftwareOrServiceCategoryId == category);

        if (query.ManufacturerId is { } manufacturer)
            entries = entries.Where(s => s.ManufacturerId == manufacturer);

        // Ordered on the entity, then projected: EF cannot order by members of a record it built.
        // Name then id break ties, so entries that sort equal keep one order across pages.
        var ordered = query.Sort switch
        {
            "-name" => entries.OrderByDescending(s => s.Name).ThenBy(s => s.Id),
            "category" => entries
                .OrderBy(s => s.SoftwareOrServiceCategory.Name)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            "-category" => entries
                .OrderByDescending(s => s.SoftwareOrServiceCategory.Name)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            "manufacturer" => entries
                .OrderBy(s => s.Manufacturer.Name)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            "-manufacturer" => entries
                .OrderByDescending(s => s.Manufacturer.Name)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            "volumes" => entries
                .OrderBy(s => s.Volumes.Count)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            "-volumes" => entries
                .OrderByDescending(s => s.Volumes.Count)
                .ThenBy(s => s.Name)
                .ThenBy(s => s.Id),
            _ => entries.OrderBy(s => s.Name).ThenBy(s => s.Id),
        };

        var items = ordered.Select(s => new Item(
            s.Id,
            s.Name,
            s.SoftwareOrServiceCategory.Name,
            s.Manufacturer.Name,
            s.Url,
            s.Volumes.Count
        ));

        return Results.Ok(await items.ToPageAsync(window, cancellationToken));
    }
}
