using System.Globalization;

using KONGOR.MasterServer.Services; // For PhpSerialization, HeroDefinitionService
using KONGOR.MasterServer.Services.Requester;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class MatchHistoryHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    IHeroDefinitionService heroDefinitionService,
    ILogger<MatchHistoryHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private IHeroDefinitionService HeroDefinitionService { get; } = heroDefinitionService;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[MatchHistory] GrabLastMatches for '{AccountName}'")]
    private partial void LogGrabLastMatches(string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[MatchHistory] Session/Account Not Found")]
    private partial void LogSessionNotFound();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[MatchHistory] Account Not Found in DB")]
    private partial void LogAccountNotFound();

    [LoggerMessage(Level = LogLevel.Information, Message = "[MatchHistory] Overview for '{Nickname}' (Table: {Table})")]
    private partial void LogHistoryOverview(string nickname, string table);

    [LoggerMessage(Level = LogLevel.Information, Message = "[MatchHistory] Returning {Count} entries.")]
    private partial void LogReturningEntries(int count);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        // Dispatch based on function name or analyze internal logic
        // "grab_last_matches", "grab_last_matches_from_nick" -> HandleGrabLastMatches
        // "match_history_overview" -> HandleMatchHistoryOverview

        if (functionName == "match_history_overview")
        {
            return await HandleMatchHistoryOverview(Request);
        }
        else
        {
            return await HandleGrabLastMatches(context, Request);
        }
    }

    private async Task<IActionResult> HandleGrabLastMatches(HttpContext context, HttpRequest Request)
    {
        string? accountName = Request.Form["nickname"];

        if (string.IsNullOrEmpty(accountName))
        {
            string? cookie = ClientRequestHelper.GetCookie(Request);
            accountName = context.Items["SessionAccountName"] as string
                          ?? await DistributedCache.GetAccountNameForSessionCookie(cookie ?? "NULL");
        }

        LogGrabLastMatches(accountName ?? "NULL");

        if (accountName is null)
        {
            LogSessionNotFound();
            return new UnauthorizedObjectResult("Session Not Found");
        }

        Account? account = await MerrickContext.Accounts
            .FirstOrDefaultAsync(a => a.Name.Equals(accountName));

        if (account is null)
        {
            LogAccountNotFound();
            return new NotFoundObjectResult("Account Not Found");
        }

        List<int> lastMatchIDs = await MerrickContext.PlayerStatistics
            .Where(ps => ps.AccountID == account.ID)
            .OrderByDescending(ps => ps.MatchID)
            .Take(100)
            .Select(ps => ps.MatchID)
            .ToListAsync();

        Dictionary<int, string> lastStats = new();
        foreach (int info in lastMatchIDs)
        {
            lastStats[info] = info.ToString(CultureInfo.InvariantCulture);
        }

        Dictionary<string, object> response = new()
        {
            { "last_stats", lastStats },
            { "success", "True" },
            { "hosttime", (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        return new OkObjectResult(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleMatchHistoryOverview(HttpRequest Request)
    {
        string? nickname = Request.Form["nickname"];
        string? table = Request.Form["table"];

        LogHistoryOverview(nickname ?? "NULL", table ?? "NULL");

        if (string.IsNullOrEmpty(nickname))
        {
            return new BadRequestObjectResult("Missing nickname");
        }

        var query = from ps in MerrickContext.PlayerStatistics
                    join ms in MerrickContext.MatchStatistics on ps.MatchID equals ms.MatchID
                    where ps.AccountName == nickname
                    select new { ps, ms };

        switch (table)
        {
            case "campaign": // Season Normal (Ranked)
                query = query.Where(x => x.ps.RankedMatch == 1);
                break;
            case "player": // Public Game Stats
                query = query.Where(x => x.ps.PublicMatch == 1);
                break;
            case "midwars": // Others/Midwars
            case "other":
            case "others":
                query = query.Where(x => x.ms.GameMode == "midwars");
                break;
            case "riftwars": // Riftwars
                query = query.Where(x => x.ms.GameMode == "riftwars");
                break;
            default:
                // Strict filtering
                query = query.Where(x => false);
                break;
        }

        var historyData = await query
            .OrderByDescending(x => x.ms.TimestampRecorded)
            .Take(100)
            .Select(x => new
            {
                x.ps.MatchID,
                x.ps.Team,
                x.ps.HeroKills,
                x.ps.HeroDeaths,
                x.ps.HeroAssists,
                x.ps.HeroProductID,
                x.ps.Win,
                x.ms.Map,
                x.ms.TimestampRecorded,
                x.ms.TimePlayed,
                x.ms.FileName
            })
            .ToListAsync();

        Dictionary<string, string> matchHistoryOverview = new();

        int i = 0;
        foreach (var match in historyData)
        {
            string map = match.Map;
            string date = match.TimestampRecorded.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            int duration = match.TimePlayed;

            uint baseHeroId = HeroDefinitionService.GetBaseHeroId(match.HeroProductID ?? 0);
            string heroIdentifierString = HeroDefinitionService.GetHeroIdentifier(match.HeroProductID ?? 0);

            string matchData = string.Join(',',
                match.MatchID,
                match.Win,
                match.Team,
                match.HeroKills,
                match.HeroDeaths,
                match.HeroAssists,
                baseHeroId,
                duration,
                map,
                date,
                heroIdentifierString
            );

            matchHistoryOverview.Add("m" + i, matchData);
            i++;
        }

        LogReturningEntries(matchHistoryOverview.Count);
        return new OkObjectResult(PhpSerialization.Serialize(matchHistoryOverview));
    }
}
