#nullable disable

using System;
using System.Collections.Generic;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags;

public class ElectronicDeviceTag : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<ElectronicDeviceTypeElectronicDeviceTag> Types { get; set; }
}
