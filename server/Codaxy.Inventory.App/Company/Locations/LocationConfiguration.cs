#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Company.Locations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.Property(l => l.Name).HasMaxLength(50);
        builder.Property(l => l.Description).HasMaxLength(1000);
        builder.Property(l => l.PostalCode).HasMaxLength(8);
        builder.Property(l => l.Street).HasMaxLength(50);
        builder.Property(l => l.Room).HasMaxLength(30);

        // The default fills the rows that predate the flag: none of them is marked.
        builder.Property(l => l.IsDisposal).HasDefaultValue(false);
    }
}
