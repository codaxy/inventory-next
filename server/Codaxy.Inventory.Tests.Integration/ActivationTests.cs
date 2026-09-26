using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Licenses.Activations;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Licenses.Activations.List.Endpoint.Item;

/// <summary>
/// Office, a per-user volume of five seats on a licence expiring soon, with one active and one
/// deactivated activation; Antivirus, a per-device volume on an expired licence, with a device of a
/// type that holds licences and one that does not. A test that changes data makes its own activation.
/// </summary>
public class ActivationApplication : InventoryApplication
{
    public static readonly Guid Office = Guid.CreateVersion7();
    public static readonly Guid Antivirus = Guid.CreateVersion7();
    public static readonly Guid Ana = Guid.CreateVersion7();

    public static Seed.SeededLicense OfficeLicense = null!;
    public static Seed.SeededLicense AntivirusLicense = null!;
    public static Guid Laptop;
    public static Guid Monitor;
    public static Guid ActiveForAna;
    public static Guid Deactivated;

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        var category = new SoftwareOrServiceCategory
        {
            Id = Guid.CreateVersion7(),
            Name = "Application",
        };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Maker" };
        context.AddRange(category, maker);
        context.SoftwareOrServices.AddRange(
            new SoftwareOrService
            {
                Id = Office,
                Name = "Office",
                SoftwareOrServiceCategoryId = category.Id,
                ManufacturerId = maker.Id,
            },
            new SoftwareOrService
            {
                Id = Antivirus,
                Name = "Antivirus",
                SoftwareOrServiceCategoryId = category.Id,
                ManufacturerId = maker.Id,
            }
        );
        context.Persons.Add(new Person { Id = Ana, Name = "Ana Anić" });
        await context.SaveChangesAsync();

        OfficeLicense = await Seed.LicenseWithVolumeAsync(
            context,
            Office,
            "Office licence",
            seats: 5,
            expires: Today.AddDays(5)
        );
        AntivirusLicense = await Seed.LicenseWithVolumeAsync(
            context,
            Antivirus,
            "Antivirus licence",
            seats: 2,
            volumeType: Seed.PerDevice,
            expires: Today.AddDays(-10)
        );
        Laptop = await Seed.DeviceAsync(context, "lap-ana", holdsLicences: true);
        Monitor = await Seed.DeviceAsync(context, "mon-ana", holdsLicences: false);

        ActiveForAna = await Seed.ActivationAsync(
            context,
            OfficeLicense.Volume,
            person: Ana,
            quantity: 2
        );
        Deactivated = await Seed.ActivationAsync(
            context,
            OfficeLicense.Volume,
            quantity: 1,
            deactivated: new DateOnly(2026, 3, 1)
        );
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class ActivationTests(ActivationApplication app) : IClassFixture<ActivationApplication>
{
    private const string Url = "/api/licenses/activations";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<HttpResponseMessage> PostAsync(object body) =>
        await (await Client()).PostAsJsonAsync(Url, body);

    private async Task<ActivationDetail> ActivateForAnaAsync(int quantity = 1)
    {
        var response = await PostAsync(
            new
            {
                volumeId = ActivationApplication.OfficeLicense.Volume,
                personId = ActivationApplication.Ana,
                activationDate = "2026-04-01",
                quantity,
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ActivationDetail>())!;
    }

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    private Task<List<AuditLog>> AuditFor(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id).OrderBy(a => a.TimeCreated).ToListAsync()
        );

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Activating_for_a_person_counts_the_seats_and_is_audited_as_the_user()
    {
        var before = await app.InScopeAsync(c =>
            c.Activations.Where(a =>
                    a.VolumeId == ActivationApplication.OfficeLicense.Volume
                    && a.DeactivationDate == null
                )
                .SumAsync(a => a.Quantity)
        );

        var activation = await ActivateForAnaAsync(quantity: 1);

        Assert.Equal(
            ("Office", "Office licence", "Ana Anić", 1, new DateOnly(2026, 4, 1)),
            (
                activation.Software.Name,
                activation.License.Name,
                activation.Person!.Name,
                activation.Quantity,
                activation.ActivationDate
            )
        );
        Assert.Null(activation.Device);
        Assert.Null(activation.DeactivationDate);
        Assert.Equal((5, before + 1), (activation.Volume.Quantity, activation.Volume.InUse));
        Assert.Equal("soon", activation.License.Expiry);

        var audit = Assert.Single(await AuditFor(activation.Id));
        Assert.Equal(
            ("Create", "Activation", Editor),
            (audit.ActionType, audit.Table, audit.Email)
        );
    }

    [Fact]
    public async Task Activating_for_a_device_whose_type_holds_licences()
    {
        var response = await PostAsync(
            new
            {
                volumeId = ActivationApplication.AntivirusLicense.Volume,
                deviceId = ActivationApplication.Laptop,
                activationDate = "2026-04-01",
            }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var activation = (await response.Content.ReadFromJsonAsync<ActivationDetail>())!;
        Assert.Equal("lap-ana", activation.Device!.Name);
        Assert.Null(activation.Person);
        Assert.Equal("expired", activation.License.Expiry);

        await (await Client()).DeleteAsync($"{Url}/{activation.Id}");
    }

    [Fact]
    public async Task Seats_past_the_volume_are_allowed_as_the_original_only_warned()
    {
        var activation = await ActivateForAnaAsync(quantity: 50);

        Assert.True(activation.Volume.InUse > activation.Volume.Quantity);

        await (await Client()).DeleteAsync($"{Url}/{activation.Id}");
    }

    [Fact]
    public async Task A_per_user_volume_needs_a_person_and_no_device()
    {
        var volume = ActivationApplication.OfficeLicense.Volume;

        Assert.Equal(
            ["Choose who the seats are for."],
            await ErrorsOf(
                await PostAsync(new { volumeId = volume, activationDate = "2026-04-01" }),
                "personId"
            )
        );
        Assert.Equal(
            ["A per-user volume is activated for a person."],
            await ErrorsOf(
                await PostAsync(
                    new
                    {
                        volumeId = volume,
                        personId = ActivationApplication.Ana,
                        deviceId = ActivationApplication.Laptop,
                        activationDate = "2026-04-01",
                    }
                ),
                "deviceId"
            )
        );
        Assert.Equal(
            ["That person no longer exists."],
            await ErrorsOf(
                await PostAsync(
                    new
                    {
                        volumeId = volume,
                        personId = Guid.CreateVersion7(),
                        activationDate = "2026-04-01",
                    }
                ),
                "personId"
            )
        );
    }

    [Fact]
    public async Task A_per_device_volume_needs_a_device_whose_type_holds_licences()
    {
        var volume = ActivationApplication.AntivirusLicense.Volume;

        Assert.Equal(
            ["Choose the device the seats are for."],
            await ErrorsOf(
                await PostAsync(new { volumeId = volume, activationDate = "2026-04-01" }),
                "deviceId"
            )
        );
        Assert.Equal(
            ["Devices of this type cannot hold licences."],
            await ErrorsOf(
                await PostAsync(
                    new
                    {
                        volumeId = volume,
                        deviceId = ActivationApplication.Monitor,
                        activationDate = "2026-04-01",
                    }
                ),
                "deviceId"
            )
        );
        Assert.Equal(
            ["That device no longer exists."],
            await ErrorsOf(
                await PostAsync(
                    new
                    {
                        volumeId = volume,
                        deviceId = Guid.CreateVersion7(),
                        activationDate = "2026-04-01",
                    }
                ),
                "deviceId"
            )
        );
        Assert.Equal(
            ["This volume is activated for a device."],
            await ErrorsOf(
                await PostAsync(
                    new
                    {
                        volumeId = volume,
                        deviceId = ActivationApplication.Laptop,
                        personId = ActivationApplication.Ana,
                        activationDate = "2026-04-01",
                    }
                ),
                "personId"
            )
        );
    }

    [Theory]
    [InlineData("{\"activationDate\":\"2026-04-01\"}", "volumeId")]
    [InlineData(
        "{\"volumeId\":\"00000000-0000-0000-0000-000000000001\",\"activationDate\":\"2026-04-01\"}",
        "volumeId"
    )]
    [InlineData("{\"volumeId\":\"00000000-0000-0000-0000-000000000001\"}", "activationDate")]
    [InlineData(
        "{\"volumeId\":\"00000000-0000-0000-0000-000000000001\",\"activationDate\":\"2026-04-01\",\"quantity\":0}",
        "quantity"
    )]
    public async Task An_activation_needs_an_existing_volume_a_date_and_a_seat(
        string body,
        string field
    )
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
    public async Task Deactivating_ends_the_seats_and_reactivating_restores_them_both_audited()
    {
        var activation = await ActivateForAnaAsync();
        var client = await Client();

        var deactivated = await client.PostAsJsonAsync(
            $"{Url}/{activation.Id}/deactivate",
            new { date = "2026-05-01" }
        );
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        var ended = (await deactivated.Content.ReadFromJsonAsync<ActivationDetail>())!;
        Assert.Equal(new DateOnly(2026, 5, 1), ended.DeactivationDate);
        Assert.Equal(activation.Volume.InUse - 1, ended.Volume.InUse);

        var reactivated = await client.PostAsync($"{Url}/{activation.Id}/reactivate", null);
        Assert.Equal(HttpStatusCode.OK, reactivated.StatusCode);
        var again = (await reactivated.Content.ReadFromJsonAsync<ActivationDetail>())!;
        Assert.Null(again.DeactivationDate);
        Assert.Equal(activation.Volume.InUse, again.Volume.InUse);

        var audit = await AuditFor(activation.Id);
        Assert.Equal(["Create", "Update", "Update"], audit.Select(a => a.ActionType));
        Assert.Contains("\"DeactivationDate\": \"2026-05-01\"", audit[1].NewValuesJson);
        Assert.Contains("\"DeactivationDate\": null", audit[2].NewValuesJson);

        await client.DeleteAsync($"{Url}/{activation.Id}");
    }

    [Fact]
    public async Task A_transition_from_the_wrong_state_is_a_conflict()
    {
        var client = await Client();

        var twice = await client.PostAsJsonAsync(
            $"{Url}/{ActivationApplication.Deactivated}/deactivate",
            new { date = "2026-06-01" }
        );
        var active = await client.PostAsync(
            $"{Url}/{ActivationApplication.ActiveForAna}/reactivate",
            null
        );

        Assert.Equal(HttpStatusCode.Conflict, twice.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, active.StatusCode);
    }

    [Fact]
    public async Task A_deactivation_needs_a_date_on_or_after_the_activation()
    {
        var activation = await ActivateForAnaAsync();
        var client = await Client();

        Assert.Equal(
            ["An activation cannot end before it began."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    $"{Url}/{activation.Id}/deactivate",
                    new { date = "2026-03-31" }
                ),
                "date"
            )
        );
        Assert.Contains(
            "date",
            (
                await (
                    await client.PostAsJsonAsync($"{Url}/{activation.Id}/deactivate", new { })
                ).Content.ReadFromJsonAsync<ValidationProblemDetails>()
            )!
                .Errors
                .Keys
        );
        Assert.Equal(
            HttpStatusCode.OK,
            (
                await client.PostAsJsonAsync(
                    $"{Url}/{activation.Id}/deactivate",
                    new { date = "2026-04-01" }
                )
            ).StatusCode
        );

        await client.DeleteAsync($"{Url}/{activation.Id}");
    }

    [Fact]
    public async Task Deleting_removes_the_activation_and_is_audited()
    {
        var activation = await ActivateForAnaAsync();
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{activation.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{activation.Id}")).StatusCode
        );
        Assert.Equal("Delete", (await AuditFor(activation.Id)).Last().ActionType);
    }

    [Fact]
    public async Task An_unknown_activation_is_not_found_for_every_verb()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await client.PostAsJsonAsync($"{Url}/{id}/deactivate", new { date = "2026-05-01" })
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PostAsync($"{Url}/{id}/reactivate", null)).StatusCode
        );
    }

    [Fact]
    public async Task Lists_with_the_assignee_and_the_licences_expiry()
    {
        var row = (await ListAsync("q=ana+anić")).Items.Single(i =>
            i.Id == ActivationApplication.ActiveForAna
        );

        Assert.Equal(
            ("Office", "Office licence", "Ana Anić", false, 2, "soon"),
            (row.Software, row.License, row.Assignee, row.ForDevice, row.Quantity, row.Expiry)
        );
    }

    [Fact]
    public async Task Searches_software_licence_person_and_device()
    {
        Assert.All(
            (await ListAsync("q=antivirus")).Items,
            i => Assert.Equal("Antivirus", i.Software)
        );
        Assert.Contains(
            (await ListAsync("q=office+licence")).Items,
            i => i.Id == ActivationApplication.ActiveForAna
        );
        Assert.Empty((await ListAsync("q=nobody-by-this-name")).Items);
    }

    [Fact]
    public async Task Filters_by_software_licence_status_and_expiry()
    {
        var office = await ListAsync($"softwareId={ActivationApplication.Office}");
        var license = await ListAsync($"licenseId={ActivationApplication.OfficeLicense.License}");
        var active = await ListAsync("status=active");
        var deactivated = await ListAsync("status=deactivated");
        var soon = await ListAsync("expiry=soon");
        var expired = await ListAsync("expiry=expired");

        Assert.All(office.Items, i => Assert.Equal("Office", i.Software));
        Assert.All(license.Items, i => Assert.Equal("Office licence", i.License));
        Assert.All(active.Items, i => Assert.Null(i.DeactivationDate));
        Assert.Contains(deactivated.Items, i => i.Id == ActivationApplication.Deactivated);
        Assert.All(deactivated.Items, i => Assert.NotNull(i.DeactivationDate));
        Assert.All(soon.Items, i => Assert.Equal("soon", i.Expiry));
        Assert.Contains(soon.Items, i => i.Id == ActivationApplication.ActiveForAna);
        Assert.All(expired.Items, i => Assert.Equal("expired", i.Expiry));
        Assert.Empty((await ListAsync("expiry=none")).Items);
        Assert.Empty((await ListAsync("expiry=regular")).Items);
    }

    [Fact]
    public async Task Sorts_newest_first_by_default_and_by_each_column()
    {
        var all = (await ListAsync("pageSize=100")).Items;
        Assert.Equal(
            all.OrderByDescending(i => i.ActivationDate).Select(i => i.ActivationDate),
            all.Select(i => i.ActivationDate)
        );

        foreach (var key in new[] { "software", "license", "assignee", "deactivated", "activated" })
        {
            Assert.Equal(all.Count, (await ListAsync($"sort={key}&pageSize=100")).Items.Count);
            Assert.Equal(all.Count, (await ListAsync($"sort=-{key}&pageSize=100")).Items.Count);
        }

        var first = await ListAsync("pageSize=1");
        Assert.Single(first.Items);
        Assert.True(first.Total >= 2);
        Assert.Empty((await ListAsync("page=999")).Items);
    }

    [Theory]
    [InlineData("sort=quantity")]
    [InlineData("status=paused")]
    [InlineData("expiry=later")]
    [InlineData("pageSize=101")]
    public async Task Refuses_a_query_outside_the_convention(string query)
    {
        var response = await (await Client()).GetAsync($"{Url}/?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Volumes_of_a_software_come_with_their_seats_in_use()
    {
        var client = await Client();

        var volumes = await client.GetFromJsonAsync<
            List<App.Licenses.Activations.Volumes.Endpoint.Volume>
        >($"{Url}/volumes?softwareId={ActivationApplication.Office}");
        var volume = Assert.Single(volumes!);

        Assert.Equal(
            ("Office licence", "Per user", 5),
            (volume.License, volume.Type, volume.Quantity)
        );
        Assert.True(volume.InUse >= 2);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/volumes")).StatusCode
        );
    }

    [Fact]
    public async Task Options_offer_software_with_volumes_and_devices_that_hold_licences()
    {
        var options = await (
            await Client()
        ).GetFromJsonAsync<App.Licenses.Activations.Options.Endpoint.Response>($"{Url}/options");

        Assert.Equal(["Antivirus", "Office"], options!.Software.Select(s => s.Text));
        Assert.Contains(options.Licenses, l => l.Text == "Office licence");
        Assert.Contains(options.People, p => p.Text == "Ana Anić");
        Assert.Contains(options.Devices, d => d.Id == ActivationApplication.Laptop);
        Assert.DoesNotContain(options.Devices, d => d.Id == ActivationApplication.Monitor);
    }
}
