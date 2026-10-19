namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

/// <summary>
/// Asks the hosted check to run now rather than at its next interval: saving the alert settings does,
/// so a recipient just added hears at once of what is due. Asks made before it runs are one check.
/// </summary>
public sealed class ExpiryAlertRequests
{
    private readonly SemaphoreSlim pending = new(0, 1);

    public void CheckNow()
    {
        try
        {
            pending.Release();
        }
        catch (SemaphoreFullException)
        {
            // One is already waiting to be served.
        }
    }

    /// <summary>Until a check is asked for or <paramref name="interval"/> has passed.</summary>
    public Task WaitAsync(TimeSpan interval, CancellationToken cancellationToken) =>
        pending.WaitAsync(interval, cancellationToken);
}
