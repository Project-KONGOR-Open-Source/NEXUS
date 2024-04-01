namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server) : TCPSession(server)
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

        // Temporarily Force Accepting The Connection, Until Implemented Properly
        // "\0" is the NULL character, which has the value 0 in the ASCII table and is used to determine the end of C-style strings (also known as the NULL Terminator)
        if (message.EndsWith("\0en\0en\0"))
        {
            //ChatBuffer response = new();
            //const short accept = 0x1C00;
            //response.WriteInt16(accept);
            //SendAsync(response.Buffer);

            TCPBuffer response = new();
            const short accept = 0x1C00;
            response.Append(BitConverter.GetBytes(accept));
            SendAsync(response.Data);
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Session ID {Id} Caught A Socket Error With Code {error}");
    }
}
