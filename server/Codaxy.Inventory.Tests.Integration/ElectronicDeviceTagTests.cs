using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>Three types and two tags; a test that changes data works on a tag of its own.</summary>
public class TagApplication : InventoryApplication
{
    public static readonly Guid Laptop = Guid.CreateVersion7();
    public static readonly Guid Phone = Guid.CreateVersion7();
    public static readonly Guid Monitor = Guid.CreateVersion7();
    public static readonly Guid Mobile = Guid.CreateVersion7();
    public static readonly Guid Spare = Guid.CreateVersion7();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        context.ElectronicDeviceTypes.AddRange(
            new ElectronicDeviceType { Id = Laptop, Name = "Laptop" },
            new ElectronicDeviceType { Id = Phone, Name = "Phone" },
            new ElectronicDeviceType { Id = Monitor, Name = "Monitor" }
        );
        context.ElectronicDeviceTags.AddRange(
            new ElectronicDeviceTag
            {
                Id = Mobile,
                Name = "Mobile",
                Description = "Carried around",
                Types =
                [
                    new() { ElectronicDeviceTypeId = Laptop },
                    new() { ElectronicDeviceTypeId = Phone },
                ],
            },
            new ElectronicDeviceTag { Id = Spare, Name = "Spare" }
        );

        await context.SaveChangesAsync();
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class ElectronicDeviceTagTests(TagApplication app) : IClassFixture<TagApplication>
{
    private const string Url = "/api/electronic-devices/tags";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<TagDetail> CreateAsync(string name, params Guid[] types)
    {
        var response = await (await Client()).PostAsJsonAsync(
            Url,
            new
            {
                name,
                description = "made by a test",
                typeIds = types,
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TagDetail>())!;
    }

    private Task<List<AuditLog>> AuditFor(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id).OrderBy(a => a.TimeCreated).ToListAsync()
        );

    private Task<List<Guid>> LinksOf(Guid tag) =>
        app.InScopeAsync(c =>
            c.ElectronicDeviceTypeElectronicDeviceTags.Where(l => l.ElectronicDeviceTagId == tag)
                .Select(l => l.ElectronicDeviceTypeId)
                .ToListAsync()
        );

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creating_a_tag_links_its_types_and_is_audited_as_the_user()
    {
        var tag = await CreateAsync("  Portable  ", TagApplication.Laptop, TagApplication.Monitor);

        Assert.Equal("Portable", tag.Name);
        Assert.Equal(["Laptop", "Monitor"], tag.Types.Select(t => t.Name));
        Assert.Equal(
            new HashSet<Guid> { TagApplication.Laptop, TagApplication.Monitor },
            (await LinksOf(tag.Id)).ToHashSet()
        );

        var audit = Assert.Single(await AuditFor(tag.Id));
        Assert.Equal(
            ("Create", "ElectronicDeviceTag", Editor),
            (audit.ActionType, audit.Table, audit.Email)
        );
        Assert.Contains("\"Name\": \"Portable\"", audit.NewValuesJson);
        Assert.Null(audit.OldValuesJson);
    }

    [Fact]
    public async Task Editing_a_tag_changes_its_words_and_makes_its_links_match()
    {
        var tag = await CreateAsync("Handheld", TagApplication.Laptop, TagApplication.Phone);

        var response = await (await Client()).PutAsJsonAsync(
            $"{Url}/{tag.Id}",
            new
            {
                name = "Pocketable",
                description = "",
                typeIds = new[] { TagApplication.Phone, TagApplication.Monitor },
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<TagDetail>())!;
        Assert.Equal(("Pocketable", (string?)null), (edited.Name, edited.Description));
        Assert.Equal(["Monitor", "Phone"], edited.Types.Select(t => t.Name));
        Assert.Equal(
            new HashSet<Guid> { TagApplication.Phone, TagApplication.Monitor },
            (await LinksOf(tag.Id)).ToHashSet()
        );

        var update = (await AuditFor(tag.Id)).Last();
        Assert.Equal("Update", update.ActionType);
        Assert.Contains("\"Name\": \"Handheld\"", update.OldValuesJson);
        Assert.Contains("\"Name\": \"Pocketable\"", update.NewValuesJson);
    }

    [Fact]
    public async Task Editing_only_the_links_keeps_the_tag_and_saves_them()
    {
        var tag = await CreateAsync("Links only", TagApplication.Laptop);

        await (await Client()).PutAsJsonAsync(
            $"{Url}/{tag.Id}",
            new
            {
                name = "Links only",
                description = "made by a test",
                typeIds = Array.Empty<Guid>(),
            }
        );

        Assert.Empty(await LinksOf(tag.Id));
    }

    [Fact]
    public async Task Deleting_a_tag_drops_its_links_and_is_audited()
    {
        var tag = await CreateAsync("Doomed", TagApplication.Laptop, TagApplication.Phone);
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{tag.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{tag.Id}")).StatusCode
        );
        Assert.Empty(await LinksOf(tag.Id));

        // The types themselves are untouched.
        Assert.Equal(3, await app.InScopeAsync(c => c.ElectronicDeviceTypes.CountAsync()));

        var delete = (await AuditFor(tag.Id)).Last();
        Assert.Equal("Delete", delete.ActionType);
        Assert.Contains("\"Name\": \"Doomed\"", delete.OldValuesJson);
        Assert.Null(delete.NewValuesJson);
    }

    [Theory]
    [InlineData("{\"typeIds\":[]}", "name")]
    [InlineData("{\"name\":\"   \"}", "name")]
    [InlineData("{\"name\":\"" + "x" + "\",\"description\":\"" + "\"}", null)]
    public async Task A_tag_needs_a_name(string body, string? field)
    {
        var response = await (await Client()).PostAsync(
            Url,
            new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        );

        if (field is null)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return;
        }

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains(field, problem!.Errors.Keys);
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
    public async Task Refuses_a_type_that_does_not_exist_and_saves_nothing()
    {
        var before = await app.InScopeAsync(c => c.ElectronicDeviceTags.CountAsync());

        var response = await (await Client()).PostAsJsonAsync(
            Url,
            new { name = "Ghost", typeIds = new[] { Guid.CreateVersion7() } }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "typeIds",
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
        Assert.Equal(before, await app.InScopeAsync(c => c.ElectronicDeviceTags.CountAsync()));
    }

    [Fact]
    public async Task An_unknown_tag_is_not_found_to_read_edit_or_delete()
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
    public async Task Reads_a_tag_with_its_types()
    {
        var tag = await (await Client()).GetFromJsonAsync<TagDetail>(
            $"{Url}/{TagApplication.Mobile}"
        );

        Assert.Equal(("Mobile", "Carried around"), (tag!.Name, tag.Description));
        Assert.Equal(["Laptop", "Phone"], tag.Types.Select(t => t.Name));
    }

    [Fact]
    public async Task Lists_with_type_counts_searching_name_and_description()
    {
        var client = await Client();

        var carried = await client.GetFromJsonAsync<
            Page<App.ElectronicDevices.Tags.List.Endpoint.Item>
        >($"{Url}/?q=carried");
        var mobile = Assert.Single(carried!.Items);
        Assert.Equal(("Mobile", 2), (mobile.Name, mobile.TypeCount));

        var spare = await client.GetFromJsonAsync<
            Page<App.ElectronicDevices.Tags.List.Endpoint.Item>
        >($"{Url}/?q=SPARE");
        Assert.Equal(0, Assert.Single(spare!.Items).TypeCount);
    }

    [Fact]
    public async Task Sorts_by_name_or_type_count_and_pages()
    {
        var client = await Client();

        var byTypes = await client.GetFromJsonAsync<
            Page<App.ElectronicDevices.Tags.List.Endpoint.Item>
        >($"{Url}/?sort=-types&q=mobile");
        var byName = await client.GetFromJsonAsync<
            Page<App.ElectronicDevices.Tags.List.Endpoint.Item>
        >($"{Url}/?pageSize=1");

        Assert.Equal("Mobile", byTypes!.Items[0].Name);
        Assert.Single(byName!.Items);
        Assert.True(byName.Total >= 2);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?sort=description")).StatusCode
        );
    }

    [Fact]
    public async Task Options_offer_every_type_by_name()
    {
        var options = await (
            await Client()
        ).GetFromJsonAsync<App.ElectronicDevices.Tags.Options.Endpoint.Response>($"{Url}/options");

        Assert.Equal(["Laptop", "Monitor", "Phone"], options!.Types.Select(t => t.Text));
    }
}
