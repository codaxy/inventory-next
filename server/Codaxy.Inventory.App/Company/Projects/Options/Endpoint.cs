using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Projects.Options;

/// <summary>The clients and people a project's form and the list's filters pick from.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapGet("/options", Handle);

    public sealed record Response(
        IReadOnlyList<AssetOption> Clients,
        IReadOnlyList<AssetOption> People
    );

    private static async Task<IResult> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        Results.Ok(
            new Response(
                await context
                    .Clients.AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new AssetOption(c.Id, c.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .Persons.AsNoTracking()
                    .OrderBy(p => p.Name)
                    .Select(p => new AssetOption(p.Id, p.Name))
                    .ToListAsync(cancellationToken)
            )
        );
}
