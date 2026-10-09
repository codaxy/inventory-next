namespace Codaxy.Inventory.App.Shared.Documents;

/// <summary>What every printed document shares: a deployment's, since offices differ.</summary>
public sealed class DocumentOptions
{
    public const string Section = "Documents";

    /// <summary>The language a document opens in; its reader can choose another.</summary>
    public string DefaultLanguage { get; set; } = DocumentLanguages.English;

    /// <summary>The city documents are signed in; empty, the line is left for a hand.</summary>
    public string Place { get; set; } = "";

    /// <summary>Why these options stop the start, or null: a default the application does not print in.</summary>
    public string? Problem =>
        DocumentLanguages.IsSupported(DefaultLanguage)
            ? null
            : $"Documents:DefaultLanguage must be one of: {string.Join(", ", DocumentLanguages.Supported)}.";

    public bool IsValid => Problem is null;
}
