namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ClientChatSessionMetadata
{
    public required string RemoteIP { get; set; }

    public required string SessionCookie { get; set; }

    public required string SessionAuthenticationHash { get; set; }

    public required int ChatProtocolVersion { get; set; }

    public required byte OperatingSystemIdentifier { get; set; }

    public required byte OperatingSystemVersionMajor { get; set; }

    public required byte OperatingSystemVersionMinor { get; set; }

    public required byte OperatingSystemVersionPatch { get; set; }

    public required string OperatingSystemBuildCode { get; set; }

    public required string OperatingSystemArchitecture { get; set; }

    public required byte ClientVersionMajor { get; set; }

    public required byte ClientVersionMinor { get; set; }

    public required byte ClientVersionPatch { get; set; }

    public required byte ClientVersionRevision { get; set; }

    public required ChatProtocol.ChatClientStatus LastKnownClientState { get; set; }

    public required ChatProtocol.ChatModeType ClientChatModeState { get; set; }

    public required string ClientRegion { get; set; }

    public required string ClientLanguage { get; set; }
}
