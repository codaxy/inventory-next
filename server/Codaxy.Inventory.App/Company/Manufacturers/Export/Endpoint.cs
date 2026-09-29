using Codaxy.CodeReports.CodeModel;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.Manufacturers.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.Manufacturers.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder manufacturers) =>
        manufacturers.MapGet("/export", Handle);

    /// <summary>One manufacturer: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "Name         ")]
        public string Name { get; init; } = "";

        [TableColumn(HeaderText = "URL         ")]
        public string? Url { get; init; }

        [TableColumn(HeaderText = "Devices")]
        public int Devices { get; init; }

        [TableColumn(HeaderText = "Software")]
        public int Software { get; init; }
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

        var devices = context.ElectronicDevices;
        var software = context.SoftwareOrServices;
        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(m => new Row
            {
                Name = m.Name,
                Url = m.URL,
                Devices = devices.Count(d => d.ManufacturerId == m.Id),
                Software = software.Count(s => s.ManufacturerId == m.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Excel.File(rows, "Manufacturers.Export", filtered);
    }
}
