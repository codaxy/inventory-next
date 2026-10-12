using Codaxy.Inventory.App.Administration.Users;

namespace Codaxy.Inventory.Tests.Unit;

public class UserOptionsTests
{
    [Fact]
    public void Admits_an_allowed_address_whatever_its_case_and_spacing()
    {
        var options = new UserOptions { AllowedUsers = [" One@Codaxy.com "] };

        Assert.True(options.Admits("one@codaxy.com"));
        Assert.True(options.Admits("ONE@CODAXY.COM "));
        Assert.False(options.Admits("two@codaxy.com"));
    }

    [Fact]
    public void Admits_nobody_when_the_list_is_empty()
    {
        Assert.False(new UserOptions().Admits("anyone@codaxy.com"));
    }

    [Fact]
    public void A_denied_address_is_not_admitted_even_when_allowed()
    {
        var options = new UserOptions
        {
            AllowedUsers = ["one@codaxy.com", "two@codaxy.com"],
            DeniedUsers = ["ONE@codaxy.com"],
        };

        Assert.False(options.Admits("one@codaxy.com"));
        Assert.Equal(["two@codaxy.com"], options.Admitted());
    }

    [Fact]
    public void A_seed_user_without_a_role_names_itself()
    {
        var options = new UserOptions
        {
            SeedUsers = [new() { Email = "admin@codaxy.com", Name = "Admin" }],
        };

        Assert.Contains("admin@codaxy.com", options.Problem);
        Assert.Null(new UserOptions().Problem);
    }
}
