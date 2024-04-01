namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatServer(IPAddress address, int port) : TCPServer(address, port)
{
    protected override TCPSession CreateSession() => new ChatSession(this);

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Server Caught A Socket Error With Code {error}");
    }
}
