using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Shared.Links;
using Codaxy.Inventory.Web.Auth;

namespace Codaxy.Inventory.Web.Setup;

public static class OptionsSetup
{
    public static IServiceCollection AddInventoryOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Top-level keys: what belongs to the deployment as a whole, read by more than one area.
        services
            .AddOptions<ApplicationOptions>()
            .Bind(configuration)
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();
        services.AddOptions<AuthOptions>().Bind(configuration.GetSection(AuthOptions.Section));
        services
            .AddOptions<UserOptions>()
            .Bind(configuration.GetSection(UserOptions.Section))
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();

        return services;
    }
}
