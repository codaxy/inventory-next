using System.Net.Http.Json;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

public static class SignedIn
{
    /// <summary>A client holding a session, signed in the way a person is: by a one-time code.</summary>
    public static async Task<HttpClient> ClientAsync(
        this InventoryApplication app,
        string email = "reader@codaxy.com"
    )
    {
        var client = app.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

        await client.PostAsJsonAsync("/api/auth/one-time-code/request", new { email });

        var verified = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = app.Emails.LastCodeFor(email) }
        );
        verified.EnsureSuccessStatusCode();

        return client;
    }
}
