namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetServerList()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? gameType = Request.Form.ContainsKey("gametype") ? Request.Form["gametype"].ToString() : null;

        List<MatchServer> servers = await DistributedCache.GetMatchServers();
        List<MatchServerManager> serverManagers = await DistributedCache.GetMatchServerManagers();

        switch (gameType)
        {
            case "10": // List Of Match Servers On Which Matches Can Be Joined
                ServerForJoinListResponse serversForJoin = new (servers, serverManagers, cookie);

                return Ok(PhpSerialization.Serialize(serversForJoin));

            case "90": // List Of Match Servers On Which Matches Can Be Created
                string? region = Request.Form.ContainsKey("region") ? Request.Form["region"].ToString() : null;

                ServerForCreateListResponse serversForCreate = new (servers, serverManagers, region, cookie);

                return Ok(PhpSerialization.Serialize(serversForCreate));

            default:
                Logger.LogError($@"[BUG] Unknown Server List Game Type ""{gameType ?? "NULL"}""");

                return UnprocessableEntity($@"Unknown Server List Game Type ""{gameType ?? "NULL"}""");
        }
    }
}
