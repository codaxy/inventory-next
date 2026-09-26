#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.Locations;

namespace Codaxy.Inventory.App.Directory.Vendors;

public class Vendor
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string RegistrationNumber { get; set; }
    public string VATNumber { get; set; }
    public string Web { get; set; }
    public string ContactPerson { get; set; }
    public string MobilePhone { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}
