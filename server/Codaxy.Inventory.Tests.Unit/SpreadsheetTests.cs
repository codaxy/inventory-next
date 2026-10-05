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

    private static IXLWorksheet Sheet(TimeZoneInfo zone) =>
        new XLWorkbook(new MemoryStream(Spreadsheet.Write(Rows, "Check", zone))).Worksheet(1);

    [Fact]
    public void Writes_an_instant_in_the_zone_with_its_own_daylight_saving()
    {
        var sheet = Sheet(TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade"));

        Assert.Equal(new DateTime(2026, 7, 1, 12, 0, 0), sheet.Cell(2, 2).GetDateTime());
        Assert.Equal(new DateTime(2026, 1, 15, 11, 0, 0), sheet.Cell(3, 2).GetDateTime());
        // Past midnight there, so the next day.
        Assert.Equal(new DateTime(2026, 1, 16, 0, 30, 0), sheet.Cell(3, 3).GetDateTime());
        Assert.True(sheet.Cell(2, 3).IsEmpty());
    }

    [Fact]
    public void Names_the_zone_in_an_instant_column_header_only()
    {
        var sheet = Sheet(TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade"));

        Assert.Equal("Name", sheet.Cell(1, 1).GetString());
        Assert.Equal("Last Modified (Europe/Belgrade)", sheet.Cell(1, 2).GetString());
        Assert.Equal("Ended (Europe/Belgrade)", sheet.Cell(1, 3).GetString());
    }

    [Fact]
    public void Writes_UTC_when_the_request_names_no_zone()
    {
        var sheet = Sheet(Spreadsheet.Zone(null)!);

        Assert.Equal("Last Modified (UTC)", sheet.Cell(1, 2).GetString());
        Assert.Equal(new DateTime(2026, 7, 1, 10, 0, 0), sheet.Cell(2, 2).GetDateTime());
    }

    [Fact]
    public void Knows_no_zone_by_an_unknown_name()
    {
        Assert.Same(TimeZoneInfo.Utc, Spreadsheet.Zone(""));
        Assert.Null(Spreadsheet.Zone("Mars/Olympus_Mons"));
    }
}
