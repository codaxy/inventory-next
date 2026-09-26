using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Shared.Assets;

namespace Codaxy.Inventory.App.Licenses.Licenses;

/// <summary>
/// What creating and editing a licence take: the asset's fields — validated, checked and applied by
/// <see cref="AssetWrites"/>, as every asset's are — the licence's own, and its volumes.
/// </summary>
public sealed record LicenseForm(
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
    Guid? LicenseTypeId,
    Guid? LicenseModelId,
    Guid? ExpirationModelId,
    DateOnly? ExpirationDate,
    [property: Range(0, 1_000_000_000, ErrorMessage = "A fee is not negative.")]
        decimal? SubscriptionFee,
    Guid? CurrencyId,
    Guid? PeriodId,
    bool AutoRenew,
    Guid? BusinessEntityId,
    [property: StringLength(500, ErrorMessage = "A URL is at most 500 characters.")]
        string? ManagementConsoleUrl,
    [property: StringLength(200, ErrorMessage = "A registration number is at most 200 characters.")]
        string? RegistrationNumber,
    [property: StringLength(500, ErrorMessage = "A key identifier is at most 500 characters.")]
        string? KeyIdentifier,
    Guid? LocationId,
    string? Url,
    IReadOnlyList<VolumeForm>? Volumes,
    DateTimeOffset? LastModified
) : IAssetForm;

/// <param name="Id">An existing volume's, kept as it is; absent for one to add.</param>
public sealed record VolumeForm(
    Guid? Id,
    Guid? SoftwareOrServiceId,
    int? VolumeTypeId,
    int? Quantity,
    string? Description
);

public sealed record Ref(Guid Id, string Name);

public sealed record VolumeTypeRef(int Id, string Name);

/// <param name="InUse">Seats taken by activations still active.</param>
/// <param name="Held">Why it cannot be removed — activations, a cloud or software on it — or none.</param>
public sealed record VolumeDetail(
    Guid Id,
    Ref Software,
    VolumeTypeRef Type,
    int Quantity,
    string? Description,
    int InUse,
    int ActivationCount,
    string? Held
);

/// <summary>A licence as its page shows it: every field, the names beside the ids.</summary>
public sealed record LicenseDetail(
    Guid Id,
    int? Number,
    string Name,
    string? InvoiceNumber,
    Ref Vendor,
    decimal PurchaseValue,
    DateOnly PurchaseDate,
    string? Description,
    Ref Person,
    Ref? Confidentiality,
    Ref? Integrity,
    Ref? Availability,
    Ref? Importance,
    bool Incomplete,
    Ref? LicenseType,
    Ref? LicenseModel,
    Ref? ExpirationModel,
    DateOnly? ExpirationDate,
    string? Expiry,
    decimal? SubscriptionFee,
    Ref? Currency,
    Ref? Period,
    bool AutoRenew,
    Ref? BusinessEntity,
    string? ManagementConsoleUrl,
    string? RegistrationNumber,
    string? KeyIdentifier,
    Ref? Location,
    string? Url,
    DateTimeOffset LastModified,
    IReadOnlyList<VolumeDetail> Volumes
);
