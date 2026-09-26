namespace Codaxy.Inventory.App.Persistence;

/// <summary>Who is acting, as the host knows it: the signed-in user's address, or none.</summary>
public interface ICurrentUser
{
    string? Email { get; }
}
