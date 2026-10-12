using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Shared.Documents.Settings;

/// <summary>What a printed document takes from the settings, so no document's answer repeats it.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder api) => api.MapGet("/documents/settings", Handle);

    /// <param name="Place">Where documents are signed, or empty for a hand to write.</param>
    public sealed record Response(string DefaultLanguage, string Place);

    private static async Task<Response> Handle(
        SettingStore settings,
        CancellationToken cancellationToken
    )
    {
        var stored = await settings.LoadAsync(cancellationToken);
        return new(
            DocumentLanguages.Canonical(stored.Get(DocumentSettings.DefaultLanguage))!,
            stored.Get(DocumentSettings.Place).Trim()
        );
    }
}
