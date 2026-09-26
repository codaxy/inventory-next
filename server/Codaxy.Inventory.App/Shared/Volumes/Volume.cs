#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Licenses.Activations;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Shared.Volumes;

public class Volume : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid SoftwareOrServiceId { get; set; }
    public int VolumeTypeId { get; set; }
    public Guid LicenseId { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public ICollection<Activation> Activations { get; set; }

    public SoftwareOrService SoftwareOrService { get; set; }
    public VolumeType VolumeType { get; set; }
    public License License { get; set; }
}
