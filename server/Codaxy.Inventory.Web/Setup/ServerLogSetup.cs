using Codaxy.Inventory.App.Administration.ServerLogs;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;

namespace Codaxy.Inventory.Web.Setup;

public static class ServerLogSetup
{
    /// <summary>
    /// The server log: Serilog's file sink added beside the framework's console logger, so code logs
    /// through <c>ILogger</c> and only this file names Serilog. The framework's <c>Logging:LogLevel</c>
    /// filters it like every other provider; Serilog itself lets everything through.
    ///
    /// One rendered compact-JSON object per line — a newline in a logged value stays escaped inside
    /// its string, so nothing logged can start an entry of its own — in a file per day, capped in size,
    /// and deleted after <c>ServerLog:RetentionDays</c>.
    /// </summary>
    public static WebApplicationBuilder AddInventoryServerLog(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(ServerLogOptions.Section);
        var options = section.Get<ServerLogOptions>() ?? new ServerLogOptions();

        var folder = options.ResolvePath(
            builder.Environment.ContentRootPath,
            builder.Environment.WebRootPath
                ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot")
        );

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                new RenderedCompactJsonFormatter(),
                Path.Combine(folder, ServerLogOptions.FilePrefix + ".log"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                retainedFileTimeLimit: TimeSpan.FromDays(options.RetentionDays),
                // Appends through the operating system, so a second writer — a restart overlapping
                // the old process, two instances on one volume — cannot overwrite a line. Unshared, the
                // sink writes at a position it tracks itself and interleaves with anyone else.
                shared: true
            )
            .CreateLogger();

        // A provider added directly, not through `AddSerilog`: that adds a filter letting every level
        // through for Serilog, which outranks `Logging:LogLevel` and fills the file with Debug.
        builder.Logging.AddProvider(new SerilogLoggerProvider(logger, dispose: true));

        builder.Services.AddOptions<ServerLogOptions>().Bind(section);
        builder.Services.AddSingleton<ServerLogFolder>();

        return builder;
    }
}
