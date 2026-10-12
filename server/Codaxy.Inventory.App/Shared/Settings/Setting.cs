using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Settings;

/// <summary>
/// One setting's stored value, as JSON. What it means — its type, default and rules — is its
/// <see cref="SettingDefinition"/>'s; a row per setting, so the audit log names the one that changed.
/// </summary>
public class Setting : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.Property(s => s.Key).HasMaxLength(100);
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Value).HasColumnType("jsonb");
    }
}
