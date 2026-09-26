using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>The seed person holds a device and a license: enough for an id to find something.</summary>
public class SearchApplication : InventoryApplication
{
    public static Seed.Basics Basics = null!;
    public static Guid Device;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        Basics = await Seed.BasicsAsync(context);
        Device = await Seed.DeviceAsync(context, "Searched laptop", holdsLicenses: true);
    }
}

public class SearchByIdTests(SearchApplication app) : IClassFixture<SearchApplication>
{
    private sealed record Total(int total);

    private async Task<int> TotalAsync(string list, string q)
    {
        var response = await (await app.ClientAsync("editor@codaxy.com")).GetAsync(
            $"{list}?q={Uri.EscapeDataString(q)}"
        );
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Total>())!.total;
    }

    public static TheoryData<string> Lists =>
        new()
        {
            "/api/administration/audit-log/",
            "/api/company/clients/",
            "/api/company/locations/",
            "/api/company/manufacturers/",
            "/api/company/people/",
            "/api/company/projects/",
            "/api/company/vendors/",
            "/api/electronic-devices/",
            "/api/electronic-devices/tags/",
            "/api/electronic-devices/types/",
            "/api/furniture/",
            "/api/furniture/types/",
            "/api/informations/",
            "/api/informations/tags/",
            "/api/informations/types/",
            "/api/infrastructure/cloud-subscriptions/",
            "/api/infrastructure/software/",
            "/api/infrastructure/virtual-machines/",
            "/api/licenses/activations/",
            "/api/licenses/",
            "/api/licenses/software-services/",
        };

    /// <summary>Every list answers an id it does not hold with nothing — its id query runs, and matches no text.</summary>
    [Theory]
    [MemberData(nameof(Lists))]
    public async Task Every_list_takes_an_id_and_matches_it_exactly(string list) =>
        Assert.Equal(0, await TotalAsync(list, Guid.CreateVersion7().ToString()));

    [Fact]
    public async Task A_record_is_found_by_its_own_id_and_by_the_ids_it_points_at()
    {
        Assert.Equal(
            1,
            await TotalAsync("/api/company/people/", SearchApplication.Basics.Person.ToString())
        );
        Assert.Equal(
            1,
            await TotalAsync(
                "/api/company/vendors/",
                SearchApplication.Basics.Vendor.ToString().ToUpperInvariant()
            )
        );
        Assert.True(
            await TotalAsync("/api/administration/audit-log/", SearchApplication.Device.ToString())
                > 0
        );
    }
}
