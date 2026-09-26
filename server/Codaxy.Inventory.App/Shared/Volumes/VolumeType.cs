#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Shared.Volumes;

public class VolumeType : IIdentifiable<int>
{
    public int Id { get; set; }
    public string Text { get; set; }
}
