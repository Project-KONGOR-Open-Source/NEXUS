namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private async Task<IActionResult> HandleMatchStart()
    {
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{session}""");

        string? map = Request.Form["map"];

        if (map is null)
            return BadRequest(@"Missing Value For Form Parameter ""map""");

        string? version = Request.Form["version"];

        if (version is null)
            return BadRequest(@"Missing Value For Form Parameter ""version""");

        if (version is not "4.10.1.0")
            return BadRequest($@"Unsupported Match Server Version ""{version}""");

        string matchName = Request.Form["mname"].ToString() ?? throw new NullReferenceException("Match Name Is NULL");

        // The Host Account Name Is Expected To End With A Colon (":") Character
        string hostAccountName = Request.Form["mstr"].ToString()?.TrimEnd(':') ?? throw new NullReferenceException("Master Server Name Is NULL");

        bool isCasual = Request.Form["casual"].ToString().Equals("0") ? false : Request.Form["casual"].ToString().Equals("1") ? true
            : throw new ArgumentOutOfRangeException("casual", Request.Form["casual"].ToString(), @"Value Of Form Parameter ""casual"" Is Neither 0 Nor 1");

        int matchType = int.TryParse(Request.Form["arrangedmatchtype"], out int parsedArrangedMatchType) ? parsedArrangedMatchType
            : throw new ArgumentOutOfRangeException("arrangedmatchtype", Request.Form["arrangedmatchtype"].ToString(), @"Value Of Form Parameter ""arrangedmatchtype"" Is Invalid");

        int matchMode = int.TryParse(Request.Form["match_mode"], out int parsedMatchMode) ? parsedMatchMode
            : throw new ArgumentOutOfRangeException("match_mode", Request.Form["match_mode"].ToString(), @"Value Of Form Parameter ""match_mode"" Is Invalid");

        MatchStartData matchStartData = new ()
        {
            ServerID = matchServer.ID,
            ServerName = matchServer.Name,
            Map = map,
            Version = version,
            MatchName = matchName,
            HostAccountName = hostAccountName,
            IsCasual = isCasual,
            MatchType = (MatchType) matchType,
            MatchMode = (PublicMatchMode) matchMode,
            TimestampStarted = DateTimeOffset.UtcNow,
        };

        await DistributedCache.SetMatchStartData(matchStartData);

        Dictionary<string, object> response = new ()
        {
            ["match_id"] = matchStartData.MatchID
        };

        Logger.LogInformation(@"Match ID {MatchID} Has Started - Host Name: {HostName}, Server ID: {ServerID}, Map: {Map}",
            matchStartData.MatchID, hostAccountName, matchServer.ID, map);

        return Ok(PhpSerialization.Serialize(response));
    }
}
