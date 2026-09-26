using Microsoft.Net.Http.Headers;

namespace Codaxy.Inventory.Web.Setup;

/// <summary>
/// What a browser may keep, said on every response rather than left to its heuristics: served with
/// only a <c>Last-Modified</c>, a response is cached for a guessed lifetime, and a browser then goes on
/// answering an API URL with whatever it once got there — the shell, say, from a build that lacked the
/// route — until the cache is emptied by hand.
/// </summary>
public static class CachingSetup
{
    /// <summary>
    /// The shell and anything else static is revalidated on every load, so a deploy takes effect at
    /// once; the hashed bundles under <c>/assets</c> change name when they change content, so they are
    /// kept for a year.
    /// </summary>
    public static readonly StaticFileOptions StaticFiles = new()
    {
        OnPrepareResponse = context =>
            context.Context.Response.Headers[HeaderNames.CacheControl] =
                context.Context.Request.Path.StartsWithSegments("/assets")
                    ? "public, max-age=31536000, immutable"
                    : "no-cache",
    };

    /// <summary>Every <c>/api</c> response, errors and rate-limit refusals included, is never stored.</summary>
    public static IApplicationBuilder UseNoStoreForApi(this IApplicationBuilder app) =>
        app.Use(
            (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                    context.Response.OnStarting(() =>
                    {
                        context.Response.Headers[HeaderNames.CacheControl] = "no-store";
                        return Task.CompletedTask;
                    });

                return next();
            }
        );
}
