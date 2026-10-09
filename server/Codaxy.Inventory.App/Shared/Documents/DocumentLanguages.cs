namespace Codaxy.Inventory.App.Shared.Documents;

/// <summary>
/// The languages documents are printed in, as BCP 47 tags. The client's list
/// (<c>src/documents/languages.ts</c>) must match: it holds the text, this one what is accepted.
/// </summary>
public static class DocumentLanguages
{
    public const string English = "en";

    public static readonly IReadOnlyList<string> Supported = [English, "sr-Latn-BA"];

    /// <summary>The query parameter a PDF endpoint takes its language from.</summary>
    public const string Parameter = "lang";

    public static bool IsSupported(string? language) =>
        language is not null && Supported.Contains(language, StringComparer.Ordinal);

    /// <summary>
    /// A 400 for a <c>lang</c> the server does not print in, else null; none at all is the default.
    /// </summary>
    public static IResult? Refuse(string? language) =>
        language is null || IsSupported(language)
            ? null
            : Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [Parameter] = [$"Documents are printed in {string.Join(", ", Supported)}."],
                }
            );
}
