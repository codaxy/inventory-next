using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Projects.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapGet("/", Handle);

    /// <param name="Q">Free text over name, client and owner.</param>
    /// <param name="PersonId">The owner.</param>
    /// <param name="Sort"><c>name</c> (default), <c>client</c>, <c>owner</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? ClientId,
        Guid? PersonId,
        string? Sort,
        int? Page,
        int? PageSize
    );

    public sealed record Item(Guid Id, string Name, string Client, string Owner, int Information);

    private static readonly string[] Keys = ["name", "client", "owner"];

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (Refuse(query) is { } refused)
            return refused;

        var information = context.Informations;
        return Results.Ok(
            await Rows(context, query)
                .Select(p => new Item(
                    p.Id,
                    p.Name,
                    p.Client.Name,
                    p.ProjectOwner.Name,
                    information.Count(i => i.ProjectId == p.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }

    /// <summary>A sort outside the convention; the problem to answer with, or none.</summary>
    internal static IResult? Refuse(Query query) =>
        query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-'))
            ? Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name, client or owner."] }
            )
            : null;

    /// <summary>The projects the query selects, in its order, for the page and the export alike.</summary>
    internal static IOrderedQueryable<Project> Rows(InventoryContext context, Query query)
    {
        var projects = context.Projects.AsNoTracking();
        if (query.ClientId is { } client)
            projects = projects.Where(p => p.ClientId == client);
        if (query.PersonId is { } person)
            projects = projects.Where(p => p.ProjectOwnerId == person);
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                projects = projects.Where(p =>
                    p.Id == id || p.ClientId == id || p.ProjectOwnerId == id
                );
                continue;
            }

            var pattern = FreeText.Pattern(term);
            projects = projects.Where(p =>
                EF.Functions.ILike(p.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(p.Client.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(p.ProjectOwner.Name, pattern, FreeText.Escape)
            );
        }

        var descending = query.Sort?.StartsWith('-') == true;
        var ordered = query.Sort?.TrimStart('-') switch
        {
            "client" => descending
                ? projects.OrderByDescending(p => p.Client.Name)
                : projects.OrderBy(p => p.Client.Name),
            "owner" => descending
                ? projects.OrderByDescending(p => p.ProjectOwner.Name)
                : projects.OrderBy(p => p.ProjectOwner.Name),
            _ => descending
                ? projects.OrderByDescending(p => p.Name)
                : projects.OrderBy(p => p.Name),
        };
        return ordered.ThenBy(p => p.Name).ThenBy(p => p.Id);
    }
}
