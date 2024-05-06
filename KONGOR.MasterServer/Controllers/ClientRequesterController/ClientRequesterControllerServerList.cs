namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetServerList()
    {
        string? cookie = Request.Form["cookie"].ToString();

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? gameType = Request.Form.ContainsKey("gametype") ? Request.Form["gametype"].ToString() : null;

        List<MatchServer> servers = (await DistributedCache.HashGetAllAsync("MATCH-SERVERS"))
            .Where(entry => entry.Value.ToString() is not null)
            .Select(entry => JsonSerializer.Deserialize<MatchServer>(entry.Value.ToString()) ?? throw new NullReferenceException("Deserialized Match Server Is NULL")).ToList();

        switch (gameType)
        {
            case "10":
                return Ok(PhpSerialization.Serialize(new ServerForJoinListResponse(servers, cookie)));

            case "90":
                string? region = Request.Form.ContainsKey("region") ? Request.Form["region"].ToString() : null;

                return Ok(PhpSerialization.Serialize(new ServerForCreateListResponse(servers, region, cookie)));

            default:
                Logger.LogError($@"[BUG] Unknown Server List Game Type ""{gameType ?? "NULL"}""");

                return UnprocessableEntity($@"Unknown Server List Game Type ""{gameType ?? "NULL"}""");
        }
    }
}
