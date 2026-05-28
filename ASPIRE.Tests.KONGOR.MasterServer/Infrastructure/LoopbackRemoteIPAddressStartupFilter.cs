namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Startup filter that fixes <see cref="ConnectionInfo.RemoteIpAddress"/> to the loopback address before any downstream middleware executes.
///     <see cref="WebApplicationFactory{TEntryPoint}"/> uses a synthetic transport that does not populate the remote IP, so code paths that rely on it (forwarded-headers middleware, audit logging, rate limiting) would otherwise observe <see langword="null"/>.
/// </summary>
internal sealed class LoopbackRemoteIPAddressStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return applicationBuilder =>
        {
            applicationBuilder.Use(async (context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Loopback;

                await nextMiddleware(context);
            });

            next(applicationBuilder);
        };
    }
}
