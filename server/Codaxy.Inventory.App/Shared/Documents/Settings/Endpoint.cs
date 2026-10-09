using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Shared.Documents.Settings;

/// <summary>What a printed document takes from the deployment, so no document's answer repeats it.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder api) => api.MapGet("/documents/settings", Handle);

    /// <param name="Place">Where documents are signed, or empty for a hand to write.</param>
    public sealed record Response(string DefaultLanguage, string Place);

    private static Response Handle(IOptions<DocumentOptions> options) =>
        new(options.Value.DefaultLanguage, options.Value.Place.Trim());
}
