using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Items.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) => information.MapGet("/", Handle);

    /// <param name="Q">Free text over name, author, description, type, assignee and project.</param>
    /// <param name="PersonId">The assignee.</param>
    /// <param name="LocationId">A physical location it is kept at; the three after it, the same for the others.</param>
    /// <param name="Sort"><c>name</c> (default), <c>type</c>, <c>assignee</c>, <c>author</c>, <c>project</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? TypeId,
        Guid? PersonId,
        Guid? ProjectId,
        Guid? TagId,
        Guid? LocationId,
        Guid? VirtualMachineId,
        Guid? CloudSubscriptionId,
        Guid? SoftwareId,
        bool? Incomplete,
        string? Sort,
        int? Page,
        int? PageSize
    );

    public sealed record Item(
        Guid Id,
        string Name,
        bool Incomplete,
        string Type,
        string Assignee,
        string? Author,
        string? Project
    );

    private static readonly string[] Keys = ["name", "type", "assignee", "author", "project"];

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

        return Results.Ok(
            await Rows(context, query)
                .Select(i => new Item(
                    i.Id,
                    i.Name,
                    i.Incomplete == true,
                    i.InformationType.Name,
                    i.Person.Name,
                    i.Author,
                    i.ProjectId == null ? null : i.Project.Name
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }

    internal static IResult? Refuse(Query query) =>
        query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-'))
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Sort by name, type, assignee, author or project."],
                }
            )
            : null;

    /// <summary>The information the query selects, in its order — for the page and the export alike.</summary>
    internal static IOrderedQueryable<Information> Rows(InventoryContext context, Query query)
    {
        var rows = context.Informations.AsNoTracking();
        if (query.TypeId is { } type)
            rows = rows.Where(i => i.InformationTypeId == type);
        if (query.PersonId is { } person)
            rows = rows.Where(i => i.PersonId == person);
        if (query.ProjectId is { } project)
            rows = rows.Where(i => i.ProjectId == project);
        if (query.TagId is { } tag)
            rows = rows.Where(i => i.Tags.Any(t => t.InformationTagId == tag));
        if (query.LocationId is { } location)
            rows = rows.Where(i =>
                i.InformationLocations.Any(l => l.PhysicalLocationId == location)
            );
        if (query.VirtualMachineId is { } machine)
            rows = rows.Where(i => i.InformationLocations.Any(l => l.VirtualMachineId == machine));
        if (query.CloudSubscriptionId is { } cloud)
            rows = rows.Where(i => i.InformationLocations.Any(l => l.CloudId == cloud));
        if (query.SoftwareId is { } software)
            rows = rows.Where(i => i.InformationLocations.Any(l => l.SoftwareId == software));
        if (query.Incomplete is { } incomplete)
            rows = rows.Where(i => (i.Incomplete == true) == incomplete);
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                rows = rows.Where(i =>
                    i.Id == id
                    || i.InformationTypeId == id
                    || i.PersonId == id
                    || i.ProjectId == id
                    || i.ConfidentialityId == id
                    || i.IntegrityId == id
                    || i.AvailabilityId == id
                    || i.ImportanceId == id
                );
                continue;
            }

            var pattern = FreeText.Pattern(term);
            rows = rows.Where(i =>
                EF.Functions.ILike(i.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(i.Author, pattern, FreeText.Escape)
                || EF.Functions.ILike(i.Description, pattern, FreeText.Escape)
                || EF.Functions.ILike(i.InformationType.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(i.Person.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(i.Project.Name, pattern, FreeText.Escape)
            );
        }

        var descending = query.Sort?.StartsWith('-') == true;
        IOrderedQueryable<Information> By<T>(
            System.Linq.Expressions.Expression<Func<Information, T>> key
        ) => descending ? rows.OrderByDescending(key) : rows.OrderBy(key);

        return (
            query.Sort?.TrimStart('-') switch
            {
                "type" => By(i => i.InformationType.Name),
                "assignee" => By(i => i.Person.Name),
                "author" => By(i => i.Author),
                "project" => By(i => i.Project.Name),
                _ => By(i => i.Name),
            }
        )
            .ThenBy(i => i.Name)
            .ThenBy(i => i.Id);
    }
}
