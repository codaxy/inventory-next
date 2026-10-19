using Codaxy.Inventory.App.Shared.Email;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Email;

public sealed class SmtpEmailRelay(IOptions<SmtpOptions> options) : IEmailRelay
{
    public EmailRelay Describe()
    {
        var smtp = options.Value;
        return new(smtp.Host, smtp.Port, smtp.From, smtp.FromName, smtp.UserName);
    }
}
