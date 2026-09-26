using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapGet("/", Handle);

    /// <param name="TagIds">A type must carry every one.</param>
    /// <param name="Sort"><c>name</c> (default), <c>-name</c>, <c>tags</c>, <c>-tags</c>, <c>devices</c> or <c>-devices</c>.</param>
    public sealed record Query(
        string? Q,
        [FromQuery(Name = "tagId")] Guid[]? TagIds,
        bool? HoldsLicences,
        string? Sort,
        int? Page,
        int? PageSize
    );

    /// <param name="FirstTags">The first three tags by name; <paramref name="TagCount"/> says how many more.</param>
    public sealed record Item(
        Guid Id,
        string Name,
        bool HoldsLicences,
        string? Description,
        int TagCount,
        IReadOnlyList<string> FirstTags,
        int DeviceCount
    );

    /// <summary>As many tag names as a row shows before "+N".</summary>
    public const int Shown = 3;

    private static readonly string[] Sorts =
    [
        "name",
        "-name",
        "tags",
        "-tags",
        "devices",
        "-devices",
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
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name, tags or devices."] }
            );

        var types = context.ElectronicDeviceTypes.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            types = types.Where(t =>
                EF.Functions.ILike(t.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(t.Description, pattern, FreeText.Escape)
            );
        }

        foreach (var tagId in (query.TagIds ?? []).Distinct())
            types = types.Where(t => t.Tags.Any(l => l.ElectronicDeviceTagId == tagId));

        if (query.HoldsLicences is { } holds)
            types = types.Where(t => t.HoldLicences == holds);

        var devices = context.ElectronicDevices;

        // Ordered on the entity, then projected: EF cannot order by members of a record it built.
        // Name then id break ties, so types that sort equal keep one order across pages.
        var ordered = query.Sort switch
        {
            "-name" => types.OrderByDescending(t => t.Name).ThenBy(t => t.Id),
            "tags" => types.OrderBy(t => t.Tags.Count).ThenBy(t => t.Name).ThenBy(t => t.Id),
            "-tags" => types
                .OrderByDescending(t => t.Tags.Count)
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            "devices" => types
                .OrderBy(t => devices.Count(d => d.ElectronicDeviceTypeId == t.Id))
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            "-devices" => types
                .OrderByDescending(t => devices.Count(d => d.ElectronicDeviceTypeId == t.Id))
                .ThenBy(t => t.Name)
                .ThenBy(t => t.Id),
            _ => types.OrderBy(t => t.Name).ThenBy(t => t.Id),
        };

        var items = ordered.Select(t => new Item(
            t.Id,
            t.Name,
            t.HoldLicences,
            t.Description,
            t.Tags.Count,
            t.Tags.Select(l => l.ElectronicDeviceTag.Name).OrderBy(n => n).Take(Shown).ToList(),
            devices.Count(d => d.ElectronicDeviceTypeId == t.Id)
        ));

        return Results.Ok(await items.ToPageAsync(window, cancellationToken));
    }
}
