namespace Codaxy.Inventory.App.Administration.ServerLogs.Days;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder serverLog) => serverLog.MapGet("/days", Handle);

    /// <summary>`YYYY-MM-DD`, newest first: the days that have a file, as the server dated them.</summary>
    public sealed record Response(IReadOnlyList<DateOnly> Days);

    private static Response Handle(ServerLogFolder folder) => new(LogFiles.Days(folder.Path));
}
