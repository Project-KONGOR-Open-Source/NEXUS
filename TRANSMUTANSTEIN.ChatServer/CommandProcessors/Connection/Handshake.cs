namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.NET_CHAT_CL_CONNECT)]
public class Handshake : CommandProcessorsBase, ICommandProcessor
{
    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        byte[] _ = buffer.ReadCommandBytes();
        int accountID = buffer.ReadInt32();
        string sessionCookie = buffer.ReadString();
        string remoteIP = buffer.ReadString();
        string sessionAuthenticationHash = buffer.ReadString();
        int chatProtocolVersion = buffer.ReadInt32();
        byte operatingSystemIdentifier = buffer.ReadInt8();
        byte operatingSystemVersionMajor = buffer.ReadInt8();
        byte operatingSystemVersionMinor = buffer.ReadInt8();
        byte operatingSystemVersionPatch = buffer.ReadInt8();
        string operatingSystemBuildCode = buffer.ReadString();
        string operatingSystemArchitecture = buffer.ReadString();
        byte clientVersionMajor = buffer.ReadInt8();
        byte clientVersionMinor = buffer.ReadInt8();
        byte clientVersionPatch = buffer.ReadInt8();
        byte clientVersionRevision = buffer.ReadInt8();
        byte lastKnownClientState = buffer.ReadInt8();
        byte clientChatModeState = buffer.ReadInt8();
        string clientRegion = buffer.ReadString();
        string clientLanguage = buffer.ReadString();

        // TODO: Run Checks

        int debug = await MerrickContext.Users.CountAsync();
        Logger.LogCritical("TEST");

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
