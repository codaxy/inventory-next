using Codaxy.Inventory.App.Shared.Paging;

namespace Codaxy.Inventory.App.Administration.ServerLogs.List;

public static class Endpoint
{
    /// <summary>Longer than a day in any timezone, short enough that a request reads a handful of files.</summary>
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(7);

    public static void Map(RouteGroupBuilder serverLog) => serverLog.MapGet("/", Handle);

    /// <param name="From">Inclusive; with <paramref name="To"/>, the viewer's own day.</param>
    /// <param name="To">Exclusive.</param>
    /// <param name="Level">The least severe level shown.</param>
    /// <param name="Q">Free text: every whitespace-separated term must match, case-insensitively.</param>
    /// <param name="Sort"><c>-time</c>, newest first, or <c>time</c>.</param>
    public sealed record Query(
        DateTimeOffset? From,
        DateTimeOffset? To,
        string? Level,
        string? Q,
        string? Sort,
        int? Page,
        int? PageSize
    );

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        ServerLogFolder folder,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        Dictionary<string, string[]> errors = [];

        if (query.From is null || query.To is null || query.To <= query.From)
            errors["from"] = ["Give a range: from before to."];
        else if (query.To - query.From > MaxRange)
            errors["to"] = ["The range is at most seven days."];

        var minimum = query.Level is null ? 0 : Array.IndexOf(LogFiles.Levels, query.Level);
        if (minimum < 0)
            errors["level"] = ["Level is one of " + string.Join(", ", LogFiles.Levels) + "."];

        if (query.Sort is not (null or "time" or "-time"))
            errors["sort"] = ["Sort by time or -time."];

        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var entries = await LogFiles.ReadAsync(
            folder.Path,
            query.From!.Value,
            query.To!.Value,
            cancellationToken
        );
        var terms = (query.Q ?? "").Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        // An unparsed line has no level of its own; it is shown whatever the filter, since it may be
        // the one line that explains the rest.
        IEnumerable<(LogEntry Entry, int Order)> matching = entries
            .Select((entry, order) => (entry, order))
            .Where(e => e.entry.Raw || Array.IndexOf(LogFiles.Levels, e.entry.Level) >= minimum)
            .Where(e => terms.All(term => Matches(e.entry, term)));

        // Written order breaks ties: entries of one millisecond keep the order they were logged in.
        matching =
            query.Sort == "time"
                ? matching.OrderBy(e => e.Entry.Time).ThenBy(e => e.Order)
                : matching.OrderByDescending(e => e.Entry.Time).ThenByDescending(e => e.Order);

        var all = matching.Select(e => e.Entry).ToList();

        return Results.Ok(
            new Page<LogEntry>([.. all.Skip(window.Skip).Take(window.Size)], all.Count)
        );
    }

    private static bool Matches(LogEntry entry, string term) =>
        Contains(entry.Message, term)
        || Contains(entry.Category, term)
        || Contains(entry.Exception, term)
        || Contains(entry.Level, term)
        || Contains(entry.TraceId, term);

    private static bool Contains(string? text, string term) =>
        text?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
}
