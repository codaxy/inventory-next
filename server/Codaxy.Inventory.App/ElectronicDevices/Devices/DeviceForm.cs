using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices;

/// <summary>
/// What creating and editing a device take: the asset's fields — validated, checked and applied by
/// <see cref="AssetWrites"/>, as every asset's are — and the device's own. "Warranty" is the entity's
/// "guarantee".
/// </summary>
public sealed record DeviceForm(
    string? Name,
    string? InvoiceNumber,
    Guid? VendorId,
    decimal? PurchaseValue,
    DateOnly? PurchaseDate,
    string? Description,
    Guid? PersonId,
    Guid? ConfidentialityId,
    Guid? IntegrityId,
    Guid? AvailabilityId,
    bool Incomplete,
    Guid? BusinessEntityId,
    Guid? LocationId,
    string? Url,
    Guid? TypeId,
    Guid? ManufacturerId,
    DateOnly? ManufacturingDate,
    [property: StringLength(200, ErrorMessage = "A model name is at most 200 characters.")]
        string? ModelName,
    [property: StringLength(200, ErrorMessage = "A model code is at most 200 characters.")]
        string? ModelCode,
    [property: StringLength(200, ErrorMessage = "A serial number is at most 200 characters.")]
        string? SerialNumber,
    [property: StringLength(200, ErrorMessage = "A warranty number is at most 200 characters.")]
        string? WarrantyNumber,
    DateOnly? WarrantyExpirationDate,
    DateTimeOffset? LastModified
) : IAssetForm;

internal static class DeviceWrites
{
    /// <summary>The seeded asset type every device carries.</summary>
    public const string AssetType = "Electronic Device";

    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        DeviceForm form,
        CancellationToken cancellationToken
    )
    {
        if (await AssetWrites.CheckAsync(context, form, cancellationToken) is { } asset)
            return asset;

        var errors = new Dictionary<string, string[]>();
        if (
            form.TypeId is { } type
            && !await context.ElectronicDeviceTypes.AnyAsync(t => t.Id == type, cancellationToken)
        )
            errors["typeId"] = ["That choice no longer exists."];
        if (
            form.ManufacturerId is { } manufacturer
            && !await context.Manufacturers.AnyAsync(m => m.Id == manufacturer, cancellationToken)
        )
            errors["manufacturerId"] = ["That choice no longer exists."];

        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static async Task ApplyAsync(
        InventoryContext context,
        Asset asset,
        ElectronicDevice device,
        DeviceForm form,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        await AssetWrites.ApplyAsync(context, asset, form, clock, cancellationToken);
        device.ElectronicDeviceTypeId = form.TypeId;
        device.ManufacturerId = form.ManufacturerId;
        device.ManufacturingDate = form.ManufacturingDate;
        device.ModelName = Text(form.ModelName);
        device.ModelCode = Text(form.ModelCode);
        device.SerialNumber = Text(form.SerialNumber);
        device.GuaranteeNumber = Text(form.WarrantyNumber);
        device.GuaranteeExpirationDate = form.WarrantyExpirationDate;
    }
}
