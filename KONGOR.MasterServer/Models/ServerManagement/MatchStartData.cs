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

    public required int ArrangedMatchType { get; set; }

    public required int MatchMode { get; set; }

    public DateTimeOffset TimestampStarted { get; set; } = DateTimeOffset.UtcNow;

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
