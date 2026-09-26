using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.Informations.Items;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Infrastructure.CloudSubscriptions;
using Codaxy.Inventory.App.Infrastructure.Softwares;
using Codaxy.Inventory.App.Infrastructure.VirtualMachines;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// Build-01, a virtual machine; a volume of Azure under the Azure license, and on it the Prod
/// subscription and the Tools software; the "Runbook" kept on all three.
/// </summary>
public class InfrastructureApplication : InventoryApplication
{
    public static readonly Guid Build = Guid.CreateVersion7();
    public static readonly Guid Prod = Guid.CreateVersion7();
    public static readonly Guid Tools = Guid.CreateVersion7();
    public static Seed.SeededLicense Azure = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var basics = await Seed.BasicsAsync(context);
        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Cloud" };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Microsoft" };
        var azure = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Azure",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = maker.Id,
        };
        context.AddRange(category, maker, azure);
        await context.SaveChangesAsync();
        Azure = await Seed.LicenseWithVolumeAsync(context, azure.Id, "Azure license");

        context.VirtualMachines.Add(
            new VirtualMachine
            {
                Id = Build,
                Name = "Build-01",
                IPAddress = "10.0.0.1",
            }
        );
        context.Clouds.Add(
            new Cloud
            {
                Id = Prod,
                Name = "Prod",
                VolumeId = Azure.Volume,
                ManagementURL = "https://portal.azure.com",
            }
        );
        context.Softwares.Add(
            new Software
            {
                Id = Tools,
                Name = "Tools",
                VolumeId = Azure.Volume,
            }
        );
        var type = new InformationType { Id = Guid.CreateVersion7(), Name = "Runbook" };
        var runbook = new Information
        {
            Id = Guid.CreateVersion7(),
            Name = "Runbook",
            InformationTypeId = type.Id,
            PersonId = basics.Person,
        };
        context.AddRange(type, runbook);
        context.Informations.Add(
            new Information
            {
                Id = Guid.CreateVersion7(),
                Name = "Elsewhere",
                InformationTypeId = type.Id,
                PersonId = basics.Person,
            }
        );
        context.InformationLocations.AddRange(
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = runbook.Id,
                VirtualMachineId = Build,
            },
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = runbook.Id,
                CloudId = Prod,
            },
            new InformationLocation
            {
                Id = Guid.CreateVersion7(),
                InformationId = runbook.Id,
                SoftwareId = Tools,
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

public class InfrastructureTests(InfrastructureApplication app)
    : IClassFixture<InfrastructureApplication>
{
    private const string Url = "/api/infrastructure";

    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    private sealed record IdOnly(Guid Id);

    public static TheoryData<string, string, object, object> RoundTrips =>
        new()
        {
            {
                "virtual-machines",
                "VirtualMachine",
                new { name = "Build-02", ipAddress = "10.0.0.2" },
                new { name = "Build-03" }
            },
            {
                "cloud-subscriptions",
                "Cloud",
                new { name = "Staging", volumeId = InfrastructureApplication.Azure.Volume },
                new
                {
                    name = "Staging 2",
                    volumeId = InfrastructureApplication.Azure.Volume,
                    managementUrl = "https://portal.example",
                }
            },
            {
                "software",
                "Software",
                new { name = "Agent", volumeId = InfrastructureApplication.Azure.Volume },
                new { name = "Agent 2", volumeId = InfrastructureApplication.Azure.Volume }
            },
        };

    [Theory]
    [MemberData(nameof(RoundTrips))]
    public async Task Each_round_trips_with_its_audit_rows(
        string url,
        string table,
        object create,
        object update
    )
    {
        var client = await Client();
        var created = await client.PostAsJsonAsync($"{Url}/{url}", create);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<IdOnly>())!.Id;

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"{Url}/{url}/{id}", update)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{url}/{id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{url}/{id}")).StatusCode
        );
        Assert.Equal(
            [$"{table}:Create", $"{table}:Update", $"{table}:Delete"],
            await app.InScopeAsync(c =>
                c.AuditLogs.Where(a => a.EntityId == id)
                    .OrderBy(a => a.TimeCreated)
                    .Select(a => a.Table + ":" + a.ActionType)
                    .ToListAsync()
            )
        );
    }

    [Theory]
    [InlineData(
        "virtual-machines",
        "build-01 ",
        "A virtual machine with this name already exists."
    )]
    [InlineData(
        "cloud-subscriptions",
        "PROD",
        "A cloud subscription with this name already exists."
    )]
    [InlineData("software", "tools", "A software with this name already exists.")]
    public async Task A_name_is_unique_whatever_its_case(string url, string name, string message) =>
        Assert.Equal(
            [message],
            await ErrorsOf(
                await (await Client()).PostAsJsonAsync(
                    $"{Url}/{url}",
                    new { name, volumeId = InfrastructureApplication.Azure.Volume }
                ),
                "name"
            )
        );

    [Theory]
    [InlineData("cloud-subscriptions")]
    [InlineData("software")]
    public async Task The_volume_must_be_given_and_exist(string url)
    {
        var client = await Client();
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync($"{Url}/{url}", new { name = "No volume" }),
                "volumeId"
            )
        );
        Assert.Equal(
            ["That choice no longer exists."],
            await ErrorsOf(
                await client.PostAsJsonAsync(
                    $"{Url}/{url}",
                    new { name = "Ghost volume", volumeId = Guid.CreateVersion7() }
                ),
                "volumeId"
            )
        );
    }

    [Theory]
    [InlineData("virtual-machines", "Build")]
    [InlineData("cloud-subscriptions", "Prod")]
    [InlineData("software", "Tools")]
    public async Task One_information_is_kept_on_is_not_deleted(string url, string which)
    {
        var id = which switch
        {
            "Build" => InfrastructureApplication.Build,
            "Prod" => InfrastructureApplication.Prod,
            _ => InfrastructureApplication.Tools,
        };
        var response = await (await Client()).DeleteAsync($"{Url}/{url}/{id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "A piece of information is kept on it, so it cannot be deleted.",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
    }

    [Fact]
    public async Task A_cloud_subscription_names_its_volume_and_license_and_the_information_on_it()
    {
        var prod = (
            await (await Client()).GetFromJsonAsync<CloudSubscriptionDetail>(
                $"{Url}/cloud-subscriptions/{InfrastructureApplication.Prod}"
            )
        )!;

        Assert.Equal(
            ("Azure license", InfrastructureApplication.Azure.License),
            (prod.Volume.License, prod.Volume.LicenseId)
        );
        Assert.StartsWith("Azure · Azure license · ", prod.Volume.Text);
        Assert.Equal(
            ("https://portal.azure.com", "Runbook"),
            (prod.ManagementUrl, prod.Information.Items.Single().Name)
        );
    }

    [Fact]
    public async Task Lists_with_their_information_count_and_the_information_list_filters_by_each()
    {
        var client = await Client();
        var machines = (
            await client.GetFromJsonAsync<
                Page<App.Infrastructure.VirtualMachines.List.Endpoint.Item>
            >($"{Url}/virtual-machines/?q=10.0.0")
        )!;
        Assert.Equal(
            ("Build-01", 1),
            (machines.Items.Single().Name, machines.Items.Single().Information)
        );
        var software = (
            await client.GetFromJsonAsync<Page<App.Infrastructure.Softwares.List.Endpoint.Item>>(
                $"{Url}/software/?q=azure&sort=-license"
            )
        )!;
        Assert.Equal(
            ("Tools", "Azure license", "Azure"),
            (
                software.Items.Single().Name,
                software.Items.Single().License,
                software.Items.Single().Software
            )
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync($"{Url}/virtual-machines/?sort=license")).StatusCode
        );

        foreach (
            var filter in new[]
            {
                $"virtualMachineId={InfrastructureApplication.Build}",
                $"cloudSubscriptionId={InfrastructureApplication.Prod}",
                $"softwareId={InfrastructureApplication.Tools}",
            }
        )
            Assert.Equal(
                1,
                (
                    await client.GetFromJsonAsync<Page<App.Informations.Items.List.Endpoint.Item>>(
                        $"/api/informations/?{filter}"
                    )
                )!.Total
            );
    }

    [Fact]
    public async Task The_options_offer_every_volume_by_name()
    {
        var o = (
            await (
                await Client()
            ).GetFromJsonAsync<App.Infrastructure.Softwares.Options.Endpoint.Response>(
                $"{Url}/software/options"
            )
        )!;
        Assert.Contains(
            o.Volumes,
            v =>
                v.Id == InfrastructureApplication.Azure.Volume
                && v.Text.StartsWith("Azure · Azure license")
        );
    }
}
