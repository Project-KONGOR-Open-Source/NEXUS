using System.Net;

using Microsoft.AspNetCore.SignalR;

using TRANSMUTANSTEIN.ChatServer.Hubs;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseConfiguration(config)
                    .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
                    .ConfigureLogging((c, factory) =>
                    {
                        factory.AddConfiguration(c.Configuration.GetSection("Logging"));
                        factory.AddConsole();
                        factory.SetMinimumLevel(LogLevel.Debug);
                    })
                    .UseKestrel(options =>
                    {
                        // Default port
                        // options.ListenAnyIP(11031);

                        // Hub bound to TCP end point
                        options.Listen(IPAddress.Any, 11031, builder =>
                        {
                            builder.UseHub<Chat>();
                        });
                    })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>();
            }).Build();

        return host.RunAsync();
    }
}
