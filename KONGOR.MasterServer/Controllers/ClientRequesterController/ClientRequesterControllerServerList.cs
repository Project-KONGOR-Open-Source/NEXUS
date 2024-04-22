namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private IActionResult HandleServerList()
    {
        string? gameType = Request.Form.ContainsKey("gametype") ? Request.Form["gametype"].ToString() : null;

        switch (gameType)
        {
            case "10":
                return Ok(PhpSerialization.Serialize(new ServerForJoinListResponse { AccountKey = "TODO", AccountKeyHash = "TODO" }));

            case "90":
                string? region = Request.Form.ContainsKey("region") ? Request.Form["region"].ToString() : null;

                return Ok(PhpSerialization.Serialize(new ServerForCreateListResponse(region) { AccountKey = "TODO", AccountKeyHash = "TODO" }));

            default:
                Logger.LogError($@"[BUG] Unknown Server List Game Type ""{gameType ?? "NULL"}""");

                return UnprocessableEntity($@"Unknown Server List Game Type ""{gameType ?? "NULL"}""");
        }
    }
}
