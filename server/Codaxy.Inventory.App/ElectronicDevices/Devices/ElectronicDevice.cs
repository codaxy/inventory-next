#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices;

public class ElectronicDevice : IIdentifiableReadOnly<Guid>
{
    public Guid Id => AssetId;
    public Guid AssetId { get; set; }
    public Guid? ManufacturerId { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? GuaranteeExpirationDate { get; set; }
    public string GuaranteeNumber { get; set; }
    public string SerialNumber { get; set; }
    public string ModelCode { get; set; }
    public string ModelName { get; set; }
    public Guid? ElectronicDeviceTypeId { get; set; }

    public Asset Asset { get; set; }
    public Manufacturer Manufacturer { get; set; }
    public ElectronicDeviceType ElectronicDeviceType { get; set; }
}
