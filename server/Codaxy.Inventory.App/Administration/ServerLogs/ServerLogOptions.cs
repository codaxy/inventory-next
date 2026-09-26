namespace Codaxy.Inventory.App.Administration.ServerLogs;

/// <summary>Where the server log is written and read, and for how long it is kept.</summary>
public sealed class ServerLogOptions
{
    public const string Section = "ServerLog";

    /// <summary>
    /// Who may read the log. Signed in, until roles exist; restricting it to one is this policy's
    /// definition, in the host, and nothing else.
    /// </summary>
    public const string ReadPolicy = "ServerLog";

    /// <summary>`server-yyyyMMdd.log`, and `server-yyyyMMdd_001.log` once a day passes the size cap.</summary>
    public const string FilePrefix = "server-";

    /// <summary>A folder, relative to the content root unless absolute.</summary>
    public string Path { get; set; } = "logs";

    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// The folder as an absolute path. Refused inside the web root: the files are read only through
    /// the API, and a folder the static-file middleware serves would hand them to anyone.
    /// </summary>
    public string ResolvePath(string contentRoot, string webRoot)
    {
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(contentRoot, Path));
        var root = System
            .IO.Path.GetFullPath(webRoot)
            .TrimEnd(System.IO.Path.DirectorySeparatorChar);

        if (
            path.Equals(root, StringComparison.Ordinal)
            || path.StartsWith(
                root + System.IO.Path.DirectorySeparatorChar,
                StringComparison.Ordinal
            )
        )
            throw new InvalidOperationException(
                $"{Section}:Path resolves to {path}, inside the web root {root}, where the static-file "
                    + "middleware would serve the log to anyone. Put it outside."
            );

        return path;
    }
}
