namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ChatServer(IPAddress address, int port, IServiceProvider serviceProvider) : TCPServer(address, port)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    protected override TCPSession CreateSession() => new ChatSession(this, ServiceProvider);

    protected override void OnError(SocketError error)
    {
        Log.Error($"Chat Server Caught A Socket Error With Code {error}");
    }
}
