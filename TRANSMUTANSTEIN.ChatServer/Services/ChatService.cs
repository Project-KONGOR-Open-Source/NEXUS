namespace TRANSMUTANSTEIN.ChatServer.Services;

public class ChatService : IChatService
{
    private Core.ChatServer? ChatServer { get; set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IPAddress address = IPAddress.Any;
        int port = 55508; // TODO: Get From Configuration

        ChatServer = new Core.ChatServer(address, port);

        if (ChatServer.Start() is false)
        {
            // TODO: Log Critical Event

            return Task.FromException(new ApplicationException("Chat Server Was Unable To Start"));
        }

        Console.WriteLine($"Chat Server Listening On {ChatServer.Endpoint}");

        while (ChatServer.IsStarted)
        {
            // keep listening
        }

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
