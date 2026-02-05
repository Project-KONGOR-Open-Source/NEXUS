namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServer
{
    public required int HostAccountID { get; set; }

    public required string HostAccountName { get; set; }

    public required int ID { get; set; }

    public required string Name { get; set; }

    public int? MatchServerManagerID { get; set; }

    public required int Instance { get; set; }

    public required string IPAddress { get; set; }

    public required int Port { get; set; }

    public required string Location { get; set; }

    public required string Description { get; set; }

    public bool IsPrivate { get; set; } = false;

    public ServerStatus Status { get; set; } = ServerStatus.SERVER_STATUS_UNKNOWN;

    public string Cookie { get; set; } = Guid.CreateVersion7().ToString();

    public DateTimeOffset TimestampRegistered { get; set; } = DateTimeOffset.UtcNow;
}

// TODO: Move The ServerStatus Enum To A Shared Project So That Both The Chat Server And The Master Server Can Reference The Same Definition
