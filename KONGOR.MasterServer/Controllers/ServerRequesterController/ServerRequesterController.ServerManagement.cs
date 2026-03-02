namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    /// <summary>
    ///     Resolves account IDs to their corresponding session cookies.
    ///     Called by the match server to map a list of account IDs to their active session cookies.
    /// </summary>
    private async Task<IActionResult> HandleAccountIDsToCookie()
    {
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        Dictionary<string, object> response = [];

        Dictionary<string, string> clientCookies = [];
        Dictionary<string, string> gameCookies = [];

        EndPoint endPoint = DistributedCache.Multiplexer.GetEndPoints().Single();
        IServer redisServer = DistributedCache.Multiplexer.GetServer(endPoint);

        Dictionary<string, string> accountNameToCookie = [];

        // Build A Reverse Map Of Account Name → Session Cookie By Scanning All Active Session Cookies
        foreach (RedisKey key in redisServer.Keys(pattern: "ACCOUNT-SESSION-COOKIE:*"))
        {
            RedisValue cachedValue = await DistributedCache.StringGetAsync(key);

            if (cachedValue.IsNullOrEmpty is false)
            {
                Match match = AccountSessionCookieKeyPattern().Match(key.ToString());

                if (match.Success)
                    accountNameToCookie[cachedValue.ToString()] = match.Groups[1].Value;
            }
        }

        foreach (string key in Request.Query.Keys.Where(key => AccountIDsQueryParameterPattern().IsMatch(key)))
        {
            string? accountIDValue = Request.Query[key];

            if (string.IsNullOrWhiteSpace(accountIDValue))
                continue;

            if (int.TryParse(accountIDValue, out int accountID) is false || accountID is 0)
                continue;

            Account? account = await MerrickContext.Accounts
                .SingleOrDefaultAsync(account => account.ID == accountID);

            if (account is null)
                continue;

            if (accountNameToCookie.TryGetValue(account.Name, out string? cookie) is false)
                continue;

            clientCookies[accountID.ToString()] = cookie;
            gameCookies[accountID.ToString()] = cookie;
        }

        response["aids2cookie"] = clientCookies;
        response["aids2gcookie"] = gameCookies;

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Retrieves quick statistics for a given account, including recent match history and favourite heroes.
    ///     Called by the match server during gameplay to display player information.
    /// </summary>
    private async Task<IActionResult> HandleGetQuickStats()
    {
        // TODO: Implement Quick Statistics Retrieval

        Dictionary<string, object> response = new ()
        {
            ["quickstats"] = "OK"
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Retrieves upgrade (store item) data for a player, as requested by the match server.
    ///     This is the server-side equivalent of the client requester "get_upgrades" endpoint.
    /// </summary>
    private async Task<IActionResult> HandleGetServerUpgrades()
    {
        string? cookie = Request.Query["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Query Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"No Valid Session Cookie Could Be Found For Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Could Not Be Found");

        Dictionary<string, object> response = new ()
        {
            ["my_upgrades"] = account.User.OwnedStoreItems,
            ["selected_upgrades"] = account.SelectedStoreItems
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    /// <summary>
    ///     Updates the replay file size for a completed match.
    ///     Called by the match server after a replay file has been fully written.
    /// </summary>
    private IActionResult HandleSetReplaySize()
    {
        // TODO: Implement Replay Size Tracking

        Dictionary<string, object> response = new ()
        {
            ["replay_size_update"] = "OK"
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    // Matches The Key Format: ACCOUNT-SESSION-COOKIE:["{cookie}"]
    [GeneratedRegex(@"^ACCOUNT-SESSION-COOKIE:\[""(.+)""\]$")]
    private static partial Regex AccountSessionCookieKeyPattern();

    // Matches The Query Parameter Format: aids[{index}]
    [GeneratedRegex(@"^aids\[\d+\]$")]
    private static partial Regex AccountIDsQueryParameterPattern();
}
