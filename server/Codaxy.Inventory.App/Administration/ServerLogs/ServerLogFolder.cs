using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.App.Administration.ServerLogs;

/// <summary>The log folder as the host resolves it, for the endpoints that read it.</summary>
public sealed class ServerLogFolder(
    IOptions<ServerLogOptions> options,
    IWebHostEnvironment environment
)
{
    public string Path { get; } =
        options.Value.ResolvePath(
            environment.ContentRootPath,
            environment.WebRootPath
                ?? System.IO.Path.Combine(environment.ContentRootPath, "wwwroot")
        );
}
