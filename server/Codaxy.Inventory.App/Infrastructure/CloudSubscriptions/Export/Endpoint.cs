using Codaxy.CodeReports.CodeModel;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder cloudSubscriptions) =>
        cloudSubscriptions.MapGet("/export", Handle);

    /// <summary>One cloud subscription: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "Name         ")]
        public string Name { get; init; } = "";

        [TableColumn(HeaderText = "Management URL         ")]
        public string? ManagementUrl { get; init; }

        [TableColumn(HeaderText = "License         ")]
        public string License { get; init; } = "";

        [TableColumn(HeaderText = "Software         ")]
        public string Software { get; init; } = "";

        [TableColumn(HeaderText = "Information")]
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
                ManagementUrl = t.ManagementURL,
                License = t.Volume.License.Asset.Name,
                Software = t.Volume.SoftwareOrService.Name,
                Information = locations.Count(l => l.CloudId == t.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Excel.File(rows, "CloudSubscriptions.Export", filtered);
    }
}
