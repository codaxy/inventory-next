using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

/// <summary>
/// An alert sent: the asset, the date it was about and its stage — 0 the first notice, 1 and 2 the
/// reminders — so each stage of a date is sent once and a moved date starts over. Not audited — mail
/// sent, which nobody edits.
/// </summary>
public class ExpiryAlert
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public int Stage { get; set; }
    public DateTimeOffset SentAt { get; set; }

    public Asset Asset { get; set; } = null!;
}

public class ExpiryAlertConfiguration : IEntityTypeConfiguration<ExpiryAlert>
{
    public void Configure(EntityTypeBuilder<ExpiryAlert> builder)
    {
        // Rows sent before reminders existed were first notices.
        builder.Property(a => a.Stage).HasDefaultValue(0);
        builder
            .HasIndex(a => new
            {
                a.AssetId,
                a.ExpirationDate,
                a.Stage,
            })
            .IsUnique();
        builder
            .HasOne(a => a.Asset)
            .WithMany()
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
