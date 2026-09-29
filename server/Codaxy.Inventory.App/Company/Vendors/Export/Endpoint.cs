using Codaxy.CodeReports.CodeModel;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.Vendors.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.Vendors.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapGet("/export", Handle);

    /// <summary>One vendor: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "Name         ")]
        public string Name { get; init; } = "";

        [TableColumn(HeaderText = "Contact Person         ")]
        public string? ContactPerson { get; init; }

        [TableColumn(HeaderText = "Email         ")]
        public string? Email { get; init; }

        [TableColumn(HeaderText = "Phone         ")]
        public string? Phone { get; init; }

        [TableColumn(HeaderText = "Mobile Phone         ")]
        public string? MobilePhone { get; init; }

        [TableColumn(HeaderText = "Location         ")]
        public string? Location { get; init; }

        [TableColumn(HeaderText = "Registration Number         ")]
        public string? RegistrationNumber { get; init; }

        [TableColumn(HeaderText = "VAT Number         ")]
        public string? VatNumber { get; init; }

        [TableColumn(HeaderText = "Web         ")]
        public string? Web { get; init; }

        [TableColumn(HeaderText = "Assets")]
        public int Assets { get; init; }
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

        var assets = context.Assets;
        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(v => new Row
            {
                Name = v.Name,
                ContactPerson = v.ContactPerson,
                Email = v.Email,
                Phone = v.Phone,
                MobilePhone = v.MobilePhone,
                Location = v.Location,
                RegistrationNumber = v.RegistrationNumber,
                VatNumber = v.VATNumber,
                Web = v.Web,
                Assets = assets.Count(a => a.VendorId == v.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Excel.File(rows, "Vendors.Export", filtered);
    }
}
