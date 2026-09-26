using System.Linq.Expressions;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Assets;

/// <summary>A kind of record attached to another: how many, and the first of them.</summary>
public sealed record Section<T>(int Total, IReadOnlyList<T> Items);

/// <param name="Type">The device's or the furniture's type.</param>
/// <param name="Model">The model it is.</param>
public sealed record AssetRow(Guid Id, int? Number, string Name, string? Type, string? Model);

public sealed record LicenseRow(
    Guid Id,
    int? Number,
    string Name,
    string Vendor,
    DateOnly? ExpirationDate,
    string? Expiry
);

/// <summary>The devices, furniture and licenses among the assets a predicate selects.</summary>
public sealed record AssetSections(
    Section<AssetRow> Devices,
    Section<AssetRow> Furniture,
    Section<LicenseRow> Licenses
);

/// <summary>
/// The assets attached to a record — held by a person, bought from a vendor, kept at a location — as
/// its page shows them: per kind the total and the first rows by name.
/// </summary>
public static class AssetHoldings
{
    public const int First = 10;

    public static async Task<AssetSections> SectionsAsync(
        InventoryContext context,
        Expression<Func<Asset, bool>> attached,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var assets = context.Assets.AsNoTracking().Where(attached);
        var devices = assets.Where(a => a.ElectronicDevice != null);
        var furniture = assets.Where(a => a.Furniture != null);
        var licenses = assets.Where(a => a.License != null);

        var licenseRows = await licenses
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Take(First)
            .Select(a => new
            {
                a.Id,
                a.InventoryNumber,
                a.Name,
                Vendor = a.Vendor.Name,
                a.License.SubscriptionExpirationDate,
            })
            .ToListAsync(cancellationToken);

        return new AssetSections(
            new Section<AssetRow>(
                await devices.CountAsync(cancellationToken),
                await devices
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.Id)
                    .Take(First)
                    .Select(a => new AssetRow(
                        a.Id,
                        a.InventoryNumber,
                        a.Name,
                        a.ElectronicDevice.ElectronicDeviceType.Name,
                        a.ElectronicDevice.ModelName
                    ))
                    .ToListAsync(cancellationToken)
            ),
            new Section<AssetRow>(
                await furniture.CountAsync(cancellationToken),
                await furniture
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.Id)
                    .Take(First)
                    .Select(a => new AssetRow(
                        a.Id,
                        a.InventoryNumber,
                        a.Name,
                        a.Furniture.FurnitureType.Name,
                        a.Furniture.Model
                    ))
                    .ToListAsync(cancellationToken)
            ),
            new Section<LicenseRow>(
                await licenses.CountAsync(cancellationToken),
                licenseRows
                    .Select(l => new LicenseRow(
                        l.Id,
                        l.InventoryNumber,
                        l.Name,
                        l.Vendor,
                        l.SubscriptionExpirationDate,
                        Expiry.Status(l.SubscriptionExpirationDate, today)
                    ))
                    .ToList()
            )
        );
    }

    /// <summary>Any other kind by the same rule: the total of <paramref name="rows"/>, and the first of them as <paramref name="first"/> orders and projects them.</summary>
    public static async Task<Section<T>> SectionAsync<TEntity, T>(
        IQueryable<TEntity> rows,
        Func<IQueryable<TEntity>, IQueryable<T>> first,
        CancellationToken cancellationToken
    ) =>
        new(
            await rows.CountAsync(cancellationToken),
            await first(rows).Take(First).ToListAsync(cancellationToken)
        );
}
