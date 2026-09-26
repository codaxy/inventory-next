namespace Codaxy.Inventory.App.Shared.Search;

/// <summary>
/// The list convention's free text: whitespace-separated terms, every one of which must match, each
/// as an <c>ILIKE</c> pattern with its wildcards escaped so they match themselves.
/// </summary>
public static class FreeText
{
    public const string Escape = @"\";

    /// <summary>The terms, a leading <c>#</c> dropped: "#100893" is how the screens show a number.</summary>
    public static IEnumerable<string> Terms(string? q) =>
        (q ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.TrimStart('#'))
            .Where(t => t.Length > 0);

    /// <summary>
    /// The id a term names, when it is one: a GUID term matches ids exactly — the record's own and
    /// those of what it points at — and no column's text.
    /// </summary>
    public static Guid? Id(string term) => Guid.TryParse(term, out var id) ? id : null;

    /// <summary><c>%term%</c>, with <c>\</c>, <c>%</c> and <c>_</c> in the term matching themselves.</summary>
    public static string Pattern(string term) =>
        $"%{term.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
}
