using Codaxy.CodeReports.CodeModel;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Microsoft.EntityFrameworkCore;
using Query = Codaxy.Inventory.App.Informations.Items.List.Endpoint.Query;

namespace Codaxy.Inventory.App.Informations.Items.Export;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder information) => information.MapGet("/export", Handle);

    /// <summary>A piece of information as the original's spreadsheet has it; the headers are its own.</summary>
    public sealed class Row
    {
        [TableColumn(HeaderText = "Name     ")]
        public string Name { get; init; } = "";

        [TableColumn(HeaderText = "Type     ")]
        public string Type { get; init; } = "";

        [TableColumn(HeaderText = "Assignee   ")]
        public string Assignee { get; init; } = "";

        [TableColumn(HeaderText = "Author     ")]
        public string? Author { get; init; }

        [TableColumn(HeaderText = "Project     ")]
        public string? Project { get; init; }
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

        var rows = await List
            .Endpoint.Rows(context, query)
            .Select(i => new Row
            {
                Name = i.Name,
                Type = i.InformationType.Name,
                Assignee = i.Person.Name,
                Author = i.Author,
                Project = i.ProjectId == null ? null : i.Project.Name,
            })
            .ToListAsync(cancellationToken);

        var filtered =
            !string.IsNullOrWhiteSpace(query.Q)
            || query.TypeId is not null
            || query.PersonId is not null
            || query.ProjectId is not null
            || query.TagId is not null
            || query.LocationId is not null
            || query.VirtualMachineId is not null
            || query.CloudSubscriptionId is not null
            || query.SoftwareId is not null
            || query.Incomplete is not null;

        return Excel.File(rows, "Information.Export", filtered);
    }
}
