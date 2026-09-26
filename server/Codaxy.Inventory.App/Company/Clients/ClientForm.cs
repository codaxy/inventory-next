using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Clients;

/// <summary>What creating and editing a client take.</summary>
public sealed record ClientForm(
    [property:
        Required(ErrorMessage = "Give the client a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name
);

/// <param name="Owner">The person who leads it.</param>
public sealed record ProjectRow(Guid Id, string Name, string? Owner);

/// <summary>A client as its page shows it: its projects' total and the first of them.</summary>
public sealed record ClientDetail(
    Guid Id,
    string Name,
    int ProjectCount,
    IReadOnlyList<ProjectRow> Projects
);

internal static class Clients
{
    /// <summary>What the page shows of the client's projects before "see all".</summary>
    public const int First = 10;

    /// <summary>
    /// No other client holds the name, whatever its case — the original refused a duplicate. Both
    /// sides trimmed: the original saved values with stray spaces. Checked here, not by an index: the
    /// schema is frozen, so two saves at once can still both pass.
    /// </summary>
    public static async Task<IResult?> CheckNameAsync(
        InventoryContext context,
        string name,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var lower = name.Trim().ToLowerInvariant();
        var taken = await context.Clients.AnyAsync(
            c => c.Id != except && c.Name.Trim().ToLower() == lower,
            cancellationToken
        );

        return taken
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] = ["A client with this name already exists."],
                }
            )
            : null;
    }

    public static void Apply(Client client, ClientForm form) => client.Name = form.Name!.Trim();

    public static async Task<ClientDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var name = await context
            .Clients.Where(c => c.Id == id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null)
            return null;

        var projects = context.Projects.AsNoTracking().Where(p => p.ClientId == id);
        return new ClientDetail(
            id,
            name,
            await projects.CountAsync(cancellationToken),
            await projects
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .Take(First)
                .Select(p => new ProjectRow(p.Id, p.Name, p.ProjectOwner.Name))
                .ToListAsync(cancellationToken)
        );
    }
}
