#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Directory.People;
using Codaxy.Inventory.App.Directory.Projects;
using Codaxy.Inventory.App.Informations.Tags;
using Codaxy.Inventory.App.Informations.Types;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Classification;

namespace Codaxy.Inventory.App.Informations.Items;

public class Information : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid InformationTypeId { get; set; }
    public Guid PersonId { get; set; }
    public Guid? ConfidentialityId { get; set; }
    public Guid? IntegrityId { get; set; }
    public Guid? AvailabilityId { get; set; }
    public Guid? ImportanceId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Note { get; set; }
    public string Author { get; set; }
    public string AccessRights { get; set; }
    public bool? PersonalInformation { get; set; }
    public bool? ClientsPersonalInformation { get; set; }
    public bool? Incomplete { get; set; }

    public InformationType InformationType { get; set; }
    public Person Person { get; set; }
    public Confidentiality Confidentiality { get; set; }
    public Integrity Integrity { get; set; }
    public Availability Availability { get; set; }
    public Importance Importance { get; set; }
    public Project Project { get; set; }
    public virtual ICollection<InformationTagInformation> Tags { get; set; }
    public virtual ICollection<InformationLocation> InformationLocations { get; set; }
}
