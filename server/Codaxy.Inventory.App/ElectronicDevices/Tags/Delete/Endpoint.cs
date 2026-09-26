using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder tags) => tags.MapDelete("/{id:guid}", Handle);

    /// <summary>The tag goes; the database drops the links that named it.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var tag = await context.ElectronicDeviceTags.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );

        if (tag is null)
            return Results.NotFound();

        context.ElectronicDeviceTags.Remove(tag);
        await context.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
