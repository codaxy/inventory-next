using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Shared.Documents;

/// <summary>What every printed document shares.</summary>
public static class DocumentSettings
{
    /// <summary>The city documents are signed in; empty, the line is left for a hand.</summary>
    public static readonly SettingDefinition<string> Place = new(
        "documents.place",
        "",
        tidy: place => place.Trim()
    );

    /// <summary>The language a document opens in; its reader can choose another.</summary>
    public static readonly SettingDefinition<string> DefaultLanguage = new(
        "documents.default_language",
        DocumentLanguages.English,
        value =>
            DocumentLanguages.IsSupported(value)
                ? null
                : $"Must be one of: {string.Join(", ", DocumentLanguages.Supported)}.",
        language => DocumentLanguages.Canonical(language) ?? language
    );
}
