namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Represents the match information available at the time that the match starts.
///     This information is used to populate the match statistics once the match ends.
/// </summary>
public class MatchStartData
{
    public required int MatchID { get; set; }

    public required string MatchName { get; set; }

    public required int ServerID { get; set; }

    public required string ServerName { get; set; }

    public required string HostAccountName { get; set; }

    public required string Map { get; set; }

    public required string Version { get; set; }

    public required bool IsCasual { get; set; }

    public required int MatchType { get; set; }

    public required string MatchMode { get; set; }

    public DateTimeOffset TimestampStarted { get; set; } = DateTimeOffset.UtcNow;

    public int ConnectedPlayersCount { get; set; } = 0;

    public int MaximumPlayersCount { get; set; } = 10;

    /// <summary>
    ///     Deprecated skill-based server filter that was used for matchmaking.
    ///     <code>
    ///         0 -> Noobs Only
    ///         1 -> Noobs Allowed
    ///         2 -> Pro
    ///     </code>
    ///     This feature is no longer active and the field has no functional purpose.
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    ///     Whether the match is part of an organized league system.
    ///     <code>
    ///         0 -> Regular Match (public, matchmaking, tournament, etc.)
    ///         1 -> League Match (organized competitive league play)
    ///     </code>
    ///     <remark>
    ///         League matches belong to a dedicated competitive league structure with player rosters, seasonal standings, and
    ///         separate win/loss tracking.
    ///     </remark>
    /// </summary>
    public int League { get; set; } = 0;

    public MatchOptions Options { get; set; } = MatchOptions.None;
}
