using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.ExpiryAlerts;

/// <summary>
/// Checks for expiry alerts at start and every <see cref="ExpiryAlertOptions.Interval"/>. A failed
/// check is logged and left to the next one, which finds the same alerts still due.
/// </summary>
public sealed class ExpiryAlertWorker(
    IServiceScopeFactory scopes,
    IOptions<ExpiryAlertOptions> options,
    ILogger<ExpiryAlertWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);

        do await CheckAsync(stoppingToken);
        while (await timer.WaitForNextTickAsync(stoppingToken));
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
