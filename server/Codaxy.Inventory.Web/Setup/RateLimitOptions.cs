namespace Codaxy.Inventory.Web.Setup;

/// <summary>
/// How many sign-in attempts one caller may make in a window. Configured rather than compiled in:
/// the right number depends on how many people sit behind one address, which the application cannot
/// know.
/// </summary>
public sealed class RateLimitOptions
{
    public const string Section = "RateLimit:SignIn";

    public int Permits { get; init; } = 10;

    public TimeSpan Window { get; init; } = TimeSpan.FromMinutes(5);
}
