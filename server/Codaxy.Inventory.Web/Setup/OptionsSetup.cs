using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Shared.Documents;
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
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();
        services
            .AddOptions<DocumentOptions>()
            .Bind(configuration.GetSection(DocumentOptions.Section))
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();

        return services;
    }
}
