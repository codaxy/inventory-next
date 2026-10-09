using Codaxy.Inventory.App.Shared.Documents;

namespace Codaxy.Inventory.Tests.Unit;

public class DocumentOptionsTests
{
    [Fact]
    public void Unset_documents_are_in_english_and_valid()
    {
        var options = new DocumentOptions();

        Assert.Equal(("en", "", true), (options.DefaultLanguage, options.Place, options.IsValid));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("sr-Latn-BA")]
    public void A_language_documents_are_printed_in_is_valid(string language) =>
        Assert.True(new DocumentOptions { DefaultLanguage = language }.IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("sr")]
    [InlineData("sr-latn-ba")]
    [InlineData("de")]
    public void Any_other_stops_the_start(string language) =>
        Assert.False(new DocumentOptions { DefaultLanguage = language }.IsValid);
}
