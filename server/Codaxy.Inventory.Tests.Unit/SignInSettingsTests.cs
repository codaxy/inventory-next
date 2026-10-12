using System.Text.Json;
using Codaxy.Inventory.App.Shared.Access;

namespace Codaxy.Inventory.Tests.Unit;

public class SignInSettingsTests
{
    [Theory]
    [InlineData("someone@codaxy.com")]
    [InlineData("someone@other.com")]
    public void Allows_an_address_in_any_of_several_domains(string email) =>
        Assert.True(SignInSettings.Allows(["codaxy.com", "other.com"], email));

    [Fact]
    public void Refuses_an_address_outside_them() =>
        Assert.False(SignInSettings.Allows(["codaxy.com"], "someone@example.com"));

    [Fact]
    public void Refuses_an_address_with_no_domain_at_all() =>
        Assert.False(SignInSettings.Allows(["codaxy.com"], "someone"));

    [Fact]
    public void Allows_any_domain_when_there_are_none() =>
        Assert.True(SignInSettings.Allows([], "someone@example.com"));

    [Fact]
    public void Ignores_the_case_of_the_address() =>
        Assert.True(SignInSettings.Allows(["codaxy.com"], "SOMEONE@CODAXY.COM"));

    [Fact]
    public void Saving_writes_each_domain_as_an_address_does_once() =>
        Assert.Equal(
            ("[\"codaxy.com\",\"other.com\"]", null),
            Accept(" @Codaxy.com ", "codaxy.com", "", "other.com")
        );

    [Theory]
    [InlineData("codaxy")]
    [InlineData("codaxy..com")]
    [InlineData("someone@codaxy.com")]
    [InlineData("-codaxy.com")]
    public void Refuses_what_is_not_a_domain(string domain) =>
        Assert.Equal((null, $"\"{domain}\" is not a domain."), Accept(domain));

    [Fact]
    public void An_empty_list_may_be_saved() => Assert.Equal(("[]", null), Accept());

    private static (string?, string?) Accept(params string[] domains) =>
        SignInSettings.AllowedDomains.Accept(JsonSerializer.SerializeToElement(domains));
}
