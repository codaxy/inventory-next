using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Licenses.Licenses.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Licenses.Licenses.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/export", Handle);

    /// <summary>One license as the original's spreadsheet has it; the headers are its own.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "No")]
        public int? InventoryNumber { get; init; }

        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Vendor")]
        public string Vendor { get; init; } = "";

        [XLColumn(Header = "Purchase Value")]
        public decimal PurchaseValue { get; init; }

        [XLColumn(Header = "Purchase Date")]
        public DateOnly PurchaseDate { get; init; }

        [XLColumn(Header = "Expiration Date")]
        public DateOnly? ExpirationDate { get; init; }

        [XLColumn(Header = "Last Modified")]
        public DateTimeOffset LastModified { get; init; }
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
                LastModified = l.Asset.LastModified,
            })
            .ToListAsync(cancellationToken);

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.VendorId is not null
            || query.PurchasedFrom is not null
            || query.PurchasedTo is not null
            || query.Expiry is not null
            || query.Incomplete is not null;

        return Spreadsheet.File(rows, "Licenses.Export", filtered);
    }
}
