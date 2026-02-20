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
        bool endpointRequiresCookieValidation = Request.Query["f"].SingleOrDefault() is not "auth" and not "pre_auth" and not "srpAuth";
        bool accountSessionCookieIsValid = (await DistributedCache.ValidateAccountSessionCookie(Request.Form["cookie"].ToString() ?? "NULL")).IsValid;

        if (endpointRequiresCookieValidation.Equals(true) && accountSessionCookieIsValid.Equals(false))
        {
            Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Has Made A Client Request With Forged Cookie ""{Request.Form["cookie"]}""");

            return Unauthorized($@"Unrecognized Cookie ""{Request.Form["cookie"]}""");
        }

        if (endpointRequiresCookieValidation.Equals(false) && accountSessionCookieIsValid.Equals(true))
            Logger.LogError("[BUG] Endpoint Does Not Require Cookie Validation But A Valid Cookie Was Found");

        return await HandleClientRequest();
    }

    private async Task<IActionResult> HandleClientRequest()
    {
        return Request.Query["f"].SingleOrDefault() switch
        {
            // authentication
            "auth"                          => await HandleAuthentication(),
            "pre_auth"                      => await HandlePreAuthentication(),
            "srpAuth"                       => await HandleSRPAuthentication(),

            // statistics
            "get_account_all_hero_stats"    => await GetHeroStatistics(),
            "get_match_stats"               => await GetMatchStatistics(),
            //"client_events_info"            => Ok(@"{""success"":true,""data"":[],""errors"":""invalid region"",""vested_threshold"":5,""0"":true}"),
            //"get_special_messages"          => Ok(@"a:4:{s:4:""date"";s:10:""2022-04-20"";s:8:""messages"";a:0:{}s:16:""vested_threshold"";i:5;i:0;b:1;}"),
            "get_products"                  => GetProducts(),
            "get_upgrades"                  => await GetUpgrades(),
            "get_initStats"                 => await GetInitialStatistics(),
            "show_simple_stats"             => await GetSimpleStatistics(),

            // servers
            "server_list"                   => await GetServerList(),

            // friends
            "remove_buddy2"                 => await RemoveFriend(),

            // fallback
            null                            => await HandleClientRequestWithNoQueryString(),

            // default
            _                               => throw new NotImplementedException($"Unsupported Client Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }

    private async Task<IActionResult> HandleClientRequestWithNoQueryString()
    {
        return Request.Form["f"].SingleOrDefault() switch
        {
            // statistics
            "get_player_award_summ"         => await GetPlayerAwardSummary(),
            "get_seasons"                   => await GetSeasons(),
            "match_history_overview"        => await GetMatchHistoryOverview(),
            "show_stats"                    => await GetStatistics(),

            // guides
            "get_guide_list_filtered"       => await GetGuideList(),
            "get_guide"                     => await GetGuide(),

            // default
            _                               => throw new NotImplementedException($"Unsupported Client Requester Controller Form Parameter: f={Request.Form["f"].Single()}")
        };
    }
}
