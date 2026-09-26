#nullable disable

using System;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Locations;

public class State : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }

    public Country Country { get; set; }
}
