#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.Entities;

namespace Codaxy.Inventory.Persistence;

public class InventoryContextSeedData
{
    private readonly InventoryContext context;

    public InventoryContextSeedData(InventoryContext _context)
    {
        this.context = _context;
    }

    public void SeedData()
    {
        if (context.AssetCategories.Count() == 0)
        {
            context.AssetCategories.AddRange(
                new List<AssetCategory>()
                {
                    new AssetCategory
                    {
                        Id = new Guid("5fb2eccb-c940-48f9-aac6-0a204cdce42e"),
                        Name = "Entities",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("a9871483-5d51-4134-9a38-425de55acdba"),
                        Name = "Information",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("20e2cb55-bb14-4fb9-9455-12ef169d24e6"),
                        Name = "Software",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("e1bbabd5-5bc8-424b-bf14-687c912e1d5b"),
                        Name = "Equipment",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("5653b18d-2036-4f2a-a54d-1efa8db6ec85"),
                        Name = "Services",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("6c993228-80e4-45eb-baf6-877310986aca"),
                        Name = "Premises",
                    },
                    new AssetCategory
                    {
                        Id = new Guid("b99d0b01-d36e-4b43-ad52-89fbf05b26f3"),
                        Name = "Transportation",
                    },
                }
            );
            context.SaveChanges();
        }

        if (context.AssetStatuses.Count() == 0)
        {
            context.AssetStatuses.AddRange(
                new List<AssetStatus>()
                {
                    new AssetStatus
                    {
                        Id = Guid.CreateVersion7(),
                        Status = "Acquired",
                        Description = "Status reserved for the beginning of the asset's lifecycle",
                    },
                    new AssetStatus
                    {
                        Id = Guid.CreateVersion7(),
                        Status = "Active",
                        Description = "Status  reserved for assets that are being actively used",
                    },
                    new AssetStatus
                    {
                        Id = Guid.CreateVersion7(),
                        Status = "In Transit",
                        Description = "Status reserved for assets that are changing their state",
                    },
                    new AssetStatus
                    {
                        Id = Guid.CreateVersion7(),
                        Status = "Inactive",
                        Description = "Status reserved for assets that are not being actively used",
                    },
                    new AssetStatus
                    {
                        Id = Guid.CreateVersion7(),
                        Status = "Decommissioned",
                        Description = "Status reserved for the end of the asset's lifecycle",
                    },
                }
            );
            context.SaveChanges();
        }

        if (context.AssetTypes.Count() == 0)
        {
            var entitiesId = context
                .AssetCategories.Where(c => "Entities".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Business Units",
                        AssetCategoryId = entitiesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Employees",
                        AssetCategoryId = entitiesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Associates",
                        AssetCategoryId = entitiesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Providers",
                        AssetCategoryId = entitiesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Clients",
                        AssetCategoryId = entitiesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Other Stakeholders",
                        AssetCategoryId = entitiesId,
                    },
                }
            );

            var informationId = context
                .AssetCategories.Where(c => "Information".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Databases",
                        AssetCategoryId = informationId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Documentation",
                        AssetCategoryId = informationId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Contracts",
                        AssetCategoryId = informationId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Archives",
                        AssetCategoryId = informationId,
                    },
                }
            );

            var softwareId = context
                .AssetCategories.Where(c => "Software".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Application software",
                        AssetCategoryId = softwareId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "System software",
                        AssetCategoryId = softwareId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Licenses",
                        AssetCategoryId = softwareId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Source code",
                        AssetCategoryId = softwareId,
                    },
                }
            );

            var equipmentId = context
                .AssetCategories.Where(c => "Equipment".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Computer equipment",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Mobile devices",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "BYOD devices",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Communication equipment",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Data Storage",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Technical equipment",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Furniture and fixtures",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Virtual machines",
                        AssetCategoryId = equipmentId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Electronic Device",
                        AssetCategoryId = equipmentId,
                    },
                }
            );

            var servicesId = context
                .AssetCategories.Where(c => "Services".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Cloud",
                        AssetCategoryId = servicesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Communication",
                        AssetCategoryId = servicesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Utilities",
                        AssetCategoryId = servicesId,
                    },
                }
            );

            var premisesId = context
                .AssetCategories.Where(c => "Premises".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Data centers",
                        AssetCategoryId = premisesId,
                    },
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Locations & Buildings",
                        AssetCategoryId = premisesId,
                    },
                }
            );

            var transportationId = context
                .AssetCategories.Where(c => "Transportation".Equals(c.Name))
                .FirstOrDefault()
                .Id;
            context.AssetTypes.AddRange(
                new List<AssetType>()
                {
                    new AssetType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Vehicles",
                        AssetCategoryId = transportationId,
                    },
                }
            );
        }

        var statuses = context.AssetStatuses.ToDictionary(a => a.Status);
        var substatus = context.AssetSubstatuses.ToArray();

        if (substatus.Length == 0)
        {
            var acquired = statuses["Acquired"];
            context.AssetSubstatuses.AddRange(
                new List<AssetSubstatus>()
                {
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = acquired.Id,
                        Substatus = "Ordered",
                        Description =
                            "A new item ordered from the vendor/service provider or in the process of being built, created or procured differently",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = acquired.Id,
                        Substatus = "Delivered",
                        Description =
                            "A new item delivered but still not registered as a company’s asset and not ready for use. E.g., waiting for the installation, deployment, testing, etc.",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = acquired.Id,
                        Substatus = "Available",
                        Description =
                            "A registered asset which is in working order and available for use/to be assigned (In stock).",
                    },
                }
            );

            var active = statuses["Active"];
            context.AssetSubstatuses.AddRange(
                new List<AssetSubstatus>()
                {
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = active.Id,
                        Substatus = "Operational",
                        Description = "A status representing regular usage of the asset",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = active.Id,
                        Substatus = "Redundant",
                        Description =
                            "A specific state for assets functioning in a \"standby\" mode (i.e., Disaster Recovery equipment)",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = active.Id,
                        Substatus = "Maintained",
                        Description =
                            "Asset is under maintenance or other process which suspends active usage",
                    },
                }
            );

            var inTransit = statuses["In Transit"];
            context.AssetSubstatuses.AddRange(
                new List<AssetSubstatus>()
                {
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending order",
                        Description = "A procurement/creation of the new asset has been requested",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending registration",
                        Description =
                            "Waiting for asset to become registered and available for use",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending repair",
                        Description =
                            "Waiting for asset to be repaired/become operational or usable again",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending relocation",
                        Description = "Waiting for transfer/relocation of asset to finish",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending decommissioning",
                        Description = "Waiting for decommissioning process to finish",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inTransit.Id,
                        Substatus = "Pending return",
                        Description = "Waiting for asset return process to finish",
                    },
                }
            );

            var inactive = statuses["Inactive"];
            context.AssetSubstatuses.AddRange(
                new List<AssetSubstatus>()
                {
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inactive.Id,
                        Substatus = "Non-operational",
                        Description =
                            "Asset cannot be actively used because it’s broken, malfunctioning, corrupted, etc.",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inactive.Id,
                        Substatus = "Loaned",
                        Description =
                            "Asset is being used by the third party. The company is still a legal owner",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = inactive.Id,
                        Substatus = "Retired",
                        Description =
                            "Asset is no longer in use; it reached the end of life, become obsolete, expired, etc.",
                    },
                }
            );

            var decommissioned = statuses["Decommissioned"];
            context.AssetSubstatuses.AddRange(
                new List<AssetSubstatus>()
                {
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = decommissioned.Id,
                        Substatus = "Changed owner - uncontrolled",
                        Description =
                            "Asset is no longer associated with the company - it was stolen or lost",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = decommissioned.Id,
                        Substatus = "Changed owner - controlled",
                        Description =
                            "Asset is no longer associated with the company - it was sold or donated",
                    },
                    new AssetSubstatus
                    {
                        Id = Guid.CreateVersion7(),
                        AssetStatusId = decommissioned.Id,
                        Substatus = "Disposed",
                        Description =
                            "Asset is no longer associated with the company - it was disposed",
                    },
                }
            );
        }

        if (context.Confidentialities.Count() == 0)
        {
            context.Confidentialities.AddRange(
                new List<Confidentiality>()
                {
                    new Confidentiality
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Public",
                        Weight = 1,
                        Description = "According to classification schema",
                    },
                    new Confidentiality
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Internal",
                        Weight = 2,
                        Description = "According to classification schema",
                    },
                    new Confidentiality
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Confidential",
                        Weight = 3,
                        Description = "According to classification schema",
                    },
                }
            );
        }

        if (context.Integrities.Count() == 0)
        {
            context.Integrities.AddRange(
                new List<Integrity>()
                {
                    new Integrity
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Low",
                        Weight = 1,
                        Description = "Minimal or no impact on business processes",
                    },
                    new Integrity
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Medium",
                        Weight = 2,
                        Description = "Noticable impact on business processes",
                    },
                    new Integrity
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "High",
                        Weight = 3,
                        Description =
                            "Substantial impact on business processes is caused if the resource's integrity or correctnes is degraded",
                    },
                }
            );
        }

        if (context.Availabilities.Count() == 0)
        {
            context.Availabilities.AddRange(
                new List<Availability>()
                {
                    new Availability
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Low",
                        Weight = 1,
                        Description =
                            "Minimal or no impact at all if the resource is not avaliable for three (3) days or less.",
                    },
                    new Availability
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Medium",
                        Weight = 2,
                        Description =
                            "Noticable impact if the resource is not available for 24 hours or less.",
                    },
                    new Availability
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "High",
                        Weight = 3,
                        Description =
                            "Substantial impact if the resource is not available for four (4) hours or less.",
                    },
                }
            );
        }

        if (context.Importances.Count() == 0)
        {
            context.Importances.AddRange(
                new List<Importance>()
                {
                    new Importance
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Low",
                        Description = "Less important resource",
                    },
                    new Importance
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "Medium",
                        Description = "Important resource",
                    },
                    new Importance
                    {
                        Id = Guid.CreateVersion7(),
                        Level = "High",
                        Description = "Very important resource",
                    },
                }
            );
        }

        if (context.LicenseModels.Count() == 0)
        {
            context.LicenseModels.AddRange(
                new List<LicenseModel>()
                {
                    new LicenseModel { Id = Guid.CreateVersion7(), Text = "OEM" },
                    new LicenseModel { Id = Guid.CreateVersion7(), Text = "Standalone" },
                }
            );
        }

        if (context.LicenseTypes.Count() == 0)
        {
            context.LicenseTypes.AddRange(
                new List<LicenseType>()
                {
                    new LicenseType { Id = Guid.CreateVersion7(), Text = "Commercial" },
                    new LicenseType { Id = Guid.CreateVersion7(), Text = "Open source" },
                    new LicenseType { Id = Guid.CreateVersion7(), Text = "Freeware," },
                    new LicenseType { Id = Guid.CreateVersion7(), Text = "Shareware" },
                }
            );
        }

        if (context.LicenseExpirationModels.Count() == 0)
        {
            context.LicenseExpirationModels.AddRange(
                new List<LicenseExpirationModel>()
                {
                    new LicenseExpirationModel
                    {
                        Id = Guid.CreateVersion7(),
                        Text = "Subscription",
                    },
                    new LicenseExpirationModel { Id = Guid.CreateVersion7(), Text = "Perpetual" },
                    new LicenseExpirationModel { Id = Guid.CreateVersion7(), Text = "PayAsYouGo" },
                }
            );
        }

        if (context.Sequences.Count() == 0)
        {
            context.Sequences.AddRange(
                new List<Sequence>()
                {
                    new Sequence { Id = Guid.CreateVersion7(), AssetInventoryNumber = 100000 },
                }
            );
        }

        if (context.SoftwareOrServiceCategories.Count() == 0)
        {
            context.SoftwareOrServiceCategories.AddRange(
                new List<SoftwareOrServiceCategory>()
                {
                    new SoftwareOrServiceCategory
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "OS server",
                    },
                    new SoftwareOrServiceCategory
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "OS client",
                    },
                    new SoftwareOrServiceCategory
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Application",
                    },
                    new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Database" },
                    new SoftwareOrServiceCategory
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Hypervisor",
                    },
                    new SoftwareOrServiceCategory
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Copyright/IP",
                    },
                    new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "Other" },
                }
            );
        }

        if (context.VolumeTypes.Count() == 0)
        {
            context.VolumeTypes.AddRange(
                new List<VolumeType>()
                {
                    new VolumeType { Id = 1, Text = "Per user" },
                    new VolumeType { Id = 2, Text = "Per network" },
                    new VolumeType { Id = 3, Text = "Per device" },
                    new VolumeType { Id = 4, Text = "Per server" },
                }
            );
        }

        if (context.Periods.Count() == 0)
        {
            context.Periods.AddRange(
                new List<Period>()
                {
                    new Period { Id = Guid.CreateVersion7(), Text = "Month" },
                    new Period { Id = Guid.CreateVersion7(), Text = "Year" },
                }
            );
        }

        if (context.Currencies.Count() == 0)
        {
            context.Currencies.AddRange(
                new List<Currency>()
                {
                    new Currency { Id = Guid.CreateVersion7(), Text = "KM" },
                    new Currency { Id = Guid.CreateVersion7(), Text = "$" },
                    new Currency { Id = Guid.CreateVersion7(), Text = "€" },
                }
            );
        }

        if (context.MaintenanceTypes.Count() == 0)
        {
            context.MaintenanceTypes.AddRange(
                new List<MaintenanceType>()
                {
                    new MaintenanceType { Id = Guid.CreateVersion7(), Text = "Contract" },
                    new MaintenanceType { Id = Guid.CreateVersion7(), Text = "AdHoc" },
                }
            );
        }

        if (context.ElectronicDeviceTags.Count() == 0)
        {
            context.ElectronicDeviceTags.AddRange(
                new List<ElectronicDeviceTag>()
                {
                    new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "Mobile" },
                    new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "HasData" },
                }
            );
        }

        if (context.ElectronicDeviceTypes.Count() == 0)
        {
            context.ElectronicDeviceTypes.AddRange(
                new List<ElectronicDeviceType>()
                {
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Desktop",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Laptop",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Tablet",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Mobile phone",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Monitor",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Keyboard",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Mouse",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Headset",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Microphone",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Speakers",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Camera",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "CCTV",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Printer",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Scanner",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Copier",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Fax",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Power bank",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "KVM switch",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "KVM console",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Computer rack",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Rack part",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Docking station",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "External optical drive",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "External hard drive",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Computer components",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Server",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Data storage",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Firewall",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Gateway",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Router",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Network bridge",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Modem",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Wireless access point",
                        Description = "",
                    },
                    new ElectronicDeviceType
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Switch",
                        Description = "",
                    },
                }
            );
        }

        if (context.InformationTypes.Count() == 0)
        {
            context.InformationTypes.AddRange(
                new List<InformationType>()
                {
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Backup" },
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Source code" },
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Project task log" },
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Contract" },
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Access rights" },
                    new InformationType { Id = Guid.CreateVersion7(), Name = "Other" },
                }
            );
        }

        if (context.Persons.Count() == 0)
        {
            context.Persons.AddRange(
                new List<Person>()
                {
                    new Person { Id = Guid.CreateVersion7(), Name = "Aleksina Matić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Andrea Baćo" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Andrej Šimić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Danijela Umjenović" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Đorđe Vukelić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Dragan Bjelošević" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Igor Timarac" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Jovana Romčević Šukalo" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Maja Mihajlović" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Marko Stijak" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Marko Sikirica" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Mihajlo Novaković" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Milica Tadić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Nebojša Perić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Ognjen Kremenović" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Radmila Kecman" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Saša Tatar" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Vladimir Dangubić" },
                    new Person { Id = Guid.CreateVersion7(), Name = "Vladimir Karadža" },
                }
            );
        }

        if (context.Countries.Count() == 0)
        {
            context.Countries.AddRange(
                new List<Country>()
                {
                    new Country { Code = "BA", Name = "Bosnia and Herzegovina" },
                }
            );
        }

        if (context.Cities.Count() == 0)
        {
            context.Cities.AddRange(
                new List<City>()
                {
                    new City
                    {
                        Id = new Guid("7af8d45c-70f7-41f1-8a03-f015d2554244"),
                        Name = "Banja Luka",
                        CountryCode = "BA",
                    },
                }
            );
        }

        if (context.Locations.Count() == 0)
        {
            context.Locations.AddRange(
                new List<Location>()
                {
                    new Location
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Kancelarija Merkur",
                        StateId = null,
                        PostalCode = "78000",
                        CountryCode = "BA",
                        Room = "Merkur",
                        CityId = new Guid("7af8d45c-70f7-41f1-8a03-f015d2554244"),
                        HouseNumber = 17,
                        Street = "Bulevar srpske vojske",
                        Floor = 7,
                    },
                    new Location
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Kancelarija Mars",
                        StateId = null,
                        PostalCode = "78000",
                        CountryCode = "BA",
                        Room = "Mars",
                        CityId = new Guid("7af8d45c-70f7-41f1-8a03-f015d2554244"),
                        HouseNumber = 17,
                        Street = "Bulevar srpske vojske",
                        Floor = 7,
                    },
                    new Location
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Kancelarija Neptun",
                        StateId = null,
                        PostalCode = "78000",
                        CountryCode = "BA",
                        Room = "Neptun",
                        CityId = new Guid("7af8d45c-70f7-41f1-8a03-f015d2554244"),
                        HouseNumber = 17,
                        Street = "Bulevar srpske vojske",
                        Floor = 7,
                    },
                    new Location
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Kancelarija Direktor",
                        StateId = null,
                        PostalCode = "78000",
                        CountryCode = "BA",
                        Room = "Direktor",
                        CityId = new Guid("7af8d45c-70f7-41f1-8a03-f015d2554244"),
                        HouseNumber = 17,
                        Street = "Bulevar srpske vojske",
                        Floor = 7,
                    },
                }
            );
        }

        if (context.Manufacturers.Count() == 0)
        {
            context.Manufacturers.AddRange(
                new List<Manufacturer>()
                {
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "ASUS",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Canon",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "CheckPoint",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "CISCO",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Conteg RUN",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Crypton",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Dell ",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Drvex doo",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Gigabyte",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Goal",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Gorenje",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "HP",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Huawei",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "IKEA",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Intel",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Lenovo",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Lexmark",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "LG",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Logitech",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Microsoft",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Samsung",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Sencha",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Škoda",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Vivax",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Xiaomi ",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Acer",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Apple",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Panasonic",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Google",
                        URL = "",
                    },
                    new Manufacturer
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "GitHub",
                        URL = "",
                    },
                }
            );
        }

        if (context.Vendors.Count() == 0)
        {
            context.Vendors.AddRange(
                new List<Vendor>()
                {
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Elnos BL doo",
                        Location = "Karađorđeva 79b, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Miki, Božić Miroslav s.p.",
                        Location = "Dositejeva 172, Laktasi",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Kabinet Plus d.o.o.",
                        Location = "Krajiških brigada 57, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "3D Box sp",
                        Location = "Jovana Dučića 2, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Assetmax AG",
                        Location = "Uraniastrasse 34, Zürich",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "PROINTER ITSS d.o.o. Banja Luka",
                        Location = "Vuka Karadžića 2, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Blicnet d.o.o. Banja Luka",
                        Location = "Majke Jugovića 25, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "VodafoneZiggo b.v.",
                        Location = "Atoomweg 100, Utrecht",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "AGRAMINVEST d.o.o.",
                        Location = "Vukovarska 1, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Vesna Ševa",
                        Location = "Maksima Gorkog 18A, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "GitHub Inc.",
                        Location = "88 Colin P. Kelly Jr. Street, San Francisco",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Stylos doo",
                        Location = "Veselina Masleše 13, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Godaddy.com",
                        Location = "",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Preventiva d.o.o.",
                        Location = "Mirka Kovačevića 13, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "EastCode d.o.o Banja Luka",
                        Location = "Bulevar Desanke Maksimović 10, Banja Luka",
                    },
                    new Vendor
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "Telekomunikacije RS, a.d. Banjaluka",
                        Location = "Vuka Karadžića 2, Banja Luka",
                    },
                }
            );
        }

        context.SaveChanges();

        if (context.ElectronicDeviceTypeElectronicDeviceTags.Count() == 0)
        {
            var hasDataId = context.ElectronicDeviceTags.Where(t => t.Name == "HasData").First().Id;
            var mobileId = context.ElectronicDeviceTags.Where(t => t.Name == "Mobile").First().Id;
            string[] hasData =
            {
                "Desktop",
                "Laptop",
                "Tablet",
                "Mobile phone",
                "Camera",
                "CCTV",
                "External hard drive",
                "Server",
                "Data storage",
            };
            string[] mobile =
            {
                "Laptop",
                "Tablet",
                "Mobile phone",
                "Camera",
                "External hard drive",
            };
            var list = new List<ElectronicDeviceTypeElectronicDeviceTag>();
            foreach (var data in hasData)
            {
                var id = context
                    .ElectronicDeviceTypes.Where(t => t.Name == data)
                    .FirstOrDefault()
                    .Id;
                list.Add(
                    new ElectronicDeviceTypeElectronicDeviceTag
                    {
                        ElectronicDeviceTypeId = id,
                        ElectronicDeviceTagId = hasDataId,
                    }
                );
            }

            foreach (var data in mobile)
            {
                var id = context
                    .ElectronicDeviceTypes.Where(t => t.Name == data)
                    .FirstOrDefault()
                    .Id;
                list.Add(
                    new ElectronicDeviceTypeElectronicDeviceTag
                    {
                        ElectronicDeviceTypeId = id,
                        ElectronicDeviceTagId = mobileId,
                    }
                );
            }

            context.ElectronicDeviceTypeElectronicDeviceTags.AddRange(list);
        }

        context.SaveChanges();
    }
}
