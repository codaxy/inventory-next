using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.Locations.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.Locations.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapGet("/export", Handle);

    /// <summary>One location: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Description")]
        public string? Description { get; init; }

        [XLColumn(Header = "Street")]
        public string? Street { get; init; }

        [XLColumn(Header = "House Number")]
        public int? HouseNumber { get; init; }

        [XLColumn(Header = "Floor")]
        public int? Floor { get; init; }

        [XLColumn(Header = "Room")]
        public string? Room { get; init; }

        [XLColumn(Header = "Postal Code")]
        public string? PostalCode { get; init; }

        [XLColumn(Header = "City")]
        public string City { get; init; } = "";

        [XLColumn(Header = "State")]
        public string? State { get; init; }

        [XLColumn(Header = "Country")]
        public string? Country { get; init; }

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
            .Select(l => new Row
            {
                Name = l.Name,
                Description = l.Description,
                Street = l.Street,
                HouseNumber = l.HouseNumber,
                Floor = l.Floor,
                Room = l.Room,
                PostalCode = l.PostalCode,
                City = l.City.Name,
                State = l.StateId == null ? null : l.State.Name,
                Country = l.Country.Name,
                Assets = assets.Count(a => a.LocationId == l.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Spreadsheet.File(rows, "Locations.Export", filtered);
    }
}
