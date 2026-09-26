using System.Globalization;
using Codaxy.CodeReports;
using Codaxy.CodeReports.Controls;
using Codaxy.CodeReports.Data;
using Codaxy.CodeReports.Exporters.Xlio;
using Codaxy.CodeReports.Styling;

namespace Codaxy.Inventory.App.Shared.Export;

/// <summary>
/// A list as a spreadsheet, as the original wrote it: one row type per list, whose properties marked
/// <c>[TableColumn]</c> are the columns, laid out by CodeReports and written by its xlsx exporter. The
/// headers keep the original's trailing spaces — CodeReports sizes a column by its header's width.
/// </summary>
public static class Excel
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Write<TRow>(IReadOnlyCollection<TRow> rows)
        where TRow : class
    {
        // CodeReports reads its own texts by the thread's culture and throws on one it does not ship —
        // the invariant culture of a container among them. The spreadsheet is English, so that is set
        // for the write and put back.
        var (culture, ui) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = English;
        try
        {
            return WriteIn(rows);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (culture, ui);
        }
    }

    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private static byte[] WriteIn<TRow>(IReadOnlyCollection<TRow> rows)
        where TRow : class
    {
        var data = new DataContext();
        data.AddTable("data", rows.ToArray());

        var flow = new Flow { Orientation = FlowOrientation.Vertical };
        flow.AddTable<TRow>("data");

        using var stream = new MemoryStream();
        XlsxReportWriter.WriteToStream(Report.CreateReport(flow, data), Themes.Default, stream);
        return stream.ToArray();
    }

    /// <summary>The file, downloaded as the name given — the original's names, "Licenses.Export.xlsx".</summary>
    public static IResult File<TRow>(IReadOnlyCollection<TRow> rows, string name)
        where TRow : class => Results.File(Write(rows), ContentType, name);
}
