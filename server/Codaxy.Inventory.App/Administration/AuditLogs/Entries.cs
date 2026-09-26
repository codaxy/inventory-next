using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.AuditLogs;

/// <summary>One logged change as a list shows it.</summary>
public sealed record Entry(
    Guid Id,
    DateTimeOffset Time,
    string Email,
    string Action,
    string Table,
    Guid EntityId,
    string? Label,
    int? InventoryNumber,
    IReadOnlyList<string> Changed
);

internal static class Entries
{
    public const string AssetTable = "Asset";

    /// <summary>
    /// Shapes rows for a list. The label is the row's own <c>Name</c>; an asset's subtype row — a
    /// device, a licence, furniture — has none, and shares its asset's id, so it takes the asset's
    /// name and number: from the table while the asset exists, from its last logged values once it
    /// does not.
    /// </summary>
    public static async Task<IReadOnlyList<Entry>> ShapeAsync(
        InventoryContext context,
        IReadOnlyList<AuditLog> rows,
        CancellationToken cancellationToken
    )
    {
        var parsed = rows.Select(row =>
                (
                    Row: row,
                    Old: AuditValues.Parse(row.OldValuesJson),
                    New: AuditValues.Parse(row.NewValuesJson)
                )
            )
            .ToList();

        var subtypeIds = parsed
            .Where(p => p.Row.Table != AssetTable && (p.New.Has("AssetId") || p.Old.Has("AssetId")))
            .Select(p => p.Row.EntityId)
            .Distinct()
            .ToList();

        var assets = await AssetLabelsAsync(context, subtypeIds, cancellationToken);

        return
        [
            .. parsed.Select(p =>
            {
                var values = p.New == AuditValues.Empty ? p.Old : p.New;
                var asset = assets.GetValueOrDefault(p.Row.EntityId);

                return new Entry(
                    p.Row.Id,
                    p.Row.TimeCreated,
                    p.Row.Email,
                    p.Row.ActionType,
                    p.Row.Table,
                    p.Row.EntityId,
                    values.GetString("Name") ?? asset.Name,
                    values.GetInt("InventoryNumber") ?? asset.InventoryNumber,
                    AuditValues.ChangedNames(p.Old, p.New)
                );
            }),
        ];
    }

    private static async Task<
        Dictionary<Guid, (string? Name, int? InventoryNumber)>
    > AssetLabelsAsync(
        InventoryContext context,
        List<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        if (ids.Count == 0)
            return [];

        var labels = (
            await context
                .Assets.Where(a => ids.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.InventoryNumber,
                })
                .ToListAsync(cancellationToken)
        ).ToDictionary(a => a.Id, a => ((string?)a.Name, a.InventoryNumber));

        var deleted = ids.Where(id => !labels.ContainsKey(id)).ToList();

        if (deleted.Count == 0)
            return labels;

        var logged = await context
            .AuditLogs.Where(a => a.Table == AssetTable && deleted.Contains(a.EntityId))
            .OrderByDescending(a => a.TimeCreated)
            .Select(a => new { a.EntityId, Json = a.NewValuesJson ?? a.OldValuesJson })
            .ToListAsync(cancellationToken);

        foreach (var row in logged.DistinctBy(r => r.EntityId))
        {
            var values = AuditValues.Parse(row.Json);
            labels[row.EntityId] = (values.GetString("Name"), values.GetInt("InventoryNumber"));
        }

        return labels;
    }
}
