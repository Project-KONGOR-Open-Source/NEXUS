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

    /// <summary>
    ///     The free-text reason for the current chat mode (e.g. "Away From Keyboard").
    ///     Set by the client via <c>CHAT_CMD_SET_CHAT_MODE_TYPE</c>.
    /// </summary>
    public string ClientChatModeReason { get; set; } = string.Empty;

    public required string ClientRegion { get; set; }

    public required string ClientLanguage { get; set; }

    public MatchServer? MatchServerConnectedTo { get; set; }
}
