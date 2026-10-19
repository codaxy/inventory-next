#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Furnitures.Types;

public class FurnitureType : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>A key of <c>TypeIcons.Furniture</c>; none is the generic icon.</summary>
    public string Icon { get; set; }
}
