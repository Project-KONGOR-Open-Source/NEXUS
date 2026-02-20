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
            "client_events_info"            => GetClientEventsInformation(),
            "get_special_messages"          => GetSpecialMessages(),
            "claim_season_rewards"          => ClaimSeasonRewards(),
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

            // store
            "get_daily_special"             => await GetDailySpecial(),

            // guides
            "get_guide_list_filtered"       => await GetGuideList(),
            "get_guide"                     => await GetGuide(),

            // default
            _                               => throw new NotImplementedException($"Unsupported Client Requester Controller Form Parameter: f={Request.Form["f"].Single()}")
        };
    }

    /// <summary>
    ///     Returns client events information as a JSON response.
    ///     The client uses this to populate the HoN Event notification panel with revival events, newbie events, and other promotional content.
    ///     Currently returns an empty events object as the revival and newbie event systems are not yet implemented.
    /// </summary>
    private IActionResult GetClientEventsInformation()
    {
        Dictionary<string, object> response = new ()
        {
            ["success"] = true,
            ["data"] = new Dictionary<string, object>(),
            ["errors"] = string.Empty,
            ["vested_threshold"] = 5,
            ["0"] = true
        };

        return Ok(JsonSerializer.Serialize(response));
    }

    /// <summary>
    ///     Returns special messages to be displayed in the client's HoN Event notification panel.
    ///     Messages are loaded from the announcements configuration file. Each message URL points to the
    ///     master server's announcements endpoint, which serves an HTML page embedding the configured external URL
    ///     in an iframe that the client's embedded web browser can render.
    ///     The client generates an MD5 hash from the title, URL, and date to track which messages have already been seen.
    /// </summary>
    private IActionResult GetSpecialMessages()
    {
        string date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        string baseURL = $"{Request.Scheme}://{Request.Host}";

        List<Dictionary<string, object>> messages = JSONConfiguration.AnnouncementsConfiguration.SpecialMessages
            .Select((specialMessage, index) => new Dictionary<string, object>
            {
                ["message_id"] = index + 1,
                ["title"]      = specialMessage.Title,
                ["url"]        = $"{baseURL}/announcements/{index}",
                ["start_time"] = date,
                ["end_time"]   = string.Empty,
                ["left_secs"]  = 0
            })
            .ToList();

        Dictionary<string, object> response = new ()
        {
            ["date"] = date,
            ["messages"] = messages,
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Claims season rewards for the requesting account.
    ///     Currently returns an acknowledgement response as the seasonal rewards system is not yet implemented.
    /// </summary>
    private IActionResult ClaimSeasonRewards()
    {
        Dictionary<string, object> response = new ()
        {
            ["vested_threshold"] = 5
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Returns the daily special offers for the in-game store.
    ///     Each special entry includes the product details, discount information, and ownership status for the requesting account.
    ///     The client displays up to five specials in the store's specials tab.
    /// </summary>
    private async Task<IActionResult> GetDailySpecial()
    {
        string? accountIDString = Request.Form["account_id"];

        if (accountIDString is null || int.TryParse(accountIDString, out int accountID).Equals(false))
            return BadRequest(@"Missing Or Invalid Value For Form Parameter ""account_id""");

        Account? account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleOrDefaultAsync(queriedAccount => queriedAccount.ID == accountID);

        if (account is null)
            return NotFound($@"Account With ID ""{accountID}"" Was Not Found");

        StoreItemConfiguration storeItems = JSONConfiguration.StoreItemConfiguration;
        DailySpecialsConfiguration dailySpecials = JSONConfiguration.DailySpecialsConfiguration;

        List<Dictionary<string, object>> dailySpecialItems = [];

        for (int index = 0; index < dailySpecials.Specials.Count; index++)
        {
            DailySpecialEntry special = dailySpecials.Specials[index];
            StoreItem? storeItem = storeItems.GetByID(special.StoreItemID);

            if (storeItem is null)
                continue;

            string heroName = string.Empty;

            if (storeItem.StoreItemType is StoreItemType.AlternativeAvatar)
            {
                int underscoreIndex = storeItem.Code.IndexOf('_');
                int dotIndex = storeItem.Code.IndexOf('.');

                if (underscoreIndex >= 0 && dotIndex > underscoreIndex)
                    heroName = storeItem.Code[(underscoreIndex + 1)..dotIndex];
            }

            int discountedGoldCost = storeItem.GoldCost * (100 - special.DiscountPercentageGold) / 100;
            int discountedSilverCost = storeItem.SilverCost * (100 - special.DiscountPercentageSilver) / 100;

            dailySpecialItems.Add(new Dictionary<string, object>
            {
                ["panel_index"]         = index + 1,
                ["product_id"]          = storeItem.ID.ToString(),
                ["hero_name"]           = heroName,
                ["item_name"]           = storeItem.Code,
                ["item_cname"]          = storeItem.Name,
                ["item_type"]           = MapStoreItemTypeToCategoryName(storeItem.StoreItemType),
                ["purchasable"]         = storeItem.Purchasable,
                ["item_code"]           = storeItem.PrefixedCode,
                ["local_content"]       = storeItem.Resource,
                ["plinko_only"]         = 0,
                ["gold_coins"]          = storeItem.GoldCost,
                ["silver_coins"]        = storeItem.SilverCost,
                ["discount_off"]        = special.DiscountPercentageGold,
                ["discount_silver"]     = special.DiscountPercentageSilver,
                ["current_gold_coins"]  = discountedGoldCost,
                ["current_silver_coins"] = discountedSilverCost,
                ["tag_title"]           = special.TagTitle,
                ["tag_date_limit"]      = special.TagDateLimit,
                ["item_page"]           = 0,
                ["is_owned"]            = account.User.OwnedStoreItems.Contains(storeItem.PrefixedCode)
            });
        }

        Dictionary<string, object> response = new ()
        {
            ["list"] = dailySpecialItems,
            ["vested_threshold"] = 5
        };

        return Ok(JsonSerializer.Serialize(response));
    }

    /// <summary>
    ///     Maps a <see cref="StoreItemType"/> to the category name string expected by the game client.
    /// </summary>
    private static string MapStoreItemTypeToCategoryName(StoreItemType type) => type switch
    {
        StoreItemType.AlternativeAvatar  => "Alt Avatar",
        StoreItemType.AnnouncerVoice     => "Alt Announcement",
        StoreItemType.Courier            => "Couriers",
        StoreItemType.Hero               => "Hero",
        StoreItemType.Ward               => "Ward",
        StoreItemType.Taunt              => "Taunt",
        StoreItemType.Miscellaneous      => "Misc",
        StoreItemType.EarlyAccessProduct => "EAP",
        StoreItemType.ChatNameColour     => "Name Color",
        StoreItemType.ChatSymbol         => "Symbol",
        StoreItemType.AccountIcon        => "Account Icon",
        StoreItemType.Enhancement        => "Enhancement",
        StoreItemType.Mastery            => "Mastery",
        _                                => "Misc"
    };
}
