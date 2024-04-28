namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatServer(IPAddress address, int port, IServiceProvider serviceProvider) : TCPServer(address, port)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatServer>>();

    protected override TCPSession CreateSession() => new ChatSession(this, ServiceProvider);

    protected override void OnError(SocketError error)
    {
        Logger.LogError($"Chat Server Caught A Socket Error With Code {error}");
    }
}
