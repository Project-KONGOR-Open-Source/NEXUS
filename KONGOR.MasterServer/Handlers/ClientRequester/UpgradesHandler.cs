using System.Globalization;

using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

using MERRICK.DatabaseContext.Extensions;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class UpgradesHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    IPlayerStatisticsService statisticsService,
    ILogger<UpgradesHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private IPlayerStatisticsService StatisticsService { get; } = statisticsService;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Upgrades] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Upgrades] HandleGetUpgrades Called. Cookie: '{Cookie}'")]
    private partial void LogHandleGetUpgrades(string cookie);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Upgrades] Validating cookie via DistributedCache...")]
    private partial void LogValidatingCookie();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Upgrades] Validation Result: IsValid={IsValid}, Account={Account}")]
    private partial void LogValidationResult(bool isValid, string? account);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[Upgrades] Validation Failed for cookie '{Cookie}'")]
    private partial void LogValidationFailed(string cookie);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Upgrades] Account '{AccountName}' not found in DB.")]
    private partial void LogAccountNotFound(string accountName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Upgrades] Response Length: {Length}")]
    private partial void LogResponseLength(int length);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Upgrades] CRITICAL FAILURE in HandleGetUpgrades")]
    private partial void LogCriticalFailure(Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Upgrades] Response Payload: \n{Payload}")]
    private partial void LogResponsePayload(string payload);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        if (functionName == "debug_upgrades_diff")
        {
            return await HandleDebugUpgradesDiff(context, Request);
        }
        else
        {
            return await HandleGetUpgrades(context, Request);
        }
    }

    private async Task<IActionResult> HandleGetUpgrades(HttpContext context, HttpRequest Request)
    {
        try
        {
            string? cookie = ClientRequestHelper.GetCookie(Request);
            if (string.IsNullOrEmpty(cookie))
            {
                LogMissingCookie();
                return new UnauthorizedResult();
            }

            LogHandleGetUpgrades(cookie);

            string? sessionAccountName = context.Items["SessionAccountName"] as string;

            if (sessionAccountName is null)
            {
                LogValidatingCookie();
                (bool accountSessionCookieIsValid, string? cacheAccountName) =
                    await DistributedCache.ValidateAccountSessionCookie(cookie);
                LogValidationResult(accountSessionCookieIsValid, cacheAccountName);

                if (accountSessionCookieIsValid)
                {
                    sessionAccountName = cacheAccountName;
                }
            }

            if (sessionAccountName is null)
            {
                LogValidationFailed(cookie);
                return new UnauthorizedObjectResult($@"No Session Found For Cookie ""{cookie}""");
            }

            Account? account = await MerrickContext.Accounts
                .Include(a => a.User)
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.Name == sessionAccountName);

            if (account is null)
            {
                LogAccountNotFound(sessionAccountName);
                return new UnauthorizedObjectResult(@"Account Not Found");
            }

            // Optimized Statistics Retrieval
            PlayerStatisticsAggregatedDTO stats = await StatisticsService.GetAggregatedStatisticsAsync(account.ID);
            ShowSimpleStatsResponse fullStats = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats);

            double rnkRating = 1500.0 + stats.RankedRatingChange;
            double csRating = 1500.0 + stats.CasualRatingChange;

            Dictionary<string, object> accountFieldStats = new()
            {
                ["account_id"] = account.ID.ToString(),
                ["super_id"] = account.ID.ToString(),
                ["beta"] = "1",
                ["lan"] = "0",
                ["account_type"] = "4", // Legacy/Verified
                ["standing"] = "3",

                ["rnk_amm_team_rating"] = rnkRating.ToString("F3", CultureInfo.InvariantCulture),
                ["cs_amm_team_rating"] = csRating.ToString("F3", CultureInfo.InvariantCulture),
                ["mid_amm_team_rating"] = "1500.000",
                ["rift_amm_team_rating"] = "1500.000",
                ["rb_amm_team_rating"] = "1500.000",

                ["acc_games_played"] = stats.TotalMatches.ToString(),
                ["rnk_games_played"] = stats.RankedMatches.ToString(),
                ["cs_games_played"] = stats.CasualMatches.ToString(),
                ["mid_games_played"] = "0",
                ["rift_games_played"] = "0",
                ["bot_games_played"] = "0",

                ["acc_discos"] = stats.Disconnected.ToString(),
                ["rnk_discos"] = stats.RankedDiscos.ToString(),
                ["cs_discos"] = stats.CasualDiscos.ToString(),
                ["mid_discos"] = "0",
                ["rift_discos"] = "0",

                ["rnk_herokills"] = stats.RankedKills.ToString(),
                ["cs_herokills"] = stats.CasualKills.ToString(),

                ["rnk_deaths"] = stats.RankedDeaths.ToString(),
                ["cs_deaths"] = stats.CasualDeaths.ToString(),

                ["rnk_heroassists"] = stats.RankedAssists.ToString(),
                ["cs_heroassists"] = stats.CasualAssists.ToString(),

                ["rnk_exp"] = stats.RankedExp.ToString(),
                ["cs_exp"] = stats.CasualExp.ToString(),
                ["rb_exp"] = "0",

                ["rnk_gold"] = stats.RankedGold.ToString(),
                ["cs_gold"] = stats.CasualGold.ToString(),
                ["rb_gold"] = "0",

                ["rnk_secs"] = stats.RankedSeconds.ToString(),
                ["cs_secs"] = stats.CasualSeconds.ToString(),

                ["level"] = fullStats.Level.ToString(),
                ["level_exp"] = fullStats.LevelExperience.ToString(),

                ["campaign_casual_mmr"] = "1500.00",
                ["campaign_casual_medal"] = "0",
                ["campaign_casual_discos"] = 0, // Int
                ["campaign_casual_match_played"] = 0, // Int
                ["campaign_normal_mmr"] = "1500.00",
                ["campaign_normal_medal"] = "0",
                ["campaign_normal_discos"] = 0, // Int
                ["campaign_normal_match_played"] = 0 // Int
            };

            Dictionary<int, object> fieldStats = new()
            {
                { account.ID, accountFieldStats }
            };

            Dictionary<string, object> myUpgradesInfo = new();
            foreach (string item in account.User.OwnedStoreItems)
            {
                // Parity: Only send data, end_time, start_time (a:3 structure per item)
                myUpgradesInfo[item] = new Dictionary<string, string>
                {
                    { "data", "" },
                    { "end_time", "0" },
                    { "start_time", "0" }
                };
            }

            Dictionary<string, object> response = new()
            {
                ["standing"] = "3", // Top level standing
                ["field_stats"] = fieldStats,

                ["points"] = account.User.GoldCoins.ToString(CultureInfo.InvariantCulture),
                ["mmpoints"] = account.User.SilverCoins.ToString(CultureInfo.InvariantCulture),
                ["dice_tokens"] = fullStats.DiceTokens.ToString(), // String as per working capture
                ["season_level"] = fullStats.SeasonLevel,
                ["slot_id"] = fullStats.CustomIconSlotID.ToString(),

                ["my_upgrades"] = account.User.OwnedStoreItems.Distinct().ToList(),
                ["selected_upgrades"] = account.SelectedStoreItems, // Order parity
                ["game_tokens"] = fullStats.GameTokens, // Order parity: after selected
                ["my_upgrades_info"] = myUpgradesInfo, // Order parity: last of this block

                ["season_normal"] = fullStats.SimpleSeasonStats,
                ["season_casual"] = fullStats.SimpleCasualSeasonStats,
                ["vested_threshold"] = 5,
                ["0"] = true
            };

            string serializedResponse = PhpSerialization.Serialize(response);
            LogResponseLength(serializedResponse.Length);
            LogResponsePayload(serializedResponse);

            return new ContentResult { Content = serializedResponse, ContentType = "text/plain; charset=utf-8", StatusCode = 200 };
        }
        catch (Exception ex)
        {
            LogCriticalFailure(ex);
            return new StatusCodeResult(500);
        }
    }

    private async Task<IActionResult> HandleDebugUpgradesDiff(HttpContext context, HttpRequest Request)
    {
        try
        {
            string? cookie = Request.Form["cookie"];
            if (string.IsNullOrEmpty(cookie))
            {
                return new BadRequestObjectResult("Missing cookie");
            }

            string? sessionAccountName = context.Items["SessionAccountName"] as string
                                         ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

            if (sessionAccountName is null)
            {
                return new UnauthorizedObjectResult("Session Not Found");
            }

            Account? account = await MerrickContext.Accounts
                .Include(a => a.User)
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.Name == sessionAccountName);

            if (account is null)
            {
                return new NotFoundObjectResult("Account Not Found");
            }

            StringBuilder report = new();
            report.AppendLine($"--- Payload Comparison Report for {sessionAccountName} ---");

            PlayerStatisticsAggregatedDTO stats = await StatisticsService.GetAggregatedStatisticsAsync(account.ID);
            ShowSimpleStatsResponse fullStats = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats);

            Dictionary<string, object> initStatsReference = new()
            {
                { "points", account.User.SilverCoins },
                { "mmpoints", account.User.GoldCoins },
                { "standing", "3" },
                { "0", true }
            };

            Dictionary<string, object> fieldStats = new()
            {
                ["nickname"] = account.GetNameWithClanTag(),
                ["account_id"] = fullStats.ID
            };
            Dictionary<string, object> myUpgradesInfo = new();
            foreach (string item in account.User.OwnedStoreItems)
            {
                myUpgradesInfo[item] = new Dictionary<string, string>
                {
                    { "data", "" },
                    { "end_time", "0" },
                    { "start_time", "0" }
                };
            }

            Dictionary<string, object> upgradesFix = new()
            {
                ["field_stats"] = fieldStats,
                ["my_upgrades_info"] = myUpgradesInfo,
                ["points"] = account.User.GoldCoins.ToString(CultureInfo.InvariantCulture),
                ["mmpoints"] = account.User.SilverCoins.ToString(CultureInfo.InvariantCulture),
                ["game_tokens"] = fullStats.GameTokens,
                ["standing"] = "3",
                ["0"] = true
            };

            report.AppendLine($"\n[Critical Key Analysis]");
            report.AppendLine($"InitStats (Working): Key '0' is STRING. Value: {initStatsReference["0"]}");
            report.AppendLine($"Upgrades (Proposed): Key '0' is STRING. Value: {upgradesFix["0"]}");

            report.AppendLine($"\n[Serialization Preview]");
            report.AppendLine($"InitStats '0' -> {PhpSerialization.Serialize(initStatsReference["0"])} (inside dict with string keys)");

            Dictionary<string, bool> d1 = new() { ["0"] = true };
            Dictionary<int, bool> d2 = new() { [0] = true };

            report.AppendLine($"Dict<string,bool> {{[\"0\"]=true}} -> {PhpSerialization.Serialize(d1)}");
            report.AppendLine($"Dict<int,bool> {{[0]=true}}       -> {PhpSerialization.Serialize(d2)}");

            report.AppendLine($"\n[Conclusion]");
            report.AppendLine("If InitStats works with s:1:\"0\";b:1;, then Upgrades MUST match it unless the client parser expects different types for different packets.");
            report.AppendLine("Current Fix sets it to i:0;b:1; (Int Key). Verify if this is d sired.");

            return new OkObjectResult(report.ToString());
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = 500 };
        }
    }
}
