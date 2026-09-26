using Microsoft.AspNetCore.Hosting;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

public static class ScratchServerLog
{
    /// <summary>
    /// A log folder of the test's own. Every factory that starts the application needs it: the
    /// default is `logs` under the content root — the source tree — where a test run's entries land
    /// in the developer's own log beside a running `dotnet run`.
    /// </summary>
    public static IWebHostBuilder UseScratchServerLog(this IWebHostBuilder builder) =>
        builder.UseSetting(
            "ServerLog:Path",
            Path.Combine(
                Path.GetTempPath(),
                "inventory-next-tests",
                Guid.CreateVersion7().ToString(),
                "logs"
            )
        );
}
