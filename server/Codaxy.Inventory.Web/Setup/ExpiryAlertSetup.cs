using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Codaxy.Inventory.Web.ExpiryAlerts;

namespace Codaxy.Inventory.Web.Setup;

public static class ExpiryAlertSetup
{
    public static IServiceCollection AddInventoryExpiryAlerts(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<ExpiryAlertOptions>()
            .Bind(configuration.GetSection(ExpiryAlertOptions.Section))
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();
        services.AddScoped<ExpiryAlertCheck>();
        services.AddHostedService<ExpiryAlertWorker>();

        return services;
    }
}
