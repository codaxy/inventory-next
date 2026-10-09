using Codaxy.Inventory.App.Shared.Documents;

namespace Codaxy.Inventory.Tests.Unit;

public class DocumentOptionsTests
{
    [Fact]
    public void Unset_documents_are_in_english_and_valid()
    {
        var options = new DocumentOptions();

        Assert.Equal(("en", "", null), (options.DefaultLanguage, options.Place, options.Problem));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("sr-Latn-BA")]
    [InlineData("sr-latn-ba")]
    [InlineData("EN")]
    public void A_language_documents_are_printed_in_is_valid_whatever_its_case(string language) =>
        Assert.True(new DocumentOptions { DefaultLanguage = language }.IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("sr")]
    [InlineData("de")]
    public void Any_other_stops_the_start_naming_those_it_prints_in(string language) =>
        Assert.Equal(
            "Documents:DefaultLanguage must be one of: en, sr-Latn-BA.",
            new DocumentOptions { DefaultLanguage = language }.Problem
        );

    [Theory]
    [InlineData("sr-latn-ba", "sr-Latn-BA")]
    [InlineData("SR-LATN-BA", "sr-Latn-BA")]
    [InlineData("En", "en")]
    [InlineData("de", null)]
    [InlineData(null, null)]
    public void A_tag_is_written_as_the_list_writes_it(string? tag, string? canonical) =>
        Assert.Equal(canonical, DocumentLanguages.Canonical(tag));
}
