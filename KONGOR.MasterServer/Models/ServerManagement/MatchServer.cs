namespace KONGOR.MasterServer.Models.ServerManagement;

public class MatchServer
{
    public required int HostAccountID { get; set; }

    public required string HostAccountName { get; set; }

    public required int ID { get; set; }

    public required string Name { get; set; }

    public required MatchServerManager? MatchServerManager { get; set; }

    public required int Instance { get; set; }

    public required string IPAddress { get; set; }

    public required int Port { get; set; }

    public required string Location { get; set; }

    public required string Description { get; set; }

    public ServerStatus Status { get; set; } = ServerStatus.SERVER_STATUS_IDLE;

    public string Cookie { get; set; } = Guid.CreateVersion7().ToString();

    public DateTimeOffset TimestampRegistered { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
///     This enumeration is part of the Chat Server Protocol, and needs to match its counterpart in order for servers in the distributed cache to be handled correctly.
/// </summary>
public enum ServerStatus
{
    SERVER_STATUS_SLEEPING,
    SERVER_STATUS_IDLE,
    SERVER_STATUS_LOADING,
    SERVER_STATUS_ACTIVE,
    SERVER_STATUS_CRASHED,
    SERVER_STATUS_KILLED,

    SERVER_STATUS_UNKNOWN
};
