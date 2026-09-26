#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Infrastructure.Clouds;

public class Cloud : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid VolumeId { get; set; }
    public string Name { get; set; }
    public string ManagementURL { get; set; }

    public Volume Volume { get; set; }
}
