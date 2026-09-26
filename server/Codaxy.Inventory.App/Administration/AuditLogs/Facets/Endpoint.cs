using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.AuditLogs.Facets;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auditLog) => auditLog.MapGet("/facets", Handle);

    /// <summary>What the filter pickers offer: only values the log actually holds.</summary>
    public sealed record Response(IReadOnlyList<string> Tables, IReadOnlyList<string> Emails);

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .AuditLogs.Select(a => a.Table)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(cancellationToken),
            await context
                .AuditLogs.Select(a => a.Email)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync(cancellationToken)
        );
}
