using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Codaxy.Inventory.App.Administration.ServerLogs;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// Log files dated in the past, so the application's own writes — today's file — cannot disturb them:
/// a local day that spans two UTC files and a size-rolled part, a line that does not parse, and text
/// that tries to be something other than text.
/// </summary>
public class ServerLogApplication : InventoryApplication
{
    // The seeded days are in January; the sink would delete them as past retention the moment it
    // opened today's file.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ServerLog:RetentionDays", "100000");

        // Request lines are on in Development only; this fixture reads its own, as Development would.
        builder.UseSetting("Logging:LogLevel:Microsoft.AspNetCore.HttpLogging", "Information");
    }

    public override async Task InitializeAsync()
    {
        Directory.CreateDirectory(ServerLogPath);

        await File.WriteAllLinesAsync(
            Path.Combine(ServerLogPath, "server-20260110.log"),
            [
                Line("2026-01-10T10:00:00Z", null, "Alpha started", "App.Startup"),
                Line("2026-01-10T10:00:01Z", "Warning", "Disk low", "App.Storage"),
                Line(
                    "2026-01-10T10:00:02Z",
                    "Error",
                    "Save failed",
                    "App.Storage",
                    exception: "System.Exception: boom\n   at App.Save()"
                ),
                "not json at all \u001b[31m red",
                Line("2026-01-10T23:30:00Z", null, "Late evening", "App.Clock"),
            ]
        );

        await File.WriteAllLinesAsync(
            Path.Combine(ServerLogPath, "server-20260111.log"),
            [
                Line("2026-01-11T00:30:00Z", null, "Just after midnight", "App.Clock"),
                Line(
                    "2026-01-11T00:40:00Z",
                    "Warning",
                    "fake\u202eevil\u001b[2J\nsecond line",
                    "App.Input"
                ),
                JsonSerializer.Serialize(
                    new Dictionary<string, object>
                    {
                        ["@t"] = "2026-01-11T00:50:00Z",
                        ["@m"] = "Request and Response:\nMethod: GET",
                        ["Method"] = "GET",
                        ["PathBase"] = "",
                        ["Path"] = "/api/things",
                        ["StatusCode"] = 200,
                        ["Duration"] = 5.4,
                        ["SourceContext"] =
                            "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware",
                    }
                ),
            ]
        );

        await File.WriteAllLinesAsync(
            Path.Combine(ServerLogPath, "server-20260111_001.log"),
            [Line("2026-01-11T01:00:00Z", null, "Second part", "App.Clock")]
        );

        // Not the sink's names: never read.
        await File.WriteAllTextAsync(Path.Combine(ServerLogPath, "notes.txt"), "ignored");
        await File.WriteAllTextAsync(Path.Combine(ServerLogPath, "server-latest.log"), "ignored");

        await base.InitializeAsync();
    }

    private static string Line(
        string time,
        string? level,
        string message,
        string category,
        string? exception = null
    )
    {
        var line = new Dictionary<string, string>
        {
            ["@t"] = time,
            ["@m"] = message,
            ["SourceContext"] = category,
        };

        if (level is not null)
            line["@l"] = level;

        if (exception is not null)
            line["@x"] = exception;

        return JsonSerializer.Serialize(line);
    }
}

public class ServerLogTests(ServerLogApplication app) : IClassFixture<ServerLogApplication>
{
    private const string Url = "/api/administration/server-log";

    /// <summary>10 January in UTC.</summary>
    private const string Tenth = "from=2026-01-10T00:00:00Z&to=2026-01-11T00:00:00Z";

    /// <summary>11 January in a UTC+2 timezone: it starts at 22:00 on the 10th.</summary>
    private const string EleventhEast = "from=2026-01-10T22:00:00Z&to=2026-01-11T22:00:00Z";

    private async Task<Page<LogEntry>> List(string query)
    {
        var client = await app.ClientAsync();
        return (await client.GetFromJsonAsync<Page<LogEntry>>($"{Url}/?{query}"))!;
    }

    [Fact]
    public async Task Refuses_a_caller_without_a_session()
    {
        var response = await app.CreateClient().GetAsync($"{Url}/days");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Days_are_the_dated_files_newest_first()
    {
        var client = await app.ClientAsync();
        var days = (
            await client.GetFromJsonAsync<App.Administration.ServerLogs.Days.Endpoint.Response>(
                $"{Url}/days"
            )
        )!.Days;

        var seeded = days.Where(d => d.Year == 2026 && d.Month == 1).ToList();
        Assert.Equal([new DateOnly(2026, 1, 11), new DateOnly(2026, 1, 10)], seeded);
    }

    [Fact]
    public async Task A_local_day_reads_across_the_files_it_spans()
    {
        var page = await List(EleventhEast + "&sort=time");

        Assert.Equal(
            ["Late evening", "Just after midnight", "Second part"],
            page.Items.Where(e => e.Category == "App.Clock").Select(e => e.Message)
        );
        Assert.DoesNotContain(page.Items, e => e.Message == "Alpha started");
    }

    [Fact]
    public async Task Lists_newest_first_unless_asked_for_oldest()
    {
        var newest = await List(Tenth);
        var oldest = await List(Tenth + "&sort=time");

        Assert.Equal("Late evening", newest.Items[0].Message);
        Assert.Equal("Alpha started", oldest.Items[0].Message);
    }

    [Fact]
    public async Task A_minimum_level_keeps_the_more_severe_and_every_unparsed_line()
    {
        var page = await List(Tenth + "&level=Warning");

        Assert.Equal(3, page.Total);
        Assert.Contains(page.Items, e => e.Raw);
        Assert.All(
            page.Items.Where(e => !e.Raw),
            e => Assert.Contains(e.Level, new[] { "Warning", "Error" })
        );
    }

    [Fact]
    public async Task Every_search_term_has_to_match_somewhere()
    {
        Assert.Equal("Disk low", Assert.Single((await List(Tenth + "&q=disk+LOW")).Items).Message);
        Assert.Equal("Save failed", Assert.Single((await List(Tenth + "&q=boom")).Items).Message);
        Assert.Equal(2, (await List(Tenth + "&q=app.storage")).Total);
    }

    [Fact]
    public async Task Pages_through_a_day_with_the_list_convention()
    {
        var page = await List(Tenth + "&sort=time&pageSize=2&page=2");

        Assert.Equal(5, page.Total);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal("Save failed", page.Items[0].Message);
    }

    [Fact]
    public async Task Text_that_would_act_is_shown_as_markers()
    {
        var entry = (await List(EleventhEast + "&q=fake")).Items.Single();

        Assert.Equal("fake⟨U+202E⟩evil⟨ESC⟩[2J\nsecond line", entry.Message);
    }

    [Fact]
    public async Task A_line_that_does_not_parse_is_shown_raw_where_it_was_written()
    {
        var entries = (await List(Tenth + "&sort=time")).Items.ToList();
        var raw = entries.Single(e => e.Raw);

        Assert.Equal("not json at all ⟨ESC⟩[31m red", raw.Message);
        Assert.Equal("Save failed", entries[entries.IndexOf(raw) - 1].Message);
    }

    [Fact]
    public async Task A_request_is_one_line()
    {
        var entry = (await List(EleventhEast + "&q=api/things")).Items.Single();

        Assert.Equal("GET /api/things → 200 · 5 ms", entry.Message);
    }

    [Fact]
    public async Task An_exception_travels_with_its_entry()
    {
        var entry = (await List(Tenth + "&level=Error")).Items.Single(e => !e.Raw);

        Assert.Equal("System.Exception: boom\n   at App.Save()", entry.Exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("from=2026-01-11T00:00:00Z&to=2026-01-10T00:00:00Z")]
    [InlineData("from=2026-01-01T00:00:00Z&to=2026-01-20T00:00:00Z")]
    [InlineData(Tenth + "&level=Loud")]
    [InlineData(Tenth + "&sort=level")]
    [InlineData(Tenth + "&pageSize=101")]
    public async Task Refuses_a_query_outside_the_convention(string query)
    {
        var client = await app.ClientAsync();
        var response = await client.GetAsync($"{Url}/?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_application_writes_its_own_requests_to_todays_file()
    {
        var client = await app.ClientAsync();
        await client.GetAsync($"{Url}/days");

        var now = DateTimeOffset.UtcNow;
        var from = Uri.EscapeDataString(now.AddHours(-1).ToString("O"));
        var to = Uri.EscapeDataString(now.AddHours(1).ToString("O"));

        var page = await List($"from={from}&to={to}&q=server-log/days");

        Assert.Contains(
            page.Items,
            e => e.Message.StartsWith("GET /api/administration/server-log/days → 200")
        );
    }
}

/// <summary>A file older than the retention, and one within it, before the application starts.</summary>
public class RetentionApplication : InventoryApplication
{
    public static string FileFor(int daysAgo) =>
        $"server-{DateTime.Now.AddDays(-daysAgo):yyyyMMdd}.log";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ServerLog:RetentionDays", "30");
    }

    public override async Task InitializeAsync()
    {
        Directory.CreateDirectory(ServerLogPath);

        foreach (var days in new[] { 40, 5 })
        {
            var file = Path.Combine(ServerLogPath, FileFor(days));
            await File.WriteAllTextAsync(file, "");
            File.SetLastWriteTime(file, DateTime.Now.AddDays(-days));
        }

        await base.InitializeAsync();
    }
}

public class ServerLogRetentionTests(RetentionApplication app) : IClassFixture<RetentionApplication>
{
    [Fact]
    public async Task A_day_past_the_retention_is_deleted_and_one_within_it_kept()
    {
        // The sink applies the retention when it opens today's file, on the first entry written.
        var client = await app.ClientAsync();
        await client.GetAsync("/api/administration/server-log/days");

        Assert.False(
            File.Exists(Path.Combine(app.ServerLogPath, RetentionApplication.FileFor(40)))
        );
        Assert.True(File.Exists(Path.Combine(app.ServerLogPath, RetentionApplication.FileFor(5))));
    }

    [Fact]
    public async Task Outside_development_a_request_leaves_no_line()
    {
        var client = await app.ClientAsync();
        await client.GetAsync("/api/administration/server-log/days");

        var today = Path.Combine(app.ServerLogPath, RetentionApplication.FileFor(0));

        Assert.DoesNotContain(
            "HttpLoggingMiddleware",
            File.Exists(today) ? await ReadSharedAsync(today) : ""
        );
    }

    private static async Task<string> ReadSharedAsync(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
