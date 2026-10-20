using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Details = Codaxy.Inventory.App.MyEquipment.Details.Endpoint.Response;
using Me = Codaxy.Inventory.Web.Auth.Session.Me.Endpoint;
using Response = Codaxy.Inventory.App.MyEquipment.Get.Endpoint.Response;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// An employee, Mia, with a laptop whose warranty ends in ten days and an editor on it, a monitor whose
/// warranty ends past the window, a tablet whose warranty ended years ago, a seat on a subscription that lapses in five days, and a seat on one
/// that runs a year, a license in her name, and a laptop written off; someone else with a device of their own.
/// </summary>
public class MyEquipmentApplication : PeopleSignInApplication
{
    public const string Mia = "mia.employee@codaxy.com";

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public Guid Laptop { get; private set; }
    public Guid Monitor { get; private set; }
    public Guid Tablet { get; private set; }
    public Guid Lapsing { get; private set; }
    public Guid Lasting { get; private set; }
    public Guid Lapsed { get; private set; }
    public Guid OthersDevice { get; private set; }

    /// <summary>A user who is a person too, turned off from signing in as one, with a device.</summary>
    public const string Uma = "uma.both@codaxy.com";
    public Guid UmasDevice { get; private set; }
    public Guid WrittenOff { get; private set; }
    public Guid Stale { get; private set; }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await PeopleMaySignInAsync(true);
        var mia = await PersonAsync("Mia Employee", Mia);
        var other = await PersonAsync("Ole Other", "ole.other@codaxy.com");
        var uma = await PersonAsync("Uma Both", Uma, canSignIn: false);
        await this.UserAsync(Uma, BuiltInRoles.Admin);

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        Laptop = await Device(context, "Mia's laptop", mia, Today.AddDays(10));
        Monitor = await Device(context, "Mia's monitor", mia, Today.AddDays(200));
        Tablet = await Device(context, "Mia's tablet", mia, Today.AddDays(-1000));
        // Their type carries an icon, which the cards show.
        (await context.ElectronicDeviceTypes.SingleAsync(t => t.Name == "Seed laptop")).Icon =
            "laptop";
        await context.SaveChangesAsync();
        OthersDevice = await Device(context, "Ole's laptop", other, Today.AddDays(5));
        UmasDevice = await Device(context, "Uma's laptop", uma, Today.AddDays(300));

        // Written off with its warranty ending in three days: nothing to warn of.
        WrittenOff = await Device(context, "Mia's old laptop", mia, Today.AddDays(3));
        var city = new City
        {
            Id = Guid.CreateVersion7(),
            Name = "Dublin",
            CountryCode = "IE",
        };
        var writtenOff = new Location
        {
            Id = Guid.CreateVersion7(),
            Name = "Written off",
            CountryCode = "IE",
            CityId = city.Id,
            IsDisposal = true,
        };
        context.Countries.Add(new Country { Code = "IE", Name = "Ireland" });
        context.AddRange(city, writtenOff);
        (await context.Assets.SingleAsync(a => a.Id == WrittenOff)).LocationId = writtenOff.Id;
        // Mia's laptop at the office, her monitor at home; Uma's one thing at the office.
        Location Place(string name) =>
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = name,
                CountryCode = "IE",
                CityId = city.Id,
            };
        var office = Place("Office");
        var home = Place("Home");
        context.AddRange(office, home);
        (await context.Assets.SingleAsync(a => a.Id == Laptop)).LocationId = office.Id;
        (await context.Assets.SingleAsync(a => a.Id == Monitor)).LocationId = home.Id;
        (await context.Assets.SingleAsync(a => a.Id == UmasDevice)).LocationId = office.Id;
        await context.SaveChangesAsync();

        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "App" };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Maker" };
        var suite = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Suite",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = maker.Id,
        };
        context.AddRange(category, maker, suite);
        await context.SaveChangesAsync();

        var lapsing = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "Lapsing subscription",
            expires: Today.AddDays(5)
        );
        await Seed.ActivationAsync(context, lapsing.Volume, person: mia);
        Lapsing = lapsing.License;

        var lasting = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "Lasting subscription",
            expires: Today.AddDays(365)
        );
        await Seed.ActivationAsync(context, lasting.Volume, person: mia);
        Lasting = lasting.License;

        var lapsed = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "Lapsed subscription",
            expires: Today.AddDays(-30)
        );
        await Seed.ActivationAsync(context, lapsed.Volume, person: mia);
        Lapsed = lapsed.License;

        var editor = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "Editor subscription",
            volumeType: Seed.PerDevice,
            expires: Today.AddDays(365)
        );
        await Seed.ActivationAsync(context, editor.Volume, device: Laptop);

        // On the written-off laptop, a subscription lapsing in four days: gone with it.
        var stale = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "Stale subscription",
            volumeType: Seed.PerDevice,
            expires: Today.AddDays(4)
        );
        await Seed.ActivationAsync(context, stale.Volume, device: WrittenOff);
        Stale = stale.License;

        // A license held in her name, named by a part number, its subscription running a year.
        var held = await Seed.LicenseWithVolumeAsync(
            context,
            suite.Id,
            "FC-10-0090G-809",
            expires: Today.AddDays(365)
        );
        (await context.Assets.SingleAsync(a => a.Id == held.License)).PersonId = mia;
        await context.SaveChangesAsync();
    }

    private static async Task<Guid> Device(
        InventoryContext context,
        string name,
        Guid holder,
        DateOnly warranty
    )
    {
        var id = await Seed.DeviceAsync(context, name, holdsLicenses: true);
        var asset = await context
            .Assets.Include(a => a.ElectronicDevice)
            .SingleAsync(a => a.Id == id);
        asset.PersonId = holder;
        asset.ElectronicDevice.WarrantyExpirationDate = warranty;
        await context.SaveChangesAsync();
        return id;
    }
}

public class MyEquipmentTests(MyEquipmentApplication app) : IClassFixture<MyEquipmentApplication>
{
    private async Task<HttpClient> MiaAsync() => await app.SignInAsync(MyEquipmentApplication.Mia);

    private async Task<Response> MineAsync() =>
        (await (await MiaAsync()).GetFromJsonAsync<Response>("/api/my-equipment"))!;

    [Fact]
    public async Task The_page_holds_their_things_by_section_and_nobody_else_s()
    {
        var mine = await MineAsync();

        Assert.Equal("Mia Employee", mine.Name);
        Assert.Equal(4, mine.Devices.Count);
        Assert.Empty(mine.Furniture);
        Assert.Equal(
            ["Suite", "Suite", "Suite", "Suite", "Suite"],
            mine.Software.Select(t => t.Title).ToList()
        );
        Assert.DoesNotContain(mine.Devices, t => t.Id == app.OthersDevice);
    }

    [Fact]
    public async Task What_needs_attention_is_warned_by_the_dashboard_s_windows()
    {
        var mine = await MineAsync();

        Assert.Equal(
            [("subscription", app.Lapsing, "Suite"), ("warranty", app.Laptop, (string?)null)],
            mine.Warnings.Select(w => (w.Kind, w.Id, w.Uses)).ToList()
        );
        Assert.DoesNotContain(mine.Warnings, w => w.Id == app.OthersDevice);
    }

    [Fact]
    public async Task A_card_says_what_it_is_and_how_its_warranty_stands()
    {
        var mine = await MineAsync();
        var laptop = mine.Devices.Single(t => t.Id == app.Laptop);
        var monitor = mine.Devices.Single(t => t.Id == app.Monitor);

        Assert.Equal("Seed laptop", laptop.Title);
        Assert.Equal("laptop", laptop.Icon);
        Assert.Equal("Mia's laptop", laptop.Name);
        Assert.Equal("soon", laptop.Status);
        Assert.DoesNotContain("Warranty", laptop.StatusText);
        Assert.Equal("ok", monitor.Status);
        Assert.Matches(@"^[A-Z][a-z]{2}\u00A0\d{4}$", monitor.StatusText);
    }

    [Fact]
    public async Task Every_seat_is_an_activation_and_one_on_their_device_says_so()
    {
        var mine = await MineAsync();

        // Every term says how it stands on the card, running, ending soon or expired — and when it expired.
        Assert.Equal(
            ["soon", "ok", "ended", "ok", null],
            mine.Software.Select(t => t.Status).OrderBy(s => s is null).ToList()
        );
        var lapsed = mine.Software.Single(t => t.Status == "ended");
        Assert.Equal("Expired", lapsed.StatusText);
        Assert.StartsWith($"{MyEquipmentApplication.Today.AddDays(-30).Day}\u00A0", lapsed.Expired);
        // The one on her laptop is an activation of hers too, said where it is: on the laptop as its card names it.
        var laptop = mine.Devices.Single(t => t.Id == app.Laptop);
        Assert.Contains(mine.Software, t => t.Subtitle == $"On {laptop.Subtitle ?? laptop.Title}");
    }

    [Fact]
    public async Task A_warranty_long_ended_says_nothing_on_its_card_and_warns_of_nothing()
    {
        var mine = await MineAsync();
        var tablet = mine.Devices.Single(t => t.Id == app.Tablet);

        Assert.Null(tablet.Status);
        Assert.Null(tablet.StatusText);
        Assert.DoesNotContain(mine.Warnings, w => w.Id == app.Tablet);
    }

    [Fact]
    public async Task A_card_says_where_it_is_only_while_their_things_are_in_more_than_one_place()
    {
        var mine = await MineAsync();
        string? Place(Guid id) => mine.Devices.Single(t => t.Id == id).Place;

        Assert.Equal(("Office", "Home"), (Place(app.Laptop), Place(app.Monitor)));
        // Nowhere recorded, and a disposal location, are no place.
        Assert.Null(Place(app.Tablet));
        Assert.Null(Place(app.WrittenOff));

        var umas = (
            await (await app.SignInAsync(MyEquipmentApplication.Uma)).GetFromJsonAsync<Response>(
                "/api/my-equipment"
            )
        )!;
        Assert.Null(Assert.Single(umas.Devices).Place);
    }

    [Fact]
    public async Task What_was_written_off_comes_last_marked_and_warns_of_nothing()
    {
        var mine = await MineAsync();
        var last = mine.Devices[^1];

        Assert.Equal((app.WrittenOff, "Written off", null), (last.Id, last.Gone, last.Status));
        Assert.DoesNotContain(mine.Warnings, w => w.Id == app.WrittenOff);
        // Its seat goes with it, last among the activations, and its subscription warns of nothing.
        Assert.Equal("Written off", mine.Software[^1].Gone);
        Assert.DoesNotContain(mine.Warnings, w => w.Id == app.Stale);
    }

    [Fact]
    public async Task A_license_in_their_name_is_its_own_section_named_by_its_software_over_a_part_number()
    {
        var license = Assert.Single((await MineAsync()).Licenses);

        Assert.Equal(("Suite", "FC-10-0090G-809"), (license.Title, license.Name));
        // Its subscription says how it stands, running too, as an activation's does.
        Assert.Equal("ok", license.Status);
    }

    [Fact]
    public async Task A_thing_of_theirs_opens_to_what_they_would_tell_IT()
    {
        var client = await MiaAsync();

        var laptop = (
            await client.GetFromJsonAsync<Details>($"/api/my-equipment/items/device/{app.Laptop}")
        )!;
        var seat = (await client.GetFromJsonAsync<Response>("/api/my-equipment"))!.Software[0];
        var seatDetails = (
            await client.GetFromJsonAsync<Details>($"/api/my-equipment/items/seat/{seat.Id}")
        )!;

        Assert.Equal(
            ("Device name", "Mia's laptop"),
            (laptop.Facts[0].Label, laptop.Facts[0].Value)
        );
        Assert.Contains(laptop.Facts, f => f.Label == "Warranty expires" && f.Standing == "soon");
        Assert.Equal(["Suite"], laptop.Apps.Select(a => a.Name).ToList());
        Assert.Equal("Suite", seatDetails.Facts.Single(f => f.Label == "Software").Value);
        Assert.Null(seatDetails.Facts.Single(f => f.Label == "Since").Standing);
        // The license is named alone; its number comes apart, for the page to mute.
        Assert.EndsWith(" subscription", seatDetails.Facts.Single(f => f.Label == "License").Value);
    }

    [Fact]
    public async Task Someone_else_s_thing_is_not_found() =>
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await (await MiaAsync()).GetAsync(
                    $"/api/my-equipment/items/device/{app.OthersDevice}"
                )
            ).StatusCode
        );

    [Fact]
    public async Task A_user_who_is_a_person_too_sees_their_own_whatever_the_switches()
    {
        var client = await app.SignInAsync(MyEquipmentApplication.Uma);
        await app.PeopleMaySignInAsync(false);
        try
        {
            var me = await client.GetFromJsonAsync<Me.Response>("/api/auth/me");
            var mine = (await client.GetFromJsonAsync<Response>("/api/my-equipment"))!;
            var details = await client.GetAsync($"/api/my-equipment/items/device/{app.UmasDevice}");

            Assert.Equal((Me.UserKind, true), (me!.Kind, me.MyEquipment));
            Assert.Equal(("Uma Both", app.UmasDevice), (mine.Name, Assert.Single(mine.Devices).Id));
            Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        }
        finally
        {
            await app.PeopleMaySignInAsync(true);
        }
    }

    [Fact]
    public async Task A_user_who_is_no_person_is_told_there_is_nothing()
    {
        var client = await app.ClientAsync("equipment.admin@codaxy.com");

        var me = await client.GetFromJsonAsync<Me.Response>("/api/auth/me");

        Assert.False(me!.MyEquipment);
    }

    [Theory]
    [InlineData("/api/my-equipment")]
    [InlineData("/api/my-equipment/items/device/0198f3b0-0000-7000-8000-000000000000")]
    public async Task A_user_is_refused_it(string path)
    {
        var client = await app.ClientAsync("equipment.admin@codaxy.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(path)).StatusCode);
    }
}
