namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ChatSessionMetadata(ClientHandshakeRequestData data)
{
    public string RemoteIP = data.RemoteIP;

    public string SessionCookie = data.SessionCookie;
    public string SessionAuthenticationHash = data.SessionAuthenticationHash;

    public int ChatProtocolVersion = data.ChatProtocolVersion;

    public byte OperatingSystemIdentifier = data.OperatingSystemIdentifier;
    public byte OperatingSystemVersionMajor = data.OperatingSystemVersionMajor;
    public byte OperatingSystemVersionMinor = data.OperatingSystemVersionMinor;
    public byte OperatingSystemVersionPatch = data.OperatingSystemVersionPatch;
    public string OperatingSystemBuildCode = data.OperatingSystemBuildCode;
    public string OperatingSystemArchitecture = data.OperatingSystemArchitecture;

    public byte ClientVersionMajor = data.ClientVersionMajor;
    public byte ClientVersionMinor = data.ClientVersionMinor;
    public byte ClientVersionPatch = data.ClientVersionPatch;
    public byte ClientVersionRevision = data.ClientVersionRevision;

    public ChatProtocol.ChatClientStatus LastKnownClientState = data.LastKnownClientState;
    public ChatProtocol.ChatModeType ClientChatModeState = data.ClientChatModeState;

    public string ClientRegion = data.ClientRegion;
    public string ClientLanguage = data.ClientLanguage;
}
