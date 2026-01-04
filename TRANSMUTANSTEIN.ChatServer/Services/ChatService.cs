namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    public static Domain.Core.ChatServer? ChatServer { get; set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Initialise(serviceProvider.GetRequiredService<ILogger<Log>>());

        IPAddress address = IPAddress.Any;

        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

        int clientConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_CLIENT");
        if (clientConnectionsPort == 0) throw new InvalidOperationException("Chat Server Port For Client Connections Is Not Configured");

        int matchServerConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_MATCH_SERVER");
        if (matchServerConnectionsPort == 0) throw new InvalidOperationException("Chat Server Port For Match Server Connections Is Not Configured");

        int matchServerManagerConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER");
        if (matchServerManagerConnectionsPort == 0) throw new InvalidOperationException("Chat Server Port For Match Server Manager Connections Is Not Configured");

        ChatServer = new Domain.Core.ChatServer(serviceProvider, address, clientConnectionsPort, matchServerConnectionsPort, matchServerManagerConnectionsPort);

        ChatServer.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (ChatServer is null)
        {
            // TODO: Log Bug

            return Task.FromException(new ApplicationException("Chat Server Is NULL"));
        }

        if (ChatServer.IsStarted)
        {
            ChatServer.DisconnectAll();
            ChatServer.Stop();
        }

        else
        {
            // TODO: Log Bug

            return Task.FromException(new ApplicationException("Chat Server Is Not Running"));
        }

        Log.Information("Chat Server Has Stopped");

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (ChatServer is null)
        {
            // TODO: Log Bug

            throw new ApplicationException("Chat Server Is NULL");
        }

        if (ChatServer.IsDisposed)
        {
            // TODO: Log Bug

            throw new ApplicationException("Chat Server Is Already Disposed");
        }

        ChatServer.Dispose();
    }
}
