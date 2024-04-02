namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors;

[ChatCommand(ChatProtocol.NET_CHAT_CL_CONNECT)]
public class Handshake : ICommandProcessor
{
    public void Process(TCPSession session)
    {
        const short connectionAccept = 0x1C00;
        byte[] connectionAcceptBytes = BitConverter.GetBytes(connectionAccept);

        // All Packets End In A "0"

        byte[] responseSize = [3, 0];
        byte[] responseData = [connectionAcceptBytes[0], connectionAcceptBytes[1], 0];

        // Send The Size Of The Next Packet
        session.SendAsync(responseSize);
        Console.WriteLine($"Outgoing Size: {responseSize[0]}, {responseSize[1]}");

        // Send The "Accept Connection" Packet
        session.SendAsync(responseData);
        Console.WriteLine(Encoding.UTF8.GetString(responseData, 0, responseData.Length));

        // For strings, "\0" is the NULL character, which has the value 0 in the ASCII table and is used to determine the end of C-style strings (also known as the NULL Terminator).
    }
}
