#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Informations.Items;

namespace Codaxy.Inventory.App.Informations.Tags;

public class InformationTagInformation
{
    public Guid InformationTagId { get; set; }
    public Guid InformationId { get; set; }

    public InformationTag InformationTag { get; set; }
    public Information Information { get; set; }
}
