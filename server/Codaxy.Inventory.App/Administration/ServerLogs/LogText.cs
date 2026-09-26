using System.Globalization;
using System.Text;

namespace Codaxy.Inventory.App.Administration.ServerLogs;

/// <summary>
/// Logged text made safe to show. Anything that would act rather than read — a control character, a
/// terminal escape, a bidi override that reorders a line on screen, a Unicode line separator that
/// breaks one, a zero-width character that hides — becomes a visible marker such as `⟨ESC⟩` or
/// `⟨U+202E⟩`. Done on the server, so every consumer of the API gets text that cannot pass itself off
/// as something else; the client still renders it as text only.
/// </summary>
public static class LogText
{
    public static string Displayable(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var result = new StringBuilder(text.Length);

        foreach (var rune in text.EnumerateRunes())
        {
            var value = rune.Value;

            if (value is '\n' or '\t')
                result.Append((char)value);
            else if (value == 0x1B)
                result.Append("⟨ESC⟩");
            else if (value == '\r')
                result.Append("⟨CR⟩");
            else if (IsHidden(value))
                result
                    .Append("⟨U+")
                    .Append(value.ToString("X4", CultureInfo.InvariantCulture))
                    .Append('⟩');
            else
                result.Append(rune.ToString());
        }

        return result.ToString();
    }

    private static bool IsHidden(int value) =>
        value < 0x20 // C0 controls
        || value is >= 0x7F and <= 0x9F // DEL and C1 controls
        || value is 0x061C or 0x200E or 0x200F // bidi marks
        || value is >= 0x202A and <= 0x202E // bidi embeddings and overrides
        || value is >= 0x2066 and <= 0x2069 // bidi isolates
        || value is 0x2028 or 0x2029 // line and paragraph separators
        || value is 0x200B or 0x2060 or 0xFEFF; // zero-width space, word joiner, BOM
}
