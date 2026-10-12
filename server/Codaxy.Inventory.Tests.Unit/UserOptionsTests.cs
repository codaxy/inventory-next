using Codaxy.Inventory.App.Administration.Users;

namespace Codaxy.Inventory.Tests.Unit;

public class UserOptionsTests
{
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
