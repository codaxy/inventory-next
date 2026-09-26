namespace Codaxy.Inventory.App.Shared.Search;

/// <summary>
/// The list convention's free text: whitespace-separated terms, every one of which must match, each
/// as an <c>ILIKE</c> pattern with its wildcards escaped so they match themselves.
/// </summary>
public static class FreeText
{
    public const string Escape = @"\";

    public static IEnumerable<string> Terms(string? q) =>
        (q ?? "").Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

    /// <summary><c>%term%</c>, with <c>\</c>, <c>%</c> and <c>_</c> in the term matching themselves.</summary>
    public static string Pattern(string term) =>
        $"%{term.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
}
