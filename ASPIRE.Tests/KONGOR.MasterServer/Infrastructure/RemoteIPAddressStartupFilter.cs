using System.Net;

namespace ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;

/// <summary>
///     Startup Filter To Set Fake Remote IP Address
/// </summary>
public class RemoteIPAddressStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(next => async context =>
            {
                context.Connection.RemoteIpAddress = IPAddress.Loopback;

                await next(context);
            });

            next(app);
        };
    }
}