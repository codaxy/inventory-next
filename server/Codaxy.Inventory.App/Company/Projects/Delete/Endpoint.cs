using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Projects.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapDelete("/{id:guid}", Handle);

    /// <summary>A project no information is of goes; the information's foreign key refuses otherwise.</summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var project = await context.Projects.FirstOrDefaultAsync(
            p => p.Id == id,
            cancellationToken
        );
        if (project is null)
            return Results.NotFound();

        var information = await context.Informations.CountAsync(
            i => i.ProjectId == id,
            cancellationToken
        );
        if (information > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: information == 1
                    ? "A piece of information is of this project, so it cannot be deleted."
                    : $"{information} pieces of information are of this project, so it cannot be deleted."
            );

        context.Projects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
