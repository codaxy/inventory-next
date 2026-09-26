using Codaxy.Inventory.Web.Auth;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Tests.Unit;

public class SignInPolicyTests
{
    private static SignInPolicy Policy(AuthOptions options) => new(Options.Create(options));

    [Theory]
    [InlineData("someone@codaxy.com")]
    [InlineData("someone@other.com")]
    public void Allows_an_address_in_any_of_several_configured_domains(string email)
    {
        var policy = Policy(new AuthOptions { AllowedDomains = ["codaxy.com", "other.com"] });

        Assert.True(policy.Evaluate(email).IsAllowed);
    }

    [Theory]
    [InlineData("@codaxy.com")]
    [InlineData(" Codaxy.com ")]
    public void Reads_a_configured_domain_as_it_is_written(string configured)
    {
        var policy = Policy(new AuthOptions { AllowedDomains = [configured] });

        Assert.True(policy.Evaluate("someone@codaxy.com").IsAllowed);
    }

    [Fact]
    public void Refuses_an_address_with_no_domain_at_all()
    {
        var policy = Policy(new AuthOptions { AllowedDomains = ["codaxy.com"] });

        Assert.False(policy.Evaluate("someone").IsAllowed);
    }

    [Fact]
    public void Allows_an_address_in_the_configured_domain()
    {
        var policy = Policy(new AuthOptions { AllowedDomains = ["codaxy.com"] });

        Assert.True(policy.Evaluate("someone@codaxy.com").IsAllowed);
    }

    [Fact]
    public void Refuses_an_address_outside_the_configured_domain()
    {
        var policy = Policy(new AuthOptions { AllowedDomains = ["codaxy.com"] });

        var result = policy.Evaluate("someone@example.com");

        Assert.False(result.IsAllowed);

        // Refused for its domain, without naming the one that would have worked.
        Assert.True(result.IsSayable);
        Assert.DoesNotContain("codaxy.com", result.Reason);
    }

    [Fact]
    public void Allows_any_domain_when_none_is_configured()
    {
        var policy = Policy(new AuthOptions());

        Assert.True(policy.Evaluate("someone@example.com").IsAllowed);
    }

    [Fact]
    public void Refuses_an_address_outside_a_non_empty_allow_list()
    {
        var policy = Policy(new AuthOptions { AllowedUsers = ["one@codaxy.com"] });

        Assert.False(policy.Evaluate("two@codaxy.com").IsAllowed);
        Assert.True(policy.Evaluate("one@codaxy.com").IsAllowed);
    }

    [Fact]
    public void Refuses_a_denied_address_even_when_it_is_also_allowed()
    {
        var policy = Policy(
            new AuthOptions { AllowedUsers = ["one@codaxy.com"], DeniedUsers = ["one@codaxy.com"] }
        );

        Assert.False(policy.Evaluate("one@codaxy.com").IsAllowed);
    }

    [Theory]
    [InlineData("SOMEONE@CODAXY.COM")]
    [InlineData(" someone@codaxy.com ")]
    public void Ignores_case_and_surrounding_space(string email)
    {
        var policy = Policy(
            new AuthOptions
            {
                AllowedDomains = ["codaxy.com"],
                AllowedUsers = ["someone@codaxy.com"],
            }
        );

        Assert.True(policy.Evaluate(email).IsAllowed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuses_an_account_with_no_address(string? email)
    {
        Assert.False(Policy(new AuthOptions()).Evaluate(email).IsAllowed);
    }
}
