namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatService>>();

    public static Core.ChatServer? ChatServer { get; set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IPAddress address = IPAddress.Any;

        int port = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT") ?? throw new NullReferenceException("Chat Server Port Is NULL"));

        ChatServer = new Core.ChatServer(address, port, serviceProvider);

        if (ChatServer.Start() is false)
        {
            // TODO: Log Critical Event

            return Task.FromException(new ApplicationException("Chat Server Was Unable To Start"));
        }

        Logger.LogInformation($"Chat Server Listening On {ChatServer.Endpoint}");

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

        Logger.LogInformation("Chat Server Has Stopped");

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
