using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

/// <summary>
/// An alert sent: the asset and the date it was about, so a date alerts once and a moved one again.
/// Not audited — mail sent, which nobody edits.
/// </summary>
public class ExpiryAlert
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public DateTimeOffset SentAt { get; set; }

    public Asset Asset { get; set; } = null!;
}

public class ExpiryAlertConfiguration : IEntityTypeConfiguration<ExpiryAlert>
{
    public void Configure(EntityTypeBuilder<ExpiryAlert> builder)
    {
        builder.HasIndex(a => new { a.AssetId, a.ExpirationDate }).IsUnique();
        builder
            .HasOne(a => a.Asset)
            .WithMany()
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
