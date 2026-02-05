namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerChatSessionMetadata
{
    public required int ServerID { get; set; }

    public required string SessionCookie { get; set; }

    public required int ChatProtocolVersion { get; set; }

    public ChatProtocol.ServerStatus Status { get; set; } = ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN;

    public string Address { get; set; } = string.Empty;

    public int Port { get; set; }

    public int MatchID { get; set; } = -1;
}