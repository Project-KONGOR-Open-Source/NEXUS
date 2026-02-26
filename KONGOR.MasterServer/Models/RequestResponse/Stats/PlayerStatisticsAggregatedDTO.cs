namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class PlayerStatisticsAggregatedDTO
{
    public int TotalMatches { get; set; }
    public int Smackdowns { get; set; }
    public int Annihilations { get; set; }
    public int Assists { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int MVP { get; set; }
    public int Disconnected { get; set; }
    public DateTimeOffset? LastMatchDate { get; set; }
    public List<FavHeroDTO> TopHeroes { get; set; } = new();
    
    // MVP Award Helpers
    public int HighestKillStreak { get; set; } // Added property
    public int Humiliations { get; set; }
    public int Nemesis { get; set; }
    public int Retribution { get; set; }
    
    public int MostKills { get; set; }
    public int MostAssists { get; set; }
    public int LeastDeaths { get; set; }
    public int MostCreepKills { get; set; }
    public int MostHeroDamage { get; set; }
    public int MostBuildingDamage { get; set; }
    public int MostWards { get; set; }
    public int MostQuadKills { get; set; }

    // Ranked (Normal)
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
    public int RankedDenies { get; set; }
    public int RankedHeroDamage { get; set; }
    public int RankedHeroGold { get; set; }
    public int RankedGoldLost { get; set; }
    public int RankedSecondsDead { get; set; }
    public int RankedTeamCreepKills { get; set; }
    public int RankedTeamCreepDmg { get; set; }
    public int RankedTeamCreepGold { get; set; }
    public int RankedTeamCreepExp { get; set; }
    public int RankedNeutralCreepKills { get; set; }
    public int RankedNeutralCreepDmg { get; set; }
    public int RankedNeutralCreepGold { get; set; }
    public int RankedNeutralCreepExp { get; set; }
    public int RankedBuildingDmg { get; set; }
    public int RankedBuildingsRazed { get; set; }
    public int RankedBuildingExp { get; set; }
    public int RankedBuildingGold { get; set; }
    public int RankedExpDenied { get; set; }
    public int RankedGoldSpent { get; set; }
    public int RankedActions { get; set; }
    public int RankedConsumables { get; set; }
    public int RankedWards { get; set; }
    public int RankedFirstBloods { get; set; }
    public int RankedDoubleKills { get; set; }
    public int RankedTripleKills { get; set; }
    public int RankedQuadKills { get; set; }
    public int RankedAnnihilations { get; set; }
    public int RankedKS3 { get; set; }
    public int RankedKS4 { get; set; }
    public int RankedKS5 { get; set; }
    public int RankedKS6 { get; set; }
    public int RankedKS7 { get; set; }
    public int RankedKS8 { get; set; }
    public int RankedKS9 { get; set; }
    public int RankedKS10 { get; set; }
    public int RankedKS15 { get; set; }
    public int RankedSmackdowns { get; set; }
    public int RankedHumiliations { get; set; }
    public int RankedNemesis { get; set; }
    public int RankedRetribution { get; set; }
    public int RankedTimeEarningExp { get; set; }
    public int RankedBuybacks { get; set; }

    // Casual
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
    public int CasualDenies { get; set; }
    public int CasualHeroDamage { get; set; }
    public int CasualHeroGold { get; set; }
    public int CasualGoldLost { get; set; }
    public int CasualSecondsDead { get; set; }
    public int CasualTeamCreepKills { get; set; }
    public int CasualTeamCreepDmg { get; set; }
    public int CasualTeamCreepGold { get; set; }
    public int CasualTeamCreepExp { get; set; }
    public int CasualNeutralCreepKills { get; set; }
    public int CasualNeutralCreepDmg { get; set; }
    public int CasualNeutralCreepGold { get; set; }
    public int CasualNeutralCreepExp { get; set; }
    public int CasualBuildingDmg { get; set; }
    public int CasualBuildingsRazed { get; set; }
    public int CasualBuildingExp { get; set; }
    public int CasualBuildingGold { get; set; }
    public int CasualExpDenied { get; set; }
    public int CasualGoldSpent { get; set; }
    public int CasualActions { get; set; }
    public int CasualConsumables { get; set; }
    public int CasualWards { get; set; }
    public int CasualFirstBloods { get; set; }
    public int CasualDoubleKills { get; set; }
    public int CasualTripleKills { get; set; }
    public int CasualQuadKills { get; set; }
    public int CasualAnnihilations { get; set; }
    public int CasualKS3 { get; set; }
    public int CasualKS4 { get; set; }
    public int CasualKS5 { get; set; }
    public int CasualKS6 { get; set; }
    public int CasualKS7 { get; set; }
    public int CasualKS8 { get; set; }
    public int CasualKS9 { get; set; }
    public int CasualKS10 { get; set; }
    public int CasualKS15 { get; set; }
    public int CasualSmackdowns { get; set; }
    public int CasualHumiliations { get; set; }
    public int CasualNemesis { get; set; }
    public int CasualRetribution { get; set; }
    public int CasualTimeEarningExp { get; set; }
    public int CasualBuybacks { get; set; }

    // Public (Custom/Unranked)
    public int PublicMatches { get; set; }
    public int PublicWins { get; set; }
    public int PublicLosses { get; set; }
    public double PublicRatingChange { get; set; }
    public int PublicDiscos { get; set; }
    public int PublicKills { get; set; }
    public int PublicDeaths { get; set; }
    public int PublicAssists { get; set; }
    public long PublicExp { get; set; }
    public long PublicGold { get; set; }
    public long PublicSeconds { get; set; }
    public int PublicDenies { get; set; }
    public int PublicHeroDamage { get; set; }
    public int PublicHeroGold { get; set; }
    public int PublicGoldLost { get; set; }
    public int PublicSecondsDead { get; set; }
    public int PublicTeamCreepKills { get; set; }
    public int PublicTeamCreepDmg { get; set; }
    public int PublicTeamCreepGold { get; set; }
    public int PublicTeamCreepExp { get; set; }
    public int PublicNeutralCreepKills { get; set; }
    public int PublicNeutralCreepDmg { get; set; }
    public int PublicNeutralCreepGold { get; set; }
    public int PublicNeutralCreepExp { get; set; }
    public int PublicBuildingDmg { get; set; }
    public int PublicBuildingsRazed { get; set; }
    public int PublicBuildingExp { get; set; }
    public int PublicBuildingGold { get; set; }
    public int PublicExpDenied { get; set; }
    public int PublicGoldSpent { get; set; }
    public int PublicActions { get; set; }
    public int PublicConsumables { get; set; }
    public int PublicWards { get; set; }
    public int PublicFirstBloods { get; set; }
    public int PublicDoubleKills { get; set; }
    public int PublicTripleKills { get; set; }
    public int PublicQuadKills { get; set; }
    public int PublicAnnihilations { get; set; }
    public int PublicKS3 { get; set; }
    public int PublicKS4 { get; set; }
    public int PublicKS5 { get; set; }
    public int PublicKS6 { get; set; }
    public int PublicKS7 { get; set; }
    public int PublicKS8 { get; set; }
    public int PublicKS9 { get; set; }
    public int PublicKS10 { get; set; }
    public int PublicKS15 { get; set; }
    public int PublicSmackdowns { get; set; }
    public int PublicHumiliations { get; set; }
    public int PublicNemesis { get; set; }
    public int PublicRetribution { get; set; }
    public int PublicTimeEarningExp { get; set; }
    public int PublicBuybacks { get; set; }
}

public class FavHeroDTO
{
    public uint HeroId { get; set; }
    public long SecondsPlayed { get; set; }
}

public class PlayerMasteryStatDTO
{
    public uint HeroId { get; set; }
    public double Experience { get; set; }
    public int Level { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}
