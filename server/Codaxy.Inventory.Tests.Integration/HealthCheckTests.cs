using System.Net;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// Readiness is only worth anything if it goes red, so this is the case it exists for: the
/// application is up and the database is not.
/// </summary>
public class HealthCheckTests
{
    private sealed class ApplicationWithNoDatabase : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseScratchServerLog();
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting(
                "ConnectionStrings:PostgreSQL",
                // Nothing listens here, and the port is outside the range Docker hands out.
                "Host=localhost;Port=1;Database=absent;Username=absent;Password=absent;Timeout=1"
            );
        }
    }

    [Fact]
    public async Task Liveness_is_healthy_when_the_database_is_not_reachable()
    {
        using var app = new ApplicationWithNoDatabase();

        var response = await app.CreateClient().GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Readiness_is_unhealthy_when_the_database_is_not_reachable()
    {
        using var app = new ApplicationWithNoDatabase();

        var response = await app.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
