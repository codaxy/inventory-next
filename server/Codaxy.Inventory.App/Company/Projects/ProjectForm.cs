using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Projects;

/// <summary>What creating and editing a project take.</summary>
public sealed record ProjectForm(
    [property:
        Required(ErrorMessage = "Give the project a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose the client.")] Guid? ClientId,
    [property: Required(ErrorMessage = "Choose who leads it.")] Guid? OwnerId
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>A project as its page shows it: its client, its owner, and the information of it.</summary>
public sealed record ProjectDetail(
    Guid Id,
    string Name,
    AssetRef Client,
    AssetRef Owner,
    Section<InformationRow> Information
);

internal static class ProjectWrites
{
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        ProjectForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var errors = new Dictionary<string, string[]>();
        if (!await context.Clients.AnyAsync(c => c.Id == form.ClientId, cancellationToken))
            errors["clientId"] = ["That choice no longer exists."];
        if (!await context.Persons.AnyAsync(p => p.Id == form.OwnerId, cancellationToken))
            errors["ownerId"] = ["That choice no longer exists."];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        return await Unique.CheckAsync(
            context.Projects.Where(p => p.Id != except),
            p => p.Name,
            form.Name!,
            "name",
            "A project with this name already exists.",
            cancellationToken
        );
    }

    public static void Apply(Project project, ProjectForm form)
    {
        project.Name = form.Name!.Trim();
        project.ClientId = form.ClientId!.Value;
        project.ProjectOwnerId = form.OwnerId!.Value;
    }

    public static async Task<ProjectDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var project = await context
            .Projects.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Name,
                Client = new AssetRef(p.ClientId, p.Client.Name),
                Owner = new AssetRef(p.ProjectOwnerId, p.ProjectOwner.Name),
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
            return null;

        return new ProjectDetail(
            id,
            project.Name,
            project.Client,
            project.Owner,
            await AssetHoldings.SectionAsync(
                context.Informations.AsNoTracking().Where(i => i.ProjectId == id),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
