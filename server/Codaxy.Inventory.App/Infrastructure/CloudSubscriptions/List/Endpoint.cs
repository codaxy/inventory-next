using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>information</c>, <c>license</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(
        Guid Id,
        string Name,
        string License,
        string Software,
        int Information
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
                or "information"
                or "-information"
                or "license"
                or "-license"
            )
        )
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Sort by name, license or information."],
                }
            );

        var rows = context.Clouds.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                rows = rows.Where(t =>
                    t.Id == id
                    || t.VolumeId == id
                    || t.Volume.LicenseId == id
                    || t.Volume.SoftwareOrServiceId == id
                );
                continue;
            }

            var pattern = FreeText.Pattern(term);
            rows = rows.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Volume.License.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Volume.SoftwareOrService.Name, pattern, FreeText.Escape)
            );
        }

        var locations = context.InformationLocations;
        var ordered = query.Sort switch
        {
            "-name" => rows.OrderByDescending(t => t.Name),
            "information" => rows.OrderBy(t => locations.Count(l => l.CloudId == t.Id))
                .ThenBy(t => t.Name),
            "-information" => rows.OrderByDescending(t => locations.Count(l => l.CloudId == t.Id))
                .ThenBy(t => t.Name),
            "license" => rows.OrderBy(t => t.Volume.License.Asset.Name).ThenBy(t => t.Name),
            "-license" => rows.OrderByDescending(t => t.Volume.License.Asset.Name)
                .ThenBy(t => t.Name),
            _ => rows.OrderBy(t => t.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(t => t.Id)
                .Select(t => new Item(
                    t.Id,
                    t.Name,
                    t.Volume.License.Asset.Name,
                    t.Volume.SoftwareOrService.Name,
                    locations.Count(l => l.CloudId == t.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
