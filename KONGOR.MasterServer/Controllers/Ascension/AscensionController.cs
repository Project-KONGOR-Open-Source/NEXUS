namespace KONGOR.MasterServer.Controllers.Ascension;

[ApiController]
[Route(TextConstant.EmptyString)]
public class AscensionController : ControllerBase
{
    /// <summary>
    ///     Routes ascension API requests based on the "r" query parameter.
    ///     This endpoint handles the requests that retail sent to client.sea.heroesofnewerth.com.
    /// </summary>
    /// <remarks>
    ///     The game reaches this endpoint at the address it is given with "-ascensionserver", which COMPEL and WILLOWMAKER set to the master server's address.
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
    ///     For a season match, the server streams the match's replay, posts its live statistics, and posts its betting results.
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

        // TODO: Look Into What A Season Match Turns On (The Streamed Replay, The Live Match Statistics, And The Betting Results) And Whatever Else Ascension Provides
        // Until Then, No Match Is A Season Match, So Matches Behave As They Did While This Request Failed

        return Ok(new { error_code = 100, data = new { is_season_match = false, comment = Array.Empty<string>(), referee = Array.Empty<string>() } });
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
    ///     Only called for official/season matches where "is_season_match" was <see langword="true"/> in the <see cref="CheckMatch"/> response.
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
    ///     Only called for official/season matches where "is_season_match" was <see langword="true"/> in the <see cref="CheckMatch"/> response.
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
