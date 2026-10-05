using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.People.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.People.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapGet("/export", Handle);

    /// <summary>One person: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Email")]
        public string Email { get; init; } = "";

        [XLColumn(Header = "Assets")]
        public int Assets { get; init; }

        [XLColumn(Header = "Seats")]
        public int Seats { get; init; }
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
            .Select(p => new Row
            {
                Name = p.Name,
                Email = p.Email,
                Assets = assets.Count(a => a.PersonId == p.Id),
                Seats = context.Activations.Count(a =>
                    a.PersonId == p.Id && a.DeactivationDate == null
                ),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Spreadsheet.File(rows, "People.Export", filtered);
    }
}
