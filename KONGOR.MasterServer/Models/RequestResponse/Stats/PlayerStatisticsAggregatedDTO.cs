namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class PlayerStatisticsAggregatedDTO
{
    // General
    public int TotalMatches { get; set; }
    public int Smackdowns { get; set; }
    public int Annihilations { get; set; }
    public int Assists { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int MVP { get; set; }
    public int Disconnected { get; set; }

    // Ranked
    public int RankedMatches { get; set; }
    public int RankedWins { get; set; }
    public int RankedLosses { get; set; }
    public double RankedRatingChange { get; set; }
    public int RankedDiscos { get; set; }
    public int RankedKills { get; set; }
    public int RankedDeaths { get; set; }
    public int RankedAssists { get; set; }
    public long RankedExp { get; set; }
    public long RankedGold { get; set; }
    public long RankedSeconds { get; set; }

    // Casual (Public)
    public int CasualMatches { get; set; }
    public int CasualWins { get; set; }
    public int CasualLosses { get; set; }
    public double CasualRatingChange { get; set; }
    public int CasualDiscos { get; set; }
    public int CasualKills { get; set; }
    public int CasualDeaths { get; set; }
    public int CasualAssists { get; set; }
    public long CasualExp { get; set; }
    public long CasualGold { get; set; }
    public long CasualSeconds { get; set; }
}
