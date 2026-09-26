using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapGet("/", Handle);

    /// <param name="Q">Free text over name and email.</param>
    /// <param name="Sort"><c>name</c> (default), <c>email</c>, <c>assets</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    /// <param name="Assets">Every asset the person holds: devices, furniture, licences.</param>
    /// <param name="Seats">Active seats assigned to the person by name.</param>
    public sealed record Item(Guid Id, string Name, string Email, int Assets, int Seats);

    private static readonly string[] Keys = ["name", "email", "assets"];

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
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name, email or assets."] }
            );

        var people = context.Persons.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            people = people.Where(p =>
                EF.Functions.ILike(p.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(p.Email, pattern, FreeText.Escape)
            );
        }

        var assets = context.Assets;
        var descending = query.Sort?.StartsWith('-') == true;
        var ordered = query.Sort?.TrimStart('-') switch
        {
            "email" => descending
                ? people.OrderByDescending(p => p.Email)
                : people.OrderBy(p => p.Email),
            "assets" => descending
                ? people.OrderByDescending(p => assets.Count(a => a.PersonId == p.Id))
                : people.OrderBy(p => assets.Count(a => a.PersonId == p.Id)),
            _ => descending ? people.OrderByDescending(p => p.Name) : people.OrderBy(p => p.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(p => p.Name)
                .ThenBy(p => p.Id)
                .Select(p => new Item(
                    p.Id,
                    p.Name,
                    p.Email,
                    assets.Count(a => a.PersonId == p.Id),
                    context.Activations.Count(a => a.PersonId == p.Id && a.DeactivationDate == null)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
