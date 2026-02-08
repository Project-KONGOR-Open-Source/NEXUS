namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Aggregated hero statistics stored as JSON in the database.
///     Updated when matches are recorded.
/// </summary>
public class HeroStatisticsSummary
{
    public List<HeroStats> Heroes { get; set; } = [];
}

// TODO: Rename HeroStats To HeroStatistics And Consolidate With Other Hero Statistics Types

/// <summary>
///     Per-hero statistics for a specific account and game mode.
/// </summary>
public class HeroStats
{
    public required string HeroIdentifier { get; set; }

    public int GamesPlayed { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int HeroKills { get; set; }

    public int HeroDeaths { get; set; }

    public int HeroAssists { get; set; }

    public  int TeamCreepKills { get; set; }

    public int Denies { get; set; }

    public int Experience { get; set; }

    public int Gold { get; set; }

    public int Actions { get; set; }

    public int TimeEarningExperience { get; set; }
}
