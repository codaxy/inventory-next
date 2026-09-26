#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Classification;

public class ImportanceConfiguration : IEntityTypeConfiguration<Importance>
{
    public void Configure(EntityTypeBuilder<Importance> builder)
    {
        builder.Property(i => i.Level).HasMaxLength(15);
        builder.Property(i => i.Description).HasMaxLength(1000);
    }
}
