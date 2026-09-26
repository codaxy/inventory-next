#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Directory.Locations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Code);
        builder.Property(c => c.Code).HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.Name).HasMaxLength(50);
    }
}
