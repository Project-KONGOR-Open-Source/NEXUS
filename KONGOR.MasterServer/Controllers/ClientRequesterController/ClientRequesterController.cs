namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

[ApiController]
[Route("client_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ClientRequesterController(MerrickContext databaseContext, IDatabase distributedCache, ILogger<ClientRequesterController> logger) : ControllerBase
{
    # region Client Requester Controller Description
    /*
        Requests hitting the Client Requester endpoint are uniquely identified by one of two ways to specify the PHP function that they are trying to invoke.
        The PHP function identifier is specified as either 1) the query string parameter "f", in the format "client_requester.php?f={function}", or 2) the form parameter "f", in the format "f={function}".

        Additionally, the Client Requester controller is massive due to the Master Server API not being RESTful.
        This creates an integration complication, due to the fact that MVC action methods cannot be overloaded to the degree that the Master Server API requires it.
        The problem is that MVC action methods are uniquely identified by the combination of route and request method.
        And while the query string and/or the form data of requests hitting this endpoint can be different, the route is always "client_requester.php" and the method is always POST.
        This means that it is not possible to write individual action methods that uniquely identify the request by query string parameter or form data parameter.
        And instead the Client Requester controller needs to be handled using switch pattern matching, or equivalent, on these request data.
    */
    # endregion

    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [HttpPost(Name = "Client Requester All-In-One")]
    public async Task<IActionResult> ClientRequester()
    {
        // 2026-01-07: Normalize to lowercase for case-insensitive handling (PHP parity).
        string? functionName = (Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault())?.ToLower();
        
        bool endpointRequiresCookieValidation = functionName is not "auth" and not "pre_auth" and not "srpauth" and not "get_match_stats";
        (bool accountSessionCookieIsValid, string? sessionAccountName) = await DistributedCache.ValidateAccountSessionCookie((Request.Form["cookie"].ToString() ?? "NULL").Replace("-", ""));

        if (endpointRequiresCookieValidation.Equals(true) && accountSessionCookieIsValid.Equals(false))
        {
            // Fallback: Check Database for the cookie.
            // This is critical because `PurgeSessionCookies` clears Redis on startup, but the client may still hold a valid cookie.
            // The reference implementation relies on the Database column.
            string cookie = Request.Form["cookie"].ToString().Replace("-", "");
            Account? account = await MerrickContext.Accounts.FirstOrDefaultAsync(a => a.Cookie == cookie);

            if (account != null)
            {
                // Session is valid in DB. Restore to Redis.
                accountSessionCookieIsValid = true;
                sessionAccountName = account.Name;
                await DistributedCache.SetAccountNameForSessionCookie(cookie, account.Name);
                Logger.LogInformation("Restored Session From Database For Cookie {Cookie}", cookie);
            }
            else
            {
                Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Client Request With Forged Cookie ""{Request.Form["cookie"]}""");

                return Unauthorized($@"Unrecognized Cookie ""{Request.Form["cookie"]}""");
            }
        }

        if (accountSessionCookieIsValid)
            HttpContext.Items["SessionAccountName"] = sessionAccountName;

        if (endpointRequiresCookieValidation.Equals(false) && accountSessionCookieIsValid.Equals(true))
            Logger.LogError("[BUG] Endpoint Does Not Require Cookie Validation But A Valid Cookie Was Found");

        return await HandleClientRequest(functionName);
    }

    private async Task<IActionResult> HandleClientRequest(string? functionName)
    {
        // 2026-01-05: Unified Query/Form coalescing pattern for functional routing parity.
        // string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        return functionName switch
        {
            // authentication
            "auth"                          => await HandleAuthentication(),
            "pre_auth"                      => await HandlePreAuthentication(),
            "srpauth"                       => await HandleSRPAuthentication(),
            "aids2cookie"                   => await HandleAids2Cookie(),
            "logout"                        => await HandleLogout(),

            // server list
            "get_server_list"               => await GetServerList(),
            "grab_server_list"              => await GetServerList(),
            "server_list"                   => await GetServerList(),

            // heroes
            "get_all_heroes"                => await GetAllHeroes(),
            "get_hero_list"                 => await GetAllHeroes(),

            // game
            "create_game"                   => await HandleCreateGame(),
            "new_game_available"            => await HandleNewGameAvailable(),
            "final_match_stats"             => await HandleFinalMatchStats(),

            // stats
            "show_simple_stats"             => await GetSimpleStats(),
            "show_stats"                    => await GetSimpleStats(),
            "get_initstats"                 => await HandleInitStats(),
            "get_account_all_hero_stats"    => await HandleGetAccountAllHeroStats(),
            "get_account_mastery"           => await HandleGetAccountMastery(),
            "get_match_stats"               => await HandleMatchStats(),

            // upgrades
            "get_upgrades"                  => await HandleGetUpgrades(),
            "get_products"                  => await HandleGetProducts(),
            "get_daily_special"             => await HandleGetDailySpecial(),

            // guides
            "get_guide_list_filtered"       => await GetGuideList(),
            "get_guide"                     => await GetGuide(),
            
            // rewards
            "claim_season_rewards"          => await HandleClaimSeasonRewards(),
            
            // events & messages
            "client_events_info"            => await HandleClientEventsInfo(),
            "get_special_messages"          => await HandleGetSpecialMessages(),

            // default
            _                               => await Task.FromResult(BadRequest($"Unsupported Client Requester Controller Form Parameter: f={functionName}"))
        };
    }
}
