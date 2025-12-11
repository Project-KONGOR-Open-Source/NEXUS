namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    public static Domain.Core.ChatServer? ChatServer { get; set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Initialise(serviceProvider.GetRequiredService<ILogger<Log>>());

        IPAddress address = IPAddress.Any;

        int clientConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("TCP_SERVER_PORT_CLIENT")
            ?? throw new NullReferenceException("TCP Server Port For Client Connections Is NULL"));

        int matchServerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("TCP_SERVER_PORT_MATCH_SERVER")
            ?? throw new NullReferenceException("TCP Server Port For Match Server Connections Is NULL"));

        int matchServerManagerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("TCP_SERVER_PORT_MATCH_SERVER_MANAGER")
            ?? throw new NullReferenceException("TCP Server Port For Match Server Manager Connections Is NULL"));

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
