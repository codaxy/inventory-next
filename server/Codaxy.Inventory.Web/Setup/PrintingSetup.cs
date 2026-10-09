using Codaxy.Inventory.App.Shared.Printing;
using Codaxy.Inventory.Web.Printing;

namespace Codaxy.Inventory.Web.Setup;

public static class PrintingSetup
{
    /// <summary>
    /// The page printer, registered only when a browser is configured: without one, the endpoints
    /// that answer with a PDF say they do not exist rather than fail at the browser.
    /// </summary>
    public static IServiceCollection AddInventoryPrinting(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection(PrintingOptions.Section);
        // A browser that is configured and missing stops the start, naming the setting, rather
        // than failing at the first print.
        services
            .AddOptions<PrintingOptions>()
            .Bind(section)
            .ValidateBy(o => o.Problem)
            .ValidateOnStart();

        if (section.Get<PrintingOptions>() is { Enabled: true })
            services.AddSingleton<IPagePrinter, ChromiumPagePrinter>();

        return services;
    }
}
