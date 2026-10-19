namespace Codaxy.Inventory.App.Shared.Email;

/// <param name="SignsInAs">The account mail is sent from at the relay, or empty where none signs in.</param>
public sealed record EmailRelay(
    string Host,
    int Port,
    string From,
    string FromName,
    string SignsInAs
);

/// <summary>How mail leaves, as the deployment configures it, for the settings screen — never its password.</summary>
public interface IEmailRelay
{
    EmailRelay Describe();
}
