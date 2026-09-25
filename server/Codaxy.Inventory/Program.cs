using Codaxy.Inventory.Auth;
using Codaxy.Inventory.Setup;
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

// Every path the client routes to returns the shell; the API is under /api and answers for itself.
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
