using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Volumes;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) => activations.MapGet("/volumes", Handle);

    /// <param name="InUse">Seats taken by activations still active.</param>
    public sealed record Volume(
        Guid Id,
        Guid LicenseId,
        string License,
        int? LicenseNumber,
        string Type,
        int TypeId,
        int Quantity,
        int InUse,
        string? Description
    );

    /// <summary>The volumes of one software or service, each with its seats in use: what the form picks from.</summary>
    private static async Task<IResult> Handle(
        Guid? softwareId,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (softwareId is null)
            return Activations.Problem("softwareId", "Name the software or service.");

        return Results.Ok(
            await context
                .Volumes.AsNoTracking()
                .Where(v => v.SoftwareOrServiceId == softwareId)
                .OrderBy(v => v.License.Asset.Name)
                .ThenBy(v => v.Id)
                .Select(v => new Volume(
                    v.Id,
                    v.LicenseId,
                    v.License.Asset.Name,
                    v.License.Asset.InventoryNumber,
                    v.VolumeType.Text,
                    v.VolumeTypeId,
                    v.Quantity,
                    v.Activations.Where(a => a.DeactivationDate == null).Sum(a => a.Quantity),
                    v.Description
                ))
                .ToListAsync(cancellationToken)
        );
    }
}
