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

        Console.WriteLine(size is 2 ? $"Incoming Size: {buffer[0]}, {buffer[1]}" : $"Incoming: {message}");

        const short connectionRequest = 0x0C00;
        byte[] connectionRequestBytes = BitConverter.GetBytes(connectionRequest);

        if (buffer.AsSpan()[..2].SequenceEqual(connectionRequestBytes.AsSpan()))
        {
            const short connectionAccept = 0x1C00;
            byte[] connectionAcceptBytes = BitConverter.GetBytes(connectionAccept);

            // All Packets End In A "0"

            byte[] responseSize = [ 3, 0 ];
            byte[] responseData = [ connectionAcceptBytes[0], connectionAcceptBytes[1], 0 ];

            // Send The Size Of The Next Packet
            SendAsync(responseSize);
            Console.WriteLine($"Outgoing Size: {responseSize[0]}, {responseSize[1]}");

            // Send The "Accept Connection" Packet
            SendAsync(responseData);
            Console.WriteLine(Encoding.UTF8.GetString(responseData, 0, responseData.Length));
        }

        // For strings, "\0" is the NULL character, which has the value 0 in the ASCII table and is used to determine the end of C-style strings (also known as the NULL Terminator).
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Session ID {Id} Caught A Socket Error With Code {error}");
    }
}
