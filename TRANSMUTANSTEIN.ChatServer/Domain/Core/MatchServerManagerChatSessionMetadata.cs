namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerManagerChatSessionMetadata
{
    public required int ServerManagerID { get; set; }

    public required string SessionCookie { get; set; }

    public required int ChatProtocolVersion { get; set; }
}