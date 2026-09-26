using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Clients.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>projects</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(Guid Id, string Name, int Projects);

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "name" or "-name" or "projects" or "-projects"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name or projects."] }
            );

        var clients = context.Clients.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                clients = clients.Where(c => c.Id == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            clients = clients.Where(c => EF.Functions.ILike(c.Name, pattern, FreeText.Escape));
        }

        var projects = context.Projects;
        var ordered = query.Sort switch
        {
            "-name" => clients.OrderByDescending(c => c.Name),
            "projects" => clients
                .OrderBy(c => projects.Count(p => p.ClientId == c.Id))
                .ThenBy(c => c.Name),
            "-projects" => clients
                .OrderByDescending(c => projects.Count(p => p.ClientId == c.Id))
                .ThenBy(c => c.Name),
            _ => clients.OrderBy(c => c.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(c => c.Id)
                .Select(c => new Item(c.Id, c.Name, projects.Count(p => p.ClientId == c.Id)))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
