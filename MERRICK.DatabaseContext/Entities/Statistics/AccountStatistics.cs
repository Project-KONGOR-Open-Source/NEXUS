namespace MERRICK.DatabaseContext.Entities.Statistics;

[Index(nameof(AccountID), nameof(StatisticsType), IsUnique = true)]
public class AccountStatistics
{
    [Key]
    public int ID { get; set; }

    public required int AccountID { get; set; }

    public required AccountStatisticsType StatisticsType { get; set; }

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
    ///     The total expected number of placement matches is 6.
    ///     "0" means a loss, "1" means a win.
    /// </summary>
    /// <remarks>
    ///     "110110" means 6 placement matches with 4 wins and 2 losses.
    /// </remarks>
    public required string? PlacementMatchesData { get; set; }
}

public enum AccountStatisticsType
{
    Cooperative = 0,
    Public      = 1,
    Matchmaking = 2,
    MidWars     = 3,
    RiftWars    = 4
}
