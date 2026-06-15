namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Aggregated hero statistics stored as JSON in the database.
///     Updated when matches are recorded.
/// </summary>
public class HeroStatisticsSummary
{
    public List<HeroStats> Heroes { get; set; } = [];

    /// <summary>
    ///     Sums every per-hero entry into a single <see cref="HeroStats"/> representing the account's totals across all heroes in this game mode.
    ///     The <see cref="HeroStats.HeroIdentifier"/> of the result is empty, as it represents an aggregate rather than a single hero.
    /// </summary>
    public HeroStats AggregateTotals()
    {
        HeroStats totals = new () { HeroIdentifier = string.Empty };

        foreach (HeroStats hero in Heroes)
        {
            totals.GamesPlayed += hero.GamesPlayed;
            totals.Wins += hero.Wins;
            totals.Losses += hero.Losses;
            totals.Concedes += hero.Concedes;
            totals.ConcedeVotes += hero.ConcedeVotes;
            totals.Buybacks += hero.Buybacks;
            totals.Disconnections += hero.Disconnections;
            totals.Kicks += hero.Kicks;
            totals.ScoreTotal += hero.ScoreTotal;
            totals.HeroKills += hero.HeroKills;
            totals.HeroDamage += hero.HeroDamage;
            totals.HeroExperience += hero.HeroExperience;
            totals.GoldFromHeroKills += hero.GoldFromHeroKills;
            totals.HeroAssists += hero.HeroAssists;
            totals.HeroDeaths += hero.HeroDeaths;
            totals.GoldLostToDeath += hero.GoldLostToDeath;
            totals.SecondsDead += hero.SecondsDead;
            totals.TeamCreepKills += hero.TeamCreepKills;
            totals.TeamCreepDamage += hero.TeamCreepDamage;
            totals.TeamCreepExperience += hero.TeamCreepExperience;
            totals.TeamCreepGold += hero.TeamCreepGold;
            totals.NeutralCreepKills += hero.NeutralCreepKills;
            totals.NeutralCreepDamage += hero.NeutralCreepDamage;
            totals.NeutralCreepExperience += hero.NeutralCreepExperience;
            totals.NeutralCreepGold += hero.NeutralCreepGold;
            totals.BuildingDamage += hero.BuildingDamage;
            totals.ExperienceFromBuildings += hero.ExperienceFromBuildings;
            totals.BuildingsRazed += hero.BuildingsRazed;
            totals.GoldFromBuildings += hero.GoldFromBuildings;
            totals.Denies += hero.Denies;
            totals.ExperienceDenied += hero.ExperienceDenied;
            totals.Gold += hero.Gold;
            totals.GoldSpent += hero.GoldSpent;
            totals.Experience += hero.Experience;
            totals.Actions += hero.Actions;
            totals.SecondsPlayed += hero.SecondsPlayed;
            totals.ConsumablesPurchased += hero.ConsumablesPurchased;
            totals.WardsPlaced += hero.WardsPlaced;
            totals.TimeEarningExperience += hero.TimeEarningExperience;
            totals.FirstBloods += hero.FirstBloods;
            totals.DoubleKills += hero.DoubleKills;
            totals.TripleKills += hero.TripleKills;
            totals.QuadKills += hero.QuadKills;
            totals.Annihilations += hero.Annihilations;
            totals.KillStreak03 += hero.KillStreak03;
            totals.KillStreak04 += hero.KillStreak04;
            totals.KillStreak05 += hero.KillStreak05;
            totals.KillStreak06 += hero.KillStreak06;
            totals.KillStreak07 += hero.KillStreak07;
            totals.KillStreak08 += hero.KillStreak08;
            totals.KillStreak09 += hero.KillStreak09;
            totals.KillStreak10 += hero.KillStreak10;
            totals.KillStreak15 += hero.KillStreak15;
            totals.Smackdowns += hero.Smackdowns;
            totals.Humiliations += hero.Humiliations;
            totals.Nemeses += hero.Nemeses;
            totals.Retributions += hero.Retributions;
        }

        return totals;
    }
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
