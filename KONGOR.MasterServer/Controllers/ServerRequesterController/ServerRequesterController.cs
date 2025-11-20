namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

[ApiController]
[Route("server_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ServerRequesterController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<ServerRequesterController> logger) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Server Requester All-In-One")]
    public async Task<IActionResult> ServerRequester()
    {
        // TODO: Implement Server Requester Controller Cookie Validation

        //if (Cache.ValidateAccountSessionCookie(form.Cookie, out string? _).Equals(false))
        //{
        //    Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Server Controller Request With Forged Cookie ""{form.Cookie}""");

        //    return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        //}

        return Request.Query["f"].SingleOrDefault() switch
        {
            // server manager
            "replay_auth"   => await HandleReplayAuthentication(),

            // server
            "accept_key"    => await HandleAcceptKey(),
            "c_conn"        => await HandleConnectClient(),
            "new_session"   => await HandleNewSession(),
            "set_online"    => await HandleSetOnline(),

            _               => throw new NotImplementedException($"Unsupported Server Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }
}
