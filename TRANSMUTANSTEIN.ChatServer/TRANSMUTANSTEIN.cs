using System.Net;

using Microsoft.AspNetCore.SignalR;

using TRANSMUTANSTEIN.ChatServer.Hubs;

namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();

        builder.WebHost.UseKestrel(options =>
        {
            options.Listen(IPAddress.Any, 55507 /* TODO: Get From Configuration */, configure => { configure.UseHub<Chat>(); });
            options.Listen(IPAddress.Any, 55508 /* TODO: Get From Configuration */, configure => { configure.UseHub<Chat>(); configure.UseHttps(); });
        });

        var app = builder.Build();

        app.MapHub<ChatHub>(string.Empty);

        app.Run();
    }
}
