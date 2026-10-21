namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetServerList()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? gameType = Request.Form.ContainsKey("gametype") ? Request.Form["gametype"].ToString() : null;

        // "10" Lists The Match Servers On Which Matches Can Be Joined, And "90" Lists The Match Servers On Which Matches Can Be Created
        if (gameType is not ("10" or "90"))
        {
            Logger.LogError($@"[BUG] Unknown Server List Game Type ""{gameType ?? "NULL"}""");

            return UnprocessableEntity($@"Unknown Server List Game Type ""{gameType ?? "NULL"}""");
        }

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
        {
            Logger.LogError(@"[BUG] No Account Could Be Found For Account Name ""{AccountName}"" With Session Cookie ""{SessionCookie}""", accountName, cookie);

            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");
        }

        MatchLock? refusingLock = null;

        // A Server List Whose Match Lock Cannot Be Evaluated Is Served As If There Were No Lock, As A Client Connection Is
        try
        {
            (refusingLock, MatchLock? liftedLock) = await DistributedCache.FindRefusingMatchLock(account, askingServerID: null);

            if (liftedLock is not null)
            {
                Logger.LogInformation(@"Lifted The Match Lock Of Account ""{AccountName}"" On Match ID ""{MatchID}"", Because Match Server ID ""{MatchServerID}"" Is No Longer Registered",
                    liftedLock.AccountName, liftedLock.MatchID, liftedLock.ServerID);
            }
        }

        catch (Exception exception)
        {
            Logger.LogError(exception, @"Serving The Server List Of Account ""{AccountName}"" As If It Had No Match Lock, Because Its Match Lock Could Not Be Evaluated", account.Name);
        }

        if (refusingLock is not null)
        {
            try
            {
                await DistributedCache.PublishMatchLockRefusal(new MatchLockRefusal { UserID = account.User.ID, AccountName = account.Name });
            }

            catch (Exception exception)
            {
                // The Refusal Stands Even When It Cannot Be Relayed To The Chat Server, Rather Than Turning Into A Server Error, Because Serving The Server List Would Let The Player Join Or Create Another Game, And The Player's Next Attempt Publishes Its Refusal Anew
                Logger.LogError(exception, @"Failed To Publish The Match Lock Refusal Of Account ""{AccountName}"" For The Server List", account.Name);
            }

            Logger.LogInformation(@"Refused The Server List Of Account ""{AccountName}"", Because Its User Is Locked To Match ID ""{MatchID}"" On Match Server ID ""{MatchServerID}""",
                account.Name, refusingLock.MatchID, refusingLock.ServerID);
        }

        // A Refused Server List Is A Successful, Empty List, Because The Client Clears, Reads, And Redraws Its List From Every Successful Reply ("CServerList::ProcessServerList"), Which Leaves A Locked Player Nothing To Join, And A Game Creation No Match Server To Take, Including The One Which The Client Picks By Itself For A New Game ("CHostClient::StartAutoRemoteGame" Asks For The List Of Game Type "90")
        List<MatchServer> servers = refusingLock is null ? await DistributedCache.GetMatchServers() : [];

        if (gameType is "10")
        {
            ServerForJoinListResponse serversForJoin = new (servers, cookie);

            return Ok(PhpSerialization.Serialize(serversForJoin));
        }

        string? region = Request.Form.ContainsKey("region") ? Request.Form["region"].ToString() : null;

        ServerForCreateListResponse serversForCreate = new (servers, region, cookie);

        return Ok(PhpSerialization.Serialize(serversForCreate));
    }
}
