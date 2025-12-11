namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     TCP server for handling match server manager connections.
///     Creates MatchServerManagerChatSession instances for each incoming connection.
/// </summary>
public class MatchServerManagerChatServer(IServiceProvider serviceProvider, IPAddress address, int port) : TCPServer(address, port)
{
    protected override TCPSession CreateSession()
    {
        return new MatchServerManagerChatSession(this, serviceProvider);
    }

    protected override void OnStarted()
    {
        Log.Information("Chat Server Has Started Accepting Match Server Manager Connections On {Address}:{Port}", Address, Port);

        base.OnStarted();
    }

    protected override void OnError(SocketError error)
    {
        Log.Error("Chat Server Has Encountered A Match Server Manager Connection Socket Error: {SocketError}", error);

        base.OnError(error);
    }
}
