using ClosedXML.Attributes;
using ClosedXML.Excel;
using Codaxy.Inventory.App.Shared.Export;

namespace Codaxy.Inventory.Tests.Unit;

/// <summary>Instants in an export: a cell holds no zone, so the writer converts to the viewer's.</summary>
public class SpreadsheetTests
{
    public sealed class Row
    {
        [XLColumn(Header = "Name")]
        public string Name { get; init; } = "";

        [XLColumn(Header = "Last Modified")]
        public DateTimeOffset LastModified { get; init; }

        [XLColumn(Header = "Ended")]
        public DateTimeOffset? Ended { get; init; }
    }

    private static readonly Row[] Rows =
    [
        new() { Name = "Summer", LastModified = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero) },
        new()
        {
            Name = "Winter",
            LastModified = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero),
            Ended = new(2026, 1, 15, 23, 30, 0, TimeSpan.Zero),
        },
    ];

    private static IXLWorksheet Sheet(ExportZone zone) =>
        new XLWorkbook(new MemoryStream(Spreadsheet.Write(Rows, "Check", zone))).Worksheet(1);

    private static ExportZone Read(string? tz, string? label)
    {
        Assert.Null(ExportZone.Read(tz, label, 2026, out var zone));
        return zone;
    }

    [Fact]
    public void Writes_an_instant_in_the_zone_with_its_own_daylight_saving()
    {
        var sheet = Sheet(Read("Europe/Budapest", "CET/CEST"));

        Assert.Equal(new DateTime(2026, 7, 1, 12, 0, 0), sheet.Cell(2, 2).GetDateTime());
        Assert.Equal(new DateTime(2026, 1, 15, 11, 0, 0), sheet.Cell(3, 2).GetDateTime());
        // Past midnight there, so the next day.
        Assert.Equal(new DateTime(2026, 1, 16, 0, 30, 0), sheet.Cell(3, 3).GetDateTime());
        Assert.True(sheet.Cell(2, 3).IsEmpty());
    }

    [Fact]
    public void Heads_an_instant_column_only_with_the_label_the_request_gives()
    {
        var sheet = Sheet(Read("Europe/Budapest", "CET/CEST"));

        Assert.Equal("Name", sheet.Cell(1, 1).GetString());
        Assert.Equal("Last Modified (CET/CEST)", sheet.Cell(1, 2).GetString());
        Assert.Equal("Ended (CET/CEST)", sheet.Cell(1, 3).GetString());
    }

    [Theory]
    [InlineData("Europe/Budapest", "GMT+1/GMT+2")]
    [InlineData("Australia/Sydney", "GMT+10/GMT+11")]
    [InlineData("America/New_York", "GMT-5/GMT-4")]
    [InlineData("Asia/Kolkata", "GMT+5:30")]
    [InlineData("Africa/Abidjan", "GMT")]
    public void Labels_a_zone_by_its_offsets_when_the_request_gives_no_label(
        string tz,
        string label
    )
    {
        Assert.Equal(label, Read(tz, null).Label);
    }

    [Fact]
    public void Writes_UTC_when_the_request_names_no_zone()
    {
        var sheet = Sheet(Read(null, null));

        Assert.Equal("Last Modified (UTC)", sheet.Cell(1, 2).GetString());
        Assert.Equal(new DateTime(2026, 7, 1, 10, 0, 0), sheet.Cell(2, 2).GetDateTime());
    }

    [Theory]
    [InlineData("Mars/Olympus_Mons", null)]
    [InlineData("Europe/Budapest", "CET; DROP")]
    [InlineData("Europe/Budapest", "=HYPERLINK(\"x\")")]
    [InlineData("Europe/Budapest", "ABCDEFGHIJKLMNOPQRSTUVWXY")]
    public void Refuses_an_unknown_zone_or_a_label_that_is_not_an_abbreviation(
        string tz,
        string? label
    )
    {
        Assert.NotNull(ExportZone.Read(tz, label, 2026, out _));
    }
}
