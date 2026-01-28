using KONGOR.MasterServer.Models.RequestResponse.Stats;

using MERRICK.DatabaseContext.Extensions;

namespace KONGOR.MasterServer.Services;

public interface IPlayerStatisticsService
{
    Task<PlayerStatisticsAggregatedDTO> GetAggregatedStatisticsAsync(int accountId);
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
                CasualSeconds = g.Sum(x => x.PublicMatch == 1 ? (long) x.SecondsPlayed : 0)
            })
            .FirstOrDefaultAsync();

        return aggregated ?? new PlayerStatisticsAggregatedDTO();
    }
}
