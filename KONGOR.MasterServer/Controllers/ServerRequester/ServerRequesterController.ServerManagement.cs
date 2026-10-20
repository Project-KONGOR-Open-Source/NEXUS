namespace KONGOR.MasterServer.Controllers.ServerRequester;

public partial class ServerRequesterController
{
    /// <summary>
    ///     Answers the match server's periodic duplicate session check ("CGameServer::GetAccountAuth"), which posts the account IDs of its players as "aids[]" in the request body.
    ///     For each account which the asking match server has served, the reply holds, in "aids2cookie", the session cookie which the account's client on that match server holds, and, in "aids2gcookie", the account's game cookie.
    ///     The match server terminates a player whose game cookie differs as a duplicate connection, so that a player who has joined another game is removed from the match which they left.
    ///     The match server also kicks a connection whose session cookie differs as a duplicate login, and every kick is a strike toward the engine's kick ban, which, once reached, refuses the player's every reconnect until the match ends.
    ///     So NEXUS never causes a duplicate-login kick: the session cookie answered follows the client on the match server, and never a later login elsewhere.
    ///     Reconnecting to a match in progress keeps the game cookie, so that a reconnecting player is never removed.
    /// </summary>
    private async Task<IActionResult> HandleAccountIDsToCookie()
    {
        // Only A Registered Match Server May Map Account IDs To Session Cookies, Because A Session Cookie Which Is Still Live Authenticates Its Account
        // Anything Else Is Rejected With Plain Text, And Never With "auth_failure", Because The Match Server Checks This Reply From Inside Its Game Frame, Where Acting Upon An Authentication Failure Before The Match Starts Frees The Game While The Frame Is Still Running
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{session}""");

        // A Value Which Is Not An Account ID Is Skipped Rather Than Failing The Whole Check, And An Account Asked About More Than Once Is Answered Once
        List<int> requestedAccountIDs = [.. Request.Form["aids[]"]
            .Select(accountIDValue => int.TryParse(accountIDValue, NumberStyles.None, CultureInfo.InvariantCulture, out int accountID) ? accountID : 0)
            .Where(accountID => accountID > 0)
            .Distinct()];

        // Only The Accounts Which The Asking Match Server Has Served Are Answered, So That No Match Server Can Read The Session Cookies Of Accounts Which Never Connected To It
        List<int> accountIDs = await DistributedCache.GetAccountsConnectedToMatchServer(matchServer.ID, requestedAccountIDs);

        Dictionary<string, object> response = [];

        Dictionary<string, string> clientCookies = [];
        Dictionary<string, string> gameCookies = [];

        foreach (int accountID in accountIDs)
        {
            GameCookie? gameCookie = await DistributedCache.GetStoredGameCookie(accountID);

            // The Match Server Removes A Listed Player Whose Cookie Is Empty, While It Leaves Alone An Account Which Is Not Listed, So An Account Without A Readable Game Cookie Is Left Out Of Both Lists
            if (gameCookie is null)
                continue;

            // The Match Server Pairs Both Lists By Position Rather Than By Account ID, So Both Hold The Same Accounts In The Same Order
            clientCookies.Add(accountID.ToString(), gameCookie.SessionCookie);
            gameCookies.Add(accountID.ToString(), gameCookie.Cookie);
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
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        // Never Answer "auth_failure" Here, Because The Match Server Checks This Reply From Inside Its Loop Over Its Clients, Where Acting Upon An Authentication Failure Before The Match Starts Deletes The Calling Client
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
}
