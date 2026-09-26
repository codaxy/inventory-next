using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>-name</c>, <c>types</c> or <c>-types</c>.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    /// <param name="FirstTypes">The first three types by name; <paramref name="TypeCount"/> says how many more.</param>
    public sealed record Item(
        Guid Id,
        string Name,
        string? Description,
        int TypeCount,
        IReadOnlyList<string> FirstTypes
    );

    /// <summary>As many type names as a row shows before "+N".</summary>
    public const int Shown = 3;

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "name" or "-name" or "types" or "-types"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name or types."] }
            );

        var tags = context.ElectronicDeviceTags.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            tags = tags.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Description, pattern, FreeText.Escape)
            );
        }

        // Ordered on the entity, then projected: EF cannot order by members of a record it built.
        // Id breaks ties, so tags that sort equal keep one order across pages.
        var ordered = query.Sort switch
        {
            "-name" => tags.OrderByDescending(t => t.Name).ThenBy(t => t.Id),
            "types" => tags.OrderBy(t => t.Types.Count).ThenBy(t => t.Name).ThenBy(t => t.Id),
            "-types" => tags.OrderByDescending(t => t.Types.Count)
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            _ => tags.OrderBy(t => t.Name).ThenBy(t => t.Id),
        };

        var items = ordered.Select(t => new Item(
            t.Id,
            t.Name,
            t.Description,
            t.Types.Count,
            t.Types.Select(l => l.ElectronicDeviceType.Name).OrderBy(n => n).Take(Shown).ToList()
        ));

        return Results.Ok(await items.ToPageAsync(window, cancellationToken));
    }
}
