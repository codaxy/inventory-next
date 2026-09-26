using Codaxy.Inventory.Web.Auth;
using Codaxy.Inventory.Web.Auth.OneTimeCodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Codaxy.Inventory.Tests.Unit;

public class OneTimeCodeStoreTests
{
    private readonly FakeTimeProvider time = new();

    private InMemoryOneTimeCodeStore Store(TimeSpan? validity = null, TimeSpan? cooldown = null) =>
        new(
            Options.Create(
                new AuthOptions
                {
                    OneTimeCode = new AuthOptions.OneTimeCodeOptions
                    {
                        Enabled = true,
                        Validity = validity ?? TimeSpan.FromMinutes(10),
                        Cooldown = cooldown ?? TimeSpan.FromMinutes(1),
                    },
                }
            ),
            time,
            NullLogger<InMemoryOneTimeCodeStore>.Instance
        );

    [Fact]
    public void Issues_six_digits()
    {
        var code = Store().Issue("someone@codaxy.com");

        Assert.Matches("^[0-9]{6}$", code);
    }

    [Fact]
    public void Accepts_the_code_it_issued()
    {
        var store = Store();
        var code = store.Issue("someone@codaxy.com");

        Assert.True(store.Consume("someone@codaxy.com", code));
    }

    [Fact]
    public void Accepts_the_code_whatever_the_case_of_the_address()
    {
        var store = Store();
        var code = store.Issue("someone@codaxy.com");

        Assert.True(store.Consume("SOMEONE@CODAXY.COM", code));
    }

    [Fact]
    public void Refuses_a_code_twice()
    {
        var store = Store();
        var code = store.Issue("someone@codaxy.com");

        Assert.True(store.Consume("someone@codaxy.com", code));
        Assert.False(store.Consume("someone@codaxy.com", code));
    }

    [Fact]
    public void Refuses_the_correct_code_after_a_wrong_attempt()
    {
        var store = Store();
        var code = store.Issue("someone@codaxy.com");

        Assert.False(store.Consume("someone@codaxy.com", "000000"));
        Assert.False(store.Consume("someone@codaxy.com", code));
    }

    [Fact]
    public void Refuses_a_code_for_an_address_that_asked_for_none()
    {
        Assert.False(Store().Consume("someone@codaxy.com", "123456"));
    }

    [Fact]
    public void Refuses_a_code_once_it_has_expired()
    {
        var store = Store(TimeSpan.FromMinutes(10));
        var code = store.Issue("someone@codaxy.com");

        time.Advance(TimeSpan.FromMinutes(10));

        Assert.False(store.Consume("someone@codaxy.com", code));
    }

    [Fact]
    public void Keeps_a_code_until_the_instant_it_expires()
    {
        var store = Store(TimeSpan.FromMinutes(10));
        var code = store.Issue("someone@codaxy.com");

        time.Advance(TimeSpan.FromMinutes(9));

        Assert.True(store.Consume("someone@codaxy.com", code));
    }

    [Fact]
    public void Replaces_an_outstanding_code_when_a_new_one_is_asked_for()
    {
        var store = Store(cooldown: TimeSpan.FromMinutes(1));

        store.Issue("someone@codaxy.com");
        time.Advance(TimeSpan.FromMinutes(1));
        var second = store.Issue("someone@codaxy.com");

        Assert.NotNull(second);
        Assert.True(store.Consume("someone@codaxy.com", second));
    }

    [Fact]
    public void Issues_nothing_while_the_address_is_within_its_cooldown()
    {
        var store = Store(cooldown: TimeSpan.FromMinutes(1));

        Assert.NotNull(store.Issue("someone@codaxy.com"));

        time.Advance(TimeSpan.FromSeconds(59));

        Assert.Null(store.Issue("someone@codaxy.com"));
    }

    [Fact]
    public void Issues_again_once_the_cooldown_has_passed()
    {
        var store = Store(cooldown: TimeSpan.FromMinutes(1));

        store.Issue("someone@codaxy.com");
        time.Advance(TimeSpan.FromMinutes(1));

        Assert.NotNull(store.Issue("someone@codaxy.com"));
    }

    [Fact]
    public void Holds_the_cooldown_per_address()
    {
        var store = Store(cooldown: TimeSpan.FromMinutes(1));

        store.Issue("one@codaxy.com");

        Assert.NotNull(store.Issue("two@codaxy.com"));
    }

    [Fact]
    public void Leaves_a_consumed_code_no_cooldown_behind()
    {
        var store = Store(cooldown: TimeSpan.FromMinutes(1));
        var code = store.Issue("someone@codaxy.com");

        Assert.True(store.Consume("someone@codaxy.com", code!));

        // Signing in and out again must not wait a minute for a new code.
        Assert.NotNull(store.Issue("someone@codaxy.com"));
    }
}
