using KONGOR.MasterServer.Logging;
using KONGOR.MasterServer.Services.Requester;

namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

[ApiController]
[Route("client_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ClientRequesterController(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<ClientRequesterController> logger,
    ClientRequestDispatcher clientRequestDispatcher) : ControllerBase
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;
    private ClientRequestDispatcher Dispatcher { get; } = clientRequestDispatcher;

    [HttpPost(Name = "Client Requester All-In-One")]
    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> ClientRequester()
    {
        string? functionName = null;
        string? cookieRaw = "NULL";

        if (Request.HasFormContentType)
        {
            functionName = Request.Form["f"].FirstOrDefault();
            cookieRaw = Request.Form["cookie"].FirstOrDefault() ?? "NULL";
        }

        // Fallback to Query if Form is empty or not present
        if (string.IsNullOrEmpty(functionName))
        {
            functionName = Request.Query["f"].FirstOrDefault();
        }

        if (cookieRaw == "NULL")
        {
            cookieRaw = Request.Query["cookie"].FirstOrDefault() ?? "NULL";
        }

        functionName = functionName?.ToLower();

        if (functionName == null)
        {
            return BadRequest("Missing function parameter 'f'");
        }

        bool endpointRequiresCookieValidation = functionName is not "auth" and not "pre_auth" and not "srpauth"
            and not "get_match_stats" and not "upload_replay";

        Logger.LogProcessingRequest(functionName, cookieRaw, endpointRequiresCookieValidation);

        (bool accountSessionCookieIsValid, string? sessionAccountName) =
            await DistributedCache.ValidateAccountSessionCookie(cookieRaw);

        if (endpointRequiresCookieValidation && !accountSessionCookieIsValid)
        {
            Logger.LogRedisMiss(functionName, cookieRaw);
        }

        if (endpointRequiresCookieValidation.Equals(true) && accountSessionCookieIsValid.Equals(false))
        {
            string cookie = cookieRaw;
            Logger.LogAttemptingDBFallback(cookie);

            string? altCookie = null;
            if (cookie.Length == 32 && Guid.TryParse(cookie, out Guid guid))
            {
                altCookie = guid.ToString(); // Dashed
            }
            else if (cookie.Contains("-") && Guid.TryParse(cookie, out Guid guid2))
            {
                altCookie = guid2.ToString("N"); // No Dashes
            }

            Account? account = await MerrickContext.Accounts.FirstOrDefaultAsync(a =>
                a.Cookie == cookie || (altCookie != null && a.Cookie == altCookie));

            if (account != null)
            {
                // Session is valid in DB. Restore to Redis.
                accountSessionCookieIsValid = true;
                sessionAccountName = account.Name;
                await DistributedCache.SetAccountNameForSessionCookie(cookie, account.Name);
                Logger.LogDBHit(cookie, account.Name);
            }
            else
            {
                Logger.LogDBMiss(cookie);
                Logger.LogForgedCookie(Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN", cookieRaw);

                return Unauthorized($@"Unrecognized Cookie ""{cookieRaw}""");
            }
        }

        if (accountSessionCookieIsValid)
        {
            HttpContext.Items["SessionAccountName"] = sessionAccountName;
        }

        return await Dispatcher.DispatchAsync(functionName, HttpContext);
    }
}
