using Codaxy.Inventory.App.Shared.Icons;

namespace Codaxy.Inventory.Tests.Unit;

public class TypeIconsTests
{
    [Fact]
    public void An_icon_is_none_or_one_of_its_kind_s_set()
    {
        Assert.True(TypeIcons.IsValid(null, TypeIcons.Device));
        Assert.True(TypeIcons.IsValid("laptop", TypeIcons.Device));
        Assert.False(TypeIcons.IsValid("chair", TypeIcons.Device));
        Assert.True(TypeIcons.IsValid("wardrobe", TypeIcons.Furniture));
        Assert.False(TypeIcons.IsValid("Wardrobe", TypeIcons.Furniture));
    }
}
