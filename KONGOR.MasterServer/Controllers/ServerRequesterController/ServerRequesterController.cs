namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

[ApiController]
[Route("server_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ServerRequesterController(ILogger<ServerRequesterController> logger, IMemoryCache cache) : ControllerBase
{
    private ILogger Logger { get; } = logger;
    private IMemoryCache Cache { get; } = cache;

    [HttpPost(Name = "Server Requester All-In-One")]
    public async Task<IActionResult> ServerRequester([FromForm] Dictionary<string, string> /* ServerRequestForm */ form)
    {
        // TODO: Implement Server Requester Controller

        //if (Cache.ValidateAccountSessionCookie(form.Cookie, out string? _).Equals(false))
        //{
        //    Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Server Controller Request With Forged Cookie ""{form.Cookie}""");

        //    return Unauthorized($@"Unrecognized Cookie ""{form.Cookie}""");
        //}

        return Ok();
    }
}
