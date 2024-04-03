namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService : IHostedService, IDisposable
{
    public static Core.ChatServer? ChatServer { get; set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IPAddress address = IPAddress.Any;

        // TODO: Get From Configuration
        // TODO: Make Distinction Between Chat Service (55507, 55508) Ports And Chat Server Ports (5555x > 1: Client, 2: Manager, 3: Server)
        int port = 55551;

        ChatServer = new Core.ChatServer(address, port);

        if (ChatServer.Start() is false)
        {
            // TODO: Log Critical Event

            return Task.FromException(new ApplicationException("Chat Server Was Unable To Start"));
        }

        Console.WriteLine($"Chat Server Listening On {ChatServer.Endpoint}");

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

        Console.WriteLine("Chat Server Has Stopped");

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
