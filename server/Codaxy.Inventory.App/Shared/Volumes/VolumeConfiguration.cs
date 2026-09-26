#nullable disable

using Codaxy.Inventory.App.Licenses.Licenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Volumes;

public class VolumeConfiguration : IEntityTypeConfiguration<Volume>
{
    public void Configure(EntityTypeBuilder<Volume> builder)
    {
        builder.HasOne(a => a.License).WithMany(a => a.Volumes).OnDelete(DeleteBehavior.Restrict);
    }
}
