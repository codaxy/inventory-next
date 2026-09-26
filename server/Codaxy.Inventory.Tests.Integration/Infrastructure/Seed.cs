using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Vendors;
using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Licenses.Activations;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Classification;
using Codaxy.Inventory.App.Shared.Volumes;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

/// <summary>
/// The codebooks and directory rows the licence area stands on, in a test database that starts empty:
/// the "Licenses" and "Electronic Device" asset types, a vendor and a person, the sequence, the
/// classification with the original's weights, the volume types by their seeded ids, and the licence
/// codebooks. Written once per database.
/// </summary>
public static class Seed
{
    public sealed record Basics(
        Guid Vendor,
        Guid Person,
        Guid LicenseAssetType,
        Guid DeviceAssetType
    );

    public const int PerUser = 1;
    public const int PerDevice = 3;

    public static async Task<Basics> BasicsAsync(InventoryContext context)
    {
        if (await context.Vendors.FirstOrDefaultAsync(v => v.Name == "Seed vendor") is { } vendor)
            return new(
                vendor.Id,
                (await context.Persons.FirstAsync(p => p.Name == "Seed person")).Id,
                (await context.AssetTypes.FirstAsync(t => t.Name == "Licenses")).Id,
                (await context.AssetTypes.FirstAsync(t => t.Name == "Electronic Device")).Id
            );

        var software = new AssetCategory { Id = Guid.CreateVersion7(), Name = "Software" };
        var equipment = new AssetCategory { Id = Guid.CreateVersion7(), Name = "Equipment" };
        var licenses = new AssetType
        {
            Id = Guid.CreateVersion7(),
            Name = "Licenses",
            AssetCategoryId = software.Id,
        };
        var devices = new AssetType
        {
            Id = Guid.CreateVersion7(),
            Name = "Electronic Device",
            AssetCategoryId = equipment.Id,
        };
        var seller = new Vendor { Id = Guid.CreateVersion7(), Name = "Seed vendor" };
        var person = new Person { Id = Guid.CreateVersion7(), Name = "Seed person" };

        context.AddRange(software, equipment, licenses, devices, seller, person);
        context.Sequences.Add(
            new Sequence { Id = Guid.CreateVersion7(), AssetInventoryNumber = 100000 }
        );

        foreach (var (level, weight) in new[] { ("Low", 1), ("Medium", 2), ("High", 3) })
        {
            context.Integrities.Add(
                new Integrity
                {
                    Id = Guid.CreateVersion7(),
                    Level = level,
                    Weight = weight,
                }
            );
            context.Availabilities.Add(
                new Availability
                {
                    Id = Guid.CreateVersion7(),
                    Level = level,
                    Weight = weight,
                }
            );
            context.Importances.Add(new Importance { Id = Guid.CreateVersion7(), Level = level });
        }

        foreach (
            var (level, weight) in new[] { ("Public", 1), ("Internal", 2), ("Confidential", 3) }
        )
            context.Confidentialities.Add(
                new Confidentiality
                {
                    Id = Guid.CreateVersion7(),
                    Level = level,
                    Weight = weight,
                }
            );

        context.VolumeTypes.AddRange(
            new VolumeType { Id = PerUser, Text = "Per user" },
            new VolumeType { Id = 2, Text = "Per network" },
            new VolumeType { Id = PerDevice, Text = "Per device" },
            new VolumeType { Id = 4, Text = "Per server" }
        );

        context.LicenseTypes.Add(
            new LicenseType { Id = Guid.CreateVersion7(), Text = "Commercial" }
        );
        context.LicenseModels.Add(new LicenseModel { Id = Guid.CreateVersion7(), Text = "OEM" });
        context.LicenseExpirationModels.Add(
            new LicenseExpirationModel { Id = Guid.CreateVersion7(), Text = "Subscription" }
        );
        context.Periods.Add(new Period { Id = Guid.CreateVersion7(), Text = "Year" });
        context.Currencies.Add(new Currency { Id = Guid.CreateVersion7(), Text = "€" });
        context.BusinessEntities.Add(
            new BusinessEntity { Id = Guid.CreateVersion7(), Text = "Seed entity" }
        );

        await context.SaveChangesAsync();
        return new(seller.Id, person.Id, licenses.Id, devices.Id);
    }

    public sealed record SeededLicense(Guid License, Guid Volume);

    /// <summary>A licence of one volume of the given software, bought for the given seats.</summary>
    public static async Task<SeededLicense> LicenseWithVolumeAsync(
        InventoryContext context,
        Guid software,
        string name = "Seed licence",
        int seats = 5,
        int volumeType = PerUser,
        DateOnly? expires = null
    )
    {
        var basics = await BasicsAsync(context);
        var id = Guid.CreateVersion7();
        var volume = new Volume
        {
            Id = Guid.CreateVersion7(),
            SoftwareOrServiceId = software,
            VolumeTypeId = volumeType,
            Quantity = seats,
        };

        context.Assets.Add(
            new Asset
            {
                Id = id,
                Name = name,
                AssetTypeId = basics.LicenseAssetType,
                VendorId = basics.Vendor,
                PersonId = basics.Person,
                PurchaseDate = new DateOnly(2026, 1, 15),
                PurchaseValue = 100,
                LastModified = DateTimeOffset.UtcNow,
            }
        );
        context.Licenses.Add(
            new License
            {
                AssetId = id,
                SubscriptionExpirationDate = expires,
                Volumes = [volume],
            }
        );

        await context.SaveChangesAsync();
        return new(id, volume.Id);
    }

    /// <summary>An electronic device of a type that holds licences, or of one that does not.</summary>
    public static async Task<Guid> DeviceAsync(
        InventoryContext context,
        string name,
        bool holdsLicences
    )
    {
        var basics = await BasicsAsync(context);
        var typeName = holdsLicences ? "Seed laptop" : "Seed monitor";
        var type =
            await context.ElectronicDeviceTypes.FirstOrDefaultAsync(t => t.Name == typeName)
            ?? context
                .ElectronicDeviceTypes.Add(
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = typeName,
                        HoldLicences = holdsLicences,
                    }
                )
                .Entity;

        var id = Guid.CreateVersion7();
        context.Assets.Add(
            new Asset
            {
                Id = id,
                Name = name,
                InventoryNumber = Random.Shared.Next(200000, 900000),
                AssetTypeId = basics.DeviceAssetType,
                VendorId = basics.Vendor,
                PersonId = basics.Person,
                LastModified = DateTimeOffset.UtcNow,
                ElectronicDevice = new ElectronicDevice
                {
                    AssetId = id,
                    ElectronicDeviceTypeId = type.Id,
                },
            }
        );

        await context.SaveChangesAsync();
        return id;
    }

    public static async Task<Guid> ActivationAsync(
        InventoryContext context,
        Guid volume,
        Guid? person = null,
        Guid? device = null,
        int quantity = 1,
        DateOnly? deactivated = null
    )
    {
        var activation = new Activation
        {
            Id = Guid.CreateVersion7(),
            VolumeId = volume,
            PersonId = person ?? (device is null ? (await BasicsAsync(context)).Person : null),
            AssetId = device,
            Quantity = quantity,
            ActivationDate = new DateOnly(2026, 2, 1),
            DeactivationDate = deactivated,
        };
        context.Activations.Add(activation);
        await context.SaveChangesAsync();
        return activation.Id;
    }
}
