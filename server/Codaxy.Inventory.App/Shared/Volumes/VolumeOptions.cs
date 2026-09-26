using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Volumes;

/// <param name="Text">"Rider · JetBrains All Products · Per user": the software, the license, and the volume's description or type.</param>
public sealed record VolumeRef(Guid Id, string Text, Guid LicenseId, string License);

/// <summary>Every volume, named as the activations name one, for a picker of the volume something is bought under.</summary>
public static class VolumeOptions
{
    public static Task<List<AssetOption>> AllAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        context
            .Volumes.AsNoTracking()
            .OrderBy(v => v.SoftwareOrService.Name)
            .ThenBy(v => v.License.Asset.Name)
            .Select(v => new AssetOption(
                v.Id,
                v.SoftwareOrService.Name
                    + " · "
                    + v.License.Asset.Name
                    + " · "
                    + (
                        v.Description != null && v.Description != ""
                            ? v.Description
                            : v.VolumeType.Text
                    )
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
                v.SoftwareOrService.Name
                    + " · "
                    + v.License.Asset.Name
                    + " · "
                    + (
                        v.Description != null && v.Description != ""
                            ? v.Description
                            : v.VolumeType.Text
                    ),
                v.LicenseId,
                v.License.Asset.Name
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
