namespace MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(AccountID), nameof(Type), IsUnique = true)]
public class AccountStatistics
{
    [Key]
    public int ID { get; set; }

    public int AccountID { get; set; }

    [ForeignKey(nameof(AccountID))]
    public required Account Account { get; set; }

    public required AccountStatisticsType Type { get; set; }

    public int MatchesPlayed { get; set; } = 0;

    public int MatchesWon { get; set; } = 0;

    public int MatchesLost { get; set; } = 0;

    public int MatchesDisconnected { get; set; } = 0;

    public int MatchesConceded { get; set; } = 0;

    public int MatchesKicked { get; set; } = 0;

    public double SkillRating { get; set; } = 1500.0;

    public int HeroKills { get; set; } = 0;

    public int HeroAssists { get; set; } = 0;

    public int HeroDeaths { get; set; } = 0;

    public int WardsPlaced { get; set; } = 0;

    public int Smackdowns { get; set; } = 0;

    public double PerformanceScore => (HeroKills + HeroAssists) / Math.Max(1, HeroDeaths);

    /// <summary>
    ///     The total number of placement matches expected before a seasonal rank is assigned.
    /// </summary>
    public const int ExpectedPlacementMatchCount = 6;

    /// <summary>
    ///     The total expected number of placement matches is <see cref="ExpectedPlacementMatchCount"/>.
    ///     "0" means a loss, "1" means a win.
    /// </summary>
    /// <remarks>
    ///     "110110" means 6 placement matches with 4 wins and 2 losses.
    /// </remarks>
    public required string? PlacementMatchesData { get; set; }

    /// <summary>
    ///     Whether the placement phase for this statistics row is still incomplete.
    ///     Always <see langword="false"/> for queues that do not track placement matches.
    /// </summary>
    /// <remarks>
    ///     The placement phase only gates the visible medal, and is recorded at statistics submission, where a disconnected participant does not consume a placement match.
    ///     It is distinct from the matchmaking "provisional" rating period, which is a hidden rating-convergence mechanic that amplifies rating changes during a player's first matches of a game type and can also end early once their rating is high enough.
    /// </remarks>
    public bool IsInPlacementPhase => PlacementMatchesData is not null && PlacementMatchesData.Length < ExpectedPlacementMatchCount;

    /// <summary>
    ///     Aggregated per-hero statistics stored as JSON.
    ///     Updated automatically when matches are recorded.
    /// </summary>
    public HeroStatisticsSummary HeroStatistics { get; set; } = new ();

    /// <summary>
    ///     Aggregated award statistics stored as JSON.
    ///     Updated automatically when matches are recorded.
    /// </summary>
    public AwardStatisticsSummary AwardStatistics { get; set; } = new ();
}

public enum AccountStatisticsType
{
    Cooperative       = 0,
    Public            = 1,
    Matchmaking       = 2,
    MatchmakingCasual = 3,
    MidWars           = 4,
    RiftWars          = 5
}
