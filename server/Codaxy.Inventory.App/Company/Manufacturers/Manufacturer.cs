#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Manufacturers;

public class Manufacturer : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string URL { get; set; }
}
