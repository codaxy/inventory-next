using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.Clients.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.Clients.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapGet("/export", Handle);

    /// <summary>One client: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Projects")]
        public int Projects { get; init; }
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

        var projects = context.Projects;
        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(c => new Row
            {
                Name = c.Name,
                Projects = projects.Count(p => p.ClientId == c.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered = !string.IsNullOrWhiteSpace(query.Q);

        return Spreadsheet.File(rows, "Clients.Export", filtered);
    }
}
