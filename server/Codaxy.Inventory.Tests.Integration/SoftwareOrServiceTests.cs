using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Volumes;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Licenses.SoftwareServices.List.Endpoint.Item;

/// <summary>
/// Two categories, two manufacturers and two entries — Windows, which a licence volume names, and
/// Postgres, which nothing does; a test that changes data works on an entry of its own.
/// </summary>
public class SoftwareOrServiceApplication : InventoryApplication
{
    public static readonly Guid Os = Guid.CreateVersion7();
    public static readonly Guid Database = Guid.CreateVersion7();
    public static readonly Guid Microsoft = Guid.CreateVersion7();
    public static readonly Guid Community = Guid.CreateVersion7();
    public static readonly Guid Windows = Guid.CreateVersion7();
    public static readonly Guid Postgres = Guid.CreateVersion7();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        context.SoftwareOrServiceCategories.AddRange(
            new SoftwareOrServiceCategory { Id = Os, Name = "Test OS" },
            new SoftwareOrServiceCategory { Id = Database, Name = "Test database" }
        );
        context.Manufacturers.AddRange(
            new Manufacturer { Id = Microsoft, Name = "Test Microsoft" },
            new Manufacturer { Id = Community, Name = "Test community" }
        );
        context.SoftwareOrServices.AddRange(
            new SoftwareOrService
            {
                Id = Windows,
                Name = "Windows",
                SoftwareOrServiceCategoryId = Os,
                ManufacturerId = Microsoft,
                Url = "https://microsoft.com/windows",
            },
            new SoftwareOrService
            {
                Id = Postgres,
                Name = "Postgres",
                SoftwareOrServiceCategoryId = Database,
                ManufacturerId = Community,
            }
        );
        await context.SaveChangesAsync();

        await Seed.LicenseWithVolumeAsync(context, Windows);
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class SoftwareOrServiceTests(SoftwareOrServiceApplication app)
    : IClassFixture<SoftwareOrServiceApplication>
{
    private const string Url = "/api/licenses/software-services";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private static object Form(string name, string? url = null) =>
        new
        {
            name,
            categoryId = SoftwareOrServiceApplication.Database,
            manufacturerId = SoftwareOrServiceApplication.Community,
            url,
        };

    private async Task<SoftwareOrServiceDetail> CreateAsync(string name)
    {
        var response = await (await Client()).PostAsJsonAsync(Url, Form(name, "https://x.test"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<SoftwareOrServiceDetail>())!;
    }

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    private Task<List<AuditLog>> AuditFor(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id).OrderBy(a => a.TimeCreated).ToListAsync()
        );

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creating_an_entry_saves_it_trimmed_and_is_audited_as_the_user()
    {
        var entry = await CreateAsync("  Redis  ");

        Assert.Equal(
            ("Redis", "Test database", "Test community", "https://x.test", 0),
            (entry.Name, entry.Category.Name, entry.Manufacturer.Name, entry.Url, entry.VolumeCount)
        );

        var audit = Assert.Single(await AuditFor(entry.Id));
        Assert.Equal(
            ("Create", "SoftwareOrService", Editor),
            (audit.ActionType, audit.Table, audit.Email)
        );
        Assert.Contains("\"Name\": \"Redis\"", audit.NewValuesJson);
    }

    [Fact]
    public async Task Editing_an_entry_changes_every_field_and_is_audited()
    {
        var entry = await CreateAsync("Mongo");

        var response = await (await Client()).PutAsJsonAsync(
            $"{Url}/{entry.Id}",
            new
            {
                name = "MongoDB",
                categoryId = SoftwareOrServiceApplication.Os,
                manufacturerId = SoftwareOrServiceApplication.Microsoft,
                url = "",
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<SoftwareOrServiceDetail>())!;
        Assert.Equal(
            ("MongoDB", "Test OS", "Test Microsoft", (string?)null),
            (edited.Name, edited.Category.Name, edited.Manufacturer.Name, edited.Url)
        );

        var update = (await AuditFor(entry.Id)).Last();
        Assert.Equal("Update", update.ActionType);
        Assert.Contains("\"Name\": \"Mongo\"", update.OldValuesJson);
        Assert.Contains("\"Name\": \"MongoDB\"", update.NewValuesJson);
    }

    [Fact]
    public async Task Deleting_an_entry_no_volume_names_removes_it_and_is_audited()
    {
        var entry = await CreateAsync("Doomed");
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{entry.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{entry.Id}")).StatusCode
        );
        Assert.Equal("Delete", (await AuditFor(entry.Id)).Last().ActionType);
    }

    [Fact]
    public async Task An_entry_a_volume_names_is_not_deleted_and_nothing_cascades()
    {
        var volumesBefore = await app.InScopeAsync(c => c.Volumes.CountAsync());
        var activationsBefore = await app.InScopeAsync(c => c.Activations.CountAsync());

        var response = await (await Client()).DeleteAsync(
            $"{Url}/{SoftwareOrServiceApplication.Windows}"
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.StartsWith(
            "A licence volume",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.Equal(volumesBefore, await app.InScopeAsync(c => c.Volumes.CountAsync()));
        Assert.Equal(activationsBefore, await app.InScopeAsync(c => c.Activations.CountAsync()));
    }

    [Theory]
    [InlineData("{\"categoryId\":null}", "name")]
    [InlineData("{\"name\":\"x\"}", "categoryId")]
    [InlineData(
        "{\"name\":\"x\",\"categoryId\":\"00000000-0000-0000-0000-000000000001\"}",
        "manufacturerId"
    )]
    public async Task An_entry_needs_a_name_a_category_and_a_manufacturer(string body, string field)
    {
        var response = await (await Client()).PostAsync(
            Url,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            field,
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
    }

    [Fact]
    public async Task Refuses_a_category_or_manufacturer_that_does_not_exist()
    {
        var client = await Client();

        var category = await client.PostAsJsonAsync(
            Url,
            new
            {
                name = "Ghost",
                categoryId = Guid.CreateVersion7(),
                manufacturerId = SoftwareOrServiceApplication.Community,
            }
        );
        var manufacturer = await client.PostAsJsonAsync(
            Url,
            new
            {
                name = "Ghost",
                categoryId = SoftwareOrServiceApplication.Os,
                manufacturerId = Guid.CreateVersion7(),
            }
        );

        Assert.Contains(
            "categoryId",
            (await category.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
        Assert.Contains(
            "manufacturerId",
            (await manufacturer.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
    }

    [Fact]
    public async Task Refuses_a_name_or_url_past_their_length()
    {
        var client = await Client();

        var name = await client.PostAsJsonAsync(Url, Form(new string('n', 201)));
        var url = await client.PostAsJsonAsync(Url, Form("ok", new string('u', 501)));

        Assert.Contains(
            "name",
            (await name.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
        Assert.Contains(
            "url",
            (await url.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
    }

    [Fact]
    public async Task A_name_is_taken_whatever_its_case_but_an_entry_keeps_its_own()
    {
        var client = await Client();

        var duplicate = await client.PostAsJsonAsync(Url, Form(" WINDOWS "));
        var own = await client.PutAsJsonAsync(
            $"{Url}/{SoftwareOrServiceApplication.Postgres}",
            Form("postgres")
        );

        Assert.Equal(
            ["A software or service with this name already exists."],
            (await duplicate.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors["name"]
        );
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        await client.PutAsJsonAsync(
            $"{Url}/{SoftwareOrServiceApplication.Postgres}",
            Form("Postgres")
        );
    }

    [Fact]
    public async Task An_unknown_entry_is_not_found_to_read_edit_or_delete()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", Form("x"))).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Reads_an_entry_with_its_volume_count()
    {
        var entry = await (await Client()).GetFromJsonAsync<SoftwareOrServiceDetail>(
            $"{Url}/{SoftwareOrServiceApplication.Windows}"
        );

        Assert.Equal(
            ("Windows", "Test OS", 1),
            (entry!.Name, entry.Category.Name, entry.VolumeCount)
        );
    }

    [Fact]
    public async Task Searches_name_url_category_and_manufacturer()
    {
        Assert.Equal("Windows", Assert.Single((await ListAsync("q=microsoft.com")).Items).Name);
        Assert.Contains((await ListAsync("q=test+community")).Items, i => i.Name == "Postgres");
        Assert.Contains((await ListAsync("q=TEST+OS")).Items, i => i.Name == "Windows");
    }

    [Fact]
    public async Task Filters_by_category_and_manufacturer()
    {
        var os = await ListAsync($"categoryId={SoftwareOrServiceApplication.Os}");
        var community = await ListAsync($"manufacturerId={SoftwareOrServiceApplication.Community}");

        Assert.All(os.Items, i => Assert.Equal("Test OS", i.Category));
        Assert.Contains(os.Items, i => i.Name == "Windows");
        Assert.All(community.Items, i => Assert.Equal("Test community", i.Manufacturer));
    }

    [Fact]
    public async Task Sorts_by_each_column_and_pages()
    {
        var byVolumes = (await ListAsync("sort=-volumes&pageSize=100")).Items;
        Assert.Equal(
            byVolumes.OrderByDescending(i => i.VolumeCount).Select(i => i.VolumeCount),
            byVolumes.Select(i => i.VolumeCount)
        );

        // The database's collation decides the order of text; the two directions mirror each other.
        foreach (var column in new[] { "name", "category", "manufacturer" })
        {
            var up = (await ListAsync($"sort={column}&pageSize=100")).Items;
            var down = (await ListAsync($"sort=-{column}&pageSize=100")).Items;
            Assert.Equal(up.Count, down.Count);
            Assert.NotEqual(up[0].Id, down[0].Id);
        }

        var first = await ListAsync("pageSize=1");
        Assert.Single(first.Items);
        Assert.True(first.Total >= 2);
        Assert.Empty((await ListAsync("page=999")).Items);

        var client = await Client();
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?sort=url")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?pageSize=101")).StatusCode
        );
    }

    [Fact]
    public async Task Options_offer_categories_and_manufacturers_by_name()
    {
        var options = await (
            await Client()
        ).GetFromJsonAsync<App.Licenses.SoftwareServices.Options.Endpoint.Response>(
            $"{Url}/options"
        );

        Assert.Contains(options!.Categories, c => c.Text == "Test OS");
        Assert.Contains(options.Manufacturers, m => m.Text == "Test Microsoft");
    }
}
