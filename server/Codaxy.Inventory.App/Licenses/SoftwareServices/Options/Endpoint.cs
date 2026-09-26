using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    /// <summary>What the editor's pickers and the list's filters offer, by name.</summary>
    public sealed record Response(
        IReadOnlyList<Option> Categories,
        IReadOnlyList<Option> Manufacturers
    );

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .SoftwareOrServiceCategories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new Option(c.Id, c.Name))
                .ToListAsync(cancellationToken),
            await context
                .Manufacturers.AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(m => new Option(m.Id, m.Name))
                .ToListAsync(cancellationToken)
        );
}
