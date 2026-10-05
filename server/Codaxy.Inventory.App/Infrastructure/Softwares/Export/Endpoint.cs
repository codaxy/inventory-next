using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Infrastructure.Softwares.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Infrastructure.Softwares.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder software) => software.MapGet("/export", Handle);

    /// <summary>One software entry: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "License")]
        public string License { get; init; } = "";

        [XLColumn(Header = "Software")]
        public string Software { get; init; } = "";

        [XLColumn(Header = "Information")]
        public int Information { get; init; }
    }

    /// <summary>The list as a spreadsheet: the same query, every row it selects, not one page.</summary>
    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (List.Endpoint.Refuse(query) is { } refused)
            return refused;

        var locations = context.InformationLocations;
        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(t => new Row
            {
                Name = t.Name,
                License = t.Volume.License.Asset.Name,
                Software = t.Volume.SoftwareOrService.Name,
                Information = locations.Count(l => l.SoftwareId == t.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Spreadsheet.File(rows, "Software.Export", filtered);
    }
}
