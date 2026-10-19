namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     The copy-on-write master (HON's "cowmaster") which a Linux match server manager starts, and forks each of its match servers from.
///     It authenticates like a match server, with an empty server instance, but it never hosts a match, so it is a non-joinable match server, kept apart from the <see cref="MatchServer"/> records, where no server list, matchmaking, or match server look-up can find it.
/// </summary>
public class CopyOnWriteMaster
{
    public required int HostAccountID { get; set; }

    public required string HostAccountName { get; set; }

    public required int ID { get; set; }

    public required string Name { get; set; }

    public required int? MatchServerManagerID { get; set; }

    public required string IPAddress { get; set; }

    public required int Port { get; set; }

    public required string Location { get; set; }

    public required string Description { get; set; }

    public ServerStatus Status { get; set; } = ServerStatus.SERVER_STATUS_UNKNOWN;

    public string Cookie { get; set; } = Guid.CreateVersion7().ToString();

    public DateTimeOffset TimestampRegistered { get; set; } = DateTimeOffset.UtcNow;
}
