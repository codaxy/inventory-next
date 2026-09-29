using System.Reflection;

namespace Codaxy.Inventory.Web.Version;

public static class Endpoint
{
    /// <summary>
    /// The build: <c>26.9.29+1222.eaea98a</c> — the commit's UTC date, then its time and sha — stamped
    /// by the image build; <c>dev</c>, with the checkout's sha where there
    /// is one, anywhere else.
    /// </summary>
    public static readonly string Current = Shorten(
        typeof(Endpoint)
            .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?? "dev"
    );

    /// <summary>Anonymous, so a deployment can be asked what it runs without signing in.</summary>
    public static void MapVersion(this IEndpointRouteBuilder app) =>
        app.MapGet("/api/version", () => Results.Text(Current));

    /// <summary>The SDK appends a checkout's full sha; seven characters say as much.</summary>
    private static string Shorten(string version) =>
        version.Split('+') is [var core, var sha] && sha.Length == 40
            ? $"{core}+{sha[..7]}"
            : version;
}
