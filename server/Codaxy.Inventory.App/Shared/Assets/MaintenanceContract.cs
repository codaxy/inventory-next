#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.Vendors;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Shared.Assets;

public class MaintenanceContract : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid? AssetId { get; set; }
    public Guid VendorId { get; set; }
    public Guid? MaintenanceTypeId { get; set; }
    public DateOnly? ServiceDueDate { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string ContactName { get; set; }
    public string ContactNumber { get; set; }
    public string ContactEmail { get; set; }
    public string ContractNumber { get; set; }
    public string Description { get; set; }

    public Asset Asset { get; set; }
    public Vendor Vendor { get; set; }
    public MaintenanceType MaintenanceType { get; set; }
}
