namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    public Domain.Core.ChatServer? ChatServer { get; set; }

    public void Dispose()
    {
        if (ChatServer is null)
        {
            Log.Error(
                "Chat Server Is NULL During Disposal (Attempting To Dispose Before Start or After Startup Failure?)");
            return;
        }

        if (ChatServer.IsDisposed)
        {
            return;
        }

        ChatServer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Initialise(serviceProvider.GetRequiredService<ILogger<Log>>());

        IPAddress address = IPAddress.Any;

        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

        int clientConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_CLIENT");

        int matchServerConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_MATCH_SERVER");

        int matchServerManagerConnectionsPort = configuration.GetValue<int>("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER");

        ChatServer = new Domain.Core.ChatServer(serviceProvider, address, clientConnectionsPort,
            matchServerConnectionsPort, matchServerManagerConnectionsPort);

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
            Log.Warning("Chat Server Is Not Running (Already Stopped?)");
            return Task.CompletedTask;
        }

        Log.Information("Chat Server Has Stopped");

        return Task.CompletedTask;
    }
}