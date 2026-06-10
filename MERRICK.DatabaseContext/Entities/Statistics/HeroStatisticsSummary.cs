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
///     Per-hero statistics for a specific account and game mode, accumulated across every match recorded for that account, hero, and mode.
/// </summary>
public class HeroStats
{
    public required string HeroIdentifier { get; set; }

    public int GamesPlayed { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Concedes { get; set; }

    public int ConcedeVotes { get; set; }

    public int Buybacks { get; set; }

    public int Disconnections { get; set; }

    public int Kicks { get; set; }

    public int ScoreTotal { get; set; }

    public int HeroKills { get; set; }

    public int HeroDamage { get; set; }

    public int HeroExperience { get; set; }

    public int GoldFromHeroKills { get; set; }

    public int HeroAssists { get; set; }

    public int HeroDeaths { get; set; }

    public int GoldLostToDeath { get; set; }

    public int SecondsDead { get; set; }

    public int TeamCreepKills { get; set; }

    public int TeamCreepDamage { get; set; }

    public int TeamCreepExperience { get; set; }

    public int TeamCreepGold { get; set; }

    public int NeutralCreepKills { get; set; }

    public int NeutralCreepDamage { get; set; }

    public int NeutralCreepExperience { get; set; }

    public int NeutralCreepGold { get; set; }

    public int BuildingDamage { get; set; }

    public int ExperienceFromBuildings { get; set; }

    public int BuildingsRazed { get; set; }

    public int GoldFromBuildings { get; set; }

    public int Denies { get; set; }

    public int ExperienceDenied { get; set; }

    public int Gold { get; set; }

    public int GoldSpent { get; set; }

    public int Experience { get; set; }

    public int Actions { get; set; }

    public int SecondsPlayed { get; set; }

    public int ConsumablesPurchased { get; set; }

    public int WardsPlaced { get; set; }

    public int TimeEarningExperience { get; set; }

    public int FirstBloods { get; set; }

    public int DoubleKills { get; set; }

    public int TripleKills { get; set; }

    public int QuadKills { get; set; }

    public int Annihilations { get; set; }

    public int KillStreak03 { get; set; }

    public int KillStreak04 { get; set; }

    public int KillStreak05 { get; set; }

    public int KillStreak06 { get; set; }

    public int KillStreak07 { get; set; }

    public int KillStreak08 { get; set; }

    public int KillStreak09 { get; set; }

    public int KillStreak10 { get; set; }

    public int KillStreak15 { get; set; }

    public int Smackdowns { get; set; }

    public int Humiliations { get; set; }

    public int Nemeses { get; set; }

    public int Retributions { get; set; }
}
