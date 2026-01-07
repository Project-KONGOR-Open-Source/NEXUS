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

    private async Task<IActionResult> HandleGetAccountIdFromCookie()
    {
        // Check both Form and Query for the cookie to be robust against different client implementations
        // LOGS SHOW: The client sends 'session' in the form data, not 'cookie'.
        string? cookie = Request.Form["cookie"].FirstOrDefault() 
                      ?? Request.Query["cookie"].FirstOrDefault()
                      ?? Request.Form["session"].FirstOrDefault()
                      ?? Request.Query["session"].FirstOrDefault();

        if (cookie is null)
        {
            string formKeys = Request.HasFormContentType ? string.Join(", ", Request.Form.Keys) : "N/A";
            Logger.LogError("Missing Cookie In aids2cookie Request. Query: {Query}, Form: {Form}, FormKeys: {FormKeys}", Request.QueryString, Request.HasFormContentType ? "Present" : "None", formKeys);
            return BadRequest(PhpSerialization.Serialize(new { error = "Missing Cookie" }));
        }


        // Normalize the cookie by removing hyphens, as the client might add them but the DB stores them without
        cookie = cookie.Replace("-", string.Empty);

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            // Fallback: Check the database for the cookie to handle cases where the cache has been cleared (e.g. server restart)
            var accountData = await MerrickContext.Accounts
                .Where(account => account.Cookie == cookie)
                .Select(account => new { account.Name, account.ID })
                .FirstOrDefaultAsync();

            if (accountData is not null)
            {
                // Re-populate the cache
                await DistributedCache.SetAccountNameForSessionCookie(cookie, accountData.Name);
                return Ok(PhpSerialization.Serialize(accountData.ID));
            }

            Logger.LogError(@"Cookie ""{Cookie}"" Not Found In Cache Or Database (Bypassed Controller Validation)", cookie);
            return Unauthorized(PhpSerialization.Serialize(new { error = "Invalid Session" }));
        }

        int? accountId = await MerrickContext.Accounts
            .Where(account => account.Name.Equals(accountName))
            .Select(account => (int?)account.ID)
            .FirstOrDefaultAsync();

        if (accountId is null)
        {
            Logger.LogError(@"Account Name ""{AccountName}"" From Cookie Not Found In Database", accountName);
            return NotFound(PhpSerialization.Serialize(new { error = "Account Not Found" }));
        }

        return Ok(PhpSerialization.Serialize(accountId.Value));
    }
}
