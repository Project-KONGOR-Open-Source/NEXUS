using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class HeroStatsHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<HeroStatsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[HeroStats] Request for {FunctionName}")]
    private partial void LogRequestForFunction(string functionName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[HeroStats] Returning empty dictionary stub.")]
    private partial void LogReturningEmptyStub();

    [LoggerMessage(Level = LogLevel.Information, Message = "[HeroStats] Returning mastery info for {AccountName}")]
    private partial void LogReturningMasteryInfo(string accountName);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        LogRequestForFunction(functionName ?? "NULL");

        if (functionName == "get_account_mastery")
        {
            return await HandleGetAccountMastery(context, Request);
        }

        // Stubs for get_account_all_hero_stats, get_campaign_hero_stats
        Dictionary<string, object> response = new();
        LogReturningEmptyStub();
        return new OkObjectResult(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleGetAccountMastery(HttpContext context, HttpRequest Request)
    {
        string? cookie = ClientRequestHelper.GetCookie(Request);

        if (string.IsNullOrEmpty(cookie))
        {
            return new BadRequestObjectResult("Missing cookie");
        }

        string? accountName = context.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            return new UnauthorizedObjectResult("Session Not Found");
        }

        Account? account = await MerrickContext.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account is null)
        {
            return new NotFoundObjectResult("Account Not Found");
        }

        // Replicating logic from original stub: returns mastery info structure
        Dictionary<string, object> response = new()
        {
            { "error_code", 0 },
            { "account_id", account.ID },
            { "mastery_info", new Dictionary<string, object>() }
        };

        LogReturningMasteryInfo(accountName);
        return new OkObjectResult(PhpSerialization.Serialize(response));
    }
}
