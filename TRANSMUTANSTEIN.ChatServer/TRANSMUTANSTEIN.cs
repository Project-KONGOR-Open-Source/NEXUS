using System.Net;

using TRANSMUTANSTEIN.ChatServer.Hubs;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();
        builder.Services.AddHostedService<TCPListenerService>();

        var app = builder.Build();

        app.UseRouting();
        app.MapHub<ChatHub>("0.0.0.0:11031");

        app.Run();




        //var config = new ConfigurationBuilder()
        //    .AddCommandLine(args)
        //    .Build();

        //var host = new WebHostBuilder()
        //    .UseConfiguration(config)
        //    .UseSetting(WebHostDefaults.PreventHostingStartupKey, "true")
        //    .ConfigureLogging(factory =>
        //    {
        //        factory.AddConsole();
        //    })
        //    .UseKestrel(options =>
        //    {
        //        // Default port
        //        options.ListenLocalhost(5000);

        //        // Hub bound to TCP end point
        //        options.Listen(IPAddress.Any, 9001, builder =>
        //        {
        //            builder.UseHub<Chat>();
        //        });
        //    })
        //    .UseContentRoot(Directory.GetCurrentDirectory())
        //    .UseIISIntegration()
        //    .UseStartup<Startup>()
        //    .Build();

        //host.Run();
    }
}
