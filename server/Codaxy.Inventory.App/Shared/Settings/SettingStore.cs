using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Settings;

/// <summary>
/// The settings as stored, read from the database for each request that asks — uncached, so a change
/// applies on the next request.
/// </summary>
public sealed class SettingStore(InventoryContext context, ILogger<SettingStore> logger)
{
    public async Task<SettingValues> LoadAsync(CancellationToken cancellationToken) =>
        new(
            await context
                .Settings.AsNoTracking()
                .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken),
            logger
        );
}

/// <summary>One read of every setting.</summary>
public sealed class SettingValues(IReadOnlyDictionary<string, string> stored, ILogger logger)
{
    /// <summary>
    /// The stored value, or the default where there is none or it cannot be used — an edit by hand
    /// is logged, never a stopped request.
    /// </summary>
    public T Get<T>(SettingDefinition<T> definition)
    {
        if (!stored.TryGetValue(definition.Key, out var json))
            return definition.Default;

        var (value, problem) = definition.Read(json);
        if (problem is not null)
            logger.LogWarning(
                "Setting {Key} holds {Value}, read as its default: {Problem}",
                definition.Key,
                json,
                problem
            );

        return value;
    }
}
