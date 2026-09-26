#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Company.Locations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(50);
    }
}
