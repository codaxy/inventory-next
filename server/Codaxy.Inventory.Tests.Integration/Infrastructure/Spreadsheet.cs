using System.IO.Compression;
using System.Text;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

/// <summary>An xlsx is a zip of XML: its text, every part of it, is enough to say what a sheet holds.</summary>
public static class Spreadsheet
{
    public static async Task<string> TextOf(HttpResponseMessage response)
    {
        using var zip = new ZipArchive(
            new MemoryStream(await response.Content.ReadAsByteArrayAsync())
        );
        var text = new StringBuilder();
        foreach (
            var entry in zip.Entries.Where(e =>
                e.FullName.StartsWith("xl/") && e.FullName.EndsWith(".xml")
            )
        )
        {
            using var reader = new StreamReader(entry.Open());
            text.Append(await reader.ReadToEndAsync());
        }
        return text.ToString();
    }
}
