using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Vendors;
using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.ElectronicDevices.Types.List.Endpoint.Item;

/// <summary>
/// Three tags and two types — a laptop that holds licences, carries two tags and has two devices, and
/// a monitor with neither; a test that changes data works on a type of its own.
/// </summary>
public class TypeApplication : InventoryApplication
{
    public static readonly Guid Mobile = Guid.CreateVersion7();
    public static readonly Guid HasData = Guid.CreateVersion7();
    public static readonly Guid Spare = Guid.CreateVersion7();
    public static readonly Guid Laptop = Guid.CreateVersion7();
    public static readonly Guid Monitor = Guid.CreateVersion7();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        context.ElectronicDeviceTags.AddRange(
            new ElectronicDeviceTag { Id = Mobile, Name = "Mobile" },
            new ElectronicDeviceTag { Id = HasData, Name = "HasData" },
            new ElectronicDeviceTag { Id = Spare, Name = "Spare" }
        );
        context.ElectronicDeviceTypes.AddRange(
            new ElectronicDeviceType
            {
                Id = Laptop,
                Name = "Laptop",
                HoldLicences = true,
                Description = "Portable computer",
                Tags =
                [
                    new() { ElectronicDeviceTagId = Mobile },
                    new() { ElectronicDeviceTagId = HasData },
                ],
            },
            new ElectronicDeviceType { Id = Monitor, Name = "Monitor" }
        );

        var category = new AssetCategory { Id = Guid.CreateVersion7(), Name = "Hardware" };
        var assetType = new AssetType
        {
            Id = Guid.CreateVersion7(),
            Name = "Electronic Device",
            AssetCategoryId = category.Id,
        };
        var vendor = new Vendor { Id = Guid.CreateVersion7(), Name = "Vendor" };
        var person = new Person { Id = Guid.CreateVersion7(), Name = "Owner" };
        context.AddRange(category, assetType, vendor, person);

        foreach (var name in new[] { "lap-1", "lap-2" })
        {
            var id = Guid.CreateVersion7();
            context.Add(
                new Asset
                {
                    Id = id,
                    Name = name,
                    AssetTypeId = assetType.Id,
                    VendorId = vendor.Id,
                    PersonId = person.Id,
                    LastModified = DateTimeOffset.UtcNow,
                    ElectronicDevice = new ElectronicDevice
                    {
                        AssetId = id,
                        ElectronicDeviceTypeId = Laptop,
                    },
                }
            );
        }

        await context.SaveChangesAsync();
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class ElectronicDeviceTypeTests(TypeApplication app) : IClassFixture<TypeApplication>
{
    private const string Url = "/api/electronic-devices/types";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<TypeDetail> CreateAsync(string name, params Guid[] tags)
    {
        var response = await (await Client()).PostAsJsonAsync(
            Url,
            new
            {
                name,
                holdsLicences = true,
                description = "made by a test",
                tagIds = tags,
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TypeDetail>())!;
    }

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    private Task<List<AuditLog>> AuditFor(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id).OrderBy(a => a.TimeCreated).ToListAsync()
        );

    private Task<List<Guid>> LinksOf(Guid type) =>
        app.InScopeAsync(c =>
            c.ElectronicDeviceTypeElectronicDeviceTags.Where(l => l.ElectronicDeviceTypeId == type)
                .Select(l => l.ElectronicDeviceTagId)
                .ToListAsync()
        );

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creating_a_type_links_its_tags_and_is_audited_as_the_user()
    {
        var type = await CreateAsync("  Tablet  ", TypeApplication.Mobile, TypeApplication.Spare);

        Assert.Equal(("Tablet", true), (type.Name, type.HoldsLicences));
        Assert.Equal(["Mobile", "Spare"], type.Tags.Select(t => t.Name));
        Assert.Equal(0, type.DeviceCount);
        Assert.Equal(
            new HashSet<Guid> { TypeApplication.Mobile, TypeApplication.Spare },
            (await LinksOf(type.Id)).ToHashSet()
        );

        var audit = Assert.Single(await AuditFor(type.Id));
        Assert.Equal(
            ("Create", "ElectronicDeviceType", Editor),
            (audit.ActionType, audit.Table, audit.Email)
        );
        Assert.Contains("\"HoldLicences\": true", audit.NewValuesJson);
    }

    [Fact]
    public async Task Editing_a_type_changes_its_fields_and_makes_its_links_match()
    {
        var type = await CreateAsync("Phablet", TypeApplication.Mobile, TypeApplication.HasData);

        var response = await (await Client()).PutAsJsonAsync(
            $"{Url}/{type.Id}",
            new
            {
                name = "Big phone",
                holdsLicences = false,
                description = "",
                tagIds = new[] { TypeApplication.HasData, TypeApplication.Spare },
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<TypeDetail>())!;
        Assert.Equal(
            ("Big phone", false, (string?)null),
            (edited.Name, edited.HoldsLicences, edited.Description)
        );
        Assert.Equal(["HasData", "Spare"], edited.Tags.Select(t => t.Name));
        Assert.Equal(
            new HashSet<Guid> { TypeApplication.HasData, TypeApplication.Spare },
            (await LinksOf(type.Id)).ToHashSet()
        );

        var update = (await AuditFor(type.Id)).Last();
        Assert.Equal("Update", update.ActionType);
        Assert.Contains("\"Name\": \"Phablet\"", update.OldValuesJson);
        Assert.Contains("\"Name\": \"Big phone\"", update.NewValuesJson);
    }

    [Fact]
    public async Task Deleting_an_unused_type_drops_its_links_and_is_audited()
    {
        var type = await CreateAsync("Doomed", TypeApplication.Mobile);
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{type.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{type.Id}")).StatusCode
        );
        Assert.Empty(await LinksOf(type.Id));
        Assert.True(
            await app.InScopeAsync(c =>
                c.ElectronicDeviceTags.AnyAsync(t => t.Id == TypeApplication.Mobile)
            )
        );

        var delete = (await AuditFor(type.Id)).Last();
        Assert.Equal("Delete", delete.ActionType);
        Assert.Null(delete.NewValuesJson);
    }

    [Fact]
    public async Task A_type_devices_use_is_not_deleted_and_says_how_many()
    {
        var response = await (await Client()).DeleteAsync($"{Url}/{TypeApplication.Laptop}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.StartsWith("2 devices", problem!.Title);
        Assert.True(
            await app.InScopeAsync(c =>
                c.ElectronicDeviceTypes.AnyAsync(t => t.Id == TypeApplication.Laptop)
            )
        );
        Assert.Equal(2, (await LinksOf(TypeApplication.Laptop)).Count);
    }

    [Theory]
    [InlineData("{\"tagIds\":[]}")]
    [InlineData("{\"name\":\"   \"}")]
    public async Task A_type_needs_a_name(string body)
    {
        var response = await (await Client()).PostAsync(
            Url,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "name",
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
    }

    [Fact]
    public async Task Refuses_a_name_or_description_past_their_length()
    {
        var client = await Client();

        var longName = await client.PostAsJsonAsync(Url, new { name = new string('n', 101) });
        var longDescription = await client.PostAsJsonAsync(
            Url,
            new { name = "ok", description = new string('d', 1001) }
        );

        Assert.Contains(
            "name",
            (await longName.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
        Assert.Contains(
            "description",
            (await longDescription.Content.ReadFromJsonAsync<ValidationProblemDetails>())!
                .Errors
                .Keys
        );
    }

    [Fact]
    public async Task A_name_is_taken_whatever_its_case_but_a_type_keeps_its_own()
    {
        var client = await Client();

        var duplicate = await client.PostAsJsonAsync(Url, new { name = " LAPTOP " });
        var own = await client.PutAsJsonAsync(
            $"{Url}/{TypeApplication.Monitor}",
            new { name = "monitor" }
        );
        var others = await client.PutAsJsonAsync(
            $"{Url}/{TypeApplication.Monitor}",
            new { name = "Laptop" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(
            ["A type with this name already exists."],
            (await duplicate.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors["name"]
        );
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, others.StatusCode);

        await client.PutAsJsonAsync($"{Url}/{TypeApplication.Monitor}", new { name = "Monitor" });
    }

    [Fact]
    public async Task Refuses_a_tag_that_does_not_exist_and_saves_nothing()
    {
        var before = await app.InScopeAsync(c => c.ElectronicDeviceTypes.CountAsync());

        var response = await (await Client()).PostAsJsonAsync(
            Url,
            new { name = "Ghost", tagIds = new[] { Guid.CreateVersion7() } }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "tagIds",
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
        Assert.Equal(before, await app.InScopeAsync(c => c.ElectronicDeviceTypes.CountAsync()));
    }

    [Fact]
    public async Task An_unknown_type_is_not_found_to_read_edit_or_delete()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", new { name = "x" })).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Reads_a_type_with_its_tags_and_devices()
    {
        var type = await (await Client()).GetFromJsonAsync<TypeDetail>(
            $"{Url}/{TypeApplication.Laptop}"
        );

        Assert.Equal(
            ("Laptop", true, "Portable computer", 2),
            (type!.Name, type.HoldsLicences, type.Description, type.DeviceCount)
        );
        Assert.Equal(["HasData", "Mobile"], type.Tags.Select(t => t.Name));
    }

    [Fact]
    public async Task Lists_with_counts_searching_name_and_description()
    {
        var laptop = Assert.Single((await ListAsync("q=portable")).Items);

        Assert.Equal(
            ("Laptop", true, 2, 2),
            (laptop.Name, laptop.HoldsLicences, laptop.TagCount, laptop.DeviceCount)
        );
        Assert.Equal(["HasData", "Mobile"], laptop.FirstTags);
        Assert.Equal(0, Assert.Single((await ListAsync("q=MONITOR")).Items).DeviceCount);
    }

    [Fact]
    public async Task Filters_by_every_given_tag_and_by_licences()
    {
        var both = await ListAsync(
            $"tagId={TypeApplication.Mobile}&tagId={TypeApplication.HasData}"
        );
        var spare = await ListAsync(
            $"tagId={TypeApplication.Mobile}&tagId={TypeApplication.Spare}&q=laptop"
        );
        var notHolding = await ListAsync("holdsLicences=false&q=monitor");
        var holding = await ListAsync("holdsLicences=true&q=monitor");

        Assert.Contains(both.Items, t => t.Name == "Laptop");
        Assert.Empty(spare.Items);
        Assert.Single(notHolding.Items);
        Assert.Empty(holding.Items);
    }

    [Fact]
    public async Task Sorts_by_name_tags_or_devices_and_pages()
    {
        Assert.Equal("Laptop", (await ListAsync("sort=-devices")).Items[0].Name);
        var byTags = (await ListAsync("sort=-tags&pageSize=100")).Items;
        Assert.Equal(
            byTags.OrderByDescending(t => t.TagCount).Select(t => t.TagCount),
            byTags.Select(t => t.TagCount)
        );
        Assert.Equal("Laptop", (await ListAsync("sort=devices&q=o")).Items[^1].Name);

        var first = await ListAsync("pageSize=1");
        Assert.Single(first.Items);
        Assert.True(first.Total >= 2);

        var client = await Client();
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?sort=description")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?pageSize=101")).StatusCode
        );
        Assert.Empty((await ListAsync("page=999")).Items);
    }

    [Fact]
    public async Task Options_offer_every_tag_by_name()
    {
        var options = await (
            await Client()
        ).GetFromJsonAsync<App.ElectronicDevices.Types.Options.Endpoint.Response>($"{Url}/options");

        Assert.Equal(["HasData", "Mobile", "Spare"], options!.Tags.Select(t => t.Text));
    }
}
