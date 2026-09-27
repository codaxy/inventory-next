using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.Informations.Items;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Response = App.Dashboard.Get.Endpoint.Response;

/// <summary>
/// One record either side of every rule the dashboard applies: a subscription lapsed with seats, one
/// lapsed idle, one lapsed 45 days ago and one 46; one ending today, in fourteen days and in fifteen; volumes over, at and under
/// what was bought; warranties ending in 29 and 30 days, and ended yesterday, 45 and 46 days ago; seats on a device written off
/// and on one in use; an asset and a piece of information left incomplete.
/// </summary>
public class DashboardApplication : InventoryApplication
{
    public const string WrittenOff = "Written off";

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public Guid LapsedWithSeats { get; private set; }
    public Guid LapsedIdle { get; private set; }
    public Guid LapsedAt45 { get; private set; }
    public Guid LapsedLongAgo { get; private set; }
    public Guid EndsToday { get; private set; }
    public Guid EndsIn14 { get; private set; }
    public Guid EndsIn15 { get; private set; }
    public Guid Over { get; private set; }
    public Guid AtQuantity { get; private set; }
    public Guid MostFree { get; private set; }
    public Guid LeastFree { get; private set; }
    public Guid WarrantyIn29 { get; private set; }
    public Guid WarrantyIn30 { get; private set; }
    public Guid WarrantyGone { get; private set; }
    public Guid WarrantyGoneAt45 { get; private set; }
    public Guid WarrantyGoneLongAgo { get; private set; }
    public Guid OnDisposed { get; private set; }
    public Guid DeactivatedOnDisposed { get; private set; }
    public Guid OnDeviceInUse { get; private set; }
    public Guid IncompleteDevice { get; private set; }
    public Guid IncompleteInformation { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Dashboard:DisposedLocations:0", WrittenOff);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "App" };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Maker" };
        var software = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Suite",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = maker.Id,
        };
        var city = Guid.CreateVersion7();
        var writtenOff = Guid.CreateVersion7();
        var office = Guid.CreateVersion7();
        context.AddRange(category, maker, software);
        context.Countries.Add(new Country { Code = "US", Name = "United States" });
        context.Cities.Add(
            new City
            {
                Id = city,
                Name = "New York",
                CountryCode = "US",
            }
        );
        context.Locations.AddRange(
            new Location
            {
                Id = writtenOff,
                Name = WrittenOff,
                CountryCode = "US",
                CityId = city,
                Street = "N/A",
            },
            new Location
            {
                Id = office,
                Name = "Office",
                CountryCode = "US",
                CityId = city,
                Street = "Hudson Street",
            }
        );
        await context.SaveChangesAsync();

        async Task<Seed.SeededLicense> License(string name, int seats, DateOnly? expires = null) =>
            await Seed.LicenseWithVolumeAsync(context, software.Id, name, seats, expires: expires);

        var lapsedWithSeats = await License("Lapsed with seats", 5, Today.AddDays(-1));
        await Seed.ActivationAsync(context, lapsedWithSeats.Volume);
        LapsedWithSeats = lapsedWithSeats.License;
        LapsedIdle = (await License("Lapsed idle", 5, Today.AddDays(-30))).License;
        LapsedAt45 = (await License("Lapsed at 45", 0, Today.AddDays(-45))).License;
        LapsedLongAgo = (await License("Lapsed long ago", 0, Today.AddDays(-46))).License;

        // No seats bought, so these say nothing about seats.
        EndsToday = (await License("Ends today", 0, Today)).License;
        EndsIn14 = (await License("Ends in 14", 0, Today.AddDays(14))).License;
        EndsIn15 = (await License("Ends in 15", 0, Today.AddDays(15))).License;

        var over = await License("Over", 2);
        await Seed.ActivationAsync(context, over.Volume, quantity: 3);
        Over = over.Volume;

        var atQuantity = await License("At quantity", 2);
        await Seed.ActivationAsync(context, atQuantity.Volume, quantity: 2);
        AtQuantity = atQuantity.Volume;

        var mostFree = await License("Most free", 10);
        await Seed.ActivationAsync(context, mostFree.Volume);
        // Deactivated: no longer holds a seat.
        await Seed.ActivationAsync(context, mostFree.Volume, deactivated: Today);
        MostFree = mostFree.Volume;
        LeastFree = (await License("Least free", 3)).Volume;

        WarrantyIn29 = await Device("Warranty in 29", warranty: Today.AddDays(29));
        WarrantyIn30 = await Device("Warranty in 30", warranty: Today.AddDays(30));
        WarrantyGone = await Device("Warranty gone", warranty: Today.AddDays(-1));
        WarrantyGoneAt45 = await Device("Warranty gone at 45", warranty: Today.AddDays(-45));
        WarrantyGoneLongAgo = await Device("Warranty gone long ago", warranty: Today.AddDays(-46));

        var perDevice = await Seed.LicenseWithVolumeAsync(
            context,
            software.Id,
            "Per device",
            // Exactly its two active seats, so it stands in neither seat section.
            2,
            Seed.PerDevice
        );
        var disposed = await Device("Disposed", location: writtenOff);
        OnDisposed = await Seed.ActivationAsync(context, perDevice.Volume, device: disposed);
        DeactivatedOnDisposed = await Seed.ActivationAsync(
            context,
            perDevice.Volume,
            device: disposed,
            deactivated: Today
        );
        var inUse = await Device("In use", location: office);
        OnDeviceInUse = await Seed.ActivationAsync(context, perDevice.Volume, device: inUse);

        IncompleteDevice = await Device("Incomplete device", incomplete: true);

        var type = new InformationType { Id = Guid.CreateVersion7(), Name = "Contract" };
        var basics = await Seed.BasicsAsync(context);
        IncompleteInformation = Guid.CreateVersion7();
        context.InformationTypes.Add(type);
        context.Informations.Add(
            new Information
            {
                Id = IncompleteInformation,
                Name = "Incomplete information",
                InformationTypeId = type.Id,
                PersonId = basics.Person,
                Incomplete = true,
            }
        );
        await context.SaveChangesAsync();

        async Task<Guid> Device(
            string name,
            DateOnly? warranty = null,
            Guid? location = null,
            bool incomplete = false
        )
        {
            var id = await Seed.DeviceAsync(context, name, holdsLicenses: true);
            var asset = await context
                .Assets.Include(a => a.ElectronicDevice)
                .SingleAsync(a => a.Id == id);
            asset.ElectronicDevice.GuaranteeExpirationDate = warranty;
            asset.LocationId = location;
            asset.Incomplete = incomplete;
            await context.SaveChangesAsync();
            return id;
        }
    }
}

public class DashboardTests(DashboardApplication app) : IClassFixture<DashboardApplication>
{
    private async Task<Response> Dashboard()
    {
        var client = await app.ClientAsync();
        return (await client.GetFromJsonAsync<Response>("/api/dashboard"))!;
    }

    [Fact]
    public async Task A_subscription_expired_recently_from_the_day_after_its_date_to_45_days_on()
    {
        var expired = (await Dashboard()).Expired.Items.Select(l => l.Id).ToList();

        Assert.Equal([app.LapsedWithSeats, app.LapsedIdle, app.LapsedAt45], expired);
        Assert.DoesNotContain(app.LapsedLongAgo, expired);
    }

    [Fact]
    public async Task One_with_seats_still_active_leads_the_expired()
    {
        var first = (await Dashboard()).Expired.Items[0];

        Assert.Equal(app.LapsedWithSeats, first.Id);
        Assert.Equal(1, first.ActiveSeats);
    }

    [Fact]
    public async Task Expiring_soon_is_from_today_to_the_fourteenth_day_soonest_first()
    {
        var soon = (await Dashboard()).ExpiringSoon.Items.Select(l => l.Id).ToList();

        Assert.Equal([app.EndsToday, app.EndsIn14], soon);
    }

    [Fact]
    public async Task A_volume_is_over_allocated_only_past_its_quantity()
    {
        var over = (await Dashboard()).OverAllocated.Items.Select(v => v.Id).ToList();

        Assert.Equal([app.Over], over);
    }

    [Fact]
    public async Task Unused_seats_count_only_active_seats_most_free_first()
    {
        var dashboard = await Dashboard();
        var unused = dashboard.Unused.Items.Select(v => v.Id).ToList();

        Assert.Equal(app.MostFree, unused[0]);
        Assert.Equal(app.LeastFree, unused[1]);
        Assert.Equal(1, dashboard.Unused.Items[0].InUse);
        Assert.DoesNotContain(app.AtQuantity, unused);
        Assert.DoesNotContain(app.Over, unused);
    }

    [Fact]
    public async Task Free_seats_on_a_lapsed_subscription_are_not_unused()
    {
        var dashboard = await Dashboard();

        Assert.DoesNotContain(
            dashboard.Unused.Items,
            v => v.License is "Lapsed with seats" or "Lapsed idle"
        );
        Assert.Equal(dashboard.Unused.Items.Sum(v => v.Quantity - v.InUse), dashboard.UnusedSeats);
    }

    [Fact]
    public async Task A_warranty_ends_soon_within_thirty_days_and_not_once_ended()
    {
        var warranties = (await Dashboard()).Warranties.Items.Select(d => d.Id).ToList();

        Assert.Equal([app.WarrantyIn29], warranties);
    }

    [Fact]
    public async Task A_warranty_ended_recently_from_the_day_after_to_45_days_on_latest_first()
    {
        var ended = (await Dashboard()).WarrantiesEnded.Items.Select(d => d.Id).ToList();

        Assert.Equal([app.WarrantyGone, app.WarrantyGoneAt45], ended);
    }

    [Fact]
    public async Task A_seat_left_active_on_a_disposed_device_is_listed()
    {
        var seats = (await Dashboard()).DisposedSeats!.Items.Select(s => s.Id).ToList();

        Assert.Equal([app.OnDisposed], seats);
    }

    [Fact]
    public async Task Incomplete_records_include_information()
    {
        var incomplete = (await Dashboard()).Incomplete.Items;

        Assert.Contains(incomplete, r => r.Id == app.IncompleteDevice && r.Kind == "device");
        Assert.Contains(
            incomplete,
            r => r.Id == app.IncompleteInformation && r.Kind == "information"
        );
    }
}

/// <summary>The same records, every window a day short of its default, each read on its own.</summary>
public class DashboardWindowsApplication : DashboardApplication
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Dashboard:SubscriptionsExpiredDays", "44");
        builder.UseSetting("Dashboard:SubscriptionsExpiringDays", "14");
        builder.UseSetting("Dashboard:WarrantiesEndedDays", "44");
        builder.UseSetting("Dashboard:WarrantiesEndingDays", "29");
    }
}

public class DashboardWindowsTests(DashboardWindowsApplication app)
    : IClassFixture<DashboardWindowsApplication>
{
    [Fact]
    public async Task Each_window_is_its_own_setting()
    {
        var client = await app.ClientAsync();
        var dashboard = (await client.GetFromJsonAsync<Response>("/api/dashboard"))!;

        Assert.Equal(
            [app.LapsedWithSeats, app.LapsedIdle],
            dashboard.Expired.Items.Select(l => l.Id)
        );
        Assert.Equal([app.EndsToday], dashboard.ExpiringSoon.Items.Select(l => l.Id));
        Assert.Equal([app.WarrantyGone], dashboard.WarrantiesEnded.Items.Select(d => d.Id));
        Assert.Empty(dashboard.Warranties.Items);
        Assert.Equal(
            (44, 14, 44, 29),
            (
                dashboard.SubscriptionsExpiredDays,
                dashboard.SubscriptionsExpiringDays,
                dashboard.WarrantiesEndedDays,
                dashboard.WarrantiesEndingDays
            )
        );
    }
}

public class DashboardWithoutDisposalTests(InventoryApplication app)
    : IClassFixture<InventoryApplication>
{
    [Fact]
    public async Task Without_disposal_locations_there_is_no_disposed_section()
    {
        var client = await app.ClientAsync();
        var dashboard = (await client.GetFromJsonAsync<Response>("/api/dashboard"))!;

        Assert.Null(dashboard.DisposedSeats);
        Assert.Equal(0, dashboard.Expired.Total);
    }
}
