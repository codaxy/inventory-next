using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Clients;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.Company.People;
using Codaxy.Inventory.App.Company.Projects;
using Codaxy.Inventory.App.Company.Vendors;
using Codaxy.Inventory.App.Informations.Items;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// A laptop made by Maker, bought from the seed vendor, kept at HQ in Banja Luka; a contract on it
/// from Service Co; Maker's Editor software; Rocket, a project of Acme with a contract document that
/// is also stored at HQ. Zagreb is a city of another country.
/// </summary>
public class CompanyApplication : InventoryApplication
{
    public static readonly Guid Hq = Guid.CreateVersion7();
    public static readonly Guid BanjaLuka = Guid.CreateVersion7();
    public static readonly Guid Zagreb = Guid.CreateVersion7();
    public static readonly Guid Srpska = Guid.CreateVersion7();
    public static readonly Guid Maker = Guid.CreateVersion7();
    public static readonly Guid ServiceCo = Guid.CreateVersion7();
    public static readonly Guid Rocket = Guid.CreateVersion7();
    public static readonly Guid Acme = Guid.CreateVersion7();
    public static Guid SeedVendor;
    public static Guid Owner;
    public static Guid Laptop;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var basics = await Seed.BasicsAsync(context);
        SeedVendor = basics.Vendor;
        Owner = basics.Person;

        context.Countries.AddRange(
            new Country { Code = "BA", Name = "Bosnia and Herzegovina" },
            new Country { Code = "HR", Name = "Croatia" }
        );
        context.Cities.AddRange(
            new City
            {
                Id = BanjaLuka,
                Name = "Banja Luka",
                CountryCode = "BA",
            },
            new City
            {
                Id = Zagreb,
                Name = "Zagreb",
                CountryCode = "HR",
            }
        );
        context.States.Add(
            new State
            {
                Id = Srpska,
                Name = "Republika Srpska",
                CountryCode = "BA",
            }
        );
        context.Locations.Add(
            new Location
            {
                Id = Hq,
                Name = "HQ",
                CountryCode = "BA",
                CityId = BanjaLuka,
                Street = "Main",
                HouseNumber = 1,
            }
        );
        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Tools" };
        context.AddRange(
            category,
            new Manufacturer
            {
                Id = Maker,
                Name = "Maker",
                URL = "https://maker.example",
            },
            new Vendor
            {
                Id = ServiceCo,
                Name = "Service Co",
                Email = "desk@service.example",
            },
            new Client { Id = Acme, Name = "Acme" }
        );
        context.SoftwareOrServices.Add(
            new SoftwareOrService
            {
                Id = Guid.CreateVersion7(),
                Name = "Editor",
                SoftwareOrServiceCategoryId = category.Id,
                ManufacturerId = Maker,
            }
        );
        context.Projects.Add(
            new Project
            {
                Id = Rocket,
                Name = "Rocket",
                ClientId = Acme,
                ProjectOwnerId = basics.Person,
            }
        );
        await context.SaveChangesAsync();

        Laptop = await Seed.DeviceAsync(context, "HQ laptop", holdsLicenses: true);
        var laptop = await context
            .Assets.Include(a => a.ElectronicDevice)
            .FirstAsync(a => a.Id == Laptop);
        laptop.LocationId = Hq;
        laptop.ElectronicDevice.ManufacturerId = Maker;
        context.MaintenanceContracts.Add(
            new MaintenanceContract
            {
                Id = Guid.CreateVersion7(),
                AssetId = Laptop,
                VendorId = ServiceCo,
                ContractNumber = "MC-1",
            }
        );
        var type = new InformationType { Id = Guid.CreateVersion7(), Name = "Contract" };
        var document = new Information
        {
            Id = Guid.CreateVersion7(),
            Name = "Rocket contract",
            InformationTypeId = type.Id,
            PersonId = basics.Person,
            ProjectId = Rocket,
        };
        context.AddRange(type, document);
        context.InformationLocations.Add(
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = document.Id,
                PhysicalLocationId = Hq,
            }
        );
        await context.SaveChangesAsync();
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class CompanyTests(CompanyApplication app) : IClassFixture<CompanyApplication>
{
    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    private static async Task<string> ConflictOf(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title!;
    }

    private Task<List<string>> AuditOf(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id)
                .OrderBy(a => a.TimeCreated)
                .Select(a => a.Table + ":" + a.ActionType)
                .ToListAsync()
        );

    /// <summary>Created, renamed and deleted through the API, each step in the audit log under the entity's name.</summary>
    private async Task RoundTripAsync(string url, string table, object create, object update)
    {
        var client = await Client();
        var created = await client.PostAsJsonAsync(url, create);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<IdOnly>())!.Id;

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"{url}/{id}", update)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{url}/{id}")).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{url}/{id}")).StatusCode);
        Assert.Equal([$"{table}:Create", $"{table}:Update", $"{table}:Delete"], await AuditOf(id));
    }

    private sealed record IdOnly(Guid Id);

    [Theory]
    [InlineData("/api/company/projects/")]
    [InlineData("/api/company/vendors/")]
    [InlineData("/api/company/manufacturers/")]
    [InlineData("/api/company/locations/")]
    public async Task Refuses_a_caller_without_a_session(string url) =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync(url)).StatusCode
        );

    // Projects

    [Fact]
    public Task A_project_round_trips_with_its_audit_rows() =>
        RoundTripAsync(
            "/api/company/projects",
            "Project",
            new
            {
                name = "Anvil",
                clientId = CompanyApplication.Acme,
                ownerId = CompanyApplication.Owner,
            },
            new
            {
                name = "Anvil 2",
                clientId = CompanyApplication.Acme,
                ownerId = CompanyApplication.Owner,
            }
        );

    [Fact]
    public async Task A_project_needs_an_unused_name_and_a_client_and_owner_that_exist()
    {
        var client = await Client();
        Assert.Equal(
            ["A project with this name already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/projects",
                    new
                    {
                        name = "ROCKET ",
                        clientId = CompanyApplication.Acme,
                        ownerId = CompanyApplication.Owner,
                    }
                ),
                "name"
            )
        );
        Assert.Equal(
            ["That choice no longer exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/projects",
                    new
                    {
                        name = "New",
                        clientId = Guid.CreateVersion7(),
                        ownerId = CompanyApplication.Owner,
                    }
                ),
                "clientId"
            )
        );
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/projects",
                    new { name = "New", clientId = CompanyApplication.Acme }
                ),
                "ownerId"
            )
        );
    }

    [Fact]
    public async Task A_project_shows_its_information_and_is_not_deleted_while_it_has_some()
    {
        var client = await Client();
        var rocket = (
            await client.GetFromJsonAsync<ProjectDetail>(
                $"/api/company/projects/{CompanyApplication.Rocket}"
            )
        )!;
        Assert.Equal(
            ("Acme", "Rocket contract"),
            (rocket.Client.Name, rocket.Information.Items.Single().Name)
        );

        Assert.Equal(
            "A piece of information is of this project, so it cannot be deleted.",
            await ConflictOf(
                await client.DeleteAsync($"/api/company/projects/{CompanyApplication.Rocket}")
            )
        );
    }

    [Fact]
    public async Task Projects_list_filtered_by_client_and_owner()
    {
        var client = await Client();
        var byClient = (
            await client.GetFromJsonAsync<Page<App.Company.Projects.List.Endpoint.Item>>(
                $"/api/company/projects/?clientId={CompanyApplication.Acme}"
            )
        )!;
        Assert.Equal(
            ("Rocket", "Acme", 1),
            (
                byClient.Items.Single().Name,
                byClient.Items.Single().Client,
                byClient.Items.Single().Information
            )
        );
        Assert.Equal(
            1,
            (
                await client.GetFromJsonAsync<Page<App.Company.Projects.List.Endpoint.Item>>(
                    $"/api/company/projects/?personId={CompanyApplication.Owner}"
                )
            )!.Total
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/company/projects/?sort=size")).StatusCode
        );
    }

    // Vendors

    [Fact]
    public Task A_vendor_round_trips_with_its_audit_rows() =>
        RoundTripAsync(
            "/api/company/vendors",
            "Vendor",
            new { name = "Globex", email = "sales@globex.example" },
            new { name = "Globex Corp" }
        );

    [Fact]
    public async Task A_vendor_needs_an_unused_name_and_a_well_formed_email()
    {
        var client = await Client();
        Assert.Equal(
            ["A vendor with this name already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync("/api/company/vendors", new { name = " service co" }),
                "name"
            )
        );
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/vendors",
                    new { name = "Other", email = "nope" }
                ),
                "email"
            )
        );
    }

    [Fact]
    public async Task A_vendor_shows_what_was_bought_from_it_and_its_contracts_and_keeps_them()
    {
        var client = await Client();
        var seed = (
            await client.GetFromJsonAsync<VendorDetail>(
                $"/api/company/vendors/{CompanyApplication.SeedVendor}"
            )
        )!;
        Assert.Equal("HQ laptop", seed.Assets.Devices.Items.Single().Name);
        var service = (
            await client.GetFromJsonAsync<VendorDetail>(
                $"/api/company/vendors/{CompanyApplication.ServiceCo}"
            )
        )!;
        Assert.Equal(
            ("HQ laptop", "MC-1"),
            (
                service.Contracts.Items.Single().Asset,
                service.Contracts.Items.Single().ContractNumber
            )
        );

        Assert.Equal(
            "1 asset names this vendor, so it cannot be deleted.",
            await ConflictOf(
                await client.DeleteAsync($"/api/company/vendors/{CompanyApplication.SeedVendor}")
            )
        );
        Assert.Equal(
            "1 maintenance contract names this vendor, so it cannot be deleted.",
            await ConflictOf(
                await client.DeleteAsync($"/api/company/vendors/{CompanyApplication.ServiceCo}")
            )
        );
    }

    [Fact]
    public async Task Vendors_list_searching_and_sorting_by_assets()
    {
        var client = await Client();
        var found = (
            await client.GetFromJsonAsync<Page<App.Company.Vendors.List.Endpoint.Item>>(
                "/api/company/vendors/?q=desk@"
            )
        )!;
        Assert.Equal("Service Co", found.Items.Single().Name);
        var byAssets = (
            await client.GetFromJsonAsync<Page<App.Company.Vendors.List.Endpoint.Item>>(
                "/api/company/vendors/?sort=-assets"
            )
        )!.Items;
        Assert.Equal(
            byAssets.OrderByDescending(v => v.Assets).Select(v => v.Assets),
            byAssets.Select(v => v.Assets)
        );
    }

    // Manufacturers

    [Fact]
    public Task A_manufacturer_round_trips_with_its_audit_rows() =>
        RoundTripAsync(
            "/api/company/manufacturers",
            "Manufacturer",
            new { name = "Initech" },
            new { name = "Initech", url = "https://initech.example" }
        );

    [Fact]
    public async Task A_manufacturer_shows_its_devices_and_software_and_keeps_them()
    {
        var client = await Client();
        var maker = (
            await client.GetFromJsonAsync<ManufacturerDetail>(
                $"/api/company/manufacturers/{CompanyApplication.Maker}"
            )
        )!;
        Assert.Equal(
            ("HQ laptop", "Editor"),
            (maker.Devices.Items.Single().Name, maker.Software.Items.Single().Name)
        );

        Assert.Equal(
            "1 device and 1 software or service name this manufacturer, so it cannot be deleted.",
            await ConflictOf(
                await client.DeleteAsync($"/api/company/manufacturers/{CompanyApplication.Maker}")
            )
        );
        Assert.Equal(
            ["A manufacturer with this name already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync("/api/company/manufacturers", new { name = "MAKER" }),
                "name"
            )
        );
    }

    // Locations

    [Fact]
    public Task A_location_round_trips_with_its_audit_rows() =>
        RoundTripAsync(
            "/api/company/locations",
            "Location",
            new
            {
                name = "Annex",
                countryCode = "BA",
                cityId = CompanyApplication.BanjaLuka,
                street = "Side",
                floor = 2,
            },
            new
            {
                name = "Annex",
                countryCode = "BA",
                cityId = CompanyApplication.BanjaLuka,
                stateId = CompanyApplication.Srpska,
                street = "Side",
                room = "12",
            }
        );

    [Fact]
    public async Task A_location_city_and_state_must_be_of_its_country()
    {
        var client = await Client();
        Assert.Equal(
            ["That city is not in the chosen country."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/locations",
                    new
                    {
                        name = "Far",
                        countryCode = "BA",
                        cityId = CompanyApplication.Zagreb,
                        street = "X",
                    }
                ),
                "cityId"
            )
        );
        Assert.Equal(
            ["That state is not in the chosen country."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/locations",
                    new
                    {
                        name = "Far",
                        countryCode = "HR",
                        cityId = CompanyApplication.Zagreb,
                        stateId = CompanyApplication.Srpska,
                        street = "X",
                    }
                ),
                "stateId"
            )
        );
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/locations",
                    new
                    {
                        name = new string('n', 51),
                        countryCode = "BA",
                        cityId = CompanyApplication.BanjaLuka,
                        street = "X",
                    }
                ),
                "name"
            )
        );
        Assert.Equal(
            ["A location with this name already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    "/api/company/locations",
                    new
                    {
                        name = "hq",
                        countryCode = "BA",
                        cityId = CompanyApplication.BanjaLuka,
                        street = "X",
                    }
                ),
                "name"
            )
        );
    }

    [Fact]
    public async Task A_location_shows_what_is_there_and_keeps_it()
    {
        var client = await Client();
        var hq = (
            await client.GetFromJsonAsync<LocationDetail>(
                $"/api/company/locations/{CompanyApplication.Hq}"
            )
        )!;
        Assert.Equal(
            ("Banja Luka", "HQ laptop", "Rocket contract"),
            (
                hq.City.Name,
                hq.Assets.Devices.Items.Single().Name,
                hq.Information.Items.Single().Name
            )
        );

        Assert.Equal(
            "1 asset and 1 piece of information are at this location, so it cannot be deleted.",
            await ConflictOf(
                await client.DeleteAsync($"/api/company/locations/{CompanyApplication.Hq}")
            )
        );
        var options = (
            await client.GetFromJsonAsync<App.Company.Locations.Options.Endpoint.Response>(
                "/api/company/locations/options"
            )
        )!;
        Assert.Equal(["BA", "HR"], options.Countries.Select(c => c.Id));
    }

    [Fact]
    public async Task The_license_list_filters_by_location()
    {
        var page = (
            await (await Client()).GetFromJsonAsync<Page<App.Licenses.Licenses.List.Endpoint.Item>>(
                $"/api/licenses/?locationId={CompanyApplication.Hq}"
            )
        )!;
        Assert.Equal(0, page.Total);
    }

    [Fact]
    public async Task Unknown_records_are_not_found()
    {
        var client = await Client();
        foreach (var url in new[] { "projects", "vendors", "manufacturers", "locations" })
        {
            var id = Guid.CreateVersion7();
            Assert.Equal(
                HttpStatusCode.NotFound,
                (await client.GetAsync($"/api/company/{url}/{id}")).StatusCode
            );
            Assert.Equal(
                HttpStatusCode.NotFound,
                (await client.DeleteAsync($"/api/company/{url}/{id}")).StatusCode
            );
        }
    }

    // Exports

    [Theory]
    [InlineData("/api/company/projects", "rocket", "Projects.Export", "Acme", "Owner")]
    [InlineData(
        "/api/company/vendors",
        "service",
        "Vendors.Export",
        "desk@service.example",
        "VAT Number"
    )]
    [InlineData(
        "/api/company/manufacturers",
        "maker",
        "Manufacturers.Export",
        "https://maker.example",
        "Devices"
    )]
    [InlineData(
        "/api/company/locations",
        "hq",
        "Locations.Export",
        "Bosnia and Herzegovina",
        "Postal Code"
    )]
    public async Task Exports_what_the_list_selects_named_filtered_when_narrowed(
        string url,
        string q,
        string name,
        string value,
        string header
    )
    {
        var client = await Client();

        var narrowed = await client.GetAsync($"{url}/export?q={q}");
        var all = await client.GetAsync($"{url}/export");

        Assert.Equal($"{name} - Filtered.xlsx", Spreadsheet.FileNameOf(narrowed));
        Assert.Equal($"{name}.xlsx", Spreadsheet.FileNameOf(all));
        var text = await Spreadsheet.TextOf(narrowed);
        Assert.Contains(value, text);
        Assert.Contains(header, text);
    }
}
