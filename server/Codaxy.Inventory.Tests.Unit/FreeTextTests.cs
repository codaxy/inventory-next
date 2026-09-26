using Codaxy.Inventory.App.Shared.Search;

namespace Codaxy.Inventory.Tests.Unit;

public class FreeTextTests
{
    [Fact]
    public void A_leading_hash_is_dropped_as_the_screens_show_a_number() =>
        Assert.Equal(["100893", "chair"], FreeText.Terms(" #100893  chair #"));

    [Fact]
    public void A_term_that_is_a_guid_names_an_id()
    {
        var id = Guid.CreateVersion7();

        Assert.Equal(id, FreeText.Id(id.ToString()));
        Assert.Equal(id, FreeText.Id(id.ToString().ToUpperInvariant()));
        Assert.Null(FreeText.Id("100893"));
        Assert.Null(FreeText.Id(id.ToString()[..8]));
    }
}
