using System.Globalization;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.MyEquipment.Details;

/// <summary>
/// One thing of the signed-in employee's, as much as they need to tell IT or a repair shop about it:
/// the codes that name it, where it is, its warranty, who maintains it and the apps on it; a license's
/// vendor and term; a seat's software, license and device. Never a key, a console address, money or the
/// classification — none is theirs to see. Anything not theirs is not found.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder mine) =>
        mine.MapGet("/items/{kind}/{id:guid}", Handle);

    /// <summary>
    /// A fact as the page shows it: a label and its value, in order; <paramref name="Number"/> the
    /// inventory number of what the value names, which the page mutes beside it; <paramref name="Copy"/>
    /// for a code someone would paste into a ticket; <paramref name="Standing"/>, on the date a
    /// warranty or a subscription expires on, how it stands — <c>ok</c>, <c>soon</c> by the
    /// dashboard's window ahead, <c>ended</c> — for the page to show that date as the card's pill.
    /// </summary>
    public sealed record Fact(
        string Label,
        string Value,
        int? Number = null,
        bool Copy = false,
        string? Standing = null
    );

    /// <summary>An app on the device: a seat, its license and how long its subscription runs.</summary>
    public sealed record App(string Name, string License, int? LicenseNumber, DateOnly? Ends);

    /// <summary>Who to call about a device: a maintenance contract on it.</summary>
    public sealed record Contract(
        string Vendor,
        string? Number,
        string? Contact,
        string? Phone,
        string? Email,
        DateOnly? Ends
    );

    public sealed record Response(
        IReadOnlyList<Fact> Facts,
        IReadOnlyList<Contract> Contracts,
        IReadOnlyList<App> Apps
    );

    private static string Day(DateOnly d) => d.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>The facts that have a value, in the order given.</summary>
    private static List<Fact> Facts(params (string Label, string? Value)[] facts) =>
        [
            .. facts
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .Select(f => new Fact(f.Label, f.Value!)),
        ];

    /// <summary>The codes that name a thing to IT, those it has, each to copy.</summary>
    private static IEnumerable<Fact> Codes(params (string Label, string? Value)[] codes) =>
        codes
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => new Fact(c.Label, c.Value!.Trim(), Copy: true));

    private static async Task<IResult> Handle(
        string kind,
        Guid id,
        UserAccess access,
        InventoryContext context,
        SettingStore settings,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (await access.CurrentPersonAsync(cancellationToken) is not { } person)
            return Results.Forbid();
        var me = person.PersonId;
        var today = Expiry.Today(clock);
        var values = await settings.LoadAsync(cancellationToken);
        var warrantyWindow = values.Get(DashboardSettings.WarrantiesEndingDays);
        var subscriptionWindow = values.Get(DashboardSettings.SubscriptionsExpiringDays);
        // A warranty or a subscription "expires" on its date, and "expired" on it once it is past.
        bool Past(DateOnly? date) => date < today;
        // How it stands, as the card's pill: the expiry fact is marked with it.
        List<Fact> Standing(List<Fact> facts, DateOnly? date, int window)
        {
            var status = date switch
            {
                null => null,
                { } e when e < today => "ended",
                { } e when e < today.AddDays(window) => "soon",
                _ => "ok",
            };
            return
            [
                .. facts.Select(f =>
                    f.Label.Contains(" expire") ? f with { Standing = status } : f
                ),
            ];
        }

        switch (kind)
        {
            case "device":
            {
                var d = await context
                    .ElectronicDevices.AsNoTracking()
                    .Where(x => x.AssetId == id && x.Asset.PersonId == me)
                    .Select(x => new
                    {
                        x.Asset.Name,
                        x.ModelCode,
                        x.Asset.InventoryNumber,
                        x.SerialNumber,
                        Location = x.Asset.Location.Name,
                        x.WarrantyExpirationDate,
                        x.WarrantyNumber,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (d is null)
                    return Results.NotFound();

                var contracts = await context
                    .MaintenanceContracts.AsNoTracking()
                    .Where(c => c.AssetId == id)
                    .OrderBy(c => c.ExpirationDate)
                    .Select(c => new Contract(
                        c.Vendor.Name,
                        c.ContractNumber,
                        c.ContactName,
                        c.ContactNumber,
                        c.ContactEmail,
                        c.ExpirationDate
                    ))
                    .ToListAsync(cancellationToken);

                var apps = await context
                    .Activations.AsNoTracking()
                    .Where(a => a.AssetId == id && a.DeactivationDate == null)
                    .OrderBy(a => a.Volume.SoftwareOrService.Name)
                    .Select(a => new App(
                        a.Volume.SoftwareOrService.Name,
                        a.Volume.License.Asset.Name,
                        a.Volume.License.Asset.InventoryNumber,
                        a.Volume.License.SubscriptionExpirationDate
                    ))
                    .ToListAsync(cancellationToken);

                // What it is, its make and model, the card already says: here, the codes, then the rest.
                List<Fact> facts =
                [
                    .. Codes(
                        ("Device name", d.Name),
                        ("Inventory number", d.InventoryNumber is { } n ? $"#{n}" : null),
                        ("Serial number", d.SerialNumber),
                        ("Model number", d.ModelCode)
                    ),
                    .. Facts(
                        ("Location", d.Location),
                        (
                            Past(d.WarrantyExpirationDate)
                                ? "Warranty expired"
                                : "Warranty expires",
                            d.WarrantyExpirationDate is { } w ? Day(w) : null
                        )
                    ),
                    .. Codes(("Warranty number", d.WarrantyNumber)),
                ];
                return Results.Ok(
                    new Response(
                        Standing(facts, d.WarrantyExpirationDate, warrantyWindow),
                        contracts,
                        apps
                    )
                );
            }
            case "furniture":
            {
                var f = await context
                    .Furnitures.AsNoTracking()
                    .Where(x => x.AssetId == id && x.Asset.PersonId == me)
                    .Select(x => new
                    {
                        x.Asset.Name,
                        x.Asset.InventoryNumber,
                        Location = x.Asset.Location.Name,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                return f is null
                    ? Results.NotFound()
                    : Results.Ok(
                        new Response(
                            [
                                .. Codes(
                                    ("Name", f.Name),
                                    (
                                        "Inventory number",
                                        f.InventoryNumber is { } n ? $"#{n}" : null
                                    )
                                ),
                                .. Facts(("Location", f.Location)),
                            ],
                            [],
                            []
                        )
                    );
            }
            case "license":
            {
                var l = await context
                    .Licenses.AsNoTracking()
                    .Where(x => x.AssetId == id && x.Asset.PersonId == me)
                    .Select(x => new
                    {
                        Vendor = x.Asset.Vendor.Name,
                        x.Asset.InventoryNumber,
                        x.SubscriptionExpirationDate,
                        x.AutoRenew,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                return l is null
                    ? Results.NotFound()
                    : Results.Ok(
                        new Response(
                            Standing(
                                [
                                    .. Codes(
                                        (
                                            "Inventory number",
                                            l.InventoryNumber is { } n ? $"#{n}" : null
                                        )
                                    ),
                                    .. Facts(
                                        ("Vendor", l.Vendor),
                                        (
                                            Past(l.SubscriptionExpirationDate)
                                                ? "Subscription expired"
                                                : "Subscription expires",
                                            l.SubscriptionExpirationDate is { } e ? Day(e) : null
                                        ),
                                        (
                                            "Renews automatically",
                                            l.AutoRenew is { } r ? (r ? "Yes" : "No") : null
                                        )
                                    ),
                                ],
                                l.SubscriptionExpirationDate,
                                subscriptionWindow
                            ),
                            [],
                            []
                        )
                    );
            }
            case "seat":
            {
                var s = await context
                    .Activations.AsNoTracking()
                    .Where(a =>
                        a.Id == id
                        && a.DeactivationDate == null
                        && (a.PersonId == me || a.Asset.PersonId == me)
                    )
                    .Select(a => new
                    {
                        Software = a.Volume.SoftwareOrService.Name,
                        License = a.Volume.License.Asset.Name,
                        LicenseNumber = a.Volume.License.Asset.InventoryNumber,
                        Ends = a.Volume.License.SubscriptionExpirationDate,
                        Device = a.Asset.Name,
                        DeviceNumber = a.Asset.InventoryNumber,
                        a.Quantity,
                        a.ActivationDate,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (s is null)
                    return Results.NotFound();

                // The license and the device are named with their numbers apart, for the page to mute.
                List<Fact> facts =
                [
                    new("Software", s.Software),
                    new("License", s.License, s.LicenseNumber),
                ];
                if (s.Device is not null)
                    facts.Add(new("On device", s.Device, s.DeviceNumber));
                facts.AddRange(
                    Facts(
                        (
                            "Seats",
                            s.Quantity > 1
                                ? s.Quantity.ToString(CultureInfo.InvariantCulture)
                                : null
                        ),
                        ("Since", Day(s.ActivationDate)),
                        (
                            Past(s.Ends) ? "Subscription expired" : "Subscription expires",
                            s.Ends is { } e ? Day(e) : null
                        )
                    )
                );
                return Results.Ok(
                    new Response(Standing(facts, s.Ends, subscriptionWindow), [], [])
                );
            }
            default:
                return Results.NotFound();
        }
    }
}
