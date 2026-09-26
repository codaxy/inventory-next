using Codaxy.Inventory.App.Administration.ServerLogs;

namespace Codaxy.Inventory.Tests.Unit;

public class ServerLogTests
{
    [Theory]
    [InlineData("plain text", "plain text")]
    [InlineData("tab\tand\nnewline", "tab\tand\nnewline")]
    [InlineData("\u001b[31mred", "⟨ESC⟩[31mred")]
    [InlineData("carriage\rreturn", "carriage⟨CR⟩return")]
    [InlineData("bell\u0007", "bell⟨U+0007⟩")]
    [InlineData("c1\u009b", "c1⟨U+009B⟩")]
    [InlineData("rtl\u202eevil", "rtl⟨U+202E⟩evil")]
    [InlineData("isolate\u2066x\u2069", "isolate⟨U+2066⟩x⟨U+2069⟩")]
    [InlineData("line\u2028sep", "line⟨U+2028⟩sep")]
    [InlineData("zero\u200bwidth", "zero⟨U+200B⟩width")]
    [InlineData("čćžšđ ✓ 😀", "čćžšđ ✓ 😀")]
    public void Displayable_text_marks_what_would_act(string input, string expected)
    {
        Assert.Equal(expected, LogText.Displayable(input));
    }

    [Fact]
    public void Nothing_displays_as_empty()
    {
        Assert.Equal("", LogText.Displayable(null));
    }

    [Fact]
    public void A_relative_path_resolves_under_the_content_root()
    {
        var root = Path.Combine(Path.GetTempPath(), "app");

        Assert.Equal(
            Path.Combine(root, "logs"),
            new ServerLogOptions().ResolvePath(root, Path.Combine(root, "wwwroot"))
        );
    }

    [Theory]
    [InlineData("wwwroot")]
    [InlineData("wwwroot/logs")]
    [InlineData("logs/../wwwroot/x")]
    public void A_path_inside_the_web_root_is_refused(string path)
    {
        var root = Path.Combine(Path.GetTempPath(), "app");

        Assert.Throws<InvalidOperationException>(() =>
            new ServerLogOptions { Path = path }.ResolvePath(root, Path.Combine(root, "wwwroot"))
        );
    }

    [Fact]
    public void A_sibling_that_only_starts_like_the_web_root_is_allowed()
    {
        var root = Path.Combine(Path.GetTempPath(), "app");

        Assert.EndsWith(
            "wwwroot-logs",
            new ServerLogOptions { Path = "wwwroot-logs" }.ResolvePath(
                root,
                Path.Combine(root, "wwwroot")
            )
        );
    }

    [Fact]
    public void A_line_without_a_level_is_information()
    {
        var entry = LogFiles.Parse(
            """{"@t":"2026-01-10T10:00:00Z","@m":"hi"}""",
            DateTimeOffset.MinValue
        );

        Assert.Equal(("Information", "hi", false), (entry.Level, entry.Message, entry.Raw));
    }

    [Fact]
    public void A_line_without_a_time_is_raw_and_keeps_the_previous_time()
    {
        var previous = new DateTimeOffset(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);
        var entry = LogFiles.Parse("""{"@m":"no time"}""", previous);

        Assert.True(entry.Raw);
        Assert.Equal(previous, entry.Time);
    }
}
