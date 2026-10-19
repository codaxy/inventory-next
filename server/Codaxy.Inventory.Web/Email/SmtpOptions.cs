namespace Codaxy.Inventory.Web.Email;

public sealed class SmtpOptions
{
    public const string Section = "Smtp";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 1025;

    public string From { get; init; } = "inventory@codaxy.com";

    /// <summary>The name shown beside <see cref="From"/>; empty, the address alone.</summary>
    public string FromName { get; init; } = "";

    /// <summary>
    /// The account to sign in to the relay with; empty, none — Mailpit and a local relay take mail
    /// from anyone, Gmail and most hosted relays only from an account.
    /// </summary>
    public string UserName { get; init; } = "";

    public string Password { get; init; } = "";

    public string? Problem =>
        UserName != "" && Password == "" ? "Smtp:Password is required with Smtp:UserName." : null;
}
