using System.Text.RegularExpressions;

namespace Codaxy.Inventory.App.Shared.Volumes;

/// <summary>What names a volume beside its license, wherever one is picked or named in passing.</summary>
public static partial class VolumeNames
{
    /// <summary>
    /// What tells volumes of one license and software apart: the description, else the type. A web
    /// address in the description is left out — it is where the seats are managed, not a name, and
    /// with nowhere to break it runs a picker's option across the screen.
    /// </summary>
    public static string Designator(string? description, string type)
    {
        if (string.IsNullOrWhiteSpace(description))
            return type;
        var text = Url().Replace(description, "");
        text = EmptyBrackets().Replace(text, "");
        text = SpaceBeforePunctuation()
            .Replace(Whitespace().Replace(text, " "), "")
            .Trim(' ', '·', '-', '–', '—', ',', ';', ':', '|');
        return text.Length == 0 ? type : text;
    }

    // A sentence's punctuation after an address is not part of it.
    [GeneratedRegex(@"\b(?:https?://|www\.)[^\s()<>]*[^\s()<>.,;:!?]", RegexOptions.IgnoreCase)]
    private static partial Regex Url();

    [GeneratedRegex(@"\(\s*\)|\[\s*\]|<\s*>")]
    private static partial Regex EmptyBrackets();

    [GeneratedRegex(@"\s+(?=[,;:.])")]
    private static partial Regex SpaceBeforePunctuation();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
