namespace KONGOR.MasterServer.Models.ServerManagement;

/// <summary>
///     Represents the match information available at the time that the match starts.
///     This information is used to populate the match statistics once the match ends.
/// </summary>
public class MatchStartData
{
    public int MatchID => TimestampStarted.GetDeterministicInt32Hash();

    public required string MatchName { get; set; }

    public required int ServerID { get; set; }

    public required string HostAccountName { get; set; }

    public required string Map { get; set; }

    public required string Version { get; set; }

    public required bool IsCasual { get; set; }

    public required int MatchType { get; set; }

    public required int MatchMode { get; set; }

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
    ///         League matches belong to a dedicated competitive league structure with player rosters, seasonal standings, and separate win/loss tracking.
    ///     </remark>
    /// </summary>
    public int League { get; set; } = 0;

    public MatchOptions Options { get; set; } = MatchOptions.None;

    // TODO: Find A Way To Generate Concurrency-Safe Incremental Match IDs That Fit Within An Int32

    private static int ComputeIncrementalMatchID(DateTimeOffset datetime)
    {
        // The Official Date And Time That Project KONGOR Started, As Per The First Commit Timestamp
        DateTimeOffset start = new (2022, 01, 05, 10, 16, 35, TimeSpan.Zero);

        // Compute The Total Number Of Seconds That Have Elapsed Since The Project KONGOR Epoch Time
        int seconds = Convert.ToInt32((datetime - start).TotalSeconds);

        // Return The Computed Number Of Seconds As The Incremental Match ID
        return seconds;
    }
}
