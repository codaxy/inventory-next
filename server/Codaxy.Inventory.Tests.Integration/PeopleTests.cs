using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Directory.Clients;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Projects;
using Codaxy.Inventory.App.Furnitures.Items;
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

using Handover = App.Directory.People.Handover.Endpoint.Response;
using Holdings = App.Directory.People.Holdings.Endpoint.Response;
using Item = App.Directory.People.List.Endpoint.Item;

/// <summary>
/// Hana holds two devices, a chair, a licence, three seats — one by name, one ended, one on her laptop
/// — a piece of information and a project; Ivo holds nothing.
/// </summary>
public class PeopleApplication : InventoryApplication
{
    public static readonly Guid Hana = Guid.CreateVersion7();
    public static readonly Guid Ivo = Guid.CreateVersion7();
    public static Guid Laptop;
    public static Guid License;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await Seed.BasicsAsync(context);

        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Tools" };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Maker" };
        var editor = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Editor",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = maker.Id,
        };
        context.AddRange(category, maker, editor);
        context.Persons.AddRange(
            new Person
            {
                Id = Hana,
                Name = "Hana Holder",
                Email = "hana@codaxy.com",
            },
            new Person
            {
                Id = Ivo,
                Name = "Ivo Idle",
                Email = "ivo@codaxy.com",
            }
        );
        await context.SaveChangesAsync();

        Laptop = await Seed.DeviceAsync(context, "Hana's laptop", holdsLicences: true);
        var monitor = await Seed.DeviceAsync(context, "Hana's monitor", holdsLicences: false);
        var perUser = await Seed.LicenseWithVolumeAsync(context, editor.Id, "Editor licence");
        var perDevice = await Seed.LicenseWithVolumeAsync(
            context,
            editor.Id,
            "Editor device licence",
            volumeType: Seed.PerDevice
        );
        License = perUser.License;

        foreach (
            var asset in await context
                .Assets.Where(a => a.Id == Laptop || a.Id == monitor || a.Id == License)
                .ToListAsync()
        )
            asset.PersonId = Hana;

        var chair = Guid.CreateVersion7();
        var basics = await Seed.BasicsAsync(context);
        context.Assets.Add(
            new Asset
            {
                Id = chair,
                Name = "Hana's chair",
                InventoryNumber = 700001,
                AssetTypeId = (
                    await context.AssetTypes.FirstAsync(t => t.Name == "Furniture and fixtures")
                ).Id,
                VendorId = basics.Vendor,
                PersonId = Hana,
                Description = "Grey",
                LastModified = DateTimeOffset.UtcNow,
                Furniture = new Furniture { AssetId = chair, Model = "Ergo" },
            }
        );
        var type = new InformationType { Id = Guid.CreateVersion7(), Name = "Contract" };
        var client = new Client { Id = Guid.CreateVersion7(), Name = "Acme" };
        context.AddRange(type, client);
        context.Informations.Add(
            new Information
            {
                Id = Guid.CreateVersion7(),
                Name = "Hana's contract",
                InformationTypeId = type.Id,
                PersonId = Hana,
            }
        );
        context.Projects.Add(
            new Project
            {
                Id = Guid.CreateVersion7(),
                Name = "Hana's project",
                ClientId = client.Id,
                ProjectOwnerId = Hana,
            }
        );
        await context.SaveChangesAsync();

        await Seed.ActivationAsync(context, perUser.Volume, person: Hana);
        await Seed.ActivationAsync(
            context,
            perUser.Volume,
            person: Hana,
            deactivated: new DateOnly(2026, 3, 1)
        );
        await Seed.ActivationAsync(context, perDevice.Volume, device: Laptop);
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class PeopleTests(PeopleApplication app) : IClassFixture<PeopleApplication>
{
    private const string Url = "/api/directory/people";

    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    [Fact]
    public async Task Refuses_a_caller_without_a_session() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync($"{Url}/")).StatusCode
        );

    [Fact]
    public async Task A_person_is_created_edited_and_deleted_with_their_audit_rows()
    {
        var client = await Client();

        var created = await client.PostAsJsonAsync(
            Url,
            new { name = "  Nina New ", email = " Nina@Codaxy.com " }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var nina = (await created.Content.ReadFromJsonAsync<PersonDetail>())!;
        Assert.Equal(("Nina New", "Nina@Codaxy.com"), (nina.Name, nina.Email));

        var edited = await client.PutAsJsonAsync(
            $"{Url}/{nina.Id}",
            new { name = "Nina Newer", email = "nina@codaxy.com" }
        );
        Assert.Equal("Nina Newer", (await edited.Content.ReadFromJsonAsync<PersonDetail>())!.Name);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{nina.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{nina.Id}")).StatusCode
        );

        var audit = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == nina.Id).OrderBy(a => a.TimeCreated).ToListAsync()
        );
        Assert.Equal(["Create", "Update", "Delete"], audit.Select(a => a.ActionType));
        Assert.All(audit, a => Assert.Equal("Person", a.Table));
    }

    [Fact]
    public async Task A_name_or_email_another_person_has_is_refused_whatever_its_case_and_spacing()
    {
        var client = await Client();

        Assert.Equal(
            ["A person with this name already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    Url,
                    new { name = "hana HOLDER", email = "other@codaxy.com" }
                ),
                "name"
            )
        );
        Assert.Equal(
            ["A person with this email address already exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    Url,
                    new { name = "Other", email = " HANA@codaxy.com" }
                ),
                "email"
            )
        );
        // Their own name and email are theirs to keep.
        Assert.Equal(
            HttpStatusCode.OK,
            (
                await client.PutAsJsonAsync(
                    $"{Url}/{PeopleApplication.Ivo}",
                    new { name = "IVO IDLE", email = "IVO@codaxy.com" }
                )
            ).StatusCode
        );
        await client.PutAsJsonAsync(
            $"{Url}/{PeopleApplication.Ivo}",
            new { name = "Ivo Idle", email = "ivo@codaxy.com" }
        );
    }

    [Theory]
    [InlineData("name", " ")]
    [InlineData("email", null)]
    [InlineData("email", "not an address")]
    public async Task A_person_needs_a_name_and_an_email_address(string field, string? value)
    {
        var form = new Dictionary<string, object?>
        {
            ["name"] = "Valid",
            ["email"] = "valid@codaxy.com",
        };
        form[field] = value;

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field));
    }

    [Fact]
    public async Task A_person_anything_is_attached_to_is_not_deleted_and_says_what()
    {
        var response = await (await Client()).DeleteAsync($"{Url}/{PeopleApplication.Hana}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "This person holds 4 assets, 2 seats, 1 piece of information and 1 project, so they cannot be deleted.",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.True(
            await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == PeopleApplication.Laptop))
        );
    }

    [Fact]
    public async Task An_unknown_person_is_not_found()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{id}/holdings")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{id}/handover")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await client.PutAsJsonAsync(
                    $"{Url}/{id}",
                    new { name = "X", email = "x@codaxy.com" }
                )
            ).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Holdings_count_every_kind_and_mark_the_seat_on_their_device()
    {
        var h = (
            await (await Client()).GetFromJsonAsync<Holdings>(
                $"{Url}/{PeopleApplication.Hana}/holdings"
            )
        )!;

        Assert.Equal(["Hana's laptop", "Hana's monitor"], h.Devices.Items.Select(d => d.Name));
        Assert.Equal(2, h.Devices.Total);
        Assert.Equal(
            ("Hana's chair", "Ergo"),
            (h.Furniture.Items.Single().Name, h.Furniture.Items.Single().Model)
        );
        Assert.Equal("Editor licence", h.Licenses.Items.Single().Name);
        Assert.Equal((3, 2), (h.Seats.Total, h.Seats.Active));
        // Active first; the ended one last.
        Assert.NotNull(h.Seats.Items[^1].DeactivationDate);
        Assert.Equal(
            "Hana's laptop",
            Assert.Single(h.Seats.Items, s => s.Device is not null).Device
        );
        Assert.Equal("Hana's contract", h.Information.Items.Single().Name);
        Assert.Equal(
            ("Hana's project", "Acme"),
            (h.Projects.Items.Single().Name, h.Projects.Items.Single().Client)
        );

        var idle = (
            await (await Client()).GetFromJsonAsync<Holdings>(
                $"{Url}/{PeopleApplication.Ivo}/holdings"
            )
        )!;
        Assert.Equal(
            0,
            idle.Devices.Total + idle.Furniture.Total + idle.Licenses.Total + idle.Seats.Total
        );
    }

    [Fact]
    public async Task The_handover_sheet_lists_every_asset_they_hold()
    {
        var sheet = (
            await (await Client()).GetFromJsonAsync<Handover>(
                $"{Url}/{PeopleApplication.Hana}/handover"
            )
        )!;

        Assert.Equal("Hana Holder", sheet.Name);
        Assert.Equal(4, sheet.Assets.Count);
        Assert.Contains(
            sheet.Assets,
            a =>
                a
                    is {
                        Name: "Hana's chair",
                        Description: "Grey",
                        Type: "Furniture and fixtures",
                        Number: 700001
                    }
        );
    }

    [Fact]
    public async Task Lists_with_what_each_holds_searching_sorting_and_paging()
    {
        var hana = Assert.Single((await ListAsync("q=hana@")).Items);
        Assert.Equal((4, 1), (hana.Assets, hana.Seats));

        var byAssets = (await ListAsync("sort=-assets&pageSize=100")).Items;
        Assert.Equal(
            byAssets.OrderByDescending(p => p.Assets).Select(p => p.Assets),
            byAssets.Select(p => p.Assets)
        );
        var byName = (await ListAsync("pageSize=100")).Items;
        Assert.Equal(
            byName.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Name).Count(),
            byName.Count
        );
        Assert.Single((await ListAsync("pageSize=1")).Items);
        Assert.Empty((await ListAsync("page=999")).Items);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Url}/?sort=age")).StatusCode
        );
    }

    [Fact]
    public async Task The_licence_and_activation_lists_filter_by_person()
    {
        var client = await Client();

        var licences = (
            await client.GetFromJsonAsync<Page<App.Licenses.Licenses.List.Endpoint.Item>>(
                $"/api/licenses/?personId={PeopleApplication.Hana}"
            )
        )!;
        Assert.Equal(["Editor licence"], licences.Items.Select(l => l.Name));

        var seats = (
            await client.GetFromJsonAsync<Page<App.Licenses.Activations.List.Endpoint.Item>>(
                $"/api/licenses/activations/?personId={PeopleApplication.Hana}"
            )
        )!;
        Assert.Equal(3, seats.Total);
        Assert.Equal(
            0,
            (
                await client.GetFromJsonAsync<Page<App.Licenses.Activations.List.Endpoint.Item>>(
                    $"/api/licenses/activations/?personId={PeopleApplication.Ivo}"
                )
            )!.Total
        );
    }
}
