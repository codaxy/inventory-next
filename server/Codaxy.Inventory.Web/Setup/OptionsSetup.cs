using Codaxy.Inventory.Web.Auth;

namespace Codaxy.Inventory.Web.Setup;

public static class OptionsSetup
{
    public static IServiceCollection AddInventoryOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions<AuthOptions>().Bind(configuration.GetSection(AuthOptions.Section));
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection(SmtpOptions.Section));

        return services;
    }
}
