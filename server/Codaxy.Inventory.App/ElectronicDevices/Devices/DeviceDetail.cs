using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices;

/// <summary>A maintenance contract on a device, as its page lists it.</summary>
/// <param name="Type">"AdHoc" or "Contract", the seeded maintenance types.</param>
public sealed record ContractRow(
    Guid Id,
    AssetRef Vendor,
    string? Type,
    DateOnly? ServiceDueDate,
    DateOnly? ExpirationDate,
    string? ContactName,
    string? ContactNumber,
    string? ContactEmail,
    string? ContractNumber,
    string? Description
);

/// <param name="Person">Who the seat is also assigned to, where it is.</param>
public sealed record SeatRow(
    Guid Id,
    string Software,
    Guid LicenseId,
    string License,
    int? LicenseNumber,
    string? Person,
    int Quantity,
    DateOnly ActivationDate,
    DateOnly? DeactivationDate
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>
/// A device as its page shows it: the asset's fields and the device's own, flat, the names beside the
/// ids; the tags of its type; its contracts; and what is attached to it — seats and information.
/// "Guarantee" in the entity is warranty everywhere else.
/// </summary>
public sealed record DeviceDetail(
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
    IReadOnlyList<AssetRef> Tags,
    AssetRef? Manufacturer,
    DateOnly? ManufacturingDate,
    string? ModelName,
    string? ModelCode,
    string? SerialNumber,
    string? WarrantyNumber,
    DateOnly? WarrantyExpirationDate,
    DateTimeOffset LastModified,
    IReadOnlyList<ContractRow> Contracts,
    Section<SeatRow> Seats,
    Section<InformationRow> Information
);

internal static class DeviceReads
{
    public static async Task<DeviceDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var d = await context
            .ElectronicDevices.AsNoTracking()
            .Where(d => d.AssetId == id)
            .Select(d => new
            {
                d.AssetId,
                d.Asset.InventoryNumber,
                d.Asset.Name,
                d.Asset.InvoiceNumber,
                Vendor = new AssetRef(d.Asset.VendorId, d.Asset.Vendor.Name),
                d.Asset.PurchaseValue,
                d.Asset.PurchaseDate,
                d.Asset.Description,
                Person = new AssetRef(d.Asset.PersonId, d.Asset.Person.Name),
                Confidentiality = d.Asset.ConfidentialityId == null
                    ? null
                    : new AssetRef(d.Asset.Confidentiality.Id, d.Asset.Confidentiality.Level),
                Integrity = d.Asset.IntegrityId == null
                    ? null
                    : new AssetRef(d.Asset.Integrity.Id, d.Asset.Integrity.Level),
                Availability = d.Asset.AvailabilityId == null
                    ? null
                    : new AssetRef(d.Asset.Availability.Id, d.Asset.Availability.Level),
                Importance = d.Asset.ImportanceId == null
                    ? null
                    : new AssetRef(d.Asset.Importance.Id, d.Asset.Importance.Level),
                d.Asset.Incomplete,
                BusinessEntity = d.Asset.BusinessEntityId == null
                    ? null
                    : new AssetRef(d.Asset.BusinessEntity.Id, d.Asset.BusinessEntity.Text),
                Location = d.Asset.LocationId == null
                    ? null
                    : new AssetRef(d.Asset.Location.Id, d.Asset.Location.Name),
                d.Asset.URL,
                Type = d.ElectronicDeviceTypeId == null
                    ? null
                    : new AssetRef(d.ElectronicDeviceType.Id, d.ElectronicDeviceType.Name),
                Tags = d.ElectronicDeviceTypeId == null
                    ? new List<AssetRef>()
                    : d
                        .ElectronicDeviceType.Tags.OrderBy(t => t.ElectronicDeviceTag.Name)
                        .Select(t => new AssetRef(
                            t.ElectronicDeviceTagId,
                            t.ElectronicDeviceTag.Name
                        ))
                        .ToList(),
                Manufacturer = d.ManufacturerId == null
                    ? null
                    : new AssetRef(d.Manufacturer.Id, d.Manufacturer.Name),
                d.ManufacturingDate,
                d.ModelName,
                d.ModelCode,
                d.SerialNumber,
                d.GuaranteeNumber,
                d.GuaranteeExpirationDate,
                d.Asset.LastModified,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (d is null)
            return null;

        var contracts = await context
            .MaintenanceContracts.AsNoTracking()
            .Where(c => c.AssetId == id)
            .OrderBy(c => c.ExpirationDate == null)
            .ThenBy(c => c.ExpirationDate)
            .ThenBy(c => c.Id)
            .Select(c => new ContractRow(
                c.Id,
                new AssetRef(c.VendorId, c.Vendor.Name),
                c.MaintenanceTypeId == null ? null : c.MaintenanceType.Text,
                c.ServiceDueDate,
                c.ExpirationDate,
                c.ContactName,
                c.ContactNumber,
                c.ContactEmail,
                c.ContractNumber,
                c.Description
            ))
            .ToListAsync(cancellationToken);

        var seats = await AssetHoldings.SectionAsync(
            context.Activations.AsNoTracking().Where(a => a.AssetId == id),
            q =>
                q.OrderBy(a => a.DeactivationDate != null)
                    .ThenByDescending(a => a.ActivationDate)
                    .ThenBy(a => a.Id)
                    .Select(a => new SeatRow(
                        a.Id,
                        a.Volume.SoftwareOrService.Name,
                        a.Volume.LicenseId,
                        a.Volume.License.Asset.Name,
                        a.Volume.License.Asset.InventoryNumber,
                        a.PersonId == null ? null : a.Person.Name,
                        a.Quantity,
                        a.ActivationDate,
                        a.DeactivationDate
                    )),
            cancellationToken
        );

        var information = await AssetHoldings.SectionAsync(
            context
                .Informations.AsNoTracking()
                .Where(i => i.InformationLocations.Any(l => l.ElectronicDeviceId == id)),
            q =>
                q.OrderBy(i => i.Name)
                    .ThenBy(i => i.Id)
                    .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
            cancellationToken
        );

        return new DeviceDetail(
            d.AssetId,
            d.InventoryNumber,
            d.Name,
            d.InvoiceNumber,
            d.Vendor,
            d.PurchaseValue,
            d.PurchaseDate,
            d.Description,
            d.Person,
            d.Confidentiality,
            d.Integrity,
            d.Availability,
            d.Importance,
            d.Incomplete,
            d.BusinessEntity,
            d.Location,
            d.URL,
            d.Type,
            d.Tags,
            d.Manufacturer,
            d.ManufacturingDate,
            d.ModelName,
            d.ModelCode,
            d.SerialNumber,
            d.GuaranteeNumber,
            d.GuaranteeExpirationDate,
            d.LastModified,
            contracts,
            seats,
            information
        );
    }
}
