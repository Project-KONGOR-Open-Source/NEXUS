using Microsoft.AspNetCore.OutputCaching;

namespace ZORGATH.WebPortal.API.Infrastructure;

/// <summary>
///     Explicitly Allows Output Caching For Authenticated Requests By Enforcing "Vary By Authorization Header"
///     And Overriding The Default "Prevent Caching If Authenticated" Behavior
/// </summary>
public sealed class AllowAuthenticatedOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        // 1. Force Enable Caching (Overrides Default Authenticated check)
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;



        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        HttpResponse response = context.HttpContext.Response;

        // Ensure we only cache successful responses
        if (response.StatusCode != StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
