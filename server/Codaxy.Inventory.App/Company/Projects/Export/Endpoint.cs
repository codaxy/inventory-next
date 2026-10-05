using ClosedXML.Attributes;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Company.Projects.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Company.Projects.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapGet("/export", Handle);

    /// <summary>One project: the list's columns, then the record's own fields.</summary>
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Client")]
        public string Client { get; init; } = "";

        [XLColumn(Header = "Owner")]
        public string Owner { get; init; } = "";

        [XLColumn(Header = "Information")]
        public int Information { get; init; }
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

        var information = context.Informations;
        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(p => new Row
            {
                Name = p.Name,
                Client = p.Client.Name,
                Owner = p.ProjectOwner.Name,
                Information = information.Count(i => i.ProjectId == p.Id),
            })
            .ToListAsync(cancellationToken);

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.ClientId is not null
            || query.PersonId is not null;

        return Spreadsheet.File(rows, "Projects.Export", filtered);
    }
}
