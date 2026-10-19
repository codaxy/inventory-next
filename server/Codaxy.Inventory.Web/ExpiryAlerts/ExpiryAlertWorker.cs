using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.ExpiryAlerts;

/// <summary>
/// Checks for expiry alerts at start, every <see cref="ExpiryAlertOptions.Interval"/>, and whenever
/// <see cref="ExpiryAlertRequests"/> asks — the interval counted again from each check. A failed check
/// is logged and left to the next one, which finds the same alerts still due.
/// </summary>
public sealed class ExpiryAlertWorker(
    IServiceScopeFactory scopes,
    ExpiryAlertRequests requests,
    IOptions<ExpiryAlertOptions> options,
    ILogger<ExpiryAlertWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAsync(stoppingToken);
            await requests.WaitAsync(options.Value.Interval, stoppingToken);
        }
    }

    private async Task CheckAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopes.CreateScope();
            await scope
                .ServiceProvider.GetRequiredService<ExpiryAlertCheck>()
                .SendDueAsync(stoppingToken);
        }
        catch (Exception e) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(e, "Expiry alerts were not sent; the next check tries again");
        }
    }
}
