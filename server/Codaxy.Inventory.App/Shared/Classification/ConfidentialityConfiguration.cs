#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Classification;

public class ConfidentialityConfiguration : IEntityTypeConfiguration<Confidentiality>
{
    public void Configure(EntityTypeBuilder<Confidentiality> builder)
    {
        builder.Property(c => c.Level).HasMaxLength(15);
        builder.Property(c => c.Description).HasMaxLength(1000);
    }
}
