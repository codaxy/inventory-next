using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Options = App.ElectronicDevices.Devices.Options.Endpoint.Response;

/// <summary>The read tests' devices, in a database of their own: these tests add and remove devices.</summary>
public class ElectronicDeviceWriteApplication : ElectronicDeviceApplication
{
    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class ElectronicDeviceWriteTests(ElectronicDeviceWriteApplication app)
    : IClassFixture<ElectronicDeviceWriteApplication>
{
    private const string Url = "/api/electronic-devices";
    private const string Editor = "editor@codaxy.com";

    private async Task<HttpClient> Client() => await app.ClientAsync(Editor);

    private async Task<Dictionary<string, object?>> FormAsync(string name)
    {
        var o = (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;
        return new()
        {
            ["name"] = name,
            ["invoiceNumber"] = "INV-42",
            ["vendorId"] = o.Vendors[0].Id,
            ["purchaseValue"] = 999.5m,
            ["purchaseDate"] = "2026-06-01",
            ["personId"] = o.People[0].Id,
            ["confidentialityId"] = o.Confidentialities.Single(c => c.Text == "Internal").Id,
            ["integrityId"] = o.Integrities.Single(c => c.Text == "Medium").Id,
            ["availabilityId"] = o.Availabilities.Single(c => c.Text == "High").Id,
            ["incomplete"] = false,
            ["typeId"] = ElectronicDeviceApplication.LaptopType,
            ["manufacturerId"] = ElectronicDeviceApplication.Lenovo,
            ["manufacturingDate"] = "2026-04-01",
            ["modelName"] = "X1 Carbon",
            ["modelCode"] = "21KC",
            ["serialNumber"] = "SN-1",
            ["warrantyNumber"] = "WN-1",
            ["warrantyExpirationDate"] = "2029-06-01",
        };
    }

    private async Task<DeviceDetail> CreateAsync(
        string name,
        Action<Dictionary<string, object?>>? change = null
    )
    {
        var form = await FormAsync(name);
        change?.Invoke(form);
        var response = await (await Client()).PostAsJsonAsync(Url, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DeviceDetail>())!;
    }

    private static Dictionary<string, object?> FormOf(DeviceDetail d) =>
        new()
        {
            ["name"] = d.Name,
            ["invoiceNumber"] = d.InvoiceNumber,
            ["vendorId"] = d.Vendor.Id,
            ["purchaseValue"] = d.PurchaseValue,
            ["purchaseDate"] = d.PurchaseDate.ToString("yyyy-MM-dd"),
            ["personId"] = d.Person.Id,
            ["incomplete"] = d.Incomplete,
            ["typeId"] = d.Type?.Id,
            ["manufacturerId"] = d.Manufacturer?.Id,
            ["modelName"] = d.ModelName,
            ["serialNumber"] = d.SerialNumber,
            ["warrantyNumber"] = d.WarrantyNumber,
            ["warrantyExpirationDate"] = d.WarrantyExpirationDate?.ToString("yyyy-MM-dd"),
            ["lastModified"] = d.LastModified,
        };

    private Task<List<string>> AuditOf(Guid id) =>
        app.InScopeAsync(c =>
            c.AuditLogs.Where(a => a.EntityId == id)
                .OrderBy(a => a.TimeCreated)
                .ThenBy(a => a.Table)
                .Select(a => a.Table + ":" + a.ActionType + ":" + a.Email)
                .ToListAsync()
        );

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    [Fact]
    public async Task Creating_numbers_it_types_the_asset_saves_every_field_and_is_audited()
    {
        var next = await app.InScopeAsync(c =>
            c.Sequences.Select(s => s.AssetInventoryNumber).FirstAsync()
        );

        var device = await CreateAsync("  X1 for Ana  ");

        Assert.Equal(next, device.Number);
        Assert.Equal(
            ("X1 for Ana", "Laptop", "Lenovo", "X1 Carbon", "21KC", "SN-1"),
            (
                device.Name,
                device.Type!.Name,
                device.Manufacturer!.Name,
                device.ModelName,
                device.ModelCode,
                device.SerialNumber
            )
        );
        Assert.Equal(
            ("WN-1", new DateOnly(2029, 6, 1), new DateOnly(2026, 4, 1)),
            (device.WarrantyNumber, device.WarrantyExpirationDate, device.ManufacturingDate)
        );
        Assert.Equal("Medium", device.Importance!.Name);
        Assert.Equal(["HasData"], device.Tags.Select(t => t.Name));
        Assert.Equal(
            "Electronic Device",
            await app.InScopeAsync(c =>
                c.Assets.Where(a => a.Id == device.Id).Select(a => a.AssetType.Name).FirstAsync()
            )
        );
        Assert.Equal(
            [$"Asset:Create:{Editor}", $"ElectronicDevice:Create:{Editor}"],
            (await AuditOf(device.Id)).Order()
        );
    }

    [Fact]
    public async Task Editing_changes_the_fields_keeps_the_number_and_is_audited()
    {
        var device = await CreateAsync("Edited device");
        var form = FormOf(device);
        form["name"] = "Edited device 2";
        form["serialNumber"] = "SN-2";
        form["warrantyNumber"] = "";
        form["warrantyExpirationDate"] = null;

        var response = await (await Client()).PutAsJsonAsync($"{Url}/{device.Id}", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var edited = (await response.Content.ReadFromJsonAsync<DeviceDetail>())!;
        Assert.Equal(
            ("Edited device 2", device.Number, "SN-2", (string?)null, (DateOnly?)null),
            (
                edited.Name,
                edited.Number,
                edited.SerialNumber,
                edited.WarrantyNumber,
                edited.WarrantyExpirationDate
            )
        );
        var update = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a =>
                    a.EntityId == device.Id
                    && a.Table == "ElectronicDevice"
                    && a.ActionType == "Update"
                )
                .SingleAsync()
        );
        Assert.Contains("\"SerialNumber\": \"SN-1\"", update.OldValuesJson);
        Assert.Contains("\"SerialNumber\": \"SN-2\"", update.NewValuesJson);
    }

    [Fact]
    public async Task An_edit_made_meanwhile_is_not_overwritten()
    {
        var device = await CreateAsync("Contested device");
        var client = await Client();
        var first = FormOf(device);
        first["name"] = "First";
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync($"{Url}/{device.Id}", first)).StatusCode
        );

        var stale = FormOf(device);
        stale["name"] = "Second";
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await client.PutAsJsonAsync($"{Url}/{device.Id}", stale)).StatusCode
        );

        var missing = FormOf(device);
        missing.Remove("lastModified");
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PutAsJsonAsync($"{Url}/{device.Id}", missing),
                "lastModified"
            )
        );
        Assert.Equal(
            "First",
            (await client.GetFromJsonAsync<DeviceDetail>($"{Url}/{device.Id}"))!.Name
        );
    }

    [Fact]
    public async Task Deleting_takes_the_device_its_asset_and_its_contracts_in_one_save()
    {
        var device = await CreateAsync("Doomed device");
        var contract = Guid.CreateVersion7();
        await app.InScopeAsync(async c =>
        {
            c.MaintenanceContracts.Add(
                new MaintenanceContract
                {
                    Id = contract,
                    AssetId = device.Id,
                    VendorId = device.Vendor.Id,
                    ContractNumber = "MC-9",
                }
            );
            return await c.SaveChangesAsync();
        });
        var client = await Client();

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{device.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{device.Id}")).StatusCode
        );
        Assert.False(await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == device.Id)));
        Assert.False(
            await app.InScopeAsync(c => c.MaintenanceContracts.AnyAsync(m => m.Id == contract))
        );
        var deletes = await app.InScopeAsync(c =>
            c.AuditLogs.Where(a =>
                    (a.EntityId == device.Id || a.EntityId == contract) && a.ActionType == "Delete"
                )
                .Select(a => a.Table)
                .ToListAsync()
        );
        Assert.Equal(["Asset", "ElectronicDevice", "MaintenanceContract"], deletes.Order());
    }

    [Fact]
    public async Task A_device_with_a_seat_or_information_on_it_is_not_deleted()
    {
        var response = await (await Client()).DeleteAsync($"{Url}/{app.Laptop}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "A seat is activated on it and a piece of information is kept on it, so the device cannot be deleted.",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.True(await app.InScopeAsync(c => c.Assets.AnyAsync(a => a.Id == app.Laptop)));
    }

    [Theory]
    [InlineData("name")]
    [InlineData("vendorId")]
    [InlineData("personId")]
    [InlineData("purchaseValue")]
    [InlineData("purchaseDate")]
    public async Task A_device_needs_the_asset_s_required_fields(string field)
    {
        var form = await FormAsync("Missing");
        form[field] = null;

        Assert.NotEmpty(await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), field));
    }

    [Theory]
    [InlineData("typeId")]
    [InlineData("manufacturerId")]
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
    public async Task Refuses_a_serial_number_past_its_length()
    {
        var form = await FormAsync("Long serial");
        form["serialNumber"] = new string('s', 201);

        Assert.NotEmpty(
            await ErrorsOf(await (await Client()).PostAsJsonAsync(Url, form), "serialNumber")
        );
    }

    [Fact]
    public async Task An_unknown_device_is_not_found_to_edit_or_delete()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();
        var form = await FormAsync("Nowhere");
        form["lastModified"] = DateTimeOffset.UtcNow;

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", form)).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Options_carry_each_type_s_tags_for_the_form()
    {
        var o = (await (await Client()).GetFromJsonAsync<Options>($"{Url}/options"))!;
        Assert.Equal(["HasData"], o.TypeTags[ElectronicDeviceApplication.LaptopType]);
        Assert.Equal([1, 2, 3], o.Confidentialities.Select(c => c.Weight));
    }
}
