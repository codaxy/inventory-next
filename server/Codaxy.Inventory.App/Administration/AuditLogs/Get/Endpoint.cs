using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Codaxy.Inventory.App.Administration.AuditLogs.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auditLog) => auditLog.MapGet("/{id:guid}", Handle);

    /// <param name="References">
    /// Display text for the values of foreign-key fields, by field and then by value. A value the
    /// target no longer holds is absent.
    /// </param>
    /// <param name="Related">The other rows written by the same save.</param>
    public sealed record Response(
        Entry Entry,
        IReadOnlyList<FieldChange> Fields,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> References,
        IReadOnlyList<Entry> Related
    );

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var row = await context
            .AuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (row is null)
            return Results.NotFound();

        var fields = AuditValues.Compare(
            AuditValues.Parse(row.OldValuesJson),
            AuditValues.Parse(row.NewValuesJson)
        );

        var related = row.TransactionId is { } transaction
            ? await context
                .AuditLogs.AsNoTracking()
                .Where(a => a.TransactionId == transaction && a.Id != row.Id)
                .OrderBy(a => a.TimeCreated)
                .ThenBy(a => a.Id)
                .ToListAsync(cancellationToken)
            : [];

        var entries = await Entries.ShapeAsync(context, [row, .. related], cancellationToken);

        return Results.Ok(
            new Response(
                entries[0],
                fields,
                await ReferencesAsync(context, row.Table, fields, cancellationToken),
                entries.Skip(1).ToList()
            )
        );
    }

    /// <summary>
    /// The first of these a key's target has is what names it. Codebooks call it <c>Text</c>,
    /// classification levels <c>Level</c>, statuses <c>Status</c>.
    /// </summary>
    private static readonly string[] DisplayProperties =
    [
        "Name",
        "Text",
        "Level",
        "Status",
        "Substatus",
        "Description",
    ];

    /// <summary>
    /// Names the targets of the entry's foreign keys, found through the EF model rather than a map per
    /// entity: any entity the log records is covered, and a key added later with it.
    /// </summary>
    private static async Task<
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    > ReferencesAsync(
        InventoryContext context,
        string table,
        IReadOnlyList<FieldChange> fields,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, IReadOnlyDictionary<string, string>> references = [];

        var entityType = context
            .Model.GetEntityTypes()
            .FirstOrDefault(t => t.ClrType.Name == table);

        if (entityType is null)
            return references;

        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            if (foreignKey.Properties is not [var property])
                continue;

            var field = fields.FirstOrDefault(f => f.Name == property.Name);
            var values = new[] { field?.Old?.Text, field?.New?.Text }
                .OfType<string>()
                .Distinct()
                .ToArray();

            if (values.Length == 0 || Display(foreignKey.PrincipalEntityType) is not { } display)
                continue;

            var names = await LookUpAsync(
                context,
                foreignKey.PrincipalEntityType,
                foreignKey.PrincipalKey.Properties[0],
                display,
                values,
                cancellationToken
            );

            if (names.Count > 0)
                references[property.Name] = names;
        }

        return references;
    }

    private static IProperty? Display(IEntityType entityType) =>
        DisplayProperties
            .Select(entityType.FindProperty)
            .FirstOrDefault(p => p?.ClrType == typeof(string));

    private sealed record Named(string Key, string? Label);

    private static async Task<IReadOnlyDictionary<string, string>> LookUpAsync(
        InventoryContext context,
        IEntityType principal,
        IProperty key,
        IProperty display,
        string[] values,
        CancellationToken cancellationToken
    )
    {
        var identifier = StoreObjectIdentifier.Table(
            principal.GetTableName()!,
            principal.GetSchema()
        );

        // Identifiers come from the model, never from the request; the values are a parameter. The
        // key is compared as text because keys are GUIDs, integers and codes alike.
        var sql = $"""
            SELECT {Quote(key.GetColumnName(identifier)!)}::text AS "Key",
                   {Quote(display.GetColumnName(identifier)!)} AS "Label"
            FROM {Quote(identifier.Name)}
            WHERE {Quote(key.GetColumnName(identifier)!)}::text = ANY(@values)
            """;

        var rows = await context
            .Database.SqlQueryRaw<Named>(sql, new NpgsqlParameter("values", values))
            .ToListAsync(cancellationToken);

        return rows.Where(r => !string.IsNullOrWhiteSpace(r.Label))
            .ToDictionary(r => r.Key, r => r.Label!, StringComparer.OrdinalIgnoreCase);
    }

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
