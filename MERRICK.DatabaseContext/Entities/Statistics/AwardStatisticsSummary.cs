namespace MERRICK.DatabaseContext.Entities.Statistics;

/// <summary>
///     Aggregated award statistics stored as JSON in the database.
///     Updated when matches are recorded.
/// </summary>
public class AwardStatisticsSummary
{
    public int MVPAwards { get; set; }

    public int AnnihilationAwards { get; set; }

    public int QuadKillAwards { get; set; }

    public int LongestKillStreakAwards { get; set; }

    public int SmackdownAwards { get; set; }

    public int MostKillsAwards { get; set; }

    public int MostAssistsAwards { get; set; }

    public int LeastDeathsAwards { get; set; }

    public int MostBuildingDamageAwards { get; set; }

    public int MostWardsDestroyedAwards { get; set; }

    public int MostHeroDamageDealtAwards { get; set; }

    public int HighestCreepScoreAwards { get; set; }
}
