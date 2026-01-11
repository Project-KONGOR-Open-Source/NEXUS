namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

public class ClientHandshakeRequestData
{
    public ClientHandshakeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        AccountID = buffer.ReadInt32();
        SessionCookie = buffer.ReadString();
        RemoteIP = buffer.ReadString();
        SessionAuthenticationHash = buffer.ReadString();
        ChatProtocolVersion = buffer.ReadInt32();
        OperatingSystemIdentifier = buffer.ReadInt8();
        OperatingSystemVersionMajor = buffer.ReadInt8();
        OperatingSystemVersionMinor = buffer.ReadInt8();
        OperatingSystemVersionPatch = buffer.ReadInt8();
        OperatingSystemBuildCode = buffer.ReadString();
        OperatingSystemArchitecture = buffer.ReadString();
        ClientVersionMajor = buffer.ReadInt8();
        ClientVersionMinor = buffer.ReadInt8();
        ClientVersionPatch = buffer.ReadInt8();
        ClientVersionRevision = buffer.ReadInt8();
        LastKnownClientState = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();
        ClientChatModeState = (ChatProtocol.ChatModeType) buffer.ReadInt8();
        ClientRegion = buffer.ReadString();
        ClientLanguage = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public int AccountID { get; }

    public string SessionCookie { get; }

    public string RemoteIP { get; }

    public string SessionAuthenticationHash { get; }

    public int ChatProtocolVersion { get; }

    public byte OperatingSystemIdentifier { get; }

    public byte OperatingSystemVersionMajor { get; }

    public byte OperatingSystemVersionMinor { get; }

    public byte OperatingSystemVersionPatch { get; }

    public string OperatingSystemBuildCode { get; }

    public string OperatingSystemArchitecture { get; }

    public byte ClientVersionMajor { get; }

    public byte ClientVersionMinor { get; }

    public byte ClientVersionPatch { get; }

    public byte ClientVersionRevision { get; }

    public ChatProtocol.ChatClientStatus LastKnownClientState { get; }

    public ChatProtocol.ChatModeType ClientChatModeState { get; }

    public string ClientRegion { get; }

    public string ClientLanguage { get; }

    public ClientChatSessionMetadata ToMetadata()
    {
        return new ClientChatSessionMetadata
        {
            RemoteIP = RemoteIP,
            SessionCookie = SessionCookie,
            SessionAuthenticationHash = SessionAuthenticationHash,
            ChatProtocolVersion = ChatProtocolVersion,
            OperatingSystemIdentifier = OperatingSystemIdentifier,
            OperatingSystemVersionMajor = OperatingSystemVersionMajor,
            OperatingSystemVersionMinor = OperatingSystemVersionMinor,
            OperatingSystemVersionPatch = OperatingSystemVersionPatch,
            OperatingSystemBuildCode = OperatingSystemBuildCode,
            OperatingSystemArchitecture = OperatingSystemArchitecture,
            ClientVersionMajor = ClientVersionMajor,
            ClientVersionMinor = ClientVersionMinor,
            ClientVersionPatch = ClientVersionPatch,
            ClientVersionRevision = ClientVersionRevision,
            LastKnownClientState = LastKnownClientState,
            ClientChatModeState = ClientChatModeState,
            ClientRegion = ClientRegion,
            ClientLanguage = ClientLanguage
        };
    }
}