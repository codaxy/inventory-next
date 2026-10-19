using Codaxy.Inventory.App.Shared.Email;

namespace Codaxy.Inventory.App.Administration.Settings.Mail;

/// <summary>How mail is sent, read-only: it is the deployment's, set in its configuration.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder settings) =>
        settings.MapGet("/mail", (IEmailRelay relay) => relay.Describe());
}
