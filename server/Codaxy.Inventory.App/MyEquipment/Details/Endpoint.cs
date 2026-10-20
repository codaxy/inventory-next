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
/// vendor and term; a seat's software, license and device; a piece of information's author,
/// description and where it is kept. Never a key, a console address, money or the
/// classification — none is theirs to see. Anything not theirs is not found.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder mine) =>
        mine.MapGet("/items/{kind}/{id:guid}", Handle);

    /// <summary>
    /// A fact as the page shows it: a label and its value, in order; <paramref name="Number"/> the
    /// inventory number of what the value names, which the page mutes beside it; <paramref name="Standing"/>, on the date a
    /// warranty or a subscription expires on, how it stands — <c>ok</c>, <c>soon</c> by the
    /// dashboard's window ahead, <c>ended</c> — for the page to show that date as the card's pill.
    /// </summary>
    public sealed record Fact(
        string Label,
        string Value,
        int? Number = null,
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

    /// <summary>The codes that name a thing to IT, those it has.</summary>
    private static IEnumerable<Fact> Codes(params (string Label, string? Value)[] codes) =>
        codes
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .Select(c => new Fact(c.Label, c.Value!.Trim()));

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
        if (await access.EquipmentHolderAsync(cancellationToken) is not { } person)
            return Results.Forbid();
        var me = person.PersonId;
        var today = Expiry.Today(clock);
        var values = await settings.LoadAsync(cancellationToken);
        var warrantyWindow = values.Get(DashboardSettings.WarrantiesEndingDays);
        var subscriptionWindow = values.Get(DashboardSettings.SubscriptionsExpiringDays);
        // When a warranty or a subscription expires — "expires" on its date, "expired" once it is past —
        // marked with how it stands, as the card's pill: by the dashboard's window ahead. None without
        // a date.
        IEnumerable<Fact> Expires(string what, DateOnly? date, int window) =>
            date is not { } e ? []
            : e < today ? [new Fact($"{what} expired", Day(e), Standing: "ended")]
            :
            [
                new Fact(
                    $"{what} expires",
                    Day(e),
                    Standing: e < today.AddDays(window) ? "soon" : "ok"
                ),
            ];

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
                    .. Facts(("Location", d.Location)),
                    .. Expires("Warranty", d.WarrantyExpirationDate, warrantyWindow),
                    .. Codes(("Warranty number", d.WarrantyNumber)),
                ];
                return Results.Ok(new Response(facts, contracts, apps));
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
                            [
                                .. Codes(
                                    (
                                        "Inventory number",
                                        l.InventoryNumber is { } n ? $"#{n}" : null
                                    )
                                ),
                                .. Facts(("Vendor", l.Vendor)),
                                .. Expires(
                                    "Subscription",
                                    l.SubscriptionExpirationDate,
                                    subscriptionWindow
                                ),
                                .. Facts(
                                    (
                                        "Renews automatically",
                                        l.AutoRenew is { } r ? (r ? "Yes" : "No") : null
                                    )
                                ),
                            ],
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
                        ("Since", Day(s.ActivationDate))
                    )
                );
                facts.AddRange(Expires("Subscription", s.Ends, subscriptionWindow));
                return Results.Ok(new Response(facts, [], []));
            }
            case "information":
            {
                var i = await context
                    .Informations.AsNoTracking()
                    .Where(x => x.Id == id && x.PersonId == me)
                    .Select(x => new { x.Author, x.Description })
                    .FirstOrDefaultAsync(cancellationToken);
                if (i is null)
                    return Results.NotFound();

                // Where it is kept, each place by what it is: one of a device, a virtual machine, a
                // software entry, a cloud subscription, a location or a web address.
                var places = await context
                    .InformationLocations.AsNoTracking()
                    .Where(l => l.InformationId == id)
                    .Select(l => new
                    {
                        Device = l.ElectronicDevice.Asset.Name,
                        DeviceNumber = (int?)l.ElectronicDevice.Asset.InventoryNumber,
                        VirtualMachine = l.VirtualMachine.Name,
                        Software = l.Software.Name,
                        Cloud = l.Cloud.Name,
                        Location = l.PhysicalLocation.Name,
                        l.URL,
                    })
                    .ToListAsync(cancellationToken);

                // Its type and project the card already says.
                List<Fact> facts = Facts(("Author", i.Author), ("Description", i.Description));
                foreach (var p in places)
                {
                    if (!string.IsNullOrWhiteSpace(p.Device))
                        facts.Add(new("Kept on device", p.Device, p.DeviceNumber));
                    facts.AddRange(
                        Facts(
                            ("Kept on virtual machine", p.VirtualMachine),
                            ("Kept in software", p.Software),
                            ("Kept in cloud subscription", p.Cloud),
                            ("Kept at", p.Location),
                            ("Kept at address", p.URL)
                        )
                    );
                }
                return Results.Ok(new Response(facts, [], []));
            }
            default:
                return Results.NotFound();
        }
    }
}
