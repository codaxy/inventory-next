using ClosedXML.Attributes;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Licenses.Activations.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Licenses.Activations.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) => activations.MapGet("/export", Handle);

    /// <summary>
    /// One activation as the original's spreadsheet has it; the headers are its own, and so is "Last
    /// Modified", which is the license's.
    /// </summary>
    public sealed class Row
    {
        [XLColumn(Header = "Software/Service")]
        public string Software { get; init; } = "";

        [XLColumn(Header = "License")]
        public string License { get; init; } = "";

        [XLColumn(Header = "User")]
        public string? User { get; init; }

        [XLColumn(Header = "Device")]
        public string? Device { get; init; }

        [XLColumn(Header = "Quantity")]
        public int Quantity { get; init; }

        [XLColumn(Header = "Activation")]
        public DateOnly ActivationDate { get; init; }

        [XLColumn(Header = "Deactivation")]
        public DateOnly? DeactivationDate { get; init; }

        [XLColumn(Header = "Last Modified")]
        public DateTime LastModified { get; set; }

        /// <summary>As stored, for <see cref="LastModified"/> to be read from; not a column.</summary>
        [XLColumn(Ignore = true)]
        public DateTimeOffset Modified { get; init; }
    }

    /// <summary>The list as a spreadsheet: the same query, every row it selects, not one page.</summary>
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
            .Select(a => new Row
            {
                Software = a.Volume.SoftwareOrService.Name,
                License = a.Volume.License.Asset.Name,
                User = a.PersonId == null ? null : a.Person.Name,
                Device = a.AssetId == null ? null : a.Asset.Name,
                Quantity = a.Quantity,
                ActivationDate = a.ActivationDate,
                DeactivationDate = a.DeactivationDate,
                Modified = a.Volume.License.Asset.LastModified,
            })
            .ToListAsync(cancellationToken);

        // To UTC once read: EF cannot translate `UtcDateTime` in a projection.
        foreach (var row in rows)
            row.LastModified = row.Modified.UtcDateTime;

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.SoftwareId is not null
            || query.LicenseId is not null
            || query.VolumeId is not null
            || query.Status is not null
            || query.Expiry is not null;

        return Spreadsheet.File(rows, "Activations.Export", filtered);
    }
}
