using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Furnitures.Items;
using Codaxy.Inventory.App.Furnitures.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Furnitures.Items.List.Endpoint.Item;
using Options = App.Furnitures.Items.Options.Endpoint.Response;
using TypeItem = App.Furnitures.Types.List.Endpoint.Item;

/// <summary>The codebooks, two types — Chair, which a desk chair is of, and Shelf, which nothing is.</summary>
public class FurnitureApplication : InventoryApplication
{
    public static readonly Guid Chair = Guid.CreateVersion7();
    public static readonly Guid Shelf = Guid.CreateVersion7();
    public static Guid DeskChair;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var basics = await Seed.BasicsAsync(context);

        context.FurnitureTypes.AddRange(
            new FurnitureType
            {
                Id = Chair,
                Name = "Chair",
                Description = "To sit on",
            },
            new FurnitureType { Id = Shelf, Name = "Shelf" }
        );
        DeskChair = Guid.CreateVersion7();
        context.Assets.Add(
            new Asset
            {
                Id = DeskChair,
                Name = "Desk chair",
                InventoryNumber = 500001,
                AssetTypeId = (
                    await context.AssetTypes.FirstAsync(t => t.Name == "Furniture and fixtures")
                ).Id,
                VendorId = basics.Vendor,
                PersonId = basics.Person,
                PurchaseDate = new DateOnly(2026, 3, 1),
                PurchaseValue = 80,
                LastModified = DateTimeOffset.UtcNow,
                Furniture = new Furniture
                {
                    AssetId = DeskChair,
                    FurnitureTypeId = Chair,
                    Model = "Ergo 2",
                },
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

public class FurnitureTests(FurnitureApplication app) : IClassFixture<FurnitureApplication>
{
    private const string Url = "/api/furniture";
    private const string Types = "/api/furniture/types";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<Options> OptionsAsync() =>
        (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;

    private async Task<Dictionary<string, object?>> FormAsync(string name)
    {
        var o = await OptionsAsync();
        return new()
        {
            ["name"] = name,
            ["invoiceNumber"] = "INV-9",
            ["vendorId"] = o.Vendors[0].Id,
            ["purchaseValue"] = 150m,
            ["purchaseDate"] = "2026-04-02",
            ["personId"] = o.People[0].Id,
            ["confidentialityId"] = o.Confidentialities.Single(c => c.Text == "Public").Id,
            ["integrityId"] = o.Integrities.Single(c => c.Text == "Low").Id,
            ["availabilityId"] = o.Availabilities.Single(c => c.Text == "Medium").Id,
            ["incomplete"] = true,
            ["typeId"] = FurnitureApplication.Chair,
            ["model"] = "Ergo 3",
        };
    }

    private async Task<FurnitureDetail> CreateAsync(
        string name,
        Action<Dictionary<string, object?>>? change = null
    )
    {
        var form = await FormAsync(name);
        change?.Invoke(form);
        var response = await (await Client()).PostAsJsonAsync(Url, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<FurnitureDetail>())!;
    }

    private static Dictionary<string, object?> FormOf(FurnitureDetail f) =>
        new()
        {
            ["name"] = f.Name,
            ["invoiceNumber"] = f.InvoiceNumber,
            ["vendorId"] = f.Vendor.Id,
            ["purchaseValue"] = f.PurchaseValue,
            ["purchaseDate"] = f.PurchaseDate.ToString("yyyy-MM-dd"),
            ["description"] = f.Description,
            ["personId"] = f.Person.Id,
            ["confidentialityId"] = f.Confidentiality?.Id,
            ["integrityId"] = f.Integrity?.Id,
            ["availabilityId"] = f.Availability?.Id,
            ["incomplete"] = f.Incomplete,
            ["typeId"] = f.Type?.Id,
            ["model"] = f.Model,
            ["lastModified"] = f.LastModified,
        };

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
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync($"{Url}/")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync($"{Types}/")).StatusCode
        );
    }

    // Furniture

    [Fact]
    public async Task Creating_numbers_it_from_the_sequence_types_the_asset_and_saves_every_field()
    {
        var next = await app.InScopeAsync(c =>
            c.Sequences.Select(s => s.AssetInventoryNumber).FirstAsync()
        );

        var chair = await CreateAsync("  Meeting chair  ");

        Assert.Equal(next, chair.Number);
        Assert.Equal(
            ("Meeting chair", "INV-9", 150m, "Chair", "Ergo 3", true),
            (
                chair.Name,
                chair.InvoiceNumber,
                chair.PurchaseValue,
                chair.Type!.Name,
                chair.Model,
                chair.Incomplete
            )
        );
        Assert.Equal("Low", chair.Importance!.Name);
        Assert.Equal(
            "Furniture and fixtures",
            await app.InScopeAsync(c =>
                c.Assets.Where(a => a.Id == chair.Id).Select(a => a.AssetType.Name).FirstAsync()
            )
        );

        var tables = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == chair.Id)
                .Select(a => a.Table + ":" + a.ActionType + ":" + a.Email)
                .ToListAsync()
        );
        Assert.Equal(
            new[] { $"Asset:Create:{Editor}", $"Furniture:Create:{Editor}" },
            tables.Order()
        );
    }

    [Fact]
    public async Task Editing_changes_the_fields_keeps_the_number_and_is_audited()
    {
        var chair = await CreateAsync("Stool");
        var form = FormOf(chair);
        form["name"] = "Bar stool";
        form["typeId"] = FurnitureApplication.Shelf;
        form["model"] = "";
        form["incomplete"] = false;

        var response = await (await Client()).PutAsJsonAsync($"{Url}/{chair.Id}", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<FurnitureDetail>())!;
        Assert.Equal(
            ("Bar stool", chair.Number, "Shelf", (string?)null, false),
            (edited.Name, edited.Number, edited.Type!.Name, edited.Model, edited.Incomplete)
        );
        var update = (await AuditFor(chair.Id)).Last(a => a.Table == "Asset");
        Assert.Equal("Update", update.ActionType);
        Assert.Contains("\"Name\": \"Stool\"", update.OldValuesJson);
        Assert.Contains("\"Name\": \"Bar stool\"", update.NewValuesJson);
    }

    [Fact]
    public async Task An_edit_made_meanwhile_is_not_overwritten()
    {
        var chair = await CreateAsync("Contested chair");
        var client = await Client();
        var first = FormOf(chair);
        first["name"] = "First";
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"{Url}/{chair.Id}", first)).StatusCode
        );

        var stale = FormOf(chair);
        stale["name"] = "Second";
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await client.PutAsJsonAsync($"{Url}/{chair.Id}", stale)).StatusCode
        );

        var missing = FormOf(chair);
        missing.Remove("lastModified");
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PutAsJsonAsync($"{Url}/{chair.Id}", missing),
                "lastModified"
            )
        );
    }

    [Fact]
    public async Task Deleting_takes_the_furniture_and_the_asset_and_is_audited()
    {
        var chair = await CreateAsync("Doomed chair");
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{chair.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{chair.Id}")).StatusCode
        );
        Assert.False(await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == chair.Id)));
        var deletes = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == chair.Id && a.ActionType == "Delete")
                .Select(a => a.Table)
                .ToListAsync()
        );
        Assert.Equal(new[] { "Asset", "Furniture" }, deletes.Order());
    }

    [Fact]
    public async Task Furniture_with_a_maintenance_contract_is_not_deleted()
    {
        var chair = await CreateAsync("Maintained chair");
        await app.InScopeAsync(async c =>
        {
            c.MaintenanceContracts.Add(
                new MaintenanceContract
                {
                    Id = Guid.CreateVersion7(),
                    AssetId = chair.Id,
                    VendorId = chair.Vendor.Id,
                }
            );
            return await c.SaveChangesAsync();
        });

        var response = await (await Client()).DeleteAsync($"{Url}/{chair.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.True(await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == chair.Id)));
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("vendorId", null)]
    [InlineData("personId", null)]
    [InlineData("purchaseValue", null)]
    [InlineData("purchaseDate", null)]
    [InlineData("purchaseValue", -5)]
    public async Task Furniture_needs_the_assets_required_fields(string field, object? value)
    {
        var form = await FormAsync("Missing");
        form[field] = value;

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field));
    }

    [Theory]
    [InlineData("typeId")]
    [InlineData("vendorId")]
    [InlineData("locationId")]
    public async Task Refuses_a_choice_that_does_not_exist(string field)
    {
        var form = await FormAsync("Ghost");
        form[field] = Guid.CreateVersion7();

        Assert.Equal(
            ["That choice no longer exists."],
            await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field)
        );
    }

    [Fact]
    public async Task Refuses_a_model_past_its_length()
    {
        var form = await FormAsync("Long model");
        form["model"] = new string('m', 301);

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), "model"));
    }

    [Fact]
    public async Task An_unknown_piece_is_not_found_to_read_edit_or_delete()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();
        var form = await FormAsync("Nowhere");
        form["lastModified"] = DateTimeOffset.UtcNow;

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", form)).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Lists_searching_number_name_model_and_filters_by_type_and_incomplete()
    {
        var desk = Assert.Single((await ListAsync("q=ergo+2")).Items);
        Assert.Equal(("Desk chair", "Chair", 500001), (desk.Name, desk.Type, desk.Number));
        Assert.Contains(
            (await ListAsync("q=500001")).Items,
            i => i.Id == FurnitureApplication.DeskChair
        );
        Assert.All(
            (await ListAsync($"typeId={FurnitureApplication.Chair}")).Items,
            i => Assert.Equal("Chair", i.Type)
        );
        Assert.All((await ListAsync("incomplete=false")).Items, i => Assert.False(i.Incomplete));
        Assert.All(
            (await ListAsync("purchasedFrom=2026-03-01&purchasedTo=2026-03-02")).Items,
            i => Assert.Equal(FurnitureApplication.DeskChair, i.Id)
        );
    }

    [Fact]
    public async Task Sorts_by_each_column_and_pages()
    {
        var all = (await ListAsync("pageSize=100")).Items;
        Assert.Equal(
            all.OrderByDescending(i => i.LastModified).Select(i => i.Id),
            all.Select(i => i.Id)
        );
        foreach (
            var key in new[]
            {
                "number",
                "name",
                "assignee",
                "location",
                "type",
                "vendor",
                "value",
                "modified",
            }
        )
            Assert.Equal(all.Count, (await ListAsync($"sort={key}&pageSize=100")).Items.Count);

        Assert.Single((await ListAsync("pageSize=1")).Items);
        Assert.Empty((await ListAsync("page=999")).Items);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Url}/?sort=colour")).StatusCode
        );
    }

    [Fact]
    public async Task Exports_what_the_list_selects_named_filtered_when_narrowed()
    {
        var client = await Client();

        var chairs = await client.GetAsync($"{Url}/export?q=desk+chair");
        var all = await client.GetAsync($"{Url}/export");

        Assert.Equal(
            "Furniture.Export - Filtered.xlsx",
            chairs.Content.Headers.ContentDisposition?.FileNameStar
                ?? chairs.Content.Headers.ContentDisposition?.FileName
        );
        Assert.Equal(
            "Furniture.Export.xlsx",
            all.Content.Headers.ContentDisposition?.FileNameStar
                ?? all.Content.Headers.ContentDisposition?.FileName
        );
        var text = await Spreadsheet.TextOf(chairs);
        Assert.Contains("Desk chair", text);
        Assert.Contains("Ergo 2", text);
        Assert.Contains("Invoice Number", text);
    }

    [Fact]
    public async Task Options_offer_the_assets_pickers_and_the_types()
    {
        var o = await OptionsAsync();

        Assert.Equal(
            ["Chair", "Shelf"],
            o.Types.Select(t => t.Text).Where(t => t is "Chair" or "Shelf")
        );
        Assert.NotEmpty(o.Vendors);
        Assert.Equal([1, 2, 3], o.Confidentialities.Select(c => c.Weight));
    }

    // Types

    [Fact]
    public async Task A_type_is_created_edited_and_deleted_with_its_audit_rows()
    {
        var client = await Client();

        var created = await client.PostAsJsonAsync(
            Types,
            new { name = "  Cabinet  ", description = "Doors" }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var type = (await created.Content.ReadFromJsonAsync<TypeDetail>())!;
        Assert.Equal(("Cabinet", "Doors", 0), (type.Name, type.Description, type.FurnitureCount));

        var edited = await client.PutAsJsonAsync(
            $"{Types}/{type.Id}",
            new { name = "Tall cabinet", description = "" }
        );
        var after = (await edited.Content.ReadFromJsonAsync<TypeDetail>())!;
        Assert.Equal(("Tall cabinet", (string?)null), (after.Name, after.Description));

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Types}/{type.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Types}/{type.Id}")).StatusCode
        );
        Assert.Equal(
            ["Create", "Update", "Delete"],
            (await AuditFor(type.Id)).Select(a => a.ActionType)
        );
    }

    [Fact]
    public async Task A_type_furniture_is_of_is_not_deleted()
    {
        var response = await (await Client()).DeleteAsync($"{Types}/{FurnitureApplication.Chair}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(
            "of this type",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
    }

    [Fact]
    public async Task A_type_name_is_required_unique_whatever_its_case_and_kept_by_its_own()
    {
        var client = await Client();

        Assert.NotEmpty(
            await ErrorsOf(await client.PostAsJsonAsync(Types, new { name = " " }), "name")
        );
        Assert.Equal(
            ["A type with this name already exists."],
            await ErrorsOf(await client.PostAsJsonAsync(Types, new { name = "CHAIR" }), "name")
        );
        Assert.Equal(
            HttpStatusCode.OK,
            (
                await client.PutAsJsonAsync(
                    $"{Types}/{FurnitureApplication.Shelf}",
                    new { name = "shelf" }
                )
            ).StatusCode
        );
        await client.PutAsJsonAsync(
            $"{Types}/{FurnitureApplication.Shelf}",
            new { name = "Shelf" }
        );
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync(Types, new { name = new string('n', 101) }),
                "name"
            )
        );
    }

    [Fact]
    public async Task Types_list_with_their_furniture_count_search_sort_and_page()
    {
        var chair = Assert.Single(
            (await (await Client()).GetFromJsonAsync<Page<TypeItem>>($"{Types}/?q=sit"))!.Items
        );
        Assert.Equal(("Chair", 1), (chair.Name, chair.FurnitureCount));

        var byCount = (
            await (await Client()).GetFromJsonAsync<Page<TypeItem>>(
                $"{Types}/?sort=-furniture&pageSize=100"
            )
        )!.Items;
        Assert.Equal(
            byCount.OrderByDescending(t => t.FurnitureCount).Select(t => t.FurnitureCount),
            byCount.Select(t => t.FurnitureCount)
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Types}/?sort=colour")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await (await Client()).PutAsJsonAsync(
                    $"{Types}/{Guid.CreateVersion7()}",
                    new { name = "x" }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await (await Client()).DeleteAsync($"{Types}/{Guid.CreateVersion7()}")).StatusCode
        );
    }
}
