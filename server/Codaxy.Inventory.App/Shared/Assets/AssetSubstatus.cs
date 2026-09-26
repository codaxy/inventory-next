#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Shared.Assets;

public class AssetSubstatus : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid AssetStatusId { get; set; }
    public string Substatus { get; set; }
    public string Description { get; set; }

    public AssetStatus AssetStatus { get; set; }
}
