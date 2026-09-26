#nullable disable

using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.ElectronicDevices.Types;

public class ElectronicDeviceTypeElectronicDeviceTagConfiguration
    : IEntityTypeConfiguration<ElectronicDeviceTypeElectronicDeviceTag>
{
    public void Configure(EntityTypeBuilder<ElectronicDeviceTypeElectronicDeviceTag> builder)
    {
        builder.HasKey(e => new { e.ElectronicDeviceTypeId, e.ElectronicDeviceTagId });

        builder
            .HasOne(et => et.ElectronicDeviceType)
            .WithMany(e => e.Tags)
            .HasForeignKey(et => et.ElectronicDeviceTypeId);

        builder
            .HasOne(et => et.ElectronicDeviceTag)
            .WithMany(e => e.Types)
            .HasForeignKey(et => et.ElectronicDeviceTagId);
    }
}
