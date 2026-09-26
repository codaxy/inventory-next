using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Assets;

/// <summary>What every path that writes an asset does the same way: the number, the type, the time.</summary>
public static partial class AssetWrites
{
    /// <summary>
    /// The next inventory number, as the original allocates it: read the single row, take its value,
    /// write it back incremented. Two creates at once can take the same number; the unique index on
    /// the number is what turns that into a failed save rather than a duplicate.
    /// </summary>
    public static async Task<int> TakeNumberAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var sequence =
            await context.Sequences.FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("The Sequence table has no row.");

        var number = sequence.AssetInventoryNumber;
        sequence.AssetInventoryNumber = number + 1;
        return number;
    }

    /// <summary>The asset type by its seeded name — part of the contract, not free text.</summary>
    public static async Task<Guid> AssetTypeAsync(
        InventoryContext context,
        string name,
        CancellationToken cancellationToken
    ) =>
        await context
            .AssetTypes.Where(t => t.Name == name)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException($"No asset type is named \"{name}\".");

    /// <summary>
    /// Now, to the microsecond PostgreSQL keeps: a value with finer ticks would come back different
    /// from what was stored, and a client echoing it for a concurrency check would never match.
    /// </summary>
    public static DateTimeOffset Now(TimeProvider clock)
    {
        var now = clock.GetUtcNow();
        return now.AddTicks(-(now.Ticks % 10));
    }

    /// <summary>
    /// Importance from the three weights, as the original's editors computed it: 3–4 Low, 5–7 Medium,
    /// 8–9 High, and none unless all three are chosen.
    /// </summary>
    public static async Task<Guid?> ImportanceAsync(
        InventoryContext context,
        Guid? confidentiality,
        Guid? integrity,
        Guid? availability,
        CancellationToken cancellationToken
    )
    {
        if (confidentiality is null || integrity is null || availability is null)
            return null;

        var weight =
            await context
                .Confidentialities.Where(c => c.Id == confidentiality)
                .Select(c => c.Weight)
                .FirstAsync(cancellationToken)
            + await context
                .Integrities.Where(i => i.Id == integrity)
                .Select(i => i.Weight)
                .FirstAsync(cancellationToken)
            + await context
                .Availabilities.Where(a => a.Id == availability)
                .Select(a => a.Weight)
                .FirstAsync(cancellationToken);

        var level = LevelFor(weight);

        return await context
            .Importances.Where(i => i.Level == level)
            .Select(i => (Guid?)i.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static string LevelFor(int weight) =>
        weight switch
        {
            <= 4 => "Low",
            <= 7 => "Medium",
            _ => "High",
        };
}
