using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.Tests.Unit;

public class VolumeNamesTests
{
    [Theory]
    [InlineData(null, "Per user")]
    [InlineData("  ", "Per user")]
    [InlineData("Second invoice", "Second invoice")]
    [InlineData("https://partner.microsoft.com/dashboard/v2/benefits/visualstudio", "Per user")]
    [InlineData("Benefits · https://partner.microsoft.com/dashboard", "Benefits")]
    [InlineData("Benefits (https://example.com/a) for 2026", "Benefits for 2026")]
    [InlineData("See www.example.com, renew yearly", "See, renew yearly")]
    public void A_designator_is_the_description_without_its_web_addresses_else_the_type(
        string? description,
        string expected
    ) => Assert.Equal(expected, VolumeNames.Designator(description, "Per user"));
}
