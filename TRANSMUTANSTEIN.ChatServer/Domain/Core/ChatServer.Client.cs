namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     TCP server for handling client connections.
///     Creates ClientChatSession instances for each incoming connection.
/// </summary>
public class ClientChatServer(IServiceProvider serviceProvider, IPAddress address, int port) : TCPServer(address, port)
{
    protected override TCPSession CreateSession()
    {
        return new ChatSession(this, serviceProvider);
    }

    protected override void OnStarted()
    {
        Log.Information("Chat Server Has Started Accepting Client Connections On {Address}:{Port}", Address, Port);

        base.OnStarted();
    }

    protected override void OnError(SocketError error)
    {
        Log.Error("Chat Server Has Encountered A Client Connection Socket Error: {SocketError}", error);

        base.OnError(error);
    }
}
