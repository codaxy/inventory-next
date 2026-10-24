#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Company.People;

public class Person : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    /// <summary>
    /// Whether the person may sign in to see their own inventory while employees may at all — a
    /// decision about this person, true until someone turns it off; see auth.md.
    /// </summary>
    public bool CanSignIn { get; set; } = true;

    /// <summary>
    /// The email as people are compared by it — trimmed, lower-case — computed by the database, which
    /// holds it unique: one person per address, the one an employee signs in as.
    /// </summary>
    public string EmailKey { get; private set; }
}

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.Property(p => p.EmailKey).HasComputedColumnSql("lower(trim(email))", stored: true);
        builder.HasIndex(p => p.EmailKey).IsUnique();
    }
}
