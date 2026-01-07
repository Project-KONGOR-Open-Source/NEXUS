namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

[ApiController]
[Route("server_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ServerRequesterController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<ServerRequesterController> logger, IOptions<OperationalConfiguration> configuration, IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;
    private OperationalConfiguration Configuration { get; } = configuration.Value;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    [HttpPost(Name = "Server Requester All-In-One")]
    public async Task<IActionResult> ServerRequester() => await HandleServerRequest();

    private async Task<IActionResult> HandleServerRequest()
    {
        // Unified Query/Form coalescing pattern for functional routing parity.
        // 2026-01-07: Normalize to lowercase for case-insensitive handling (PHP parity).
        string? functionName = (Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault())?.ToLower();

        return functionName switch
        {
            // server manager
            "replay_auth"           => await HandleServerManagerAuthentication(),
            "get_spectator_header"  => await HandleGetSpectatorHeader(),
            "set_replay_size"       => await HandleSetReplaySize(),
            "get_quickstats"        => await HandleGetQuickStats(),

            // server
            "accept_key"            => await HandleAcceptKey(),
            "auth"                  => await HandleAuthentication(),
            "c_conn"                => await HandleConnectClient(),
            "new_session"           => await HandleServerAuthentication(),
            "set_online"            => await HandleSetOnline(),
            "shutdown"              => await HandleShutdown(),
            "start_game"            => await HandleMatchStart(),
            "aids2cookie"           => await HandleGetAccountIdFromCookie(),

            // default
            _                       => throw new NotImplementedException($"Unsupported Server Requester Controller Function Parameter: f={functionName}")
        };
    }

    private Task<IActionResult> HandleGetAccountIdFromCookie()
    {
        // This function 'aids2cookie' appears to be a legacy artifact or something specific to custom server implementations
        // that is not present in the official codebase or the reference implementation in 'READ ONLY'.
        // To avoid log spam and confusing errors, we simply return OK.
        return Task.FromResult<IActionResult>(Ok());
    }
}
