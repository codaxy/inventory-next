using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Items;

/// <summary>
/// What creating and editing a piece of furniture take: the asset's fields — validated, checked and
/// applied by <see cref="AssetWrites"/>, as every asset's are — and its type and model.
/// </summary>
public sealed record FurnitureForm(
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
    [property: StringLength(300, ErrorMessage = "A model is at most 300 characters.")]
        string? Model,
    DateTimeOffset? LastModified
) : IAssetForm;

/// <summary>A piece of furniture as its page shows it: every field, the names beside the ids.</summary>
public sealed record FurnitureDetail(
    Guid Id,
    int? Number,
    string Name,
    string? InvoiceNumber,
    AssetRef Vendor,
    decimal PurchaseValue,
    DateOnly PurchaseDate,
    string? Description,
    AssetRef Person,
    AssetRef? Confidentiality,
    AssetRef? Integrity,
    AssetRef? Availability,
    AssetRef? Importance,
    bool Incomplete,
    AssetRef? BusinessEntity,
    AssetRef? Location,
    string? Url,
    AssetRef? Type,
    string? Model,
    DateTimeOffset LastModified
);

internal static class FurnitureWrites
{
    /// <summary>The seeded asset type every piece of furniture carries.</summary>
    public const string AssetType = "Furniture and fixtures";

    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        FurnitureForm form,
        CancellationToken cancellationToken
    )
    {
        if (await AssetWrites.CheckAsync(context, form, cancellationToken) is { } asset)
            return asset;

        if (
            form.TypeId is { } type
            && !await context.FurnitureTypes.AnyAsync(t => t.Id == type, cancellationToken)
        )
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["typeId"] = ["That choice no longer exists."] }
            );

        return null;
    }

    public static async Task ApplyAsync(
        InventoryContext context,
        Asset asset,
        Furniture furniture,
        FurnitureForm form,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        await AssetWrites.ApplyAsync(context, asset, form, clock, cancellationToken);
        furniture.FurnitureTypeId = form.TypeId;
        furniture.Model = string.IsNullOrWhiteSpace(form.Model) ? null : form.Model.Trim();
    }

    public static Task<FurnitureDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .Furnitures.AsNoTracking()
            .Where(f => f.AssetId == id)
            .Select(f => new FurnitureDetail(
                f.AssetId,
                f.Asset.InventoryNumber,
                f.Asset.Name,
                f.Asset.InvoiceNumber,
                new AssetRef(f.Asset.VendorId, f.Asset.Vendor.Name),
                f.Asset.PurchaseValue,
                f.Asset.PurchaseDate,
                f.Asset.Description,
                new AssetRef(f.Asset.PersonId, f.Asset.Person.Name),
                f.Asset.ConfidentialityId == null
                    ? null
                    : new AssetRef(f.Asset.Confidentiality.Id, f.Asset.Confidentiality.Level),
                f.Asset.IntegrityId == null
                    ? null
                    : new AssetRef(f.Asset.Integrity.Id, f.Asset.Integrity.Level),
                f.Asset.AvailabilityId == null
                    ? null
                    : new AssetRef(f.Asset.Availability.Id, f.Asset.Availability.Level),
                f.Asset.ImportanceId == null
                    ? null
                    : new AssetRef(f.Asset.Importance.Id, f.Asset.Importance.Level),
                f.Asset.Incomplete,
                f.Asset.BusinessEntityId == null
                    ? null
                    : new AssetRef(f.Asset.BusinessEntity.Id, f.Asset.BusinessEntity.Text),
                f.Asset.LocationId == null
                    ? null
                    : new AssetRef(f.Asset.Location.Id, f.Asset.Location.Name),
                f.Asset.URL,
                f.FurnitureTypeId == null
                    ? null
                    : new AssetRef(f.FurnitureType.Id, f.FurnitureType.Name),
                f.Model,
                f.Asset.LastModified
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
