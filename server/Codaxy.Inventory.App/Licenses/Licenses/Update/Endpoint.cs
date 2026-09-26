using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
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
        // The licence's own attributes and the asset's rules, answered together.
        if (AssetWrites.Validate(form, MiniValidator.Errors(form)) is { Count: > 0 } errors)
            return Results.ValidationProblem(errors);

        var license = await context
            .Licenses.Include(l => l.Asset)
            .Include(l => l.Volumes)
            .FirstOrDefaultAsync(l => l.AssetId == id, cancellationToken);

        if (license is null)
            return Results.NotFound();

        if (AssetWrites.CheckUnchanged(license.Asset, form, "licence") is { } changed)
            return changed;

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
