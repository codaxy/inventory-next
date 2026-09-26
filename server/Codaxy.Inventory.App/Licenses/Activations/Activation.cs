#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Licenses.Activations;

public class Activation : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid? PersonId { get; set; } // Employee
    public Guid? AssetId { get; set; } // Device
    public Guid VolumeId { get; set; }
    public int Quantity { get; set; }
    public DateOnly ActivationDate { get; set; }
    public DateOnly? DeactivationDate { get; set; }

    public Person Person { get; set; }
    public Asset Asset { get; set; }
    public Volume Volume { get; set; }
}
