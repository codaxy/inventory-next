using System.Text.Json;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Settings.List;

/// <summary>Every setting, in the catalogue's order, with its default beside its value.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder settings) => settings.MapGet("", Handle);

    /// <param name="Value">As stored, or the default where the stored value cannot be used.</param>
    public sealed record Item(string Key, string Group, JsonElement Value, JsonElement Default);

    public sealed record Response(IReadOnlyList<Item> Settings);

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) => await ReadAsync(context, cancellationToken);

    public static async Task<Response> ReadAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var stored = await context
            .Settings.AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return new(
            SettingCatalogue
                .All.Select(d =>
                {
                    var json =
                        stored.TryGetValue(d.Key, out var value) && d.StoredProblem(value) is null
                            ? value
                            : d.DefaultJson;

                    return new Item(
                        d.Key,
                        d.Group,
                        JsonDocument.Parse(json).RootElement,
                        JsonDocument.Parse(d.DefaultJson).RootElement
                    );
                })
                .ToList()
        );
    }
}
