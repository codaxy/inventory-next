using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.Infrastructure.Clouds;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Licenses.Licenses.List.Endpoint.Item;
using Options = App.Licenses.Licenses.Options.Endpoint.Response;

/// <summary>
/// The codebooks, two software entries, and three licences: Office, whose volume has an activation;
/// Backup, whose volume a cloud stands on; and Spare, with nothing on it. A test that changes data
/// makes its own licence.
/// </summary>
public class LicenseApplication : InventoryApplication
{
    public static readonly Guid Office = Guid.CreateVersion7();
    public static readonly Guid Backup = Guid.CreateVersion7();

    public static Seed.SeededLicense OfficeLicense = null!;
    public static Seed.SeededLicense BackupLicense = null!;
    public static Seed.SeededLicense SpareLicense = null!;

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
                Id = Backup,
                Name = "Backup",
                SoftwareOrServiceCategoryId = category.Id,
                ManufacturerId = maker.Id,
            }
        );
        await context.SaveChangesAsync();

        OfficeLicense = await Seed.LicenseWithVolumeAsync(
            context,
            Office,
            "Office licence",
            expires: Today.AddDays(-3)
        );
        BackupLicense = await Seed.LicenseWithVolumeAsync(
            context,
            Backup,
            "Backup licence",
            expires: Today.AddDays(200)
        );
        SpareLicense = await Seed.LicenseWithVolumeAsync(context, Office, "Spare licence");

        await Seed.ActivationAsync(context, OfficeLicense.Volume, quantity: 3);
        context.Clouds.Add(
            new Cloud
            {
                Id = Guid.CreateVersion7(),
                Name = "Seed cloud",
                VolumeId = BackupLicense.Volume,
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

public class LicenseTests(LicenseApplication app) : IClassFixture<LicenseApplication>
{
    private const string Url = "/api/licenses";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<Options> OptionsAsync() =>
        (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;

    /// <summary>A complete form, the codebooks chosen by text; the caller overrides what it tests.</summary>
    private async Task<Dictionary<string, object?>> FormAsync(string name)
    {
        var o = await OptionsAsync();
        return new()
        {
            ["name"] = name,
            ["invoiceNumber"] = "INV-1",
            ["vendorId"] = o.Vendors[0].Id,
            ["purchaseValue"] = 120.5m,
            ["purchaseDate"] = "2026-02-10",
            ["description"] = "made by a test",
            ["personId"] = o.People[0].Id,
            ["confidentialityId"] = o.Confidentialities.Single(c => c.Text == "Confidential").Id,
            ["integrityId"] = o.Integrities.Single(c => c.Text == "High").Id,
            ["availabilityId"] = o.Availabilities.Single(c => c.Text == "Medium").Id,
            ["incomplete"] = true,
            ["licenseTypeId"] = o.LicenseTypes[0].Id,
            ["licenseModelId"] = o.LicenseModels[0].Id,
            ["expirationModelId"] = o.ExpirationModels[0].Id,
            ["expirationDate"] = LicenseApplication.Today.AddDays(400).ToString("yyyy-MM-dd"),
            ["subscriptionFee"] = 9.99m,
            ["currencyId"] = o.Currencies[0].Id,
            ["periodId"] = o.Periods[0].Id,
            ["autoRenew"] = true,
            ["businessEntityId"] = o.BusinessEntities[0].Id,
            ["managementConsoleUrl"] = "https://console.test",
            ["registrationNumber"] = "REG-7",
            ["keyIdentifier"] = "0042-ABCD",
            ["url"] = "https://licence.test",
            ["volumes"] = new object[]
            {
                new
                {
                    softwareOrServiceId = LicenseApplication.Office,
                    volumeTypeId = Seed.PerUser,
                    quantity = 10,
                    description = "seats",
                },
            },
        };
    }

    private async Task<LicenseDetail> CreateAsync(
        string name,
        Action<Dictionary<string, object?>>? change = null
    )
    {
        var form = await FormAsync(name);
        change?.Invoke(form);
        var response = await (await Client()).PostAsJsonAsync(Url, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LicenseDetail>())!;
    }

    /// <summary>The form that saves what the licence already holds, for an edit to change one thing in.</summary>
    private static Dictionary<string, object?> FormOf(LicenseDetail l) =>
        new()
        {
            ["name"] = l.Name,
            ["invoiceNumber"] = l.InvoiceNumber,
            ["vendorId"] = l.Vendor.Id,
            ["purchaseValue"] = l.PurchaseValue,
            ["purchaseDate"] = l.PurchaseDate.ToString("yyyy-MM-dd"),
            ["description"] = l.Description,
            ["personId"] = l.Person.Id,
            ["confidentialityId"] = l.Confidentiality?.Id,
            ["integrityId"] = l.Integrity?.Id,
            ["availabilityId"] = l.Availability?.Id,
            ["incomplete"] = l.Incomplete,
            ["licenseTypeId"] = l.LicenseType?.Id,
            ["licenseModelId"] = l.LicenseModel?.Id,
            ["expirationModelId"] = l.ExpirationModel?.Id,
            ["expirationDate"] = l.ExpirationDate?.ToString("yyyy-MM-dd"),
            ["subscriptionFee"] = l.SubscriptionFee,
            ["currencyId"] = l.Currency?.Id,
            ["periodId"] = l.Period?.Id,
            ["autoRenew"] = l.AutoRenew,
            ["businessEntityId"] = l.BusinessEntity?.Id,
            ["managementConsoleUrl"] = l.ManagementConsoleUrl,
            ["registrationNumber"] = l.RegistrationNumber,
            ["keyIdentifier"] = l.KeyIdentifier,
            ["locationId"] = l.Location?.Id,
            ["url"] = l.Url,
            ["volumes"] = l.Volumes.Select(v => new { id = v.Id }).ToArray(),
            ["lastModified"] = l.LastModified,
        };

    private async Task<LicenseDetail> GetAsync(Guid id) =>
        (await (await Client()).GetFromJsonAsync<LicenseDetail>($"{Url}/{id}"))!;

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
    public async Task Creating_numbers_the_asset_from_the_sequence_and_saves_every_field()
    {
        var next = await app.InScopeAsync(c =>
            c.Sequences.Select(s => s.AssetInventoryNumber).FirstAsync()
        );

        var license = await CreateAsync("  Photoshop  ");

        Assert.Equal(next, license.Number);
        Assert.Equal(
            next + 1,
            await app.InScopeAsync(c =>
                c.Sequences.Select(s => s.AssetInventoryNumber).FirstAsync()
            )
        );
        Assert.Equal(
            ("Photoshop", "INV-1", 120.5m, new DateOnly(2026, 2, 10), "made by a test"),
            (
                license.Name,
                license.InvoiceNumber,
                license.PurchaseValue,
                license.PurchaseDate,
                license.Description
            )
        );
        Assert.True(license.Incomplete);
        Assert.True(license.AutoRenew);
        Assert.Equal(
            (9.99m, "https://console.test", "REG-7", "0042-ABCD", "https://licence.test"),
            (
                license.SubscriptionFee,
                license.ManagementConsoleUrl,
                license.RegistrationNumber,
                license.KeyIdentifier,
                license.Url
            )
        );
        Assert.Equal("regular", license.Expiry);

        var volume = Assert.Single(license.Volumes);
        Assert.Equal(
            ("Office", "Per user", 10, "seats", 0, null),
            (
                volume.Software.Name,
                volume.Type.Name,
                volume.Quantity,
                volume.Description,
                volume.InUse,
                volume.Held
            )
        );

        var type = await app.InScopeAsync(c =>
            c.Assets.Where(a => a.Id == license.Id).Select(a => a.AssetType.Name).FirstAsync()
        );
        Assert.Equal("Licenses", type);
    }

    [Theory]
    [InlineData("Confidential", "High", "Medium", "High")]
    [InlineData("Public", "Low", "Low", "Low")]
    [InlineData("Internal", "Medium", "Medium", "Medium")]
    public async Task Importance_is_computed_from_the_three_weights(
        string c,
        string i,
        string a,
        string importance
    )
    {
        var o = await OptionsAsync();
        var license = await CreateAsync(
            $"Weighted {c} {i} {a}",
            f =>
            {
                f["confidentialityId"] = o.Confidentialities.Single(x => x.Text == c).Id;
                f["integrityId"] = o.Integrities.Single(x => x.Text == i).Id;
                f["availabilityId"] = o.Availabilities.Single(x => x.Text == a).Id;
            }
        );

        Assert.Equal(importance, license.Importance!.Name);
    }

    [Fact]
    public async Task No_importance_unless_all_three_weights_are_chosen()
    {
        var license = await CreateAsync("Unweighted", f => f["availabilityId"] = null);

        Assert.Null(license.Importance);
    }

    [Fact]
    public async Task Creating_is_audited_as_the_user_for_the_asset_the_licence_and_the_volume()
    {
        var license = await CreateAsync("Audited");

        var rows = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == license.Id || a.EntityId == license.Volumes[0].Id)
                .Select(a => new
                {
                    a.Table,
                    a.ActionType,
                    a.Email,
                })
                .ToListAsync()
        );

        Assert.Equal(new[] { "Asset", "License", "Volume" }, rows.Select(r => r.Table).Order());
        Assert.All(rows, r => Assert.Equal(("Create", Editor), (r.ActionType, r.Email)));
    }

    [Fact]
    public async Task Editing_changes_the_fields_keeps_the_number_and_adds_and_removes_volumes()
    {
        var license = await CreateAsync("Illustrator");
        var form = FormOf(license);
        form["name"] = "Illustrator CC";
        form["incomplete"] = false;
        form["keyIdentifier"] = "";
        form["volumes"] = new object[]
        {
            new
            {
                softwareOrServiceId = LicenseApplication.Backup,
                volumeTypeId = Seed.PerDevice,
                quantity = 2,
            },
        };

        var response = await (await Client()).PutAsJsonAsync($"{Url}/{license.Id}", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<LicenseDetail>())!;
        Assert.Equal(
            ("Illustrator CC", license.Number, false, (string?)null),
            (edited.Name, edited.Number, edited.Incomplete, edited.KeyIdentifier)
        );
        Assert.Equal(
            ("Backup", "Per device", 2),
            (
                edited.Volumes.Single().Software.Name,
                edited.Volumes.Single().Type.Name,
                edited.Volumes.Single().Quantity
            )
        );
        Assert.False(
            await app.InScopeAsync(c => c.Volumes.AnyAsync(v => v.Id == license.Volumes[0].Id))
        );
        Assert.True(edited.LastModified > license.LastModified);

        var update = (await AuditFor(license.Id)).Last(a => a.Table == "Asset");
        Assert.Equal("Update", update.ActionType);
        Assert.Contains("\"Name\": \"Illustrator\"", update.OldValuesJson);
        Assert.Contains("\"Name\": \"Illustrator CC\"", update.NewValuesJson);
        Assert.Equal("Delete", (await AuditFor(license.Volumes[0].Id)).Last().ActionType);
    }

    [Fact]
    public async Task An_existing_volume_the_form_keeps_stays_as_it_was()
    {
        var license = await CreateAsync("Kept volume");
        var form = FormOf(license);
        form["volumes"] = new object[] { new { id = license.Volumes[0].Id, quantity = 999 } };

        var edited = (
            await (
                await (await Client()).PutAsJsonAsync($"{Url}/{license.Id}", form)
            ).Content.ReadFromJsonAsync<LicenseDetail>()
        )!;

        Assert.Equal(10, edited.Volumes.Single().Quantity);
    }

    [Fact]
    public async Task An_edit_made_meanwhile_is_not_overwritten()
    {
        var license = await CreateAsync("Contested");
        var client = await Client();

        var first = FormOf(license);
        first["name"] = "First";
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"{Url}/{license.Id}", first)).StatusCode
        );

        var stale = FormOf(license);
        stale["name"] = "Second";
        var response = await client.PutAsJsonAsync($"{Url}/{license.Id}", stale);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("First", (await GetAsync(license.Id)).Name);

        var missing = FormOf(license);
        missing.Remove("lastModified");
        Assert.Contains(
            "lastModified",
            (
                await (
                    await client.PutAsJsonAsync($"{Url}/{license.Id}", missing)
                ).Content.ReadFromJsonAsync<ValidationProblemDetails>()
            )!
                .Errors
                .Keys
        );
    }

    [Fact]
    public async Task A_volume_with_activations_is_not_removed_and_nothing_else_is_saved()
    {
        var office = await GetAsync(LicenseApplication.OfficeLicense.License);
        var form = FormOf(office);
        form["name"] = "Renamed alongside";
        form["volumes"] = Array.Empty<object>();

        var response = await (await Client()).PutAsJsonAsync($"{Url}/{office.Id}", form);

        Assert.Equal(
            ["The Office volume has an activation, so it cannot be removed."],
            await ErrorsOf(response, "volumes")
        );
        var after = await GetAsync(office.Id);
        Assert.Equal("Office licence", after.Name);
        Assert.Single(after.Volumes);
        Assert.Equal("The Office volume has an activation", after.Volumes[0].Held);
        Assert.Equal(3, after.Volumes[0].InUse);
    }

    [Fact]
    public async Task Deleting_takes_the_volumes_the_licence_and_the_asset_and_is_audited()
    {
        var license = await CreateAsync("Doomed");
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{license.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{license.Id}")).StatusCode
        );
        Assert.False(await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == license.Id)));
        Assert.False(
            await app.InScopeAsync(c => c.Volumes.AnyAsync(v => v.Id == license.Volumes[0].Id))
        );

        var deletes = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a =>
                    a.ActionType == "Delete"
                    && (a.EntityId == license.Id || a.EntityId == license.Volumes[0].Id)
                )
                .Select(a => a.Table)
                .ToListAsync()
        );
        Assert.Equal(new[] { "Asset", "License", "Volume" }, deletes.Order());
    }

    [Fact]
    public async Task A_licence_something_stands_on_is_not_deleted_and_says_what()
    {
        var client = await Client();

        var activated = await client.DeleteAsync(
            $"{Url}/{LicenseApplication.OfficeLicense.License}"
        );
        var clouded = await client.DeleteAsync($"{Url}/{LicenseApplication.BackupLicense.License}");

        Assert.Equal(HttpStatusCode.Conflict, activated.StatusCode);
        Assert.Equal(
            "The Office volume has an activation, so the licence cannot be deleted.",
            (await activated.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.Equal(
            "The Backup volume has a cloud, so the licence cannot be deleted.",
            (await clouded.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.True(
            await app.InScopeAsync(c =>
                c.Activations.AnyAsync(a => a.VolumeId == LicenseApplication.OfficeLicense.Volume)
            )
        );
        Assert.True(
            await app.InScopeAsync(c =>
                c.Clouds.AnyAsync(a => a.VolumeId == LicenseApplication.BackupLicense.Volume)
            )
        );
    }

    [Fact]
    public async Task A_licence_with_a_maintenance_contract_is_not_deleted()
    {
        var license = await CreateAsync("Maintained");
        var vendor = license.Vendor.Id;
        await app.InScopeAsync(async c =>
        {
            c.MaintenanceContracts.Add(
                new MaintenanceContract
                {
                    Id = Guid.CreateVersion7(),
                    AssetId = license.Id,
                    VendorId = vendor,
                }
            );
            return await c.SaveChangesAsync();
        });

        var response = await (await Client()).DeleteAsync($"{Url}/{license.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.StartsWith(
            "A maintenance contract",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("vendorId", null)]
    [InlineData("personId", null)]
    [InlineData("purchaseValue", null)]
    [InlineData("purchaseDate", null)]
    [InlineData("purchaseValue", -1)]
    public async Task A_licence_needs_its_required_fields(string field, object? value)
    {
        var form = await FormAsync("Missing");
        form[field] = value;

        var response = await (await Client()).PostAsJsonAsync(Url, form);

        Assert.NotEmpty(await ErrorsOf(response, field));
    }

    [Theory]
    [InlineData("name", 301)]
    [InlineData("description", 1001)]
    [InlineData("url", 501)]
    [InlineData("keyIdentifier", 501)]
    public async Task Refuses_text_past_its_length(string field, int length)
    {
        var form = await FormAsync("Long");
        form[field] = new string('x', length);

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field));
    }

    [Theory]
    [InlineData("vendorId")]
    [InlineData("personId")]
    [InlineData("licenseTypeId")]
    [InlineData("currencyId")]
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
    public async Task A_new_volume_needs_software_a_type_and_a_seat()
    {
        var client = await Client();

        async Task<string[]> With(object volume)
        {
            var form = await FormAsync("Bad volume");
            form["volumes"] = new[] { volume };
            return await ErrorsOf(await client.PostAsJsonAsync(Url, form), "volumes");
        }

        Assert.Equal(
            ["A volume needs a software or service, a type and a quantity."],
            await With(new { volumeTypeId = 1, quantity = 1 })
        );
        Assert.Equal(
            ["A volume has at least one seat."],
            await With(
                new
                {
                    softwareOrServiceId = LicenseApplication.Office,
                    volumeTypeId = 1,
                    quantity = 0,
                }
            )
        );
        Assert.Equal(
            ["A volume's software or service no longer exists."],
            await With(
                new
                {
                    softwareOrServiceId = Guid.CreateVersion7(),
                    volumeTypeId = 1,
                    quantity = 1,
                }
            )
        );
        Assert.Equal(
            ["A volume's type no longer exists."],
            await With(
                new
                {
                    softwareOrServiceId = LicenseApplication.Office,
                    volumeTypeId = 99,
                    quantity = 1,
                }
            )
        );
        Assert.Equal(
            ["A new licence has only new volumes."],
            await With(new { id = Guid.CreateVersion7() })
        );
    }

    [Fact]
    public async Task An_unknown_licence_is_not_found_to_read_edit_or_delete()
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
    public async Task Lists_with_the_expiry_and_searches_number_name_vendor_invoice_and_registration()
    {
        var office = Assert.Single((await ListAsync("q=office+licence")).Items);
        Assert.Equal(
            ("Office licence", "Seed vendor", "expired"),
            (office.Name, office.Vendor, office.Expiry)
        );

        var license = await CreateAsync("Searchable");
        Assert.Contains((await ListAsync($"q={license.Number}")).Items, i => i.Id == license.Id);
        Assert.Contains((await ListAsync("q=REG-7")).Items, i => i.Id == license.Id);
        Assert.Contains((await ListAsync("q=inv-1")).Items, i => i.Id == license.Id);
    }

    [Fact]
    public async Task Filters_by_vendor_purchase_dates_expiry_and_incomplete()
    {
        var o = await OptionsAsync();

        Assert.All(
            (await ListAsync($"vendorId={o.Vendors[0].Id}")).Items,
            i => Assert.Equal(o.Vendors[0].Text, i.Vendor)
        );
        Assert.All(
            (await ListAsync("purchasedFrom=2026-01-15&purchasedTo=2026-01-16")).Items,
            i => Assert.Equal(new DateOnly(2026, 1, 15), i.PurchaseDate)
        );
        Assert.Empty((await ListAsync("purchasedFrom=2026-01-16&purchasedTo=2026-01-16")).Items);
        Assert.Equal(
            "Office licence",
            Assert.Single((await ListAsync("expiry=expired")).Items).Name
        );
        Assert.Contains((await ListAsync("expiry=regular")).Items, i => i.Name == "Backup licence");
        Assert.Contains((await ListAsync("expiry=none")).Items, i => i.Name == "Spare licence");
        Assert.All((await ListAsync("expiry=soon")).Items, i => Assert.Equal("soon", i.Expiry));
        Assert.All((await ListAsync("incomplete=true")).Items, i => Assert.True(i.Incomplete));
        Assert.All((await ListAsync("incomplete=false")).Items, i => Assert.False(i.Incomplete));
    }

    [Fact]
    public async Task Sorts_by_last_modified_by_default_and_by_each_column_and_pages()
    {
        var all = (await ListAsync("pageSize=100")).Items;
        Assert.Equal(
            all.OrderByDescending(i => i.LastModified).Select(i => i.Id),
            all.Select(i => i.Id)
        );

        var byNumber = (await ListAsync("sort=number&pageSize=100")).Items;
        Assert.Equal(
            byNumber.OrderBy(i => i.Number).Select(i => i.Number),
            byNumber.Select(i => i.Number)
        );

        foreach (var key in new[] { "name", "vendor", "value", "purchased", "expires", "modified" })
            Assert.Equal(all.Count, (await ListAsync($"sort=-{key}&pageSize=100")).Items.Count);

        var first = await ListAsync("pageSize=1");
        Assert.Single(first.Items);
        Assert.True(first.Total >= 3);
        Assert.Empty((await ListAsync("page=999")).Items);
    }

    [Theory]
    [InlineData("sort=colour")]
    [InlineData("expiry=later")]
    [InlineData("pageSize=101")]
    [InlineData("page=0")]
    public async Task Refuses_a_query_outside_the_convention(string query)
    {
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Url}/?{query}")).StatusCode
        );
    }

    [Fact]
    public async Task Options_offer_every_picker_with_the_weights()
    {
        var o = await OptionsAsync();

        Assert.Equal([1, 2, 3], o.Confidentialities.Select(c => c.Weight));
        Assert.Equal(
            ["Per user", "Per network", "Per device", "Per server"],
            o.VolumeTypes.Select(t => t.Text)
        );
        Assert.Contains(o.Software, s => s.Text == "Office");
        Assert.NotEmpty(o.Vendors);
        Assert.NotEmpty(o.People);
    }
}
