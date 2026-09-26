using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses.Update;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapPut("/{id:guid}", Handle);

    /// <summary>
    /// Every field but the number, and the volumes reconciled, in one save. The body carries the
    /// last-modified time it was loaded with; one that no longer matches means someone saved since,
    /// and their edit is not overwritten.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        [FromBody] LicenseForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (form.LastModified is null)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["lastModified"] = ["Send the last-modified time the licence was loaded with."],
                }
            );

        var license = await context
            .Licenses.Include(l => l.Asset)
            .Include(l => l.Volumes)
            .FirstOrDefaultAsync(l => l.AssetId == id, cancellationToken);

        if (license is null)
            return Results.NotFound();

        if (license.Asset.LastModified != form.LastModified)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Someone changed this licence since you opened it."
            );

        if (await LicenseWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        if (
            await LicenseWrites.ReconcileVolumesAsync(context, license, form, cancellationToken) is
            { } held
        )
            return held;

        await LicenseWrites.ApplyAsync(
            context,
            license.Asset,
            license,
            form,
            clock,
            cancellationToken
        );
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(
            await LicenseWrites.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
        );
    }
}
