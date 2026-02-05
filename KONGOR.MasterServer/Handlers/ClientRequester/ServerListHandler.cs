using KONGOR.MasterServer.Services.Requester;

// Ensure using for PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class ServerListHandler(IDatabase distributedCache, ILogger<ServerListHandler> logger) : IClientRequestHandler
{
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[ServerList] Request received. Cookie: {Cookie}")]
    private partial void LogRequestReceived(string cookie);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[ServerList] Missing cookie.")]
    private partial void LogMissingCookie();

    [LoggerMessage(Level = LogLevel.Information, Message = "[ServerList] GameType: {GameType}")]
    private partial void LogGameType(string gameType);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ServerList] Found {Count} servers.")]
    private partial void LogServersFound(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ServerList] Returning list for joining (10).")]
    private partial void LogReturningJoinList();

    [LoggerMessage(Level = LogLevel.Information, Message = "[ServerList] Returning list for creation (90).")]
    private partial void LogReturningCreateList();

    [LoggerMessage(Level = LogLevel.Error, Message = "[ServerList] Unknown Server List Game Type \"{GameType}\"")]
    private partial void LogUnknownGameType(string gameType);

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

        string? gameType = Request.Form.ContainsKey("gametype") ? Request.Form["gametype"].ToString() : null;

        LogGameType(gameType ?? "NULL");

        List<MatchServer> servers = await DistributedCache.GetMatchServers();
        LogServersFound(servers.Count);

        foreach (MatchServer server in servers)
        {
            Logger.LogInformation("[ServerList] Server {ID} Status: {Status}", server.ID, server.Status);
        }

        string? remoteIpAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        switch (gameType)
        {
            case "10": // List Of Match Servers On Which Matches Can Be Joined
                LogReturningJoinList();
                ServerForJoinListResponse serversForJoin = new(servers, cookie, remoteIpAddress);
                return new OkObjectResult(PhpSerialization.Serialize(serversForJoin));

            case "90": // List Of Match Servers On Which Matches Can Be Created
                LogReturningCreateList();
                ServerForCreateListResponse serversForCreate = new(servers, cookie, remoteIpAddress);
                return new OkObjectResult(PhpSerialization.Serialize(serversForCreate));

            default:
                LogUnknownGameType(gameType ?? "NULL");
                return new UnprocessableEntityObjectResult($@"Unknown Server List Game Type ""{gameType ?? "NULL"}""");
        }
    }
}