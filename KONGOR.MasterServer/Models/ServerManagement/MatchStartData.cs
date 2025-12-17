namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Represents the match information available at the time that the match starts.
///     This information is used to populate the match statistics once the match ends.
/// </summary>
public class MatchStartData
{
    public long MatchID => TimestampStarted.ToUnixTimeMilliseconds();

    public required string MatchName { get; set; }

    public required int ServerID { get; set; }

    public required string HostAccountName { get; set; }

    public required string Map { get; set; }

    public required string Version { get; set; }

    public required bool IsCasual { get; set; }

    public required int ArrangedMatchType { get; set; }

    public required int MatchMode { get; set; }

    public DateTimeOffset TimestampStarted { get; set; } = DateTimeOffset.UtcNow;
}
