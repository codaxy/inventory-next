#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.ElectronicDevices.Tags;

namespace Codaxy.Inventory.App.ElectronicDevices.Types;

public class ElectronicDeviceTypeElectronicDeviceTag
{
    public Guid ElectronicDeviceTypeId { get; set; }
    public Guid ElectronicDeviceTagId { get; set; }

    public ElectronicDeviceType ElectronicDeviceType { get; set; }
    public ElectronicDeviceTag ElectronicDeviceTag { get; set; }
}
