namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT)]
public class ClientHandshake(MerrickContext merrick, ILogger<ClientHandshake> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<ClientHandshake> Logger { get; set; } = logger;

    public async Task Process(TCPSession session, ChatBuffer buffer)
    {
        ClientHandshakeRequestData requestData = new(buffer);

        // TODO: Check Cookie

        // TODO: Check Authentication Hash

        Response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);
        Response.PrependBufferSize();

        session.SendAsync(Response.Data);
    }
}

public class ClientHandshakeRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public int AccountID = buffer.ReadInt32();
    public string SessionCookie = buffer.ReadString();
    public string RemoteIP = buffer.ReadString();
    public string SessionAuthenticationHash = buffer.ReadString();
    public int ChatProtocolVersion = buffer.ReadInt32();
    public byte OperatingSystemIdentifier = buffer.ReadInt8();
    public byte OperatingSystemVersionMajor = buffer.ReadInt8();
    public byte OperatingSystemVersionMinor = buffer.ReadInt8();
    public byte OperatingSystemVersionPatch = buffer.ReadInt8();
    public string OperatingSystemBuildCode = buffer.ReadString();
    public string OperatingSystemArchitecture = buffer.ReadString();
    public byte ClientVersionMajor = buffer.ReadInt8();
    public byte ClientVersionMinor = buffer.ReadInt8();
    public byte ClientVersionPatch = buffer.ReadInt8();
    public byte ClientVersionRevision = buffer.ReadInt8();
    public byte LastKnownClientState = buffer.ReadInt8();
    public byte ClientChatModeState = buffer.ReadInt8();
    public string ClientRegion = buffer.ReadString();
    public string ClientLanguage = buffer.ReadString();
}
