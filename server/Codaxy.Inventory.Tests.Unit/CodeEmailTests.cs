using Codaxy.Inventory.Web.Auth.OneTimeCodes.Request;

namespace Codaxy.Inventory.Tests.Unit;

public class CodeEmailTests
{
    private static CodeEmail Compose(string? home = null) =>
        CodeEmails.Compose("042917", TimeSpan.FromMinutes(10), home);

    [Fact]
    public void The_text_gives_the_code_and_how_long_it_lasts() =>
        Assert.StartsWith(
            "Your sign-in code is 042917. It is valid for 10 minutes.\n",
            Compose().Text
        );

    [Fact]
    public void The_html_is_the_alerts_layout_with_the_code_set_large()
    {
        var html = Compose().Html;

        Assert.Contains(">Your sign-in code</h1>", html);
        Assert.Contains("font-size:30px;", html);
        Assert.Contains(">042917</p>", html);
        Assert.Contains("It is valid for 10 minutes.", html);
        Assert.Contains("@media (prefers-color-scheme: dark)", html);
        Assert.Contains(">Sent by Inventory</td>", html);
    }

    [Fact]
    public void Links_the_application_where_its_address_is_known() =>
        Assert.Contains(
            "<a href=\"https://inventory.codaxy.com/\">Inventory</a>",
            Compose("https://inventory.codaxy.com/").Html
        );

    [Fact]
    public void Keeps_the_subject() =>
        Assert.Equal("Your Inventory sign-in code", Compose().Subject);
}
