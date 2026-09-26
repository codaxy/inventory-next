using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Manufacturers.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) => manufacturers.MapGet("/", Handle);

    /// <param name="Q">Free text over name and URL.</param>
    /// <param name="Sort"><c>name</c> (default), <c>devices</c>, <c>software</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(Guid Id, string Name, string? Url, int Devices, int Software);

    private static readonly string[] Keys = ["name", "devices", "software"];

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
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Sort by name, devices or software."],
                }
            );

        var manufacturers = context.Manufacturers.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                manufacturers = manufacturers.Where(m => m.Id == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            manufacturers = manufacturers.Where(m =>
                EF.Functions.ILike(m.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(m.URL, pattern, FreeText.Escape)
            );
        }

        var devices = context.ElectronicDevices;
        var software = context.SoftwareOrServices;
        var descending = query.Sort?.StartsWith('-') == true;
        var ordered = query.Sort?.TrimStart('-') switch
        {
            "devices" => descending
                ? manufacturers.OrderByDescending(m => devices.Count(d => d.ManufacturerId == m.Id))
                : manufacturers.OrderBy(m => devices.Count(d => d.ManufacturerId == m.Id)),
            "software" => descending
                ? manufacturers.OrderByDescending(m =>
                    software.Count(s => s.ManufacturerId == m.Id)
                )
                : manufacturers.OrderBy(m => software.Count(s => s.ManufacturerId == m.Id)),
            _ => descending
                ? manufacturers.OrderByDescending(m => m.Name)
                : manufacturers.OrderBy(m => m.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(m => m.Name)
                .ThenBy(m => m.Id)
                .Select(m => new Item(
                    m.Id,
                    m.Name,
                    m.URL,
                    devices.Count(d => d.ManufacturerId == m.Id),
                    software.Count(s => s.ManufacturerId == m.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
