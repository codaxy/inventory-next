using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.AuditLogs.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auditLog) => auditLog.MapGet("/", Handle);

    /// <param name="Q">Free text: every whitespace-separated term must match somewhere.</param>
    /// <param name="From">Inclusive.</param>
    /// <param name="To">Exclusive, so consecutive ranges neither overlap nor leave a gap.</param>
    /// <param name="Sort"><c>-time</c>, newest first, or <c>time</c>.</param>
    public sealed record Query(
        string? Q,
        string? Action,
        string? Table,
        string? Email,
        DateTimeOffset? From,
        DateTimeOffset? To,
        Guid? EntityId,
        int? InventoryNumber,
        string? Sort,
        int? Page,
        int? PageSize
    );

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "time" or "-time"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by time or -time."] }
            );

        var rows = context.AuditLogs.AsNoTracking();

        foreach (var term in Terms(query.Q))
        {
            var pattern = $"%{EscapeLike(term)}%";

            rows = rows.Where(a =>
                EF.Functions.ILike(a.Email, pattern, @"\")
                || EF.Functions.ILike(a.Table, pattern, @"\")
                || EF.Functions.ILike(a.EntityId.ToString(), pattern, @"\")
                || EF.Functions.ILike(a.NewValuesJson, pattern, @"\")
                || EF.Functions.ILike(a.OldValuesJson, pattern, @"\")
            );
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
            rows = rows.Where(a => a.ActionType == query.Action);

        if (!string.IsNullOrWhiteSpace(query.Table))
            rows = rows.Where(a => a.Table == query.Table);

        if (!string.IsNullOrWhiteSpace(query.Email))
            rows = rows.Where(a => a.Email == query.Email);

        if (query.From is { } from)
            rows = rows.Where(a => a.TimeCreated >= from);

        if (query.To is { } to)
            rows = rows.Where(a => a.TimeCreated < to);

        if (query.EntityId is { } entityId)
            rows = rows.Where(a => a.EntityId == entityId);

        if (query.InventoryNumber is { } number)
        {
            var ids = await AssetIdsAsync(context, number, cancellationToken);
            rows = rows.Where(a => ids.Contains(a.EntityId));
        }

        // Id breaks ties: rows of one save can share a timestamp, and without a unique last key
        // they could appear on two pages or on none.
        var ordered =
            query.Sort == "time"
                ? rows.OrderBy(a => a.TimeCreated).ThenBy(a => a.Id)
                : rows.OrderByDescending(a => a.TimeCreated).ThenByDescending(a => a.Id);

        var page = await ordered.ToPageAsync(window, cancellationToken);

        return Results.Ok(
            new Page<Entry>(
                await Entries.ShapeAsync(context, page.Items, cancellationToken),
                page.Total
            )
        );
    }

    private static IEnumerable<string> Terms(string? q) =>
        (q ?? "").Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

    private static string EscapeLike(string term) =>
        term.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    /// <summary>
    /// The asset holding the number and the subtype rows that share its id. The logged values are
    /// read as well as the table, so an asset deleted since is still found by the number it had.
    /// </summary>
    private static async Task<List<Guid>> AssetIdsAsync(
        InventoryContext context,
        int number,
        CancellationToken cancellationToken
    )
    {
        var text = number.ToString(System.Globalization.CultureInfo.InvariantCulture);

        var logged = await context
            .Database.SqlQuery<Guid>(
                $"""
                SELECT entity_id AS "Value" FROM audit_log
                WHERE "table" = {Entries.AssetTable}
                  AND (new_values_json::jsonb ->> 'InventoryNumber' = {text}
                    OR old_values_json::jsonb ->> 'InventoryNumber' = {text})
                """
            )
            .ToListAsync(cancellationToken);

        var current = await context
            .Assets.Where(a => a.InventoryNumber == number)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        return [.. logged.Concat(current).Distinct()];
    }
}
