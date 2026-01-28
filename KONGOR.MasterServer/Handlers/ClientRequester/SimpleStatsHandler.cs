using KONGOR.MasterServer.Models.RequestResponse.Stats;
using KONGOR.MasterServer.Services;
using KONGOR.MasterServer.Services.Requester;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class SimpleStatsHandler(
    MerrickContext databaseContext,
    IPlayerStatisticsService statisticsService,
    ILogger<SimpleStatsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IPlayerStatisticsService StatisticsService { get; } = statisticsService;
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

        ShowSimpleStatsResponse response = ClientRequestHelper.CreateShowSimpleStatsResponse(account, stats);

        LogGeneratedStats(accountName);
        return new ContentResult { Content = PhpSerialization.Serialize(response), ContentType = "text/plain; charset=utf-8", StatusCode = 200 };
    }
}
