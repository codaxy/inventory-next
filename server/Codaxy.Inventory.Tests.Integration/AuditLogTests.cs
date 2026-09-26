using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Codaxy.Inventory.App.Administration.AuditLogs;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Vendors;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>A log seeded once: a page's worth of noise, an asset that exists and one deleted.</summary>
public class AuditLogApplication : InventoryApplication
{
    public const int Filler = 30;
    public static readonly DateTimeOffset T0 = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    public static readonly Guid LaptopId = Guid.CreateVersion7();
    public static readonly Guid ChairId = Guid.CreateVersion7();
    public static readonly Guid DeviceTypeId = Guid.CreateVersion7();
    public static readonly Guid Save = Guid.CreateVersion7();

    public Guid LaptopCreated { get; } = Guid.CreateVersion7();
    public Guid DeviceCreated { get; } = Guid.CreateVersion7();
    public Guid LaptopRenamed { get; } = Guid.CreateVersion7();
    public Guid ChairFurniture { get; } = Guid.CreateVersion7();
    public Guid Discount { get; } = Guid.CreateVersion7();

    public const int Seeded = Filler + 7;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        var category = new AssetCategory { Id = Guid.CreateVersion7(), Name = "Hardware" };
        var type = new AssetType
        {
            Id = Guid.CreateVersion7(),
            Name = "Laptop",
            AssetCategoryId = category.Id,
        };
        var vendor = new Vendor { Id = Guid.CreateVersion7(), Name = "Vendor" };
        var person = new Person { Id = Guid.CreateVersion7(), Name = "Owner" };

        context.AddRange(category, type, vendor, person);
        context.Add(
            new ElectronicDeviceType
            {
                Id = DeviceTypeId,
                Name = "Notebook",
                HoldLicences = false,
            }
        );
        context.Add(
            new Asset
            {
                Id = LaptopId,
                Name = "Laptop Beta",
                InventoryNumber = 100001,
                AssetTypeId = type.Id,
                VendorId = vendor.Id,
                PersonId = person.Id,
                LastModified = T0,
            }
        );

        for (var i = 0; i < Filler; i++)
            context.Add(
                Row(
                    Guid.CreateVersion7(),
                    "Volume",
                    Guid.CreateVersion7(),
                    "Update",
                    "filler@codaxy.com",
                    T0.AddMinutes(i),
                    new { Description = $"was {i}" },
                    new { Description = $"is {i}" }
                )
            );

        context.AddRange(
            Row(
                LaptopCreated,
                "Asset",
                LaptopId,
                "Create",
                "ana@codaxy.com",
                T0.AddMinutes(100),
                null,
                new
                {
                    Id = LaptopId,
                    Name = "Laptop Alpha",
                    InventoryNumber = 100001,
                },
                Save
            ),
            Row(
                DeviceCreated,
                "ElectronicDevice",
                LaptopId,
                "Create",
                "ana@codaxy.com",
                T0.AddMinutes(100),
                null,
                new { AssetId = LaptopId, ElectronicDeviceTypeId = DeviceTypeId },
                Save
            ),
            Row(
                LaptopRenamed,
                "Asset",
                LaptopId,
                "Update",
                "bob@codaxy.com",
                T0.AddMinutes(200),
                new
                {
                    Id = LaptopId,
                    Name = "Laptop Alpha",
                    InventoryNumber = 100001,
                    Incomplete = true,
                },
                new
                {
                    Id = LaptopId,
                    Name = "Laptop Beta",
                    InventoryNumber = 100001,
                    Incomplete = false,
                }
            ),
            Row(
                Guid.CreateVersion7(),
                "Asset",
                ChairId,
                "Create",
                "ana@codaxy.com",
                T0.AddMinutes(300),
                null,
                new
                {
                    Id = ChairId,
                    Name = "Old Chair",
                    InventoryNumber = 100002,
                }
            ),
            Row(
                ChairFurniture,
                "Furniture",
                ChairId,
                "Create",
                "ana@codaxy.com",
                T0.AddMinutes(300),
                null,
                new { AssetId = ChairId, Model = "Ergo" }
            ),
            Row(
                Guid.CreateVersion7(),
                "Asset",
                ChairId,
                "Delete",
                "bob@codaxy.com",
                T0.AddMinutes(400),
                new
                {
                    Id = ChairId,
                    Name = "Old Chair",
                    InventoryNumber = 100002,
                },
                null
            ),
            Row(
                Discount,
                "Volume",
                Guid.CreateVersion7(),
                "Update",
                "bob@codaxy.com",
                T0.AddMinutes(500),
                new { Description = "50 off" },
                new { Description = "50%_off" }
            )
        );

        await context.SaveChangesAsync();

        // The seeding itself was audited, as any save is; the tests are about the rows seeded above.
        await context
            .AuditLogs.Where(a => a.Email == AuditLogInterceptor.SystemUser)
            .ExecuteDeleteAsync();
    }

    private static AuditLog Row(
        Guid id,
        string table,
        Guid entityId,
        string action,
        string email,
        DateTimeOffset time,
        object? old,
        object? @new,
        Guid? transaction = null
    ) =>
        new()
        {
            Id = id,
            Table = table,
            EntityId = entityId,
            ActionType = action,
            Email = email,
            TimeCreated = time,
            TransactionId = transaction ?? Guid.CreateVersion7(),
            OldValuesJson = old is null ? null : JsonSerializer.Serialize(old),
            NewValuesJson = @new is null ? null : JsonSerializer.Serialize(@new),
        };
}

public class AuditLogTests(AuditLogApplication app) : IClassFixture<AuditLogApplication>
{
    private const string Url = "/api/administration/audit-log";

    private async Task<Page<Entry>> List(string query = "")
    {
        var client = await app.ClientAsync();
        var page = await client.GetFromJsonAsync<Page<Entry>>($"{Url}/?{query}");
        return page!;
    }

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task The_first_page_holds_the_window_and_the_total()
    {
        var page = await List("pageSize=10");

        Assert.Equal(10, page.Items.Count);
        Assert.Equal(AuditLogApplication.Seeded, page.Total);
    }

    [Fact]
    public async Task The_last_page_holds_the_remainder()
    {
        var page = await List("pageSize=10&page=4");

        Assert.Equal(AuditLogApplication.Seeded - 30, page.Items.Count);
        Assert.Equal(AuditLogApplication.Seeded, page.Total);
    }

    [Fact]
    public async Task A_page_past_the_last_is_empty_and_keeps_the_total()
    {
        var page = await List("pageSize=10&page=99");

        Assert.Empty(page.Items);
        Assert.Equal(AuditLogApplication.Seeded, page.Total);
    }

    [Theory]
    [InlineData("pageSize=101")]
    [InlineData("pageSize=0")]
    [InlineData("page=0")]
    [InlineData("sort=email")]
    public async Task Refuses_a_window_or_sort_outside_the_convention(string query)
    {
        var client = await app.ClientAsync();
        var response = await client.GetAsync($"{Url}/?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Walking_every_page_sees_each_row_once()
    {
        List<Guid> seen = [];

        for (var page = 1; page <= 8; page++)
            seen.AddRange((await List($"pageSize=5&page={page}")).Items.Select(e => e.Id));

        Assert.Equal(AuditLogApplication.Seeded, seen.Distinct().Count());
        Assert.Equal(AuditLogApplication.Seeded, seen.Count);
    }

    [Fact]
    public async Task Lists_newest_first_unless_asked_for_oldest()
    {
        var newest = await List("pageSize=100");
        var oldest = await List("pageSize=100&sort=time");

        Assert.Equal(app.Discount, newest.Items[0].Id);
        Assert.Equal(AuditLogApplication.T0, oldest.Items[0].Time);
        Assert.Equal(newest.Items.Select(e => e.Id).Reverse(), oldest.Items.Select(e => e.Id));
    }

    [Fact]
    public async Task Every_search_term_has_to_match()
    {
        var laptop = await List("q=laptop");
        var beta = await List("q=LAPTOP+beta");

        Assert.Equal(2, laptop.Total);
        Assert.Equal(app.LaptopRenamed, Assert.Single(beta.Items).Id);
    }

    [Fact]
    public async Task Wildcards_in_the_search_are_taken_literally()
    {
        var page = await List("q=" + Uri.EscapeDataString("50%_"));

        Assert.Equal(app.Discount, Assert.Single(page.Items).Id);
    }

    [Fact]
    public async Task Search_matches_the_user_and_the_entity_type()
    {
        Assert.Equal(AuditLogApplication.Filler, (await List("q=filler")).Total);
        Assert.Equal(1, (await List("q=electronicdev")).Total);
    }

    [Fact]
    public async Task Filters_by_action_type_and_user_together()
    {
        var page = await List("action=Create&table=Asset&email=ana%40codaxy.com");

        Assert.Equal(2, page.Total);
        Assert.All(page.Items, e => Assert.Equal("Create", e.Action));
    }

    [Fact]
    public async Task A_range_includes_its_start_and_excludes_its_end()
    {
        var from = Uri.EscapeDataString(AuditLogApplication.T0.AddMinutes(100).ToString("O"));
        var to = Uri.EscapeDataString(AuditLogApplication.T0.AddMinutes(300).ToString("O"));

        var page = await List($"from={from}&to={to}");

        Assert.Equal(
            new HashSet<Guid> { app.LaptopCreated, app.DeviceCreated, app.LaptopRenamed },
            page.Items.Select(e => e.Id).ToHashSet()
        );
    }

    [Fact]
    public async Task Filters_to_one_record()
    {
        var page = await List($"entityId={AuditLogApplication.LaptopId}");

        Assert.Equal(3, page.Total);
    }

    [Fact]
    public async Task An_inventory_number_finds_the_asset_and_its_subtype_rows()
    {
        var page = await List("inventoryNumber=100001");

        Assert.Equal(3, page.Total);
        Assert.Contains(page.Items, e => e.Table == "ElectronicDevice");
    }

    [Fact]
    public async Task An_inventory_number_finds_an_asset_deleted_since()
    {
        var page = await List("inventoryNumber=100002");

        Assert.Equal(3, page.Total);
    }

    [Fact]
    public async Task A_subtype_row_takes_its_assets_name_and_number()
    {
        var page = await List("pageSize=100");

        var device = page.Items.Single(e => e.Id == app.DeviceCreated);
        var furniture = page.Items.Single(e => e.Id == app.ChairFurniture);

        Assert.Equal(("Laptop Beta", 100001), (device.Label, device.InventoryNumber));
        Assert.Equal(("Old Chair", 100002), (furniture.Label, furniture.InventoryNumber));
    }

    [Fact]
    public async Task An_update_lists_only_the_fields_it_changed()
    {
        var page = await List("pageSize=100");

        Assert.Equal(
            ["Name", "Incomplete"],
            page.Items.Single(e => e.Id == app.LaptopRenamed).Changed
        );
        Assert.Empty(page.Items.Single(e => e.Id == app.LaptopCreated).Changed);
    }

    [Fact]
    public async Task An_entry_holds_every_field_and_marks_the_changed_ones()
    {
        var client = await app.ClientAsync();
        var entry =
            await client.GetFromJsonAsync<App.Administration.AuditLogs.Get.Endpoint.Response>(
                $"{Url}/{app.LaptopRenamed}"
            );

        var fields = entry!.Fields.ToDictionary(f => f.Name);

        Assert.True(fields["Name"].Changed);
        Assert.Equal("Laptop Alpha", fields["Name"].Old!.Text);
        Assert.Equal(ValueKind.Boolean, fields["Incomplete"].New!.Kind);
        Assert.False(fields["InventoryNumber"].Changed);
        Assert.Empty(entry.Related);
    }

    [Fact]
    public async Task An_entry_names_its_foreign_keys_and_the_rest_of_its_save()
    {
        var client = await app.ClientAsync();
        var entry =
            await client.GetFromJsonAsync<App.Administration.AuditLogs.Get.Endpoint.Response>(
                $"{Url}/{app.DeviceCreated}"
            );

        Assert.Equal(
            "Notebook",
            entry!.References["ElectronicDeviceTypeId"][AuditLogApplication.DeviceTypeId.ToString()]
        );
        Assert.Equal(
            "Laptop Beta",
            entry.References["AssetId"][AuditLogApplication.LaptopId.ToString()]
        );
        Assert.Equal(app.LaptopCreated, Assert.Single(entry.Related).Id);
    }

    [Fact]
    public async Task An_unknown_entry_is_not_found()
    {
        var client = await app.ClientAsync();
        var response = await client.GetAsync($"{Url}/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Facets_offer_the_types_and_users_in_the_log()
    {
        var client = await app.ClientAsync();
        var facets =
            await client.GetFromJsonAsync<App.Administration.AuditLogs.Facets.Endpoint.Response>(
                $"{Url}/facets"
            );

        Assert.Equal(["Asset", "ElectronicDevice", "Furniture", "Volume"], facets!.Tables);
        Assert.Equal(["ana@codaxy.com", "bob@codaxy.com", "filler@codaxy.com"], facets.Emails);
    }
}
