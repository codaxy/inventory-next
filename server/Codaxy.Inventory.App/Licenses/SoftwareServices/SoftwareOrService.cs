#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.Manufacturers;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices;

public class SoftwareOrService : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid SoftwareOrServiceCategoryId { get; set; }
    public Guid ManufacturerId { get; set; }
    public string Url { get; set; }

    public ICollection<Volume> Volumes { get; set; }

    public SoftwareOrServiceCategory SoftwareOrServiceCategory { get; set; }
    public Manufacturer Manufacturer { get; set; }
}
