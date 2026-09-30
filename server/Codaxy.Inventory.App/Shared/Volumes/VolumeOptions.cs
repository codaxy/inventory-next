using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Volumes;

/// <summary>A volume as it is named: its license and the license's inventory number, then its designator (<see cref="VolumeNames.Designator"/>).</summary>
public sealed record VolumeOption(Guid Id, string License, int? LicenseNumber, string Designator);

/// <inheritdoc cref="VolumeOption"/>
public sealed record VolumeRef(
    Guid Id,
    string License,
    int? LicenseNumber,
    string Designator,
    Guid LicenseId
);

/// <summary>Every volume, named as the activations name one, for a picker of the volume something is bought under.</summary>
public static class VolumeOptions
{
    public static Task<List<VolumeOption>> AllAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        context
            .Volumes.AsNoTracking()
            .OrderBy(v => v.SoftwareOrService.Name)
            .ThenBy(v => v.License.Asset.Name)
            .Select(v => new VolumeOption(
                v.Id,
                v.License.Asset.Name,
                v.License.Asset.InventoryNumber,
                VolumeNames.Designator(v.Description, v.VolumeType.Text)
            ))
            .ToListAsync(cancellationToken);

    public static Task<VolumeRef?> RefAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .Volumes.AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VolumeRef(
                v.Id,
                v.License.Asset.Name,
                v.License.Asset.InventoryNumber,
                VolumeNames.Designator(v.Description, v.VolumeType.Text),
                v.LicenseId
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
