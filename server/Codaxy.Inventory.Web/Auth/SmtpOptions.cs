namespace Codaxy.Inventory.Web.Auth;

public sealed class SmtpOptions
{
    public const string Section = "Smtp";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 1025;

    public string From { get; init; } = "inventory@codaxy.com";
}
