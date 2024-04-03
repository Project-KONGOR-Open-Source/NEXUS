namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.NET_CHAT_CL_CONNECT)]
public class Handshake : ICommandProcessor
{
    public void Process(TCPSession session, ChatBuffer buffer)
    {
        /*
           [4] unsigned long - client's account ID
           [X] string - client's cookie
           [X] string - client's external IP
           [X] string - client's auth hash
           [4] unsigned long - CHAT_PROTOCOL_VERSION
           [1] EOSType - client's operating system
           [1] unsigned char - client's operating system's major version
           [1] unsigned char - client's operating system's minor version
           [1] unsigned char - client's operating system's micro version
           [X] string - client's operating system build code
           [4] unsigned long - client's version (e.g. 3.1.0.2 would be 0x02000103)
           [1] ECrashReportingClientState - client's last known state - sending CRCS_NO_CRASH is fine.
           [1] EChatModeType - client's chat mode type
           [X] string - client's region ("cn")
           [X] string - client's language ("en" or "cn")

           - If the protocol does not match the chat server's, fails silently.
           - If the auth hash is incorrect, fails silently.
           - If the cookie is empty, fails silently.
           - Get OS version info via the Windows function GetVersionEx().
         */

        var _01 = buffer.ReadCommandBytes();

        // ID Is Wrong Because It's Supposed To Be A Long Value But It Is A GUID Instead
        // TODO: Change IDs To Long
        var _02 = buffer.ReadInt64();

        var _03 = buffer.ReadString();
        var _04 = buffer.ReadString();
        var _05 = buffer.ReadString();

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
    }
}
