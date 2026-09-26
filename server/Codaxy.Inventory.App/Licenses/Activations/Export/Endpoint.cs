using Codaxy.CodeReports.CodeModel;
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
    /// Modified", which is the licence's.
    /// </summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "Software/Service")]
        public string Software { get; init; } = "";

        [TableColumn(HeaderText = "License    ")]
        public string License { get; init; } = "";

        [TableColumn(HeaderText = "User      ")]
        public string? User { get; init; }

        [TableColumn(HeaderText = "Device      ")]
        public string? Device { get; init; }

        [TableColumn]
        public int Quantity { get; init; }

        [TableColumn(HeaderText = "Activation  ")]
        public DateOnly ActivationDate { get; init; }

        [TableColumn(HeaderText = "Deactivation")]
        public DateOnly? DeactivationDate { get; init; }

        [TableColumn(HeaderText = "Last Modified")]
        public DateTime LastModified { get; set; }

        /// <summary>As stored, for <see cref="LastModified"/> to be read from; not a column.</summary>
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

        return Excel.File(rows, "Activations.Export.xlsx");
    }
}
