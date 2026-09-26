#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Classification;

public class IntegrityConfiguration : IEntityTypeConfiguration<Integrity>
{
    public void Configure(EntityTypeBuilder<Integrity> builder)
    {
        builder.Property(i => i.Level).HasMaxLength(15);
        builder.Property(i => i.Description).HasMaxLength(1000);
    }
}
