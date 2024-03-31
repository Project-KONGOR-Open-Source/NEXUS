namespace TRANSMUTANSTEIN.ChatServer.ChatServer.Core;

public class ChatSession(TcpServer server) : TcpSession(server)
{
    protected override void OnConnected()
    {
        Console.WriteLine($"Chat Session ID {Id} Was Created");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat Session ID {Id} Has Terminated");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        Console.WriteLine($"Incoming: {message}");

        // Multicast Message To All Connected Sessions
        Server.Multicast(message);

        // If The Buffer Starts With "!disconnect", Then Terminate The Current Session
        if (message == "!disconnect") Disconnect();
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Session ID {Id} Caught A Socket Error With Code {error}");
    }
}
