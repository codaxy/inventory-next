using Codaxy.Inventory.App;
using Codaxy.Inventory.Web.Auth;
using Codaxy.Inventory.Web.Setup;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddInventoryOptions(builder.Configuration)
    .AddInventoryPersistence(builder.Configuration)
    .AddInventoryDataProtection(builder.Configuration)
    .AddInventoryAuthentication(builder.Configuration)
    .AddInventoryRateLimiting(builder.Configuration)
    .AddInventoryHttpLogging()
    .AddInventoryHealthChecks();

var app = builder.Build();

app.MigrateAndSeed();

app.UseHttpLogging();
app.UseRateLimiter();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Liveness asks whether the process is up, readiness whether it can serve: a database that is down
// must not restart the container, and must stop traffic arriving.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
app.MapAuth();
app.MapInventoryApi();

// Every path the client routes to returns the shell. An unknown /api path is a 404, not the shell:
// served the page, a fetch fails parsing HTML as JSON and says nothing about the missing route.
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
