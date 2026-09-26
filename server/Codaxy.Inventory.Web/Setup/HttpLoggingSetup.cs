using Microsoft.AspNetCore.HttpLogging;

namespace Codaxy.Inventory.Web.Setup;

public static class HttpLoggingSetup
{
    /// <summary>
    /// One line per request, with no bodies and no headers: a request body here is a sign-in
    /// attempt, and the point of logging is to see what was asked for, not what was in it.
    /// </summary>
    public static IServiceCollection AddInventoryHttpLogging(this IServiceCollection services) =>
        services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });
}
