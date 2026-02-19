namespace KONGOR.MasterServer.Controllers.Ascension;

[ApiController]
[Route(TextConstant.EmptyString)]
public class AscensionController : ControllerBase
{
    /// <summary>
    ///     Routes ascension API requests based on the "r" query parameter.
    ///     This endpoint handles all client.sea.heroesofnewerth.com requests.
    /// </summary>
    /// <remarks>
    ///     The address is hard-coded in the game client, and needs to be patched either in memory or in the executable file.
    ///
    ///     <code>
    ///         c  l  i  e  n  t  .  s  e  a  .  h  e  r  o  e  s  o  f  n  e  w  e  r  t  h  .  c  o  m
    ///         63 6C 69 65 6E 74 2E 73 65 61 2E 68 65 72 6F 65 73 6F 66 6E 65 77 65 72 74 68 2E 63 6F 6D
    ///     </code>
    /// </remarks>
    [HttpGet("/", Name = "Ascension Root"), HttpGet("index.php", Name = "Ascension Index"), HttpPost("/", Name = "Ascension Root Post"), HttpPost("index.php", Name = "Ascension Index Post")]
    public IActionResult RouteAscensionRequest([FromQuery(Name = "r")] string route)
    {
        if (string.IsNullOrEmpty(route))
            throw new NotImplementedException(@"Ascension Controller Query String Parameter ""r"" Is NULL");

        return route switch
        {
            "api/match/checkmatch"                     => CheckMatch(),
            "api/match/changematchstatus"              => ChangeMatchStatus(),
            "api/match/checkuserrole"                  => CheckUserRole(),
            "api/game/matchresult"                     => ReceiveMatchResult(),
            "api/game/matchstats"                      => ReceiveMatchStatistics(),
            "api/MasterServer/RecordSpectateStartTime" => RecordSpectateStartTime(),
            _                                          => throw new NotImplementedException($@"Unsupported Ascension Controller Route ""{route}""")
        };
    }

    /// <summary>
    ///     Checks if a match is a season match before it starts.
    ///     The server uses this information to determine whether statistics should be recorded or not for the match.
    /// </summary>
    /// <remarks>
    ///     Called by the game server before match initialisation.
    ///     Error code 100 indicates success.
    ///     The "comment" array contains account IDs of voice presenters for the match.
    ///     The "referee" array contains account IDs of live referees for the match.
    /// </remarks>
    private IActionResult CheckMatch()
    {
        string matchID = Request.Query["match_id"].ToString();

        if (string.IsNullOrEmpty(matchID))
            return BadRequest(new { error_code = 400, message = @"Missing Required Parameter ""match_id""" });

        return Ok(new { error_code = 100, data = new { is_season_match = true, comment = Array.Empty<string>(), referee = Array.Empty<string>() } });
    }

    /// <summary>
    ///     Receives a match status change notification from the game server.
    /// </summary>
    /// <remarks>
    ///     Called by the game server when match status changes.
    ///     Status 2 indicates the match has started, status 3 indicates the match has ended.
    /// </remarks>
    private IActionResult ChangeMatchStatus()
    {
        // TODO: Implement Match Status Tracking

        return Ok(new { error_code = 100 });
    }

    /// <summary>
    ///     Checks whether an account has a special role (e.g. referee or game master).
    /// </summary>
    /// <remarks>
    ///     Called by the game client during login.
    ///     A role value of "2" grants game master/referee privileges.
    /// </remarks>
    private IActionResult CheckUserRole()
    {
        // TODO: Implement Role Checking Against Account Data

        return Ok(new { error_code = 100, role = "0" });
    }

    /// <summary>
    ///     Receives match results from the game server, including betting outcomes such as winning team, first blood team, first tower kill team, and first ten-kill team.
    /// </summary>
    /// <remarks>
    ///     Only called for official/season matches where "is_season_match" was TRUE in the <see cref="CheckMatch"/> response.
    /// </remarks>
    private IActionResult ReceiveMatchResult()
    {
        // TODO: Implement Match Result Processing

        return Ok(new { error_code = 100 });
    }

    /// <summary>
    ///     Receives live match statistics from the game server as a JSON payload in the "data" POST field.
    ///     Used for live spectating and real-time match tracking.
    /// </summary>
    /// <remarks>
    ///     Only called for official/season matches where "is_season_match" was TRUE in the <see cref="CheckMatch"/> response.
    ///     The game server sends periodic updates containing combat logs and match state.
    /// </remarks>
    private IActionResult ReceiveMatchStatistics()
    {
        // TODO: Implement Live Match Statistics Processing

        return Ok(new { error_code = 100 });
    }

    /// <summary>
    ///     Records the start time of a spectator session for a given match and region.
    /// </summary>
    /// <remarks>
    ///     Called by the replay manager when a replay recording starts.
    /// </remarks>
    private IActionResult RecordSpectateStartTime()
    {
        // TODO: Implement Spectate Start Time Recording

        return Ok(new { error_code = 100 });
    }
}
