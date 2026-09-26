using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder entries) => entries.MapPost("/", Handle);

    private static async Task<IResult> Handle(
        [FromBody] SoftwareOrServiceForm form,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (
            await SoftwareServices.CheckAsync(context, form, null, cancellationToken) is { } refused
        )
            return refused;

        var entry = new SoftwareOrService { Id = Guid.CreateVersion7() };
        SoftwareServices.Apply(entry, form);
        context.SoftwareOrServices.Add(entry);

        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/licenses/software-services/{entry.Id}",
            await SoftwareServices.DetailAsync(context, entry.Id, cancellationToken)
        );
    }
}
