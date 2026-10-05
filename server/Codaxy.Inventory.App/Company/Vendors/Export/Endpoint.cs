using ClosedXML.Attributes;
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
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Contact Person")]
        public string? ContactPerson { get; init; }

        [XLColumn(Header = "Email")]
        public string? Email { get; init; }

        [XLColumn(Header = "Phone")]
        public string? Phone { get; init; }

        [XLColumn(Header = "Mobile Phone")]
        public string? MobilePhone { get; init; }

        [XLColumn(Header = "Location")]
        public string? Location { get; init; }

        [XLColumn(Header = "Registration Number")]
        public string? RegistrationNumber { get; init; }

        [XLColumn(Header = "VAT Number")]
        public string? VatNumber { get; init; }

        [XLColumn(Header = "Web")]
        public string? Web { get; init; }

        [XLColumn(Header = "Assets")]
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

        return Spreadsheet.File(rows, "Vendors.Export", filtered);
    }
}
