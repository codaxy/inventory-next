using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// An address in the domain but not on the list is refused exactly as an allowed one is answered, so
/// the endpoint cannot be used to find out who works here.
/// </summary>
public class AllowListTests
{
    private sealed class ApplicationWithAnAllowList : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting("ConnectionStrings:PostgreSQL", "Host=localhost;Database=unused");
            builder.UseSetting("Auth:AllowedDomains:0", "codaxy.com");
            builder.UseSetting("Auth:OneTimeCode:Enabled", "true");
            builder.UseSetting("Auth:AllowedUsers:0", "permitted@codaxy.com");
        }
    }

    [Fact]
    public async Task An_address_off_the_list_is_answered_as_though_it_were_on_it()
    {
        using var app = new ApplicationWithAnAllowList();
        var client = app.CreateClient();

        var refused = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/request",
            new { email = "stranger@codaxy.com" }
        );

        Assert.Equal(HttpStatusCode.NoContent, refused.StatusCode);
    }
}
