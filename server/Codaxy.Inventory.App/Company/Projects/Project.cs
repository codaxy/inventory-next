#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Company.Clients;
using Codaxy.Inventory.App.Company.People;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.App.Company.Projects;

public class Project : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectOwnerId { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; }

    public Person ProjectOwner { get; set; }
    public Client Client { get; set; }
}
