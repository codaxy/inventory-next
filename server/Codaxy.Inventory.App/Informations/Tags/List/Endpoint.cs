using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Tags.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapGet("/", Handle);

    /// <param name="Q">Free text over name and description.</param>
    /// <param name="Sort"><c>name</c> (default), <c>information</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(Guid Id, string Name, string? Description, int Information);

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "name" or "-name" or "information" or "-information"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name or information."] }
            );

        var rows = context.InformationTags.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            rows = rows.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Description, pattern, FreeText.Escape)
            );
        }

        var information = context.Informations;
        var ordered = query.Sort switch
        {
            "-name" => rows.OrderByDescending(t => t.Name),
            "information" => rows.OrderBy(t =>
                    context.InformationTagInformations.Count(l => l.InformationTagId == t.Id)
                )
                .ThenBy(t => t.Name),
            "-information" => rows.OrderByDescending(t =>
                    context.InformationTagInformations.Count(l => l.InformationTagId == t.Id)
                )
                .ThenBy(t => t.Name),
            _ => rows.OrderBy(t => t.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(t => t.Id)
                .Select(t => new Item(
                    t.Id,
                    t.Name,
                    t.Description,
                    context.InformationTagInformations.Count(l => l.InformationTagId == t.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
