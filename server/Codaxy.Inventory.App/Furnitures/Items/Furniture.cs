#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Furnitures.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;

namespace Codaxy.Inventory.App.Furnitures.Items;

public class Furniture : IIdentifiableReadOnly<Guid>
{
    public Guid Id => AssetId;

    public Guid AssetId { get; set; }
    public Guid? FurnitureTypeId { get; set; }
    public string Model { get; set; }

    public Asset Asset { get; set; }
    public FurnitureType FurnitureType { get; set; }
}
