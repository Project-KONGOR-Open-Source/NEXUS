using KONGOR.MasterServer.Models.Configuration;
using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

using Microsoft.Extensions.Options;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class SimpleStatsHandler(
    MerrickContext databaseContext,
    IPlayerStatisticsService statisticsService,
    IHeroDefinitionService heroDefinitions,
    IOptions<OperationalConfiguration> operationalConfiguration,
    ILogger<SimpleStatsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IPlayerStatisticsService StatisticsService { get; } = statisticsService;
    private IHeroDefinitionService HeroDefinitions { get; } = heroDefinitions;
    private OperationalConfiguration OperationalConfiguration { get; } = operationalConfiguration.Value;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[SimpleStats] Request received. Nickname: {AccountName}")]
    private partial void LogRequestReceived(string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[SimpleStats] Missing nickname.")]
    private partial void LogMissingNickname();

    [LoggerMessage(Level = LogLevel.Debug, Message = "[SimpleStats] Fetching account details for '{AccountName}'...")]
    private partial void LogFetchingAccount(string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[SimpleStats] Account '{AccountName}' not found.")]
    private partial void LogAccountNotFound(string accountName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[SimpleStats] Computing stats for '{AccountName}'...")]
    private partial void LogComputingStats(string accountName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[SimpleStats] Successfully generated stats for '{AccountName}'.")]
    private partial void LogGeneratedStats(string accountName);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? accountName = Request.Form["nickname"];
        
        // Logan (2025-02-13): Simple Sanitization
        // Strip any unexpected suffixes from the nickname to ensure we find the correct account.
        if (accountName?.EndsWith("|B64") == true)
        {
            accountName = accountName[..^4];
            Logger.LogInformation("[SimpleStats] Sanitized Nickname: '{AccountName}'", accountName);
        }

        LogRequestReceived(accountName ?? "NULL");

        if (accountName is null)
        {
            LogMissingNickname();
            return new BadRequestObjectResult(@"Missing Value For Form Parameter ""nickname""");
        }

        LogFetchingAccount(accountName);
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name == accountName);

        if (account is null)
        {
            LogAccountNotFound(accountName);
            return new NotFoundObjectResult($@"Account With Name ""{accountName}"" Was Not Found");
        }

        LogComputingStats(accountName);

        // Fetch aggregated player stats for the account
        PlayerStatisticsAggregatedDTO stats = await StatisticsService.GetAggregatedStatisticsAsync(account.ID);

        ShowSimpleStatsResponse props = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats, int.Parse(OperationalConfiguration.CurrentSeason), HeroDefinitions);
        
        // Attach generic stats to simple stats for simpler legacy mapping access
        props.SimpleSeasonStats.GenericStats = stats;

        string? tableParam = Request.Form["table"];
        string? functionName = Request.Form["f"].FirstOrDefault()?.ToLower() ?? Request.Query["f"].FirstOrDefault()?.ToLower();

        // Populate mastery data on all stats requests so the Profile Overview tab can display the total mastery score.
        List<PlayerMasteryStatDTO> masteryStats = await StatisticsService.GetPlayerMasteryStatsAsync(account.ID);
        List<FavHeroDTO> masteryDTOs = masteryStats.Select(m => new FavHeroDTO
        {
            HeroId = (ushort)m.HeroId,
            SecondsPlayed = (long)(m.Experience / 5.0) // Reverse XP calc: XP = Sec * 5
        }).ToList();
        
        props.MasteryInfo = ClientRequestHelper.GenerateMasteryInfo(masteryDTOs, HeroDefinitions);
        props.MasteryRewards = ClientRequestHelper.GenerateMasteryRewards();
        props.ConReward = $"{props.CurrentRankTop ?? "0"},1,3,2,11,0,0";

        // Default Hybrid Response (Now includes Mastery if populated)
        object response;
        if (tableParam == "mastery")
        {
            response = new Dictionary<object, object>
            {
                { "nickname", props.NameWithClanTag ?? accountName },
                { "mastery_info", props.MasteryInfo ?? new List<Dictionary<string, string>>() },
                { "mastery_rewards", props.MasteryRewards ?? "" }
            };
        }
        else 
        {
            response = ClientRequestHelper.CreateHybridSimpleStats(props, tableParam);
        }

        LogGeneratedStats(accountName);
        
        Logger.LogInformation("[SimpleStats] Starting PHP Serialization...");
        string finalInnerPayload = PhpSerialization.Serialize(response);
        Logger.LogInformation("[SimpleStats] Validated Payload Length: {Len}", finalInnerPayload.Length);
        
        Logger.LogInformation("[SimpleStats] Sending Plain PHP Serialized response (text/plain).");
        
        // Force Content-Type by writing directly to request
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(finalInnerPayload);

        return new EmptyResult();
    }
}
