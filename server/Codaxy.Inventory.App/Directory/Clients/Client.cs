#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Directory.Clients;

public class Client : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
