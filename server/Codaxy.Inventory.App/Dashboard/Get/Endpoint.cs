using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Dashboard.Get;

/// <summary>
/// What the dashboard counts, in one request: subscriptions and warranties ended lately or ending,
/// seats over or under what was bought, seats left on disposed devices, records left incomplete.
/// Each is a total and its first rows, most pressing first.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder dashboard) => dashboard.MapGet("/", Handle);

    /// <param name="ActiveSeats">Seats of its volumes still active: live software on a lapsed license.</param>
    public sealed record SubscriptionRow(
        Guid Id,
        int? Number,
        string Name,
        string? Vendor,
        DateOnly ExpirationDate,
        bool AutoRenew,
        int ActiveSeats
    );

    /// <param name="InUse">The quantities of its active activations.</param>
    public sealed record VolumeRow(
        Guid Id,
        Guid LicenseId,
        string License,
        string Software,
        int Quantity,
        int InUse
    );

    public sealed record WarrantyRow(
        Guid Id,
        int? Number,
        string Name,
        string? Holder,
        DateOnly Ends
    );

    public sealed record DisposedSeatRow(
        Guid Id,
        string Software,
        Guid DeviceId,
        string Device,
        int? DeviceNumber,
        string Location
    );

    /// <param name="Kind"><c>device</c>, <c>furniture</c>, <c>license</c> or <c>information</c>.</param>
    public sealed record IncompleteRow(Guid Id, string Kind, int? Number, string Name);

    /// <param name="UnusedSeats">The free seats of every volume in <c>Unused</c>, not only the first.</param>
    /// <param name="DisposedSeats">Absent where no disposal location is configured.</param>
    public sealed record Response(
        Section<SubscriptionRow> Expired,
        Section<SubscriptionRow> ExpiringSoon,
        Section<VolumeRow> OverAllocated,
        Section<VolumeRow> Unused,
        int UnusedSeats,
        Section<WarrantyRow> WarrantiesEnded,
        Section<WarrantyRow> Warranties,
        Section<DisposedSeatRow>? DisposedSeats,
        Section<IncompleteRow> Incomplete,
        int SubscriptionsExpiredDays,
        int SubscriptionsExpiringDays,
        int WarrantiesEndedDays,
        int WarrantiesEndingDays
    );

    private static async Task<IResult> Handle(
        InventoryContext context,
        IOptions<DashboardOptions> options,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        var o = options.Value;
        var today = Expiry.Today(clock);
        var soon = today.AddDays(o.SubscriptionsExpiringDays);

        var licenses = context.Licenses.AsNoTracking();
        var lapsedSince = today.AddDays(-o.SubscriptionsExpiredDays);
        var lapsed = licenses.Where(l =>
            l.SubscriptionExpirationDate < today && l.SubscriptionExpirationDate >= lapsedSince
        );
        var lapsing = licenses.Where(l =>
            l.SubscriptionExpirationDate >= today && l.SubscriptionExpirationDate < soon
        );

        // Seats in use are the quantities of a volume's active activations (domain.md).
        var volumes = context
            .Volumes.AsNoTracking()
            .Select(v => new
            {
                Volume = v,
                InUse = v.Activations.Where(a => a.DeactivationDate == null).Sum(a => a.Quantity),
            });
        var over = volumes.Where(v => v.InUse > v.Volume.Quantity);
        // Free seats on a lapsed subscription cost nothing, so they are not worth freeing.
        var unused = volumes.Where(v =>
            v.InUse < v.Volume.Quantity && !(v.Volume.License.SubscriptionExpirationDate < today)
        );

        var devices = context.ElectronicDevices.AsNoTracking();
        var endedSince = today.AddDays(-o.WarrantiesEndedDays);
        var warrantyEnd = today.AddDays(o.WarrantiesEndingDays);
        var warrantiesEnded = devices.Where(d =>
            d.GuaranteeExpirationDate < today && d.GuaranteeExpirationDate >= endedSince
        );
        var warranties = devices.Where(d =>
            d.GuaranteeExpirationDate >= today && d.GuaranteeExpirationDate < warrantyEnd
        );

        var disposed = o.DisposedLocations;
        var disposedSeats = context
            .Activations.AsNoTracking()
            .Where(a => a.DeactivationDate == null && disposed.Contains(a.Asset.Location.Name));

        var incomplete = context
            .Assets.AsNoTracking()
            .Where(a => a.Incomplete)
            .Select(a => new
            {
                a.Id,
                Kind = a.ElectronicDevice != null ? "device"
                : a.Furniture != null ? "furniture"
                : "license",
                a.InventoryNumber,
                a.Name,
            })
            .Concat(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.Incomplete == true)
                    .Select(i => new
                    {
                        i.Id,
                        Kind = "information",
                        InventoryNumber = (int?)null,
                        i.Name,
                    })
            );

        return Results.Ok(
            new Response(
                await AssetHoldings.SectionAsync(
                    lapsed,
                    q =>
                        Subscriptions(
                            q.OrderByDescending(l =>
                                    l.Volumes.SelectMany(v => v.Activations)
                                        .Any(a => a.DeactivationDate == null)
                                )
                                .ThenByDescending(l => l.SubscriptionExpirationDate)
                                .ThenBy(l => l.AssetId)
                        ),
                    cancellationToken
                ),
                await AssetHoldings.SectionAsync(
                    lapsing,
                    q =>
                        Subscriptions(
                            q.OrderBy(l => l.SubscriptionExpirationDate).ThenBy(l => l.AssetId)
                        ),
                    cancellationToken
                ),
                await AssetHoldings.SectionAsync(
                    over,
                    q =>
                        q.OrderByDescending(v => v.InUse - v.Volume.Quantity)
                            .ThenBy(v => v.Volume.Id)
                            .Select(v => new VolumeRow(
                                v.Volume.Id,
                                v.Volume.LicenseId,
                                v.Volume.License.Asset.Name,
                                v.Volume.SoftwareOrService.Name,
                                v.Volume.Quantity,
                                v.InUse
                            )),
                    cancellationToken
                ),
                await AssetHoldings.SectionAsync(
                    unused,
                    q =>
                        q.OrderByDescending(v => v.Volume.Quantity - v.InUse)
                            .ThenBy(v => v.Volume.Id)
                            .Select(v => new VolumeRow(
                                v.Volume.Id,
                                v.Volume.LicenseId,
                                v.Volume.License.Asset.Name,
                                v.Volume.SoftwareOrService.Name,
                                v.Volume.Quantity,
                                v.InUse
                            )),
                    cancellationToken
                ),
                await unused.SumAsync(v => v.Volume.Quantity - v.InUse, cancellationToken),
                await AssetHoldings.SectionAsync(
                    warrantiesEnded,
                    q =>
                        Warranties(
                            q.OrderByDescending(d => d.GuaranteeExpirationDate)
                                .ThenBy(d => d.AssetId)
                        ),
                    cancellationToken
                ),
                await AssetHoldings.SectionAsync(
                    warranties,
                    q =>
                        Warranties(
                            q.OrderBy(d => d.GuaranteeExpirationDate).ThenBy(d => d.AssetId)
                        ),
                    cancellationToken
                ),
                disposed.Length == 0
                    ? null
                    : await AssetHoldings.SectionAsync(
                        disposedSeats,
                        q =>
                            q.OrderBy(a => a.Asset.Name)
                                .ThenBy(a => a.Id)
                                .Select(a => new DisposedSeatRow(
                                    a.Id,
                                    a.Volume.SoftwareOrService.Name,
                                    a.AssetId!.Value,
                                    a.Asset.Name,
                                    a.Asset.InventoryNumber,
                                    a.Asset.Location.Name
                                )),
                        cancellationToken
                    ),
                await AssetHoldings.SectionAsync(
                    incomplete,
                    q =>
                        q.OrderBy(r => r.Name)
                            .ThenBy(r => r.Id)
                            .Select(r => new IncompleteRow(
                                r.Id,
                                r.Kind,
                                r.InventoryNumber,
                                r.Name
                            )),
                    cancellationToken
                ),
                o.SubscriptionsExpiredDays,
                o.SubscriptionsExpiringDays,
                o.WarrantiesEndedDays,
                o.WarrantiesEndingDays
            )
        );
    }

    private static IQueryable<SubscriptionRow> Subscriptions(IQueryable<License> licenses) =>
        licenses.Select(l => new SubscriptionRow(
            l.AssetId,
            l.Asset.InventoryNumber,
            l.Asset.Name,
            l.Asset.Vendor.Name,
            l.SubscriptionExpirationDate!.Value,
            l.AutoRenew == true,
            l.Volumes.SelectMany(v => v.Activations)
                .Where(a => a.DeactivationDate == null)
                .Sum(a => a.Quantity)
        ));

    private static IQueryable<WarrantyRow> Warranties(IQueryable<ElectronicDevice> devices) =>
        devices.Select(d => new WarrantyRow(
            d.AssetId,
            d.Asset.InventoryNumber,
            d.Asset.Name,
            d.Asset.Person.Name,
            d.GuaranteeExpirationDate!.Value
        ));
}
