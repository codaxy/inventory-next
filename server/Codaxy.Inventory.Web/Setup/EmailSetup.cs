using Codaxy.Inventory.App.Shared.Email;
using Codaxy.Inventory.Web.Email;

namespace Codaxy.Inventory.Web.Setup;

public static class EmailSetup
{
    public static IServiceCollection AddInventoryEmail(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection(SmtpOptions.Section));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
