using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// A person nothing is attached to goes. Anything attached refuses it — not only what the database
    /// would refuse: the asset, information and project foreign keys cascade, so an unguarded delete
    /// takes every asset the person holds with it.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var person = await context.Persons.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (person is null)
            return Results.NotFound();

        var held = new[]
        {
            Count(
                await context.Assets.CountAsync(a => a.PersonId == id, cancellationToken),
                "asset",
                "assets"
            ),
            Count(
                await context.Activations.CountAsync(a => a.PersonId == id, cancellationToken),
                "seat",
                "seats"
            ),
            Count(
                await context.Informations.CountAsync(i => i.PersonId == id, cancellationToken),
                "piece of information",
                "pieces of information"
            ),
            Count(
                await context.Projects.CountAsync(p => p.ProjectOwnerId == id, cancellationToken),
                "project",
                "projects"
            ),
        }
            .OfType<string>()
            .ToList();

        if (held.Count > 0)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"This person holds {Join(held)}, so they cannot be deleted."
            );

        context.Persons.Remove(person);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static string? Count(int n, string one, string many) =>
        n switch
        {
            0 => null,
            1 => $"1 {one}",
            _ => $"{n} {many}",
        };

    /// <summary>"3 assets, 2 seats and 1 project".</summary>
    private static string Join(List<string> parts) =>
        parts.Count == 1 ? parts[0] : $"{string.Join(", ", parts[..^1])} and {parts[^1]}";
}
