using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Tags.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapDelete("/{id:guid}", Handle);

    /// <summary>A tag goes with its links, as a device tag does: the information keeps everything but the tag. The links have no id, so the audit log records only the tag.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var entity = await context.InformationTags.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
        if (entity is null)
            return Results.NotFound();

        context.InformationTags.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
