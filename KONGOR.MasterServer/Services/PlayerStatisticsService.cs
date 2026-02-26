using Microsoft.EntityFrameworkCore;
using KONGOR.MasterServer.Models.RequestResponse.Stats;

using MERRICK.DatabaseContext.Extensions;

namespace KONGOR.MasterServer.Services;

public interface IPlayerStatisticsService
{
    Task<PlayerStatisticsAggregatedDTO> GetAggregatedStatisticsAsync(int accountId);
    Task<List<PlayerMasteryStatDTO>> GetPlayerMasteryStatsAsync(int accountId);
}

public partial class PlayerStatisticsService(
    MerrickContext databaseContext,
    ILogger<PlayerStatisticsService> logger) : IPlayerStatisticsService
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private ILogger<PlayerStatisticsService> Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Debug, Message = "[Stats] Calculating aggregated stats for Account ID: {AccountID}")]
    private partial void LogCalculatingStats(int accountId);

    public async Task<PlayerStatisticsAggregatedDTO> GetAggregatedStatisticsAsync(int accountId)
    {
        LogCalculatingStats(accountId);

        PlayerStatisticsAggregatedDTO? aggregated = await MerrickContext.PlayerStatistics
            .Where(ps => ps.AccountID == accountId)
            .GroupBy(ps => 1)
            .Select(g => new PlayerStatisticsAggregatedDTO
            {
                TotalMatches = g.Count(),
                Smackdowns = g.Sum(x => x.Smackdown),
                Annihilations = g.Sum(x => x.Annihilation),
                Assists = g.Sum(x => x.HeroAssists),
                Kills = g.Sum(x => x.HeroKills),
                Deaths = g.Sum(x => x.HeroDeaths),
                MVP = g.Sum(x => x.MVP),
                Disconnected = g.Sum(x => x.Disconnected),
                
                Humiliations = g.Sum(x => x.Humiliation),
                Nemesis = g.Sum(x => x.Nemesis),
                Retribution = g.Sum(x => x.Retribution),

                // MVP stats aggregation (Calculated from Match History)
                MostKills = g.Max(x => x.HeroKills),
                MostAssists = g.Max(x => x.HeroAssists),
                LeastDeaths = g.Min(x => x.HeroDeaths), // Note: Min deaths might be 0 for short games, but it's accurate.
                MostCreepKills = g.Max(x => x.TeamCreepKills + x.NeutralCreepKills),
                MostHeroDamage = g.Max(x => x.HeroDamage),
                MostBuildingDamage = g.Max(x => x.BuildingDamage),
                MostWards = g.Max(x => x.WardsPlaced),
                // MostQuadKills: QuadKill is a count per match. Max(QuadKill) means max QKs in one match.
                MostQuadKills = g.Max(x => x.QuadKill),

                HighestKillStreak = g.Max(x =>
                    x.KillStreak15 > 0 ? 15 :
                    x.KillStreak10 > 0 ? 10 :
                    x.KillStreak09 > 0 ? 9 :
                    x.KillStreak08 > 0 ? 8 :
                    x.KillStreak07 > 0 ? 7 :
                    x.KillStreak06 > 0 ? 6 :
                    x.KillStreak05 > 0 ? 5 :
                    x.KillStreak04 > 0 ? 4 :
                    x.KillStreak03 > 0 ? 3 : 0
                ),

                RankedMatches = g.Count(x => x.RankedMatch == 1),
                RankedWins = g.Sum(x => x.RankedMatch == 1 ? x.Win : 0),
                RankedLosses = g.Sum(x => x.RankedMatch == 1 ? x.Loss : 0),
                RankedRatingChange = g.Sum(x => x.RankedMatch == 1 ? x.RankedSkillRatingChange : 0),
                RankedDiscos = g.Sum(x => x.RankedMatch == 1 ? x.Disconnected : 0),
                RankedKills = g.Sum(x => x.RankedMatch == 1 ? x.HeroKills : 0),
                RankedDeaths = g.Sum(x => x.RankedMatch == 1 ? x.HeroDeaths : 0),
                RankedAssists = g.Sum(x => x.RankedMatch == 1 ? x.HeroAssists : 0),
                RankedExp = g.Sum(x => x.RankedMatch == 1 ? (long) x.Experience : 0),
                RankedGold = g.Sum(x => x.RankedMatch == 1 ? (long) x.Gold : 0),
                RankedSeconds = g.Sum(x => x.RankedMatch == 1 ? (long) x.SecondsPlayed : 0),

                // Ranked Extended Aggregation
                RankedDenies = g.Sum(x => x.RankedMatch == 1 ? x.Denies : 0),
                RankedHeroDamage = g.Sum(x => x.RankedMatch == 1 ? x.HeroDamage : 0),
                RankedHeroGold = g.Sum(x => x.RankedMatch == 1 ? x.GoldFromHeroKills : 0),
                RankedGoldLost = g.Sum(x => x.RankedMatch == 1 ? x.GoldLostToDeath : 0),
                RankedSecondsDead = g.Sum(x => x.RankedMatch == 1 ? x.SecondsDead : 0),
                RankedTeamCreepKills = g.Sum(x => x.RankedMatch == 1 ? x.TeamCreepKills : 0),
                RankedTeamCreepDmg = g.Sum(x => x.RankedMatch == 1 ? x.TeamCreepDamage : 0),
                RankedTeamCreepGold = g.Sum(x => x.RankedMatch == 1 ? x.TeamCreepGold : 0),
                RankedTeamCreepExp = g.Sum(x => x.RankedMatch == 1 ? x.TeamCreepExperience : 0),
                RankedNeutralCreepKills = g.Sum(x => x.RankedMatch == 1 ? x.NeutralCreepKills : 0),
                RankedNeutralCreepDmg = g.Sum(x => x.RankedMatch == 1 ? x.NeutralCreepDamage : 0),
                RankedNeutralCreepGold = g.Sum(x => x.RankedMatch == 1 ? x.NeutralCreepGold : 0),
                RankedNeutralCreepExp = g.Sum(x => x.RankedMatch == 1 ? x.NeutralCreepExperience : 0),
                RankedBuildingDmg = g.Sum(x => x.RankedMatch == 1 ? x.BuildingDamage : 0),
                RankedBuildingsRazed = g.Sum(x => x.RankedMatch == 1 ? x.BuildingsRazed : 0),
                RankedBuildingExp = g.Sum(x => x.RankedMatch == 1 ? x.ExperienceFromBuildings : 0),
                RankedBuildingGold = g.Sum(x => x.RankedMatch == 1 ? x.GoldFromBuildings : 0),
                RankedExpDenied = g.Sum(x => x.RankedMatch == 1 ? x.ExperienceDenied : 0),
                RankedGoldSpent = g.Sum(x => x.RankedMatch == 1 ? x.GoldSpent : 0),
                RankedActions = g.Sum(x => x.RankedMatch == 1 ? x.Actions : 0),
                RankedConsumables = g.Sum(x => x.RankedMatch == 1 ? x.ConsumablesPurchased : 0),
                RankedWards = g.Sum(x => x.RankedMatch == 1 ? x.WardsPlaced : 0),
                RankedFirstBloods = g.Sum(x => x.RankedMatch == 1 ? x.FirstBlood : 0),
                RankedDoubleKills = g.Sum(x => x.RankedMatch == 1 ? x.DoubleKill : 0),
                RankedTripleKills = g.Sum(x => x.RankedMatch == 1 ? x.TripleKill : 0),
                RankedQuadKills = g.Sum(x => x.RankedMatch == 1 ? x.QuadKill : 0),
                RankedAnnihilations = g.Sum(x => x.RankedMatch == 1 ? x.Annihilation : 0),
                RankedKS3 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak03 : 0),
                RankedKS4 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak04 : 0),
                RankedKS5 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak05 : 0),
                RankedKS6 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak06 : 0),
                RankedKS7 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak07 : 0),
                RankedKS8 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak08 : 0),
                RankedKS9 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak09 : 0),
                RankedKS10 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak10 : 0),
                RankedKS15 = g.Sum(x => x.RankedMatch == 1 ? x.KillStreak15 : 0),
                RankedSmackdowns = g.Sum(x => x.RankedMatch == 1 ? x.Smackdown : 0),
                RankedHumiliations = g.Sum(x => x.RankedMatch == 1 ? x.Humiliation : 0),
                RankedNemesis = g.Sum(x => x.RankedMatch == 1 ? x.Nemesis : 0),
                RankedRetribution = g.Sum(x => x.RankedMatch == 1 ? x.Retribution : 0),
                RankedTimeEarningExp = g.Sum(x => x.RankedMatch == 1 ? x.TimeEarningExperience : 0),
                RankedBuybacks = g.Sum(x => x.RankedMatch == 1 ? x.Buybacks : 0),

                CasualMatches = g.Count(x => x.PublicMatch == 1),
                CasualWins = g.Sum(x => x.PublicMatch == 1 ? x.Win : 0),
                CasualLosses = g.Sum(x => x.PublicMatch == 1 ? x.Loss : 0),
                CasualRatingChange = g.Sum(x => x.PublicMatch == 1 ? x.PublicSkillRatingChange : 0),
                CasualDiscos = g.Sum(x => x.PublicMatch == 1 ? x.Disconnected : 0),
                CasualKills = g.Sum(x => x.PublicMatch == 1 ? x.HeroKills : 0),
                CasualDeaths = g.Sum(x => x.PublicMatch == 1 ? x.HeroDeaths : 0),
                CasualAssists = g.Sum(x => x.PublicMatch == 1 ? x.HeroAssists : 0),
                CasualExp = g.Sum(x => x.PublicMatch == 1 ? (long) x.Experience : 0),
                CasualGold = g.Sum(x => x.PublicMatch == 1 ? (long) x.Gold : 0),
                CasualSeconds = g.Sum(x => x.PublicMatch == 1 ? (long) x.SecondsPlayed : 0),

                // Casual Extended Aggregation
                CasualDenies = g.Sum(x => x.PublicMatch == 1 ? x.Denies : 0),
                CasualHeroDamage = g.Sum(x => x.PublicMatch == 1 ? x.HeroDamage : 0),
                CasualHeroGold = g.Sum(x => x.PublicMatch == 1 ? x.GoldFromHeroKills : 0),
                CasualGoldLost = g.Sum(x => x.PublicMatch == 1 ? x.GoldLostToDeath : 0),
                CasualSecondsDead = g.Sum(x => x.PublicMatch == 1 ? x.SecondsDead : 0),
                CasualTeamCreepKills = g.Sum(x => x.PublicMatch == 1 ? x.TeamCreepKills : 0),
                CasualTeamCreepDmg = g.Sum(x => x.PublicMatch == 1 ? x.TeamCreepDamage : 0),
                CasualTeamCreepGold = g.Sum(x => x.PublicMatch == 1 ? x.TeamCreepGold : 0),
                CasualTeamCreepExp = g.Sum(x => x.PublicMatch == 1 ? x.TeamCreepExperience : 0),
                CasualNeutralCreepKills = g.Sum(x => x.PublicMatch == 1 ? x.NeutralCreepKills : 0),
                CasualNeutralCreepDmg = g.Sum(x => x.PublicMatch == 1 ? x.NeutralCreepDamage : 0),
                CasualNeutralCreepGold = g.Sum(x => x.PublicMatch == 1 ? x.NeutralCreepGold : 0),
                CasualNeutralCreepExp = g.Sum(x => x.PublicMatch == 1 ? x.NeutralCreepExperience : 0),
                CasualBuildingDmg = g.Sum(x => x.PublicMatch == 1 ? x.BuildingDamage : 0),
                CasualBuildingsRazed = g.Sum(x => x.PublicMatch == 1 ? x.BuildingsRazed : 0),
                CasualBuildingExp = g.Sum(x => x.PublicMatch == 1 ? x.ExperienceFromBuildings : 0),
                CasualBuildingGold = g.Sum(x => x.PublicMatch == 1 ? x.GoldFromBuildings : 0),
                CasualExpDenied = g.Sum(x => x.PublicMatch == 1 ? x.ExperienceDenied : 0),
                CasualGoldSpent = g.Sum(x => x.PublicMatch == 1 ? x.GoldSpent : 0),
                CasualActions = g.Sum(x => x.PublicMatch == 1 ? x.Actions : 0),
                CasualConsumables = g.Sum(x => x.PublicMatch == 1 ? x.ConsumablesPurchased : 0),
                CasualWards = g.Sum(x => x.PublicMatch == 1 ? x.WardsPlaced : 0),
                CasualFirstBloods = g.Sum(x => x.PublicMatch == 1 ? x.FirstBlood : 0),
                CasualDoubleKills = g.Sum(x => x.PublicMatch == 1 ? x.DoubleKill : 0),
                CasualTripleKills = g.Sum(x => x.PublicMatch == 1 ? x.TripleKill : 0),
                CasualQuadKills = g.Sum(x => x.PublicMatch == 1 ? x.QuadKill : 0),
                CasualAnnihilations = g.Sum(x => x.PublicMatch == 1 ? x.Annihilation : 0),
                CasualKS3 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak03 : 0),
                CasualKS4 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak04 : 0),
                CasualKS5 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak05 : 0),
                CasualKS6 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak06 : 0),
                CasualKS7 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak07 : 0),
                CasualKS8 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak08 : 0),
                CasualKS9 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak09 : 0),
                CasualKS10 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak10 : 0),
                CasualKS15 = g.Sum(x => x.PublicMatch == 1 ? x.KillStreak15 : 0),
                CasualSmackdowns = g.Sum(x => x.PublicMatch == 1 ? x.Smackdown : 0),
                CasualHumiliations = g.Sum(x => x.PublicMatch == 1 ? x.Humiliation : 0),
                CasualNemesis = g.Sum(x => x.PublicMatch == 1 ? x.Nemesis : 0),
                CasualRetribution = g.Sum(x => x.PublicMatch == 1 ? x.Retribution : 0),
                CasualTimeEarningExp = g.Sum(x => x.PublicMatch == 1 ? x.TimeEarningExperience : 0),
                CasualBuybacks = g.Sum(x => x.PublicMatch == 1 ? x.Buybacks : 0),

                // Public (Custom/Unranked where RankedMatch=0 AND PublicMatch=0? Or is PublicMatch a misnomer?)
                // Assuming "Public" games are those where RankedMatch == 0 and PublicMatch == 0 (Legacy HoN logic: TMM=Ranked/Casual, Public=Custom)
                // However, DTO uses "PublicMatch" field which maps to 'public_match' column.
                // If the user means "Public Game" stats tab, it often refers to 'PublicMatch == 1' in older contexts, but 'RankedMatch == 1' is TMM.
                // Let's assume Public Games are NON-Matchmaking games (RankedMatch=0).
                // But wait, Casual is Matchmaking.
                // Let's assume:
                // Ranked = RankedMatch == 1
                // Casual = PublicMatch == 1 (Wait, is Casual distinct from Public?)
                // Usually: Ranked (TMM), Casual (TMM), Public (Custom).
                // Let's filter Public as (RankedMatch == 0 AND PublicMatch == 0).
                PublicMatches = g.Count(x => x.RankedMatch == 0 && x.PublicMatch == 0),
                PublicWins = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Win : 0),
                PublicLosses = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Loss : 0),
                PublicRatingChange = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.PublicSkillRatingChange : 0),
                PublicDiscos = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Disconnected : 0),
                PublicKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.HeroKills : 0),
                PublicDeaths = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.HeroDeaths : 0),
                PublicAssists = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.HeroAssists : 0),
                PublicExp = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? (long)x.Experience : 0),
                PublicGold = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? (long)x.Gold : 0),
                PublicSeconds = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? (long)x.SecondsPlayed : 0),

                PublicDenies = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Denies : 0),
                PublicHeroDamage = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.HeroDamage : 0),
                PublicHeroGold = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.GoldFromHeroKills : 0),
                PublicGoldLost = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.GoldLostToDeath : 0),
                PublicSecondsDead = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.SecondsDead : 0),
                PublicTeamCreepKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TeamCreepKills : 0),
                PublicTeamCreepDmg = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TeamCreepDamage : 0),
                PublicTeamCreepGold = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TeamCreepGold : 0),
                PublicTeamCreepExp = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TeamCreepExperience : 0),
                PublicNeutralCreepKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.NeutralCreepKills : 0),
                PublicNeutralCreepDmg = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.NeutralCreepDamage : 0),
                PublicNeutralCreepGold = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.NeutralCreepGold : 0),
                PublicNeutralCreepExp = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.NeutralCreepExperience : 0),
                PublicBuildingDmg = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.BuildingDamage : 0),
                PublicBuildingsRazed = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.BuildingsRazed : 0),
                PublicBuildingExp = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.ExperienceFromBuildings : 0),
                PublicBuildingGold = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.GoldFromBuildings : 0),
                PublicExpDenied = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.ExperienceDenied : 0),
                PublicGoldSpent = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.GoldSpent : 0),
                PublicActions = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Actions : 0),
                PublicConsumables = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.ConsumablesPurchased : 0),
                PublicWards = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.WardsPlaced : 0),
                PublicFirstBloods = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.FirstBlood : 0),
                PublicDoubleKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.DoubleKill : 0),
                PublicTripleKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TripleKill : 0),
                PublicQuadKills = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.QuadKill : 0),
                PublicAnnihilations = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Annihilation : 0),
                PublicKS3 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak03 : 0),
                PublicKS4 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak04 : 0),
                PublicKS5 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak05 : 0),
                PublicKS6 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak06 : 0),
                PublicKS7 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak07 : 0),
                PublicKS8 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak08 : 0),
                PublicKS9 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak09 : 0),
                PublicKS10 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak10 : 0),
                PublicKS15 = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.KillStreak15 : 0),
                PublicSmackdowns = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Smackdown : 0),
                PublicHumiliations = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Humiliation : 0),
                PublicNemesis = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Nemesis : 0),
                PublicRetribution = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Retribution : 0),
                PublicTimeEarningExp = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.TimeEarningExperience : 0),
                PublicBuybacks = g.Sum(x => x.RankedMatch == 0 && x.PublicMatch == 0 ? x.Buybacks : 0),

                // Calculate Last Activity (Max Timestamp)
                LastMatchDate = MerrickContext.MatchStatistics
                    .Where(ms => g.Select(x => x.MatchID).Contains(ms.MatchID))
                    .Max(ms => (DateTimeOffset?)ms.TimestampRecorded),

                // Calculate Top 5 Heroes by Playtime
                // Note: We need to do this computation either in memory or via a subquery. 
                // Grouping by HeroProductID within the filtered set.
                TopHeroes = g.Where(x => x.HeroProductID.HasValue)
                    .GroupBy(x => x.HeroProductID!.Value)
                    .Select(heroGroup => new FavHeroDTO 
                    { 
                        HeroId = heroGroup.Key, 
                        SecondsPlayed = heroGroup.Sum(x => x.SecondsPlayed) 
                    })
                    .OrderByDescending(x => x.SecondsPlayed)
                    .Take(5)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return aggregated ?? new PlayerStatisticsAggregatedDTO();
    }

    public async Task<List<PlayerMasteryStatDTO>> GetPlayerMasteryStatsAsync(int accountId)
    {
        return await MerrickContext.PlayerStatistics
            .Where(ps => ps.AccountID == accountId && ps.HeroProductID.HasValue)
            .GroupBy(ps => ps.HeroProductID!.Value)
            .Select(g => new
            {
                HeroId = g.Key,
                SecondsPlayed = g.Sum(x => x.SecondsPlayed),
                Wins = g.Sum(x => x.Win),
                Losses = g.Sum(x => x.Loss)
            })
            .Select(x => new PlayerMasteryStatDTO
            {
                HeroId = x.HeroId,
                // XP = Seconds * 5 (Approx)
                Experience = x.SecondsPlayed * 5.0,
                // Level = (XP / 2000) + 1
                Level = (int)((x.SecondsPlayed * 5.0) / 2000.0) + 1,
                Wins = x.Wins,
                Losses = x.Losses
            })
            .ToListAsync();
    }
}
