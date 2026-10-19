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
        services
            .AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.Section))
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IEmailRelay, SmtpEmailRelay>();

        return services;
    }
}
