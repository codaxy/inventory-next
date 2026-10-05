using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Infrastructure.VirtualMachines.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder virtualMachines) =>
        virtualMachines.MapGet("/export", Handle);

    /// <summary>One virtual machine: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "IP Address")]
        public string? IpAddress { get; init; }

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
                IpAddress = t.IPAddress,
                Information = locations.Count(l => l.VirtualMachineId == t.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Spreadsheet.File(rows, "VirtualMachines.Export", filtered);
    }
}
