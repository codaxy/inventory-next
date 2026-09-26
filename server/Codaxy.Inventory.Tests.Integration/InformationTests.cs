using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Clients;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Projects;
using Codaxy.Inventory.App.Informations.Items;
using Codaxy.Inventory.App.Informations.Tags;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Informations.Items.List.Endpoint.Item;
using Options = App.Informations.Items.Options.Endpoint.Response;

/// <summary>
/// Types Contract and Backup, the tag Secret, a laptop, the HQ location, the Rocket project; the
/// "Rocket contract" of type Contract, tagged Secret, kept on the laptop and at HQ.
/// </summary>
public class InformationApplication : InventoryApplication
{
    public static readonly Guid Contract = Guid.CreateVersion7();
    public static readonly Guid Backup = Guid.CreateVersion7();
    public static readonly Guid Secret = Guid.CreateVersion7();
    public static readonly Guid Hq = Guid.CreateVersion7();
    public static readonly Guid Rocket = Guid.CreateVersion7();
    public static readonly Guid RocketContract = Guid.CreateVersion7();
    public static Guid Laptop;
    public static Guid Owner;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var basics = await Seed.BasicsAsync(context);
        Owner = basics.Person;
        Laptop = await Seed.DeviceAsync(context, "Info laptop", holdsLicenses: true);

        var city = Guid.CreateVersion7();
        var client = Guid.CreateVersion7();
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
        context.Clients.Add(new Client { Id = client, Name = "Acme" });
        context.Projects.Add(
            new Project
            {
                Id = Rocket,
                Name = "Rocket",
                ClientId = client,
                ProjectOwnerId = Owner,
            }
        );
        context.InformationTypes.AddRange(
            new InformationType { Id = Contract, Name = "Contract" },
            new InformationType { Id = Backup, Name = "Backup" }
        );
        context.InformationTags.Add(new InformationTag { Id = Secret, Name = "Secret" });
        context.Informations.Add(
            new Information
            {
                Id = RocketContract,
                Name = "Rocket contract",
                InformationTypeId = Contract,
                PersonId = Owner,
                ProjectId = Rocket,
                Author = "Legal",
            }
        );
        await context.SaveChangesAsync();
        context.InformationTagInformations.Add(
            new InformationTagInformation
            {
                InformationId = RocketContract,
                InformationTagId = Secret,
            }
        );
        context.InformationLocations.AddRange(
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = RocketContract,
                ElectronicDeviceId = Laptop,
            },
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = RocketContract,
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

public class InformationTests(InformationApplication app) : IClassFixture<InformationApplication>
{
    private const string Url = "/api/informations";

    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    private async Task<Options> OptionsAsync() =>
        (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;

    private async Task<Dictionary<string, object?>> FormAsync(string name)
    {
        var o = await OptionsAsync();
        return new()
        {
            ["name"] = name,
            ["typeId"] = InformationApplication.Backup,
            ["personId"] = InformationApplication.Owner,
            ["author"] = "Ops",
            ["personalInformation"] = true,
            ["clientsPersonalInformation"] = false,
            ["incomplete"] = false,
            ["confidentialityId"] = o.Confidentialities.Single(c => c.Text == "Confidential").Id,
            ["integrityId"] = o.Integrities.Single(c => c.Text == "High").Id,
            ["availabilityId"] = o.Availabilities.Single(c => c.Text == "Medium").Id,
            ["projectId"] = InformationApplication.Rocket,
            ["tagIds"] = new[] { InformationApplication.Secret },
            ["locations"] = new object[]
            {
                new { kind = "device", targetId = InformationApplication.Laptop },
                new { kind = "url", url = " https://backup.example " },
            },
        };
    }

    private async Task<InformationDetail> CreateAsync(
        string name,
        Action<Dictionary<string, object?>>? change = null
    )
    {
        var form = await FormAsync(name);
        change?.Invoke(form);
        var response = await (await Client()).PostAsJsonAsync(Url, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<InformationDetail>())!;
    }

    private Task<List<string>> AuditOf(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id)
                .OrderBy(a => a.TimeCreated)
                .Select(a => a.Table + ":" + a.ActionType)
                .ToListAsync()
        );

    [Theory]
    [InlineData("/api/informations/")]
    [InlineData("/api/informations/types/")]
    [InlineData("/api/informations/tags/")]
    public async Task Refuses_a_caller_without_a_session(string url) =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync(url)).StatusCode
        );

    // Information

    [Fact]
    public async Task Creating_saves_every_field_its_tags_and_locations_and_computes_the_importance()
    {
        var backup = await CreateAsync("Nightly backup");

        Assert.Equal(
            ("Backup", "Ops", true, false, "Rocket"),
            (
                backup.Type.Name,
                backup.Author,
                backup.PersonalInformation,
                backup.ClientsPersonalInformation,
                backup.Project!.Name
            )
        );
        Assert.Equal("High", backup.Importance!.Name);
        Assert.Equal(["Secret"], backup.Tags.Select(t => t.Name));
        Assert.Equal(
            [("device", "Info laptop"), ("url", "https://backup.example")],
            backup.Locations.Select(l => (l.Kind, l.Target)).OrderBy(x => x.Kind)
        );
        Assert.Equal(["Information:Create"], await AuditOf(backup.Id));
        Assert.Equal(
            2,
            await app.InScopeAsync(c =>
                c.AuditLogs.CountAsync(a =>
                    a.Table == "InformationLocation"
                    && a.ActionType == "Create"
                    && backup.Locations.Select(l => l.Id).Contains(a.EntityId)
                )
            )
        );
    }

    [Fact]
    public async Task Editing_keeps_named_locations_removes_the_rest_adds_new_and_resets_the_tags()
    {
        var backup = await CreateAsync("Weekly backup");
        var device = backup.Locations.Single(l => l.Kind == "device");
        var form = await FormAsync("Weekly backup 2");
        form["tagIds"] = Array.Empty<Guid>();
        form["confidentialityId"] = null;
        form["locations"] = new object[]
        {
            new { id = device.Id },
            new { kind = "location", targetId = InformationApplication.Hq },
        };

        var response = await (await Client()).PutAsJsonAsync($"{Url}/{backup.Id}", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<InformationDetail>())!;
        Assert.Equal("Weekly backup 2", edited.Name);
        Assert.Empty(edited.Tags);
        Assert.Null(edited.Importance);
        Assert.Equal(
            [
                ("device", device.Id),
                ("location", edited.Locations.Single(l => l.Kind == "location").Id),
            ],
            edited.Locations.Select(l => (l.Kind, l.Id)).OrderBy(x => x.Kind)
        );
        Assert.Contains("Information:Update", await AuditOf(backup.Id));
    }

    [Fact]
    public async Task A_location_must_be_one_thing_that_exists_and_a_kept_one_must_be_its_own()
    {
        var client = await Client();
        async Task<string[]> Refused(object location)
        {
            var form = await FormAsync("Refused");
            form["locations"] = new[] { location };
            return await ErrorsOf(await client.PostAsJsonAsync(Url, form), "locations");
        }

        Assert.Equal(["Give the address."], await Refused(new { kind = "url", url = " " }));
        Assert.Equal(
            ["Choose the virtual machine."],
            await Refused(new { kind = "virtualMachine", targetId = Guid.CreateVersion7() })
        );
        Assert.Equal(["Choose where it is kept."], await Refused(new { kind = "drawer" }));
        Assert.Equal(
            ["A location is no longer this information's."],
            await Refused(new { id = Guid.CreateVersion7() })
        );
    }

    [Theory]
    [InlineData("name")]
    [InlineData("typeId")]
    [InlineData("personId")]
    public async Task Information_needs_a_name_a_type_and_an_assignee(string field)
    {
        var form = await FormAsync("Missing");
        form[field] = null;

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field));
    }

    [Fact]
    public async Task Deleting_takes_its_locations_and_tag_links()
    {
        var doomed = await CreateAsync("Doomed");
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{doomed.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{doomed.Id}")).StatusCode
        );
        Assert.False(
            await app.InScopeAsync(c =>
                c.InformationLocations.AnyAsync(l => l.InformationId == doomed.Id)
            )
        );
        Assert.False(
            await app.InScopeAsync(c =>
                c.InformationTagInformations.AnyAsync(l => l.InformationId == doomed.Id)
            )
        );
    }

    [Fact]
    public async Task Lists_by_every_filter_and_exports_as_the_original()
    {
        var client = await Client();
        async Task<List<Item>> Listed(string q) =>
            (await client.GetFromJsonAsync<Page<Item>>($"{Url}/?{q}&pageSize=100"))!.Items.ToList();

        Assert.Contains(
            await Listed($"typeId={InformationApplication.Contract}"),
            i => i.Name == "Rocket contract"
        );
        Assert.All(
            await Listed($"typeId={InformationApplication.Contract}"),
            i => Assert.Equal("Contract", i.Type)
        );
        Assert.Contains(
            await Listed($"tagId={InformationApplication.Secret}"),
            i => i.Name == "Rocket contract"
        );
        Assert.Contains(
            await Listed($"locationId={InformationApplication.Hq}"),
            i => i.Name == "Rocket contract"
        );
        Assert.Contains(await Listed("q=legal"), i => i.Name == "Rocket contract");
        Assert.All(await Listed("incomplete=true"), i => Assert.True(i.Incomplete));
        var byAuthor = await Listed("sort=-author");
        Assert.Equal(byAuthor.Count, (await Listed("")).Count);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/?sort=size")).StatusCode
        );

        var export = await client.GetAsync(
            $"{Url}/export?typeId={InformationApplication.Contract}"
        );
        Assert.Equal(
            "Information.Export - Filtered.xlsx",
            export.Content.Headers.ContentDisposition?.FileNameStar
                ?? export.Content.Headers.ContentDisposition?.FileName
        );
        var text = await Spreadsheet.TextOf(export);
        Assert.Contains("Rocket contract", text);
        Assert.Contains("Assignee", text);
    }

    [Fact]
    public async Task Search_finds_information_by_its_id_and_its_type_s()
    {
        var client = await Client();
        var own = (
            await client.GetFromJsonAsync<Page<Item>>(
                $"{Url}/?q={InformationApplication.RocketContract}"
            )
        )!;
        Assert.Equal("Rocket contract", own.Items.Single().Name);
        var byProject = (
            await client.GetFromJsonAsync<Page<Item>>($"{Url}/?q={InformationApplication.Rocket}")
        )!;
        Assert.Contains(byProject.Items, i => i.Name == "Rocket contract");
    }

    [Fact]
    public async Task An_unknown_piece_is_not_found()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", await FormAsync("Nowhere"))).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    // Types and tags

    [Theory]
    [InlineData("types", "InformationType")]
    [InlineData("tags", "InformationTag")]
    public async Task A_type_or_tag_round_trips_with_its_audit_rows(string url, string table)
    {
        var client = await Client();
        var created = await client.PostAsJsonAsync($"{Url}/{url}", new { name = $"New {url}" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<InformationTypeDetail>())!.Id;

        Assert.Equal(
            HttpStatusCode.OK,
            (
                await client.PutAsJsonAsync(
                    $"{Url}/{url}/{id}",
                    new { name = $"Renamed {url}", description = "Why" }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{url}/{id}")).StatusCode
        );
        Assert.Equal([$"{table}:Create", $"{table}:Update", $"{table}:Delete"], await AuditOf(id));
    }

    [Theory]
    [InlineData("types", "CONTRACT ", "A type with this name already exists.")]
    [InlineData("tags", "secret", "A tag with this name already exists.")]
    public async Task A_type_or_tag_name_is_unique_whatever_its_case(
        string url,
        string name,
        string message
    ) =>
        Assert.Equal(
            [message],
            await ErrorsOf(
                await (await Client()).PostAsJsonAsync($"{Url}/{url}", new { name }),
                "name"
            )
        );

    [Fact]
    public async Task A_type_lists_its_information_and_is_not_deleted_while_it_has_some()
    {
        var client = await Client();
        var contract = (
            await client.GetFromJsonAsync<InformationTypeDetail>(
                $"{Url}/types/{InformationApplication.Contract}"
            )
        )!;
        Assert.Contains(contract.Information.Items, i => i.Name == "Rocket contract");

        var response = await client.DeleteAsync($"{Url}/types/{InformationApplication.Contract}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.True(
            await app.InScopeAsync(c =>
                c.Informations.AnyAsync(i => i.Id == InformationApplication.RocketContract)
            )
        );
    }

    [Fact]
    public async Task A_tag_in_use_goes_and_its_information_stays()
    {
        var client = await Client();
        var created = await client.PostAsJsonAsync($"{Url}/tags", new { name = "Temporary" });
        var tag = (await created.Content.ReadFromJsonAsync<InformationTagDetail>())!;
        var piece = await CreateAsync("Tagged", f => f["tagIds"] = new[] { tag.Id });
        Assert.Equal(
            1,
            (await client.GetFromJsonAsync<InformationTagDetail>($"{Url}/tags/{tag.Id}"))!
                .Information
                .Total
        );

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/tags/{tag.Id}")).StatusCode
        );
        var after = (await client.GetFromJsonAsync<InformationDetail>($"{Url}/{piece.Id}"))!;
        Assert.Empty(after.Tags);
    }

    [Fact]
    public async Task Types_list_with_their_information_count()
    {
        var types = (
            await (await Client()).GetFromJsonAsync<
                Page<App.Informations.Types.List.Endpoint.Item>
            >($"{Url}/types/?sort=-information")
        )!.Items;
        Assert.Equal(
            types.OrderByDescending(t => t.Information).Select(t => t.Information),
            types.Select(t => t.Information)
        );
    }
}
