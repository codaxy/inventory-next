using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Furnitures.Items.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Furnitures.Items.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapGet("/export", Handle);

    /// <summary>One piece of furniture as the original's spreadsheet has it; the headers are its own.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "No")]
        public int? InventoryNumber { get; init; }

        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Assignee")]
        public string Assignee { get; init; } = "";

        [XLColumn(Header = "Location")]
        public string? Location { get; init; }

        [XLColumn(Header = "Type")]
        public string? Type { get; init; }

        [XLColumn(Header = "Model")]
        public string? Model { get; init; }

        [XLColumn(Header = "Vendor")]
        public string Vendor { get; init; } = "";

        [XLColumn(Header = "Purchase Value")]
        public decimal PurchaseValue { get; init; }

        [XLColumn(Header = "Purchase Date")]
        public DateOnly PurchaseDate { get; init; }

        [XLColumn(Header = "Invoice Number")]
        public string? InvoiceNumber { get; init; }

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
            .Select(f => new Row
            {
                InventoryNumber = f.Asset.InventoryNumber,
                Name = f.Asset.Name,
                Assignee = f.Asset.Person.Name,
                Location = f.Asset.LocationId == null ? null : f.Asset.Location.Name,
                Type = f.FurnitureTypeId == null ? null : f.FurnitureType.Name,
                Model = f.Model,
                Vendor = f.Asset.Vendor.Name,
                PurchaseValue = f.Asset.PurchaseValue,
                PurchaseDate = f.Asset.PurchaseDate,
                InvoiceNumber = f.Asset.InvoiceNumber,
                Modified = f.Asset.LastModified,
            })
            .ToListAsync(cancellationToken);

        // To UTC once read: EF cannot translate `UtcDateTime` in a projection.
        foreach (var row in rows)
            row.LastModified = row.Modified.UtcDateTime;

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.TypeId is not null
            || query.VendorId is not null
            || query.PersonId is not null
            || query.LocationId is not null
            || query.PurchasedFrom is not null
            || query.PurchasedTo is not null
            || query.Incomplete is not null;

        return Spreadsheet.File(rows, "Furniture.Export", filtered);
    }
}
