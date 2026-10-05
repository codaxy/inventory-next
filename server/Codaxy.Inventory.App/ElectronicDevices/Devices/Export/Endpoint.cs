using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.ElectronicDevices.Devices.List.Endpoint.Query;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapGet("/export", Handle);

    /// <summary>One device as the original's spreadsheet has it; the headers are its own.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "No")]
        public int? InventoryNumber { get; init; }

        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Model Name")]
        public string? ModelName { get; init; }

        [XLColumn(Header = "Assignee")]
        public string Assignee { get; init; } = "";

        [XLColumn(Header = "Location")]
        public string? Location { get; init; }

        [XLColumn(Header = "Type")]
        public string? Type { get; init; }

        [XLColumn(Header = "Manufacturer")]
        public string? Manufacturer { get; init; }

        [XLColumn(Header = "Model Code")]
        public string? ModelCode { get; init; }

        [XLColumn(Header = "Serial Number")]
        public string? SerialNumber { get; init; }

        [XLColumn(Header = "Last Modified")]
        public DateTime LastModified { get; set; }

        /// <summary>As stored, for <see cref="LastModified"/> to be read from; not a column.</summary>
        public DateTimeOffset Modified { get; init; }
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

        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(d => new Row
            {
                InventoryNumber = d.Asset.InventoryNumber,
                Name = d.Asset.Name,
                ModelName = d.ModelName,
                Assignee = d.Asset.Person.Name,
                Location = d.Asset.LocationId == null ? null : d.Asset.Location.Name,
                Type = d.ElectronicDeviceTypeId == null ? null : d.ElectronicDeviceType.Name,
                Manufacturer = d.ManufacturerId == null ? null : d.Manufacturer.Name,
                ModelCode = d.ModelCode,
                SerialNumber = d.SerialNumber,
                Modified = d.Asset.LastModified,
            })
            .ToListAsync(cancellationToken);

        // To UTC once read: EF cannot translate `UtcDateTime` in a projection.
        foreach (var row in rows)
            row.LastModified = row.Modified.UtcDateTime;

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.TypeId is not null
            || query.TagId is not null
            || query.PersonId is not null
            || query.VendorId is not null
            || query.LocationId is not null
            || query.ManufacturerId is not null
            || query.PurchasedFrom is not null
            || query.PurchasedTo is not null
            || query.Incomplete is not null;

        return Spreadsheet.File(rows, "ElectronicDevices.Export", filtered);
    }
}
