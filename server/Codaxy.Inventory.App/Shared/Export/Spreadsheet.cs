using System.Reflection;
using ClosedXML.Attributes;
using ClosedXML.Excel;

namespace Codaxy.Inventory.App.Shared.Export;

/// <summary>
/// A list as a spreadsheet, written by ClosedXML: one row type per list, whose properties headed by
/// <c>[XLColumn(Header = …)]</c> are the columns, in declaration order. One sheet holding an Excel
/// table — header pinned, filter on every column, widths fitted to the content.
/// </summary>
public static class Spreadsheet
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string DateFormat = "d mmm yyyy";
    private const string DateTimeFormat = "d mmm yyyy hh:mm";
    private const string MoneyFormat = "#,##0.00";

    public static byte[] Write<TRow>(IReadOnlyCollection<TRow> rows, string sheet)
        where TRow : class
    {
        var columns = Columns<TRow>.All;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(sheet);

        for (var c = 0; c < columns.Length; c++)
            worksheet.Cell(1, c + 1).Value = columns[c].Header;

        var r = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < columns.Length; c++)
                Set(worksheet.Cell(r, c + 1), columns[c].Property.GetValue(row));
            r++;
        }

        // A table needs a row beneath its header; an empty list keeps one, blank.
        var table = worksheet.Range(1, 1, Math.Max(r - 1, 2), columns.Length).CreateTable(sheet);
        table.Theme = XLTableTheme.TableStyleLight9;
        worksheet.SheetView.FreezeRows(1);

        // Fitted to the content, plus room for the header's filter button, which the fit leaves out.
        foreach (var column in worksheet.ColumnsUsed())
            column.AdjustToContents().Width += 3;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // A date is a number Excel formats; `DateOnly` is written as text unless converted.
    private static void Set(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                return;
            case DateOnly date:
                cell.Value = date.ToDateTime(TimeOnly.MinValue);
                cell.Style.NumberFormat.Format = DateFormat;
                return;
            case DateTime instant:
                cell.Value = instant;
                cell.Style.NumberFormat.Format = DateTimeFormat;
                return;
            case decimal money:
                cell.Value = money;
                cell.Style.NumberFormat.Format = MoneyFormat;
                return;
            default:
                cell.Value = XLCellValue.FromObject(value);
                return;
        }
    }

    /// <summary>
    /// The file, downloaded as the original named it — "Licenses.Export.xlsx" — or, when a search or a
    /// filter narrowed the rows, "Licenses.Export - Filtered.xlsx", so a partial list is never taken for
    /// the whole. The sort does not count: it only orders.
    /// </summary>
    public static IResult File<TRow>(IReadOnlyCollection<TRow> rows, string name, bool filtered)
        where TRow : class =>
        Results.File(
            Write(rows, name.Split('.')[0]),
            ContentType,
            filtered ? $"{name} - Filtered.xlsx" : $"{name}.xlsx"
        );

    private static class Columns<TRow>
    {
        public static readonly (PropertyInfo Property, string Header)[] All = typeof(TRow)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(p => p.MetadataToken)
            .Select(p => (Property: p, Column: p.GetCustomAttribute<XLColumnAttribute>()))
            .Where(p => p.Column is { Ignore: false, Header: not null })
            .Select(p => (p.Property, p.Column!.Header!))
            .ToArray();
    }
}
