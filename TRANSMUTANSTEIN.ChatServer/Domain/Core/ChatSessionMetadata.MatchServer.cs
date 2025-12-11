namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerChatSessionMetadata
{
    public required int ServerID { get; set; }

    public required string SessionCookie { get; set; }

    public required int ChatProtocolVersion { get; set; }
}
