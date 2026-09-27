using Codaxy.Inventory.App.Dashboard;
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
        services
            .AddOptions<DashboardOptions>()
            .Bind(configuration.GetSection(DashboardOptions.Section))
            .Validate(o => o.IsValid, "Every Dashboard:*Days must be at least 1.")
            .ValidateOnStart();

        return services;
    }
}
