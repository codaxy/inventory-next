using Codaxy.CodeReports.CodeModel;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Licenses.Licenses.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Licenses.Licenses.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/export", Handle);

    /// <summary>One licence as the original's spreadsheet has it; the headers are its own.</summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "No     ")]
        public int? InventoryNumber { get; init; }

        [TableColumn(HeaderText = "Name         ")]
        public string Name { get; init; } = "";

        [TableColumn(HeaderText = "Vendor         ")]
        public string Vendor { get; init; } = "";

        [TableColumn(HeaderText = "Purchase Value")]
        public decimal PurchaseValue { get; init; }

        [TableColumn(HeaderText = "Purchase Date")]
        public DateOnly PurchaseDate { get; init; }

        [TableColumn(HeaderText = "Expiration Date")]
        public DateOnly? ExpirationDate { get; init; }

        [TableColumn(HeaderText = "Last Modified")]
        public DateTime LastModified { get; set; }

        /// <summary>As stored, for <see cref="LastModified"/> to be read from; not a column.</summary>
        public DateTimeOffset Modified { get; init; }
    }

    /// <summary>
    /// The list as a spreadsheet: the same query — search, filters, sort — every row it selects, not
    /// one page. A link, not the original's handle in a cache: the session is a cookie, which a
    /// download carries; the original's bearer token could not.
    /// </summary>
    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (List.Endpoint.Refuse(query) is { } refused)
            return refused;

        var rows = await List
            .Endpoint.Rows(context, query, Expiry.Today(clock))
            .Select(l => new Row
            {
                InventoryNumber = l.Asset.InventoryNumber,
                Name = l.Asset.Name,
                Vendor = l.Asset.Vendor.Name,
                PurchaseValue = l.Asset.PurchaseValue,
                PurchaseDate = l.Asset.PurchaseDate,
                ExpirationDate = l.SubscriptionExpirationDate,
                Modified = l.Asset.LastModified,
            })
            .ToListAsync(cancellationToken);

        // To UTC once read: EF cannot translate `UtcDateTime` in a projection.
        foreach (var row in rows)
            row.LastModified = row.Modified.UtcDateTime;

        return Excel.File(rows, "Licenses.Export.xlsx");
    }
}
