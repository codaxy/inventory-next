using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Company.Projects.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] ProjectForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (await ProjectWrites.CheckAsync(context, form, null, cancellationToken) is { } refused)
            return refused;

        var project = new Project { Id = Guid.CreateVersion7() };
        ProjectWrites.Apply(project, form);
        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/company/projects/{project.Id}",
            await ProjectWrites.DetailAsync(context, project.Id, cancellationToken)
        );
    }
}
