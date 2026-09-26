#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices;

public class ElectronicDeviceConfiguration : IEntityTypeConfiguration<ElectronicDevice>
{
    public void Configure(EntityTypeBuilder<ElectronicDevice> builder)
    {
        builder.HasKey(a => a.AssetId);
    }
}
