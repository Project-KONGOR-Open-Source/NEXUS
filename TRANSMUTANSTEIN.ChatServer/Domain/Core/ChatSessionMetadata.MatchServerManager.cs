namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class MatchServerManagerChatSessionMetadata
{
    public required int ServerManagerID { get; set; }

    public required string SessionCookie { get; set; }

    public required int ChatProtocolVersion { get; set; }

    // Server Manager Location And Identity
    public string? Location { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public int Port { get; set; }

    public string? Version { get; set; }

    // Timestamp Tracking
    public DateTimeOffset LastStatusUpdate { get; set; } = DateTimeOffset.UtcNow;

    // Child Server Tracking: IDs Of Match Servers Managed By This Manager
    public HashSet<int> ChildServerIDs { get; set; } = [];
}
