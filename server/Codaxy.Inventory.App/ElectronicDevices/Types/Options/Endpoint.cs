using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Types.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder types) => types.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    /// <summary>What the editor's picker and the list's filter offer: every tag, by name.</summary>
    public sealed record Response(IReadOnlyList<Option> Tags);

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .ElectronicDeviceTags.AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new Option(t.Id, t.Name))
                .ToListAsync(cancellationToken)
        );
}
