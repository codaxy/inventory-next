using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Codaxy.Inventory.App.Administration.ServerLogs;

/// <summary>One logged event, its text already made displayable.</summary>
/// <param name="Level">Serilog's name for it: Verbose, Debug, Information, Warning, Error, Fatal.</param>
/// <param name="Raw">A line that did not parse, shown as it stands rather than dropped.</param>
public sealed record LogEntry(
    DateTimeOffset Time,
    string Level,
    string? Category,
    string Message,
    string? Exception,
    string? TraceId,
    bool Raw
);

/// <summary>
/// The day files under the log folder, read while the sink may be appending to today's. Files roll
/// on the server's clock, so a range is read from the files dated a day either side of it and the
/// entries are filtered by their own timestamps.
/// </summary>
public static partial class LogFiles
{
    public static readonly string[] Levels =
    [
        "Verbose",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Fatal",
    ];

    /// <summary>The dates that have a file, newest first.</summary>
    public static IReadOnlyList<DateOnly> Days(string folder) =>
        [.. Files(folder).Select(f => f.Date).Distinct().OrderDescending()];

    /// <summary>Every entry stamped within [from, to), in the order written.</summary>
    public static async Task<List<LogEntry>> ReadAsync(
        string folder,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken
    )
    {
        var first = DateOnly.FromDateTime(from.UtcDateTime).AddDays(-1);
        var last = DateOnly.FromDateTime(to.UtcDateTime).AddDays(1);
        List<LogEntry> entries = [];

        foreach (
            var file in Files(folder)
                .Where(f => f.Date >= first && f.Date <= last)
                .OrderBy(f => f.Date)
                .ThenBy(f => f.Part)
        )
        {
            // The sink holds the file open for writing; reading beside it needs ReadWrite sharing.
            await using var stream = new FileStream(
                file.Path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );
            using var reader = new StreamReader(stream);

            // A line that does not parse takes the time of the one before it, so it stays where it was
            // written; before any, the start of the file's day.
            var previous = new DateTimeOffset(
                file.Date.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero
            );

            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (line.Length == 0)
                    continue;

                var entry = Parse(line, previous);
                previous = entry.Time;

                if (entry.Time >= from && entry.Time < to)
                    entries.Add(entry);
            }
        }

        return entries;
    }

    /// <summary>A rendered compact-JSON line (`@t`, `@m`, `@l`, `@x`, `@tr`), or the line as it stands.</summary>
    public static LogEntry Parse(string line, DateTimeOffset previous)
    {
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            if (
                root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("@t", out var t)
                && t.TryGetDateTimeOffset(out var time)
            )
                return new LogEntry(
                    time,
                    String(root, "@l") ?? "Information",
                    String(root, "SourceContext"),
                    LogText.Displayable(Request(root) ?? String(root, "@m") ?? String(root, "@mt")),
                    String(root, "@x") is { } x ? LogText.Displayable(x) : null,
                    String(root, "@tr"),
                    Raw: false
                );
        }
        catch (JsonException) { }

        return new LogEntry(
            previous,
            "Unknown",
            null,
            LogText.Displayable(line),
            null,
            null,
            Raw: true
        );
    }

    /// <summary>
    /// The framework's request line as one line: `GET /path → 200 · 52 ms`. Its own message spreads the
    /// same fields over six, which on a phone is a screenful per request.
    /// </summary>
    private static string? Request(JsonElement root)
    {
        if (
            String(root, "Method") is not { } method
            || String(root, "Path") is not { } path
            || !root.TryGetProperty("StatusCode", out var status)
            || status.ValueKind != JsonValueKind.Number
        )
            return null;

        var duration =
            root.TryGetProperty("Duration", out var d) && d.TryGetDouble(out var ms)
                ? string.Create(CultureInfo.InvariantCulture, $" · {ms:0} ms")
                : "";

        return $"{method} {String(root, "PathBase")}{path} → {status.GetRawText()}{duration}";
    }

    private static string? String(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private sealed record LogFile(string Path, DateOnly Date, int Part);

    /// <summary>Only names the sink writes; the folder is never walked beyond them.</summary>
    private static IEnumerable<LogFile> Files(string folder)
    {
        if (!System.IO.Directory.Exists(folder))
            yield break;

        foreach (
            var path in System.IO.Directory.EnumerateFiles(
                folder,
                ServerLogOptions.FilePrefix + "*.log"
            )
        )
        {
            var match = FileName().Match(System.IO.Path.GetFileName(path));

            if (
                match.Success
                && DateOnly.TryParseExact(
                    match.Groups["date"].Value,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date
                )
            )
                yield return new LogFile(
                    path,
                    date,
                    match.Groups["part"].Success
                        ? int.Parse(match.Groups["part"].Value, CultureInfo.InvariantCulture)
                        : 0
                );
        }
    }

    [GeneratedRegex(@"^server-(?<date>\d{8})(_(?<part>\d{3,}))?\.log$")]
    private static partial Regex FileName();
}
