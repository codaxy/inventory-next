using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Codaxy.Inventory.Tests.Integration;

public class RateLimitTests
{
    private sealed class ApplicationWithATightLimit : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting("ConnectionStrings:PostgreSQL", "Host=localhost;Database=unused");
            builder.UseSetting("Auth:AllowedDomains:0", "codaxy.com");
            builder.UseSetting("Auth:OneTimeCode:Enabled", "true");
            builder.UseSetting("RateLimit:SignIn:Permits", "2");
            builder.UseSetting("RateLimit:SignIn:Window", "00:05:00");
        }
    }

    [Fact]
    public async Task Refuses_a_caller_that_exceeds_its_permits()
    {
        using var app = new ApplicationWithATightLimit();
        var client = app.CreateClient();

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var allowed = await client.PostAsJsonAsync(
                "/api/auth/one-time-code/verify",
                new { email = "someone@codaxy.com", code = "000000" }
            );

            Assert.Equal(HttpStatusCode.Unauthorized, allowed.StatusCode);
        }

        var refused = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email = "someone@codaxy.com", code = "000000" }
        );

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.NotNull(refused.Headers.RetryAfter);
    }

    [Fact]
    public async Task Leaves_everything_else_alone()
    {
        using var app = new ApplicationWithATightLimit();
        var client = app.CreateClient();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var response = await client.GetAsync("/api/auth/options");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
