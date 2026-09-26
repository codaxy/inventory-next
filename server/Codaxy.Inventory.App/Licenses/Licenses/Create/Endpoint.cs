using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Codaxy.Inventory.App.Licenses.Licenses.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapPost("/", Handle);

    /// <summary>The asset, its licence row and the volumes, numbered from the sequence, in one save.</summary>
    private static async Task<IResult> Handle(
        [FromBody] LicenseForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        if (form.Volumes?.Any(v => v.Id is not null) == true)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["volumes"] = ["A new licence has only new volumes."],
                }
            );

        if (await LicenseWrites.CheckAsync(context, form, cancellationToken) is { } refused)
            return refused;

        var id = Guid.CreateVersion7();
        var asset = new Asset
        {
            Id = id,
            InventoryNumber = await AssetWrites.TakeNumberAsync(context, cancellationToken),
            AssetTypeId = await AssetWrites.AssetTypeAsync(
                context,
                LicenseWrites.AssetType,
                cancellationToken
            ),
        };
        var license = new License { AssetId = id, Asset = asset };

        await LicenseWrites.ApplyAsync(context, asset, license, form, clock, cancellationToken);
        await LicenseWrites.ReconcileVolumesAsync(context, license, form, cancellationToken);

        context.Assets.Add(asset);
        context.Licenses.Add(license);
        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/licenses/{id}",
            await LicenseWrites.DetailAsync(context, id, Expiry.Today(clock), cancellationToken)
        );
    }
}
