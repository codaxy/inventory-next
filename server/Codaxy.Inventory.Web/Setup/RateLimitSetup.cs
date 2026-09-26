using System.Threading.RateLimiting;
using Codaxy.Inventory.Web.Auth;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Setup;

public static class RateLimitSetup
{
    public static IServiceCollection AddInventoryRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.Section));

        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Partitioned by caller address, so one caller exhausting its permits cannot lock anyone
            // else out. Behind a proxy that is the forwarded address, which the image enables.
            options.AddPolicy(
                AuthEndpoints.SignInRateLimit,
                context =>
                {
                    var limits = context
                        .RequestServices.GetRequiredService<IOptions<RateLimitOptions>>()
                        .Value;

                    return RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limits.Permits,
                            Window = limits.Window,
                        }
                    );
                }
            );

            options.OnRejected = (context, cancellation) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = (
                        (int)retryAfter.TotalSeconds
                    ).ToString();

                return ValueTask.CompletedTask;
            };
        });
    }
}
