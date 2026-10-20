using System.Text.Json;
using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Shared.Documents;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Codaxy.Inventory.Tests.Unit;

public class SettingDefinitionTests
{
    [Fact]
    public void Every_default_may_be_saved() =>
        Assert.All(
            SettingCatalogue.All,
            definition =>
                Assert.Null(
                    definition
                        .Accept(JsonDocument.Parse(definition.DefaultJson).RootElement)
                        .Problem
                )
        );

    [Fact]
    public void Every_key_is_unique_and_names_its_group() =>
        Assert.All(
            SettingCatalogue.All.GroupBy(d => d.Key),
            group =>
            {
                Assert.Single(group);
                Assert.Matches("^[a-z_]+\\.[a-z_]+$", group.Key);
            }
        );

    [Fact]
    public void The_defaults_are_the_fallbacks() =>
        Assert.Equal(
            (15, 45, 15, 30, "", "en"),
            (
                DashboardSettings.SubscriptionsExpiredDays.Default,
                DashboardSettings.SubscriptionsExpiringDays.Default,
                DashboardSettings.WarrantiesEndedDays.Default,
                DashboardSettings.WarrantiesEndingDays.Default,
                DocumentSettings.Place.Default,
                DocumentSettings.DefaultLanguage.Default
            )
        );

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void A_window_is_at_least_a_day(int days) =>
        Assert.Equal("At least 1 day.", DashboardSettings.WarrantiesEndedDays.Problem(days));

    [Theory]
    [InlineData("en")]
    [InlineData("sr-Latn-BA")]
    [InlineData("sr-latn-ba")]
    [InlineData("EN")]
    public void A_language_documents_are_printed_in_may_be_the_default_whatever_its_case(
        string language
    ) => Assert.Null(DocumentSettings.DefaultLanguage.Problem(language));

    [Theory]
    [InlineData("")]
    [InlineData("sr")]
    [InlineData("de")]
    public void Any_other_names_those_documents_are_printed_in(string language) =>
        Assert.Equal(
            "Must be one of: en, sr-Latn-BA.",
            DocumentSettings.DefaultLanguage.Problem(language)
        );

    [Theory]
    [InlineData("sr-latn-ba", "sr-Latn-BA")]
    [InlineData("SR-LATN-BA", "sr-Latn-BA")]
    [InlineData("En", "en")]
    [InlineData("de", null)]
    [InlineData(null, null)]
    public void A_tag_is_written_as_the_list_writes_it(string? tag, string? canonical) =>
        Assert.Equal(canonical, DocumentLanguages.Canonical(tag));

    [Fact]
    public void A_stored_value_is_read_as_stored() =>
        Assert.Equal((14, null), DashboardSettings.SubscriptionsExpiringDays.Read("14"));

    [Theory]
    [InlineData("0")]
    [InlineData("\"fifteen\"")]
    [InlineData("null")]
    [InlineData("{")]
    public void One_that_cannot_be_used_reads_as_the_default_and_says_why(string json)
    {
        var (value, problem) = DashboardSettings.SubscriptionsExpiringDays.Read(json);

        Assert.Equal(45, value);
        Assert.NotNull(problem);
    }

    [Fact]
    public void A_string_is_stored_as_json() =>
        Assert.Equal("\"Banja Luka\"", DocumentSettings.Place.Serialize("Banja Luka"));

    [Fact]
    public void A_setting_with_no_row_reads_as_its_default() =>
        Assert.Equal(
            "en",
            new SettingValues(new Dictionary<string, string>(), NullLogger.Instance).Get(
                DocumentSettings.DefaultLanguage
            )
        );

    [Fact]
    public void A_row_that_cannot_be_used_reads_as_the_default() =>
        Assert.Equal(
            30,
            new SettingValues(
                new Dictionary<string, string> { ["dashboard.warranties_ending_days"] = "-1" },
                NullLogger.Instance
            ).Get(DashboardSettings.WarrantiesEndingDays)
        );
}
