#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Persistence;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.ActionType).HasMaxLength(20);
        entity.Property(e => e.Email).HasMaxLength(250);
        entity.Property(e => e.NewValuesJson);
        entity.Property(e => e.OldValuesJson);
        entity.Property(e => e.Table).HasMaxLength(50);
        entity.HasIndex(e => e.Email);

        entity.HasIndex(e => e.EntityId);
        entity.HasIndex(e => new { e.Table, e.EntityId });
    }
}
