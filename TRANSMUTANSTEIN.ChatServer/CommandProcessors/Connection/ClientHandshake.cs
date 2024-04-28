namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, ILogger<ClientHandshake> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<ClientHandshake> Logger { get; set; } = logger;

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

        // TODO: Check Cookie
        // TODO: Check Authentication Hash

        Response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}
