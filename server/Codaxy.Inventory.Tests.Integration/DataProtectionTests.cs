using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.Tests.Integration.Infrastructure;

namespace Codaxy.Inventory.Tests.Integration;

public class DataProtectionTests(InventoryApplication app) : IClassFixture<InventoryApplication>
{
    /// <summary>
    /// The key ring outliving the process is what stops a restart signing everyone out, and the only
    /// evidence of it from inside the application is that the keys reach the configured directory.
    /// </summary>
    [Fact]
    public async Task Writes_its_keys_to_the_configured_directory()
    {
        var client = app.CreateClient();
        const string email = "keyring@codaxy.com";
        await app.UserAsync(email);

        await client.PostAsJsonAsync("/api/auth/one-time-code/request", new { email });
        var response = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = app.Emails.LastCodeFor(email) }
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEmpty(Directory.GetFiles(app.KeyRingPath, "key-*.xml"));
    }
}
