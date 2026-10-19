using Codaxy.Inventory.Web.Email;

namespace Codaxy.Inventory.Tests.Unit;

public class SmtpOptionsTests
{
    [Fact]
    public void An_account_needs_its_password() =>
        Assert.Equal(
            "Smtp:Password is required with Smtp:UserName.",
            new SmtpOptions { UserName = "noreply@codaxy.com" }.Problem
        );

    [Fact]
    public void No_account_and_an_account_with_its_password_are_both_fine()
    {
        Assert.Null(new SmtpOptions().Problem);
        Assert.Null(new SmtpOptions { UserName = "noreply@codaxy.com", Password = "x" }.Problem);
    }
}
