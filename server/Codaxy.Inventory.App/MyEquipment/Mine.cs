using System.Globalization;
using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.MyEquipment;

/// <summary>
/// Something of theirs ending soon, by the dashboard's windows ahead: a device's warranty, or a
/// subscription — of a license they hold, or behind a seat they use. Not what has ended: the
/// dashboard's windows back are news to IT, and to the employee what ended is just how it stands,
/// which the card says.
/// </summary>
/// <param name="Kind"><c>warranty</c> or <c>subscription</c>.</param>
/// <param name="Name">The device, or the license.</param>
/// <param name="Uses">For a subscription behind a seat, the software it is for.</param>
public sealed record Warning(string Kind, Guid Id, string Name, DateOnly Date, string? Uses);

/// <summary>
/// One thing of theirs as a card: what it is, then its make and model; the codes that name it to IT
/// ride along for the search, not for the card.
/// </summary>
/// <param name="Kind"><c>device</c>, <c>furniture</c>, <c>license</c> or <c>seat</c>.</param>
/// <param name="Type">The type as recorded.</param>
/// <param name="Title">The type, or the name where there is none: "Laptop".</param>
/// <param name="Subtitle">The make and model, or whatever names it next best: "Lenovo ThinkPad E16 Gen2".</param>
/// <param name="Status"><c>ok</c>, <c>soon</c> or <c>ended</c>: a warranty or a subscription, where it says something; never an expired warranty.</param>
/// <param name="Icon">Its type's icon, a key of <c>TypeIcons</c>; none, the generic one.</param>
/// <param name="Gone">Where it went, written off or sold: the disposal location's name. It still
/// counts as theirs, last and pale.</param>
/// <param name="Place">Where a device or a piece of furniture is, only while their things are in more
/// than one place: the same name on every card says nothing.</param>
/// <param name="Expired">When a subscription expired, for the pill's words: "3 Nov 2025".</param>
public sealed record Thing(
    string Kind,
    Guid Id,
    string? Type,
    string Title,
    string? Subtitle,
    string Name,
    int? Number,
    string? Serial,
    string? Status,
    string? StatusText,
    string? Gone = null,
    string? Icon = null,
    string? Place = null,
    string? Expired = null
);

/// <summary>
/// Their things by section: devices, furniture, their license activations —
/// by name and on their devices — and the licenses they hold.
/// </summary>
public sealed record Things(
    IReadOnlyList<Thing> Devices,
    IReadOnlyList<Thing> Furniture,
    IReadOnlyList<Thing> Software,
    IReadOnlyList<Thing> Licenses
);

/// <summary>
/// An employee's holdings, all of them: devices, furniture and licenses they hold, and the seats theirs
/// by name or on a device they hold, active only — with what needs attention marked on them.
/// </summary>
internal static class Mine
{
    /// <summary>
    /// What of theirs ends within the dashboard's windows ahead, or ended within its windows back —
    /// one entry per device or license, the ended first, then soonest.
    /// </summary>
    public static async Task<List<Warning>> WarningsAsync(
        InventoryContext context,
        SettingStore settings,
        Guid id,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var values = await settings.LoadAsync(cancellationToken);
        var warrantyTo = today.AddDays(values.Get(DashboardSettings.WarrantiesEndingDays));
        var subscriptionTo = today.AddDays(values.Get(DashboardSettings.SubscriptionsExpiringDays));

        var warranties = await context
            .ElectronicDevices.AsNoTracking()
            .Where(d =>
                d.Asset.PersonId == id
                // A warranty on something written off or sold ends unnoticed.
                && !(d.Asset.Location != null && d.Asset.Location.IsDisposal)
                && d.WarrantyExpirationDate >= today
                && d.WarrantyExpirationDate < warrantyTo
            )
            .Select(d => new Warning(
                "warranty",
                d.AssetId,
                d.Asset.Name,
                d.WarrantyExpirationDate!.Value,
                null
            ))
            .ToListAsync(cancellationToken);

        // A license they hold, or one behind an active seat theirs by name or on a device they hold.
        var subscriptions = await context
            .Licenses.AsNoTracking()
            .Where(l =>
                l.SubscriptionExpirationDate >= today
                && l.SubscriptionExpirationDate < subscriptionTo
                && (
                    l.Asset.PersonId == id
                    || l.Volumes.Any(v =>
                        v.Activations.Any(a =>
                            a.DeactivationDate == null
                            && (
                                a.PersonId == id
                                || (
                                    a.Asset.PersonId == id
                                    && !(a.Asset.Location != null && a.Asset.Location.IsDisposal)
                                )
                            )
                        )
                    )
                )
            )
            .Select(l => new Warning(
                "subscription",
                l.AssetId,
                l.Asset.Name,
                l.SubscriptionExpirationDate!.Value,
                l.Asset.PersonId == id
                    ? null
                    : l
                        .Volumes.SelectMany(v => v.Activations)
                        .Where(a =>
                            a.DeactivationDate == null
                            && (
                                a.PersonId == id
                                || (
                                    a.Asset.PersonId == id
                                    && !(a.Asset.Location != null && a.Asset.Location.IsDisposal)
                                )
                            )
                        )
                        .Select(a => a.Volume.SoftwareOrService.Name)
                        .FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);

        return [.. warranties.Concat(subscriptions).OrderBy(w => w.Date)];
    }

    /// <summary>
    /// Everything of theirs, whole: a person holds a few hundred things at most, so the page takes them
    /// at once and searches and shortens them itself. A seat on a device they hold rides on its card; a
    /// seat — theirs by name or on a device they hold — is a license activation; a license they hold,
    /// a section of its own.
    /// </summary>
    public static async Task<Things> ThingsAsync(
        InventoryContext context,
        Guid id,
        IReadOnlyList<Warning> warnings,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var warned = warnings.ToDictionary(w => w.Id);
        var mine = context.Assets.AsNoTracking().Where(a => a.PersonId == id);

        var devices = await mine.Where(a => a.ElectronicDevice != null)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.InventoryNumber,
                Type = a.ElectronicDevice.ElectronicDeviceType.Name,
                Icon = a.ElectronicDevice.ElectronicDeviceType.Icon,
                Maker = a.ElectronicDevice.Manufacturer.Name,
                Model = a.ElectronicDevice.ModelName,
                Serial = a.ElectronicDevice.SerialNumber,
                Warranty = a.ElectronicDevice.WarrantyExpirationDate,
                Gone = a.Location != null && a.Location.IsDisposal ? a.Location.Name : null,
                Place = a.Location != null && !a.Location.IsDisposal ? a.Location.Name : null,
            })
            .ToListAsync(cancellationToken);
        var furniture = await mine.Where(a => a.Furniture != null)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.InventoryNumber,
                Type = a.Furniture.FurnitureType.Name,
                Icon = a.Furniture.FurnitureType.Icon,
                Gone = a.Location != null && a.Location.IsDisposal ? a.Location.Name : null,
                Place = a.Location != null && !a.Location.IsDisposal ? a.Location.Name : null,
                a.Furniture.Model,
            })
            .ToListAsync(cancellationToken);
        var licenses = await mine.Where(a => a.License != null)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.InventoryNumber,
                Software = a
                    .License.Volumes.OrderBy(v => v.SoftwareOrService.Name)
                    .Select(v => v.SoftwareOrService.Name)
                    .FirstOrDefault(),
                Ends = a.License.SubscriptionExpirationDate,
            })
            .ToListAsync(cancellationToken);
        var seats = await context
            .Activations.AsNoTracking()
            .Where(a => a.DeactivationDate == null && (a.PersonId == id || a.Asset.PersonId == id))
            .Select(a => new
            {
                a.Id,
                Software = a.Volume.SoftwareOrService.Name,
                LicenseId = a.Volume.LicenseId,
                License = a.Volume.License.Asset.Name,
                LicenseNumber = a.Volume.License.Asset.InventoryNumber,
                Ends = a.Volume.License.SubscriptionExpirationDate,
                a.Quantity,
                DeviceId = a.PersonId == id ? (Guid?)null : a.AssetId,
            })
            .ToListAsync(cancellationToken);

        // "3 Nov" this year, "3 Jan 2027" past it: the card's pill is short, and the year rarely news.
        string Day(DateOnly d) =>
            d.ToString(d.Year == today.Year ? "d MMM" : "d MMM yyyy", CultureInfo.InvariantCulture);
        static string Month(DateOnly d) => d.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // How it stands today, whenever it ended: running, expiring soon by the dashboard's window
        // ahead, or expired — last week or years ago, the same to someone asking whether it is
        // covered. A date alone while it runs or is expiring: the pill's colour says which, and the
        // page's hover says it in words. Once expired, "Expired", and when only in the hover.
        (string? Status, string? Text, string? Expired) Standing(Guid key, DateOnly? ends)
        {
            // A pill too wide for its card wraps before the date, never inside it ("19 / May").
            static string Whole(string date) => date.Replace(' ', '\u00A0');
            return ends switch
            {
                null => (null, null, null),
                { } e when e < today => ("ended", "Expired", Whole(Day(e))),
                _ when warned.ContainsKey(key) => ("soon", Whole(Day(ends.Value)), null),
                { } e => ("ok", Whole(Month(e)), null),
            };
        }

        // A part number ("960-001592", "90NB0000-P00160", "175W5EA#BED") or a host name ("lap-dub-ad",
        // "Des-dir-ms") names nothing to a person; "ThinkPad E16 Gen2" or "Brio 105" does.
        static bool Code(string s) =>
            !s.Contains(' ')
            && s.Any(char.IsDigit)
            && (s.Contains('-') || s.Contains('#') || s.Count(char.IsDigit) >= 6);
        static bool Host(string s) =>
            System.Text.RegularExpressions.Regex.IsMatch(
                s,
                "^[a-z0-9]+(-[a-z0-9]+)+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

        // "Lenovo ThinkPad E16 Gen2": the maker once, before the model — or before the device's own
        // name where the model is a part number and the name is not a host's; else the maker alone.
        static string? MakeAndModel(string? maker, string? model, string name)
        {
            var named =
                Blank(model) is { } given && !Code(given) ? given
                : !Code(name) && !Host(name) ? name
                : null;
            return (Blank(maker), named) switch
            {
                (null, var m) => m,
                (var k, null) => k,
                (var k, var m) when m.StartsWith(k, StringComparison.OrdinalIgnoreCase) => m,
                (var k, var m) => $"{k} {m}",
            };
        }

        var deviceCards = devices
            .Select(d =>
            {
                // An expired warranty is the usual state of an older device, not news: the card says
                // nothing, the details give the date.
                var (status, text, _) = Standing(d.Id, d.Warranty);
                if (status == "ended")
                    (status, text) = (null, null);
                var model = MakeAndModel(d.Maker, d.Model, d.Name);
                return new Thing(
                    "device",
                    d.Id,
                    Blank(d.Type),
                    Blank(d.Type) ?? d.Name,
                    model ?? (Blank(d.Type) is null ? null : d.Name),
                    d.Name,
                    d.InventoryNumber,
                    d.Serial,
                    d.Gone is null ? status : null,
                    d.Gone is null ? text : null,
                    d.Gone,
                    d.Icon,
                    Blank(d.Place)
                );
            })
            // Written off or sold last: still theirs on paper, no longer in their hands.
            .OrderBy(t => t.Gone is not null)
            .ThenBy(t => t.Title)
            .ThenBy(t => t.Subtitle)
            // Alike things in a steady order, not the database's.
            .ThenBy(t => t.Number)
            .ToList();

        var furnitureCards = furniture
            .Select(f => new Thing(
                "furniture",
                f.Id,
                Blank(f.Type),
                Blank(f.Type) ?? f.Name,
                Blank(f.Model) ?? (Blank(f.Type) is null || f.Name == f.Type ? null : f.Name),
                f.Name,
                f.InventoryNumber,
                null,
                null,
                null,
                f.Gone,
                f.Icon,
                Blank(f.Place)
            ))
            .OrderBy(t => t.Gone is not null)
            .ThenBy(t => t.Title)
            .ThenBy(t => t.Subtitle)
            // Alike things in a steady order, not the database's.
            .ThenBy(t => t.Number)
            .ToList();

        // Their things in one place: the place goes, said by every card alike. Compared whatever the
        // case, as names are unique.
        if (
            deviceCards
                .Concat(furnitureCards)
                .Select(t => t.Place?.ToLowerInvariant())
                .Where(p => p is not null)
                .Distinct()
                .Count() < 2
        )
        {
            deviceCards = [.. deviceCards.Select(t => t with { Place = null })];
            furnitureCards = [.. furnitureCards.Select(t => t with { Place = null })];
        }

        // Software is what they use, named by itself; its license is the panel's.
        // Every activation of theirs: by name, and on a device they hold — "On Dell Latitude 5550", the
        // device by its make and model as its card names it (or what it is, failing those); the device's details list them too.
        var deviceLabels = deviceCards.ToDictionary(d => d.Id, d => d.Subtitle ?? d.Title);
        // A seat on a device written off or sold went with it: marked as gone as the device is.
        var goneDevices = devices
            .Where(d => d.Gone is not null)
            .ToDictionary(d => d.Id, d => d.Gone!);
        var softwareCards = seats
            .Select(s =>
            {
                var (status, text, expired) = Standing(s.LicenseId, s.Ends);
                var on =
                    s.DeviceId is { } device && deviceLabels.TryGetValue(device, out var label)
                        ? $"On {label}"
                        : null;
                var gone =
                    s.DeviceId is { } seatDevice
                    && goneDevices.TryGetValue(seatDevice, out var where)
                        ? where
                        : null;
                // Nothing on a device that went.
                var shown = gone is null;
                return new Thing(
                    "seat",
                    s.Id,
                    null,
                    s.Software,
                    on ?? (s.Quantity > 1 ? $"{s.Quantity} seats" : null),
                    s.License,
                    s.LicenseNumber,
                    null,
                    shown ? status : null,
                    shown ? text : null,
                    gone,
                    Expired: shown ? expired : null
                );
            })
            .OrderBy(t => t.Gone is not null)
            .ThenBy(t => t.Title)
            .ThenBy(t => t.Subtitle)
            .ThenBy(t => t.Id)
            .ToList();

        // A license held in their name — mostly a holding account's — by its name, or by the software
        // it covers where the name is a part number.
        var licenseCards = licenses
            .Select(l =>
            {
                var (status, text, expired) = Standing(l.Id, l.Ends);
                var title = Code(l.Name) && Blank(l.Software) is { } covered ? covered : l.Name;
                return new Thing(
                    "license",
                    l.Id,
                    null,
                    title,
                    Blank(l.Software) is { } sw
                    && !title.Contains(sw, StringComparison.OrdinalIgnoreCase)
                        ? sw
                        : null,
                    l.Name,
                    l.InventoryNumber,
                    null,
                    status,
                    text,
                    Expired: expired
                );
            })
            .OrderBy(t => t.Title)
            .ThenBy(t => t.Number)
            .ToList();

        return new Things(deviceCards, furnitureCards, softwareCards, licenseCards);
    }
}
