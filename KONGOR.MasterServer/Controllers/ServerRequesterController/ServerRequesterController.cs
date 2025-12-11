namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

[ApiController]
[Route("server_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ServerRequesterController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<ServerRequesterController> logger, IWebHostEnvironment hostEnvironment) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;
    private IWebHostEnvironment HostEnvironment { get; } = hostEnvironment;

    [HttpPost(Name = "Server Requester All-In-One")]
    public async Task<IActionResult> ServerRequester() => await HandleServerRequest();

    private async Task<IActionResult> HandleServerRequest()
    {
        return Request.Query["f"].SingleOrDefault() switch
        {
            // server manager
            "replay_auth"   => await HandleServerManagerAuthentication(),

            // server
            "accept_key"    => await HandleAcceptKey(),
            "auth"          => await HandleAuthentication(),
            "c_conn"        => await HandleConnectClient(),
            "new_session"   => await HandleServerAuthentication(),
            "set_online"    => await HandleSetOnline(),

            // fallback
            null            => await HandleServerRequestWithNoQueryString(),

            // default
            _               => throw new NotImplementedException($"Unsupported Server Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }

    private async Task<IActionResult> HandleServerRequestWithNoQueryString()
    {
        return Request.Form["f"].SingleOrDefault() switch
        {
            // server manager
            "get_spectator_header"  => await HandleGetSpectatorHeader(),

            // default
            _                       => throw new NotImplementedException($"Unsupported Server Requester Controller Form Parameter: f={Request.Form["f"].Single()}")
        };
    }
}
