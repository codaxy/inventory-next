using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.Tests.Unit;

public class PermissionsTests
{
    [Fact]
    public void Keys_are_unique()
    {
        Assert.Equal(Permissions.All.Count, Permissions.Keys.Count);
    }
}
