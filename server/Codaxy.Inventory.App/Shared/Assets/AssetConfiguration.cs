#nullable disable

using Codaxy.Inventory.App.ElectronicDevices.Devices;
using Codaxy.Inventory.App.Furnitures.Items;
using Codaxy.Inventory.App.Licenses.Licenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Shared.Assets;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(300).IsRequired(false);
        builder.Property(a => a.Description).HasMaxLength(1000).IsRequired(false);
        builder.Property(a => a.URL).HasMaxLength(500).IsRequired(false);

        // Allocation is not concurrency-safe until the sequence replaces it, so this is what
        // actually stops two assets sharing a number. Nulls do not collide in PostgreSQL, which
        // suits a column that assets created outside the three create paths leave empty.
        builder.HasIndex(a => a.InventoryNumber).IsUnique();

        builder
            .HasOne(a => a.License)
            .WithOne(a => a.Asset)
            .HasForeignKey<License>(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.Furniture)
            .WithOne(a => a.Asset)
            .HasForeignKey<Furniture>(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(a => a.ElectronicDevice)
            .WithOne(a => a.Asset)
            .HasForeignKey<ElectronicDevice>(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
