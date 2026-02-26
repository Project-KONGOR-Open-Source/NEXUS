using System.Globalization;

using KONGOR.MasterServer.Models.Configuration;
using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

using MERRICK.DatabaseContext.Extensions;
using Microsoft.Extensions.Options;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class InitStatsHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    IPlayerStatisticsService statisticsService,
    IHeroDefinitionService heroDefinitions,
    IOptions<OperationalConfiguration> operationalConfiguration,
    ILogger<InitStatsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private IPlayerStatisticsService StatisticsService { get; } = statisticsService;
    private IHeroDefinitionService HeroDefinitions { get; } = heroDefinitions;
    private OperationalConfiguration OperationalConfiguration { get; } = operationalConfiguration.Value;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[InitStats] Request received. Cookie: {Cookie}")]
    private partial void LogRequestReceived(string cookie);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[InitStats] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[InitStats] Session not found for cookie '{Cookie}'.")]
    private partial void LogSessionNotFound(string cookie);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[InitStats] Fetching account details for '{AccountName}'...")]
    private partial void LogFetchingAccount(string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[InitStats] Account '{AccountName}' not found in DB.")]
    private partial void LogAccountNotFound(string accountName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[InitStats] Computing full stats for '{AccountName}'...")]
    private partial void LogComputingStats(string accountName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[InitStats] Constructing response dictionary.")]
    private partial void LogConstructingResponse();

    [LoggerMessage(Level = LogLevel.Information, Message = "[InitStats] Response generated. Length: {Length}")]
    private partial void LogResponseGenerated(int length);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? cookie = ClientRequestHelper.GetCookie(Request);

        LogRequestReceived(cookie ?? "NULL");

        if (cookie is null)
        {
            LogMissingCookie();
            return new BadRequestObjectResult(@"Missing Value For Form Parameter ""cookie""");
        }

        string? accountName = context.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            LogSessionNotFound(cookie);
            return new UnauthorizedObjectResult("Session Not Found");
        }

        LogFetchingAccount(accountName);
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
        {
            LogAccountNotFound(accountName);
            return new NotFoundObjectResult("Account Not Found");
        }

        LogComputingStats(accountName);

        // Fetch aggregated player stats for the account
        PlayerStatisticsAggregatedDTO stats = await StatisticsService.GetAggregatedStatisticsAsync(account.ID);

        ShowSimpleStatsResponse fullResponse = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, int.Parse(OperationalConfiguration.CurrentSeason), HeroDefinitions);

        LogConstructingResponse();
        // Restored standard keys (slot_id, tokens) as strict removal might cause client instability.
        Dictionary<string, object> response = new()
        {
            { "nickname", account.GetNameWithClanTag() },
            { "account_id", fullResponse.ID },
            { "level", fullResponse.Level.ToString(CultureInfo.InvariantCulture) },
            { "level_exp", fullResponse.LevelExperience.ToString(CultureInfo.InvariantCulture) },
            { "avatar_num", fullResponse.NumberOfAvatarsOwned },
            { "hero_num", fullResponse.NumberOfHeroesOwned },
            { "total_played", fullResponse.TotalMatchesPlayed },
            { "season_id", fullResponse.CurrentSeason.ToString(CultureInfo.InvariantCulture) },
            { "season_level", fullResponse.SeasonLevel.ToString(CultureInfo.InvariantCulture) },
            { "creep_level", fullResponse.CreepLevel },
            { "season_normal", new Dictionary<string, string>
                {
                    { "wins", fullResponse.SimpleSeasonStats.RankedMatchesWon.ToString(CultureInfo.InvariantCulture) },
                    { "losses", fullResponse.SimpleSeasonStats.RankedMatchesLost.ToString(CultureInfo.InvariantCulture) },
                    { "rnk_wins", fullResponse.SimpleSeasonStats.RankedMatchesWon.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "rnk_losses", fullResponse.SimpleSeasonStats.RankedMatchesLost.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "ranked_matches_won", fullResponse.SimpleSeasonStats.RankedMatchesWon.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "ranked_matches_lost", fullResponse.SimpleSeasonStats.RankedMatchesLost.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "win_streak", fullResponse.SimpleSeasonStats.WinStreak.ToString(CultureInfo.InvariantCulture) },
                    { "rank", fullResponse.SimpleSeasonStats.CurrentRank },
                    { "current_level", fullResponse.SimpleSeasonStats.LegacyLevel },
                    { "smr", fullResponse.SimpleSeasonStats.RankedRating },
                    { "highest_level_current", fullResponse.SimpleSeasonStats.HighestRank ?? "0" },
                    { "is_placement", fullResponse.SimpleSeasonStats.InPlacementPhase.ToString(CultureInfo.InvariantCulture) },
                    { "pub_skill", fullResponse.SimpleSeasonStats.PublicRating },
                    { "kam_rating", fullResponse.SimpleSeasonStats.CasualRating }
                }
            },
            { "season_casual", new Dictionary<string, string>
                {
                    { "con_wins", fullResponse.SimpleCasualSeasonStats.CasualWins.ToString(CultureInfo.InvariantCulture) },
                    { "con_losses", fullResponse.SimpleCasualSeasonStats.CasualLosses.ToString(CultureInfo.InvariantCulture) },
                    { "cs_wins", fullResponse.SimpleCasualSeasonStats.CasualWins.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "cs_losses", fullResponse.SimpleCasualSeasonStats.CasualLosses.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "casual_matches_won", fullResponse.SimpleCasualSeasonStats.CasualWins.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "casual_matches_lost", fullResponse.SimpleCasualSeasonStats.CasualLosses.ToString(CultureInfo.InvariantCulture) }, // Legacy Fallback
                    { "win_streak", fullResponse.SimpleCasualSeasonStats.WinStreak.ToString(CultureInfo.InvariantCulture) },
                    { "rank", fullResponse.SimpleCasualSeasonStats.CurrentRank },
                    { "current_level", fullResponse.SimpleCasualSeasonStats.LegacyLevel },
                    { "smr", fullResponse.SimpleCasualSeasonStats.RankedRating },
                    { "highest_level_current", fullResponse.SimpleCasualSeasonStats.HighestRank ?? "0" },
                    { "is_placement", fullResponse.SimpleCasualSeasonStats.InPlacementPhase.ToString(CultureInfo.InvariantCulture) },
                    { "pub_skill", fullResponse.SimpleCasualSeasonStats.PublicRating },
                    { "kam_rating", fullResponse.SimpleCasualSeasonStats.CasualRating }
                }
            },
            { "mvp_num", fullResponse.MVPAwardsCount },
            { "award_top4_name", fullResponse.Top4AwardNames },
            { "award_top4_num", fullResponse.Top4AwardCounts },
            { "points", account.User.GoldCoins.ToString(CultureInfo.InvariantCulture) },
            { "mmpoints", account.User.SilverCoins },
            { "slot_id", fullResponse.CustomIconSlotID },
            { "selected_upgrades", fullResponse.SelectedStoreItems },
            { "dice_tokens", fullResponse.DiceTokens },
            { "game_tokens", fullResponse.GameTokens },
            { "timestamp", fullResponse.ServerTimestamp },
            { "vested_threshold", 5 },
            {
                "quest_system",
                new Dictionary<string, object>
                {
                    {
                        "error", new Dictionary<string, int> { { "quest_status", 0 }, { "leaderboard_status", 0 } }
                    }
                }
            },
            {
                "season_system",
                new Dictionary<string, object>
                {
                    { "drop_diamonds", 0 }, { "cur_diamonds", 0 }, { "box_price", new Dictionary<int, int>() }
                }
            },
            {
                "con_reward",
                new Dictionary<string, object>
                {
                    { "old_lvl", 5 },
                    { "curr_lvl", 6 },
                    { "next_lvl", 0 },
                    { "require_rank", 0 },
                    { "need_more_play", 0 },
                    { "percentage_before", "0.92" },
                    { "percentage", "1.00" }
                }
            },
            { "0", true }
        };

        return new ContentResult { Content = PhpSerialization.Serialize(response), ContentType = "text/plain; charset=utf-8", StatusCode = 200 };
    }
}
