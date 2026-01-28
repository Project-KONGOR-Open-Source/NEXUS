using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

// For MatchStartData, ServerStatus, etc

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class GameHandler(IDatabase distributedCache, ILogger<GameHandler> logger) : IClientRequestHandler
{
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[GameHandler] Unknown function '{FunctionName}'.")]
    private partial void LogUnknownFunction(string functionName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[GameHandler] CreateGame request: Name='{Name}', Map='{Map}', Mode='{Mode}'")]
    private partial void LogCreateGameRequest(string name, string map, string mode);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[GameHandler] Missing required parameters.")]
    private partial void LogMissingParameters();

    [LoggerMessage(Level = LogLevel.Debug, Message = "[GameHandler] Fetching match servers...")]
    private partial void LogFetchingServers();

    [LoggerMessage(Level = LogLevel.Warning, Message = "[GameHandler] No Idle Match Servers Available. Total Servers: {Count}")]
    private partial void LogNoServersAvailable(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "[GameHandler] Selected Server: {ServerName} (ID: {ServerID})")]
    private partial void LogSelectedServer(string serverName, int serverId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[GameHandler] Unauthorized attempt to create game.")]
    private partial void LogUnauthorizedAttempt();

    [LoggerMessage(Level = LogLevel.Debug, Message = "[GameHandler] Saving MatchStartData for MatchID {MatchID}...")]
    private partial void LogSavingMatchData(int matchId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[GameHandler] Match Created! ID: {MatchID}, Host: {AccountName}")]
    private partial void LogMatchCreated(int matchId, string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[GameHandler] HandleNewGameAvailable called (STUB).")]
    private partial void LogNewGameAvailableStub();

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        // Dispatch based on function name
        switch (functionName)
        {
            case "create_game":
                return await HandleCreateGame(context, Request);
            case "new_game_available":
                return await HandleNewGameAvailable();
            default:
                LogUnknownFunction(functionName ?? "NULL");
                return new BadRequestObjectResult("Unknown function");
        }
    }

    private async Task<IActionResult> HandleCreateGame(HttpContext context, HttpRequest Request)
    {
        string? name = Request.Form["name"];
        string? map = Request.Form["map"];
        string? mode = Request.Form["mode"];
        string? teamSize = Request.Form["team_size"];
        string? privateGame = Request.Form["private"];

        LogCreateGameRequest(name ?? "NULL", map ?? "NULL", mode ?? "NULL");

        if (name is null || map is null || mode is null)
        {
            LogMissingParameters();
            return new BadRequestObjectResult(PhpSerialization.Serialize(new { error = "Missing Required Parameters" }));
        }

        LogFetchingServers();
        List<MatchServer> allServers = await DistributedCache.GetMatchServers();

        List<MatchServer> availableServers = allServers
            .Where(s => s.Status == ServerStatus.SERVER_STATUS_IDLE)
            .ToList();

        if (availableServers.Count == 0)
        {
            LogNoServersAvailable(allServers.Count);
            return new OkObjectResult(PhpSerialization.Serialize(new { match_id = 0, error = "No Servers Available" }));
        }

        MatchServer selectedServer = availableServers[Random.Shared.Next(availableServers.Count)];
        LogSelectedServer(selectedServer.Name ?? "Unknown", selectedServer.ID);

        string? accountName = context.Items["SessionAccountName"] as string;
        if (accountName is null)
        {
            LogUnauthorizedAttempt();
            return new UnauthorizedObjectResult(PhpSerialization.Serialize(new { error = "Unauthorized" }));
        }

        MatchStartData matchStartData = new()
        {
            MatchName = name,
            ServerID = selectedServer.ID,
            ServerName = selectedServer.Name ?? "Unknown Server",
            HostAccountName = accountName,
            Map = map,
            MatchMode = mode,
            Version = "4.10.1",
            IsCasual = mode.Contains("casual", StringComparison.OrdinalIgnoreCase),
            MatchType = privateGame == "1" ? (byte) 3 : (byte) 0,
            Options = MatchOptions.None
        };

        LogSavingMatchData(matchStartData.MatchID);
        await DistributedCache.SetMatchStartData(matchStartData);

        LogMatchCreated(matchStartData.MatchID, accountName);

        return new OkObjectResult(PhpSerialization.Serialize(new Dictionary<string, object>
        {
            { "match_id", matchStartData.MatchID },
            { "server_id", selectedServer.ID },
            { "server_address", selectedServer.IPAddress },
            { "server_port", selectedServer.Port },
            { "error", false }
        }));
    }

    private Task<IActionResult> HandleNewGameAvailable()
    {
        LogNewGameAvailableStub();
        return Task.FromResult<IActionResult>(new OkObjectResult(PhpSerialization.Serialize(true)));
    }
}