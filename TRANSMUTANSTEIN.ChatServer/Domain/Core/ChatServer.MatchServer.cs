namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     TCP server for handling match server connections.
///     Creates MatchServerChatSession instances for each incoming connection.
/// </summary>
public class MatchServerChatServer(IServiceProvider serviceProvider, IPAddress address, int port)
    : TCPServer(address, port)
{
    protected override TCPSession CreateSession()
    {
        return new ChatSession(this, serviceProvider);
    }

    protected override void OnStarted()
    {
        Log.Information("Chat Server Has Started Accepting Match Server Connections On {Address}:{Port}", Address,
            Port);

        base.OnStarted();
    }

    protected override void OnError(SocketError error)
    {
        Log.Error("Chat Server Has Encountered A Match Server Connection Socket Error: {SocketError}", error);

        base.OnError(error);
    }
}