using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Informations.Items;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.ElectronicDevices.Devices.List.Endpoint.Item;
using Options = App.ElectronicDevices.Devices.Options.Endpoint.Response;

/// <summary>
/// A laptop of the Laptop type (tagged HasData), made by Lenovo, at HQ, under warranty, with a
/// maintenance contract, a seat of Editor and a runbook kept on it; and a monitor with none of that.
/// </summary>
public class ElectronicDeviceApplication : InventoryApplication
{
    public Guid Laptop { get; private set; }
    public Guid Monitor { get; private set; }
    public static readonly Guid LaptopType = Guid.CreateVersion7();
    public static readonly Guid HasData = Guid.CreateVersion7();
    public static readonly Guid Lenovo = Guid.CreateVersion7();
    public static readonly Guid Hq = Guid.CreateVersion7();
    public Seed.Basics Basics { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        Basics = await Seed.BasicsAsync(context);

        var city = Guid.CreateVersion7();
        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Tools" };
        var editor = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Editor",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = Lenovo,
        };
        context.Countries.Add(new Country { Code = "BA", Name = "Bosnia and Herzegovina" });
        context.Cities.Add(
            new City
            {
                Id = city,
                Name = "Banja Luka",
                CountryCode = "BA",
            }
        );
        context.Locations.Add(
            new Location
            {
                Id = Hq,
                Name = "HQ",
                CountryCode = "BA",
                CityId = city,
                Street = "Main",
            }
        );
        context.Manufacturers.Add(new Manufacturer { Id = Lenovo, Name = "Lenovo" });
        context.ElectronicDeviceTags.Add(
            new ElectronicDeviceTag { Id = HasData, Name = "HasData" }
        );
        context.ElectronicDeviceTypes.Add(
            new ElectronicDeviceType
            {
                Id = LaptopType,
                Name = "Laptop",
                HoldLicences = true,
            }
        );
        context.AddRange(category, editor);
        await context.SaveChangesAsync();
        context.Add(
            new ElectronicDeviceTypeElectronicDeviceTag
            {
                ElectronicDeviceTypeId = LaptopType,
                ElectronicDeviceTagId = HasData,
            }
        );
        await context.SaveChangesAsync();

        Laptop = await Seed.DeviceAsync(context, "ThinkPad E16", holdsLicenses: true);
        Monitor = await Seed.DeviceAsync(context, "Monitor P24", holdsLicenses: false);
        var laptop = await context
            .Assets.Include(a => a.ElectronicDevice)
            .FirstAsync(a => a.Id == Laptop);
        laptop.InventoryNumber = 800001;
        laptop.LocationId = Hq;
        laptop.PurchaseDate = new DateOnly(2026, 5, 4);
        laptop.Incomplete = true;
        laptop.Description = "Developer machine";
        laptop.ElectronicDevice.ElectronicDeviceTypeId = LaptopType;
        laptop.ElectronicDevice.ManufacturerId = Lenovo;
        laptop.ElectronicDevice.ModelName = "E16 Gen2";
        laptop.ElectronicDevice.ModelCode = "21MA";
        laptop.ElectronicDevice.SerialNumber = "PF-123";
        laptop.ElectronicDevice.GuaranteeNumber = "W-9";
        laptop.ElectronicDevice.GuaranteeExpirationDate = new DateOnly(2028, 5, 4);
        var contractType =
            await context.MaintenanceTypes.FirstOrDefaultAsync()
            ?? context
                .MaintenanceTypes.Add(
                    new MaintenanceType { Id = Guid.CreateVersion7(), Text = "Contract" }
                )
                .Entity;
        context.MaintenanceContracts.Add(
            new MaintenanceContract
            {
                Id = Guid.CreateVersion7(),
                AssetId = Laptop,
                VendorId = Basics.Vendor,
                MaintenanceTypeId = contractType.Id,
                ContractNumber = "MC-7",
                ExpirationDate = new DateOnly(2027, 1, 1),
            }
        );
        var type = new InformationType { Id = Guid.CreateVersion7(), Name = "Runbook" };
        var runbook = new Information
        {
            Id = Guid.CreateVersion7(),
            Name = "Laptop runbook",
            InformationTypeId = type.Id,
            PersonId = Basics.Person,
        };
        context.AddRange(type, runbook);
        context.InformationLocations.Add(
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = runbook.Id,
                ElectronicDeviceId = Laptop,
            }
        );
        await context.SaveChangesAsync();

        var editorLicense = await Seed.LicenseWithVolumeAsync(
            context,
            editor.Id,
            "Editor license",
            volumeType: Seed.PerDevice
        );
        await Seed.ActivationAsync(context, editorLicense.Volume, device: Laptop);
    }
}

public class ElectronicDeviceTests(ElectronicDeviceApplication app)
    : IClassFixture<ElectronicDeviceApplication>
{
    private const string Url = "/api/electronic-devices";

    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    [Fact]
    public async Task Refuses_a_caller_without_a_session() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync($"{Url}/")).StatusCode
        );

    [Fact]
    public async Task A_device_s_detail_carries_every_field_its_type_s_tags_contracts_seats_and_information()
    {
        var laptop = (
            await (await Client()).GetFromJsonAsync<DeviceDetail>($"{Url}/{app.Laptop}")
        )!;

        Assert.Equal(
            ("ThinkPad E16", 800001, "Laptop", "Lenovo", "E16 Gen2", "21MA", "PF-123"),
            (
                laptop.Name,
                laptop.Number,
                laptop.Type!.Name,
                laptop.Manufacturer!.Name,
                laptop.ModelName,
                laptop.ModelCode,
                laptop.SerialNumber
            )
        );
        Assert.Equal(
            ("W-9", new DateOnly(2028, 5, 4)),
            (laptop.WarrantyNumber, laptop.WarrantyExpirationDate)
        );
        Assert.Equal(("HQ", true), (laptop.Location!.Name, laptop.Incomplete));
        Assert.Equal(["HasData"], laptop.Tags.Select(t => t.Name));
        Assert.Equal(
            ("MC-7", "Seed vendor"),
            (laptop.Contracts.Single().ContractNumber, laptop.Contracts.Single().Vendor.Name)
        );
        Assert.Equal(
            ("Editor", "Editor license"),
            (laptop.Seats.Items.Single().Software, laptop.Seats.Items.Single().License)
        );
        Assert.Equal("Laptop runbook", laptop.Information.Items.Single().Name);

        var monitor = (
            await (await Client()).GetFromJsonAsync<DeviceDetail>($"{Url}/{app.Monitor}")
        )!;
        Assert.Empty(monitor.Contracts);
        Assert.Equal((0, 0), (monitor.Seats.Total, monitor.Information.Total));
    }

    [Fact]
    public async Task An_unknown_device_is_not_found() =>
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await (await Client()).GetAsync($"{Url}/{Guid.CreateVersion7()}")).StatusCode
        );

    [Theory]
    [InlineData("q=pf-123")]
    [InlineData("q=21ma")]
    [InlineData("q=E16+gen2")]
    [InlineData("q=developer")]
    [InlineData("q=%23800001")]
    public async Task Searches_serial_model_description_and_the_number_as_shown(string query) =>
        Assert.Equal(app.Laptop, Assert.Single((await ListAsync(query)).Items).Id);

    [Fact]
    public async Task Filters_by_every_picker_the_list_offers()
    {
        var laptop = app.Laptop;
        foreach (
            var query in new[]
            {
                $"typeId={ElectronicDeviceApplication.LaptopType}",
                $"tagId={ElectronicDeviceApplication.HasData}",
                $"manufacturerId={ElectronicDeviceApplication.Lenovo}",
                $"locationId={ElectronicDeviceApplication.Hq}",
                "purchasedFrom=2026-05-04&purchasedTo=2026-05-05",
                "incomplete=true",
                $"q={laptop}",
            }
        )
            Assert.Equal(laptop, Assert.Single((await ListAsync(query)).Items).Id);

        Assert.Equal(2, (await ListAsync($"personId={app.Basics.Person}")).Total);
        Assert.Equal(2, (await ListAsync($"vendorId={app.Basics.Vendor}")).Total);
        Assert.Equal(app.Monitor, Assert.Single((await ListAsync("incomplete=false")).Items).Id);
    }

    [Fact]
    public async Task Sorts_by_each_column_newest_change_first_and_pages()
    {
        var all = (await ListAsync("pageSize=100")).Items;
        Assert.Equal(
            all.OrderByDescending(i => i.LastModified).Select(i => i.Id),
            all.Select(i => i.Id)
        );
        foreach (
            var key in new[]
            {
                "number",
                "name",
                "model",
                "assignee",
                "location",
                "type",
                "manufacturer",
                "modified",
            }
        )
            Assert.Equal(2, (await ListAsync($"sort=-{key}")).Items.Count);
        Assert.Single((await ListAsync("pageSize=1")).Items);
        Assert.Empty((await ListAsync("page=99")).Items);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Url}/?sort=value")).StatusCode
        );
    }

    [Fact]
    public async Task Exports_what_the_list_selects_as_the_original_named_filtered_when_narrowed()
    {
        var client = await Client();
        var narrowed = await client.GetAsync($"{Url}/export?q=pf-123");
        var all = await client.GetAsync($"{Url}/export");

        Assert.Equal(
            "ElectronicDevices.Export - Filtered.xlsx",
            narrowed.Content.Headers.ContentDisposition?.FileNameStar
                ?? narrowed.Content.Headers.ContentDisposition?.FileName
        );
        Assert.Equal(
            "ElectronicDevices.Export.xlsx",
            all.Content.Headers.ContentDisposition?.FileNameStar
                ?? all.Content.Headers.ContentDisposition?.FileName
        );
        var text = await Spreadsheet.TextOf(narrowed);
        Assert.Contains("ThinkPad E16", text);
        Assert.Contains("Serial Number", text);
        Assert.DoesNotContain("Monitor P24", text);
    }

    [Fact]
    public async Task Options_offer_the_filters_pickers()
    {
        var o = (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;
        Assert.Contains(o.Types, t => t.Text == "Laptop");
        Assert.Contains(o.Tags, t => t.Text == "HasData");
        Assert.Contains(o.Manufacturers, m => m.Text == "Lenovo");
        Assert.Contains(o.Locations, l => l.Text == "HQ");
    }
}
