using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Clients.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder clients) => clients.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A client without projects goes. One with any is refused, not only where the database would
    /// refuse: the project's foreign key cascades, so an unguarded delete takes the projects with it.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (client is null)
            return Results.NotFound();

        var projects = await context.Projects.CountAsync(p => p.ClientId == id, cancellationToken);
        if (projects > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: projects == 1
                    ? "A project is of this client, so it cannot be deleted."
                    : $"{projects} projects are of this client, so it cannot be deleted."
            );

        context.Clients.Remove(client);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
