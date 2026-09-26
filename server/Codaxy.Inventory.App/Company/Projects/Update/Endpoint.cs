using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Projects.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder projects) => projects.MapPut("/{id:guid}", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] ProjectForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var project = await context.Projects.FirstOrDefaultAsync(
            p => p.Id == id,
            cancellationToken
        );
        if (project is null)
            return Results.NotFound();

        if (await ProjectWrites.CheckAsync(context, form, id, cancellationToken) is { } refused)
            return refused;

        ProjectWrites.Apply(project, form);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(await ProjectWrites.DetailAsync(context, id, cancellationToken));
    }
}
