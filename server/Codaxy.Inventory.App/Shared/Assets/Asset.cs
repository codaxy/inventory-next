#nullable disable

using System;
using System.Collections.Generic;
using Codaxy.Inventory.App.Directory.Locations;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Vendors;
using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.Furnitures.Items;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Classification;

namespace Codaxy.Inventory.App.Shared.Assets;

public class Asset : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public int? InventoryNumber { get; set; }
    public Guid AssetTypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid? AssetSubstatusId { get; set; }
    public string URL { get; set; }
    public Guid? LocationId { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public decimal PurchaseValue { get; set; }
    public Guid VendorId { get; set; }
    public string InvoiceNumber { get; set; }
    public Guid? BusinessEntityId { get; set; }
    public Guid? ConfidentialityId { get; set; }
    public Guid? IntegrityId { get; set; }
    public Guid? AvailabilityId { get; set; }
    public Guid? ImportanceId { get; set; }
    public Guid PersonId { get; set; }
    public ICollection<MaintenanceContract> MaintenanceContract { get; set; }
    public bool Incomplete { get; set; }
    public DateTimeOffset LastModified { get; set; }

    public AssetType AssetType { get; set; }
    public AssetSubstatus AssetSubstatus { get; set; }
    public Location Location { get; set; }
    public Vendor Vendor { get; set; }
    public BusinessEntity BusinessEntity { get; set; }

    public Confidentiality Confidentiality { get; set; }
    public Integrity Integrity { get; set; }
    public Availability Availability { get; set; }
    public Importance Importance { get; set; }
    public Person Person { get; set; }

    public License License { get; private set; }
    public Furniture Furniture { get; set; }
    public ElectronicDevice ElectronicDevice { get; set; }
}
