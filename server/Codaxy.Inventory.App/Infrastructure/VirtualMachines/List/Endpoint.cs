using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder virtualMachines) =>
        virtualMachines.MapGet("/", Handle);

    /// <param name="Sort"><c>name</c> (default), <c>information</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    public sealed record Item(Guid Id, string Name, string? IpAddress, int Information);

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

        var rows = context.VirtualMachines.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                rows = rows.Where(t => t.Id == id);
                continue;
            }

            var pattern = FreeText.Pattern(term);
            rows = rows.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.IPAddress, pattern, FreeText.Escape)
            );
        }

        var locations = context.InformationLocations;
        var ordered = query.Sort switch
        {
            "-name" => rows.OrderByDescending(t => t.Name),
            "information" => rows.OrderBy(t => locations.Count(l => l.VirtualMachineId == t.Id))
                .ThenBy(t => t.Name),
            "-information" => rows.OrderByDescending(t =>
                    locations.Count(l => l.VirtualMachineId == t.Id)
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
                    t.IPAddress,
                    locations.Count(l => l.VirtualMachineId == t.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
