using KONGOR.MasterServer.Services.Requester;
using KONGOR.MasterServer.Logging;

using KONGOR.MasterServer.Services;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public class GetPlayerAwardSummHandler(MerrickContext db, IPlayerStatisticsService statisticsService, ILogger<GetPlayerAwardSummHandler> logger) : IClientRequestHandler
{
    public string FunctionName => "get_player_award_summ";

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        string cookie = context.Request.Form["cookie"].ToString();
        string nickname = context.Request.Form["nickname"].ToString();

        logger.LogProcessingRequest(FunctionName, cookie, true);

        // Account Resolution logic
        Account? account = await db.Accounts
            .Include(a => a.Statistics)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name == nickname);

        if (account == null)
        {
            logger.LogWarning("Account not found for nickname: {Nickname}", nickname);
            return new ContentResult { Content = PhpSerialization.Serialize(new Dictionary<object, object>()), ContentType = "text/html" };
        }

        PlayerStatisticsAggregatedDTO? aggregatedStats = await statisticsService.GetAggregatedStatisticsAsync(account.ID);
        
        if (aggregatedStats == null) aggregatedStats = new PlayerStatisticsAggregatedDTO();

        // AccountStatistics fallback
        AccountStatistics? stats = account.Statistics?.FirstOrDefault(s => s.Type == AccountStatisticsType.Matchmaking) 
                                   ?? account.Statistics?.FirstOrDefault();

        // Helper function
        long GetStat(long aggregatedVal, int? accountStatVal)
        {
            if (aggregatedVal > 0) return aggregatedVal;
            return accountStatVal ?? 0;
        }

        // Return indexed dictionary (1-based) to match Lua GetVal(i)
        // 1: Annihilation (mann)
        // 2: Quad Kill (mqk)
        // 3: Kill Streak (lgks)
        // 4: Smackdown (msd)
        // 5: Most Kills (mkill)
        // 6: Most Assists (masst)
        // 7: Least Deaths (ledth)
        // 8: Most Building Damage (mbdmg)
        // 9: Most Wards Killed (mwk)
        // 10: Most Hero Damage (mhdd)
        // 11: Highest Creep Score (hcs)
        
        Dictionary<object, object> response = new Dictionary<object, object>
        {
            // Inject strictly expected strings mapped in player_stats_v2.lua GetVal(...) to avoid unordered Dictionary index shifting
            { "annihilation", GetStat(aggregatedStats.Annihilations, stats?.AwardMostAnnihilations).ToString() },
            { "quadkill", GetStat(aggregatedStats.MostQuadKills, stats?.AwardMostQuadKills).ToString() },
            { "ks15", aggregatedStats.HighestKillStreak.ToString() }, 
            { "smackdown", GetStat(aggregatedStats.Smackdowns, stats?.AwardMostSmackdowns).ToString() },
            { "mkill", GetStat(aggregatedStats.MostKills, stats?.AwardMostKills).ToString() },
            { "most_assists", GetStat(aggregatedStats.MostAssists, stats?.AwardMostAssists).ToString() },
            { "least_deaths", GetStat(aggregatedStats.LeastDeaths, stats?.AwardLeastDeaths).ToString() },
            { "most_building_damage", GetStat(aggregatedStats.MostBuildingDamage, stats?.AwardMostBuildingDamage).ToString() },
            { "most_hero_damage", GetStat(aggregatedStats.MostHeroDamage, stats?.AwardMostHeroDamageDealt).ToString() },
            { "most_creeps", GetStat(aggregatedStats.MostCreepKills, stats?.AwardMostCreepKills).ToString() },
            
            // Re-bind exact hardcoded index strings so legacy positional fallbacks work
            { "1", GetStat(aggregatedStats.Annihilations, stats?.AwardMostAnnihilations).ToString() },
            { "2", GetStat(aggregatedStats.MostQuadKills, stats?.AwardMostQuadKills).ToString() },
            { "3", aggregatedStats.HighestKillStreak.ToString() }, 
            { "4", GetStat(aggregatedStats.Smackdowns, stats?.AwardMostSmackdowns).ToString() },
            { "5", GetStat(aggregatedStats.MostKills, stats?.AwardMostKills).ToString() },
            { "6", GetStat(aggregatedStats.MostAssists, stats?.AwardMostAssists).ToString() },
            { "7", GetStat(aggregatedStats.LeastDeaths, stats?.AwardLeastDeaths).ToString() },
            { "8", GetStat(aggregatedStats.MostBuildingDamage, stats?.AwardMostBuildingDamage).ToString() },
            { "9", GetStat(aggregatedStats.MostWards, stats?.AwardMostWardsKilled).ToString() },
            { "10", GetStat(aggregatedStats.MostHeroDamage, stats?.AwardMostHeroDamageDealt).ToString() },
            { "11", GetStat(aggregatedStats.MostCreepKills, stats?.AwardMostCreepKills).ToString() },

            // Original legacy keys just in case
            { "awd_mann", GetStat(aggregatedStats.Annihilations, stats?.AwardMostAnnihilations).ToString() },
            { "awd_mqk", GetStat(aggregatedStats.MostQuadKills, stats?.AwardMostQuadKills).ToString() },
            { "lgks", aggregatedStats.HighestKillStreak.ToString() },
            { "awd_msd", GetStat(aggregatedStats.Smackdowns, stats?.AwardMostSmackdowns).ToString() },
            { "awd_mkill", GetStat(aggregatedStats.MostKills, stats?.AwardMostKills).ToString() },
            { "awd_mast", GetStat(aggregatedStats.MostAssists, stats?.AwardMostAssists).ToString() },
            { "awd_ldth", GetStat(aggregatedStats.LeastDeaths, stats?.AwardLeastDeaths).ToString() },
            { "awd_mbdmg", GetStat(aggregatedStats.MostBuildingDamage, stats?.AwardMostBuildingDamage).ToString() },
            { "awd_mwk", GetStat(aggregatedStats.MostWards, stats?.AwardMostWardsKilled).ToString() },
            { "awd_mhdd", GetStat(aggregatedStats.MostHeroDamage, stats?.AwardMostHeroDamageDealt).ToString() },
            { "hcs", GetStat(aggregatedStats.MostCreepKills, stats?.AwardMostCreepKills).ToString() },
            
            { "mvp_score", aggregatedStats.MVP.ToString() },
            { "vested_threshold", 5 },
            { 0, true }
        };

        // Serialize as PHP Array (Map)
        string innerPayload = PhpSerialization.Serialize(response);
        return new ContentResult { Content = innerPayload, ContentType = "text/plain; charset=utf-8" };
    }
}
