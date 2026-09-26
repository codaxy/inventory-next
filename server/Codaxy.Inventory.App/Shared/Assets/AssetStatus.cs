#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Shared.Assets;

public class AssetStatus : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public ICollection<AssetSubstatus> AssetSubstatus { get; set; }
}
