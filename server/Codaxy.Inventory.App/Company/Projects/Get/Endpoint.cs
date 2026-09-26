using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Projects.Get;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapGet("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        await ProjectWrites.DetailAsync(context, id, cancellationToken) is { } project
            ? Results.Ok(project)
            : Results.NotFound();
}
