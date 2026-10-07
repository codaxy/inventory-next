using Codaxy.Inventory.Web.Printing;

namespace Codaxy.Inventory.Tests.Unit;

public class PrintingOptionsTests
{
    [Fact]
    public void No_browser_is_valid_and_prints_nothing()
    {
        var options = new PrintingOptions();

        Assert.Equal((false, true), (options.Enabled, options.IsValid));
    }

    [Fact]
    public void A_browser_that_is_there_is_valid()
    {
        var file = Path.GetTempFileName();
        try
        {
            Assert.True(new PrintingOptions { ChromiumPath = file }.IsValid);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void A_browser_that_is_not_there_stops_the_start() =>
        Assert.False(new PrintingOptions { ChromiumPath = "/usr/bin/no-such-chromium" }.IsValid);
}
