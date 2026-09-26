using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    /// <summary>What the editor's pickers offer: every type, by name.</summary>
    public sealed record Response(IReadOnlyList<Option> Types);

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .ElectronicDeviceTypes.AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new Option(t.Id, t.Name))
                .ToListAsync(cancellationToken)
        );
}
