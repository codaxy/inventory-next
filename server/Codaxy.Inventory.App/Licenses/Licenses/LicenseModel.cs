#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Licenses.Licenses;

public class LicenseModel : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Text { get; set; }
}
