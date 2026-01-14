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
    [HttpGet("/", Name = "Ascension Root")]
    [HttpGet("index.php", Name = "Ascension Index")]
    [HttpPost("/", Name = "Ascension Root Post")]
    [HttpPost("index.php", Name = "Ascension Index Post")]
    public IActionResult RouteAscensionRequest([FromQuery] string route)
    {
        if (string.IsNullOrEmpty(route))
            throw new NotImplementedException(@"Ascension Controller Query String Parameter ""r"" Is NULL");

        return route switch
        {
            "api/match/checkmatch"                     => CheckMatch(),
            "api/match/changematchstatus"              => ChangeMatchStatus(),
            "api/game/matchresult"                     => MatchResult(),
            "api/game/matchstats"                      => MatchStats(),
            "api/MasterServer/RecordSpectateStartTime" => RecordSpectateStartTime(),
            "api/match/checkuserrole"                  => CheckUserRole(),
            _                                          => throw new NotImplementedException($"Unsupported Ascension Controller Query String Parameter: r={Request.Query["r"].Single()}")
        };
    }

    /// <summary>
    ///     Checks if a match is a season match before the game starts.
    /// </summary>
    /// <remarks>
    ///     Called by the game server before match initialisation.
    ///     Query Parameters: match_id
    ///     Response is actively parsed by the client.
    /// </remarks>
    private IActionResult CheckMatch()
    {
        string matchID = Request.Query["match_id"].ToString();

        if (string.IsNullOrEmpty(matchID))
        {
            return BadRequest(new { error_code = 400, message = "Missing required parameter 'match_id'" });
        }

        // TODO: Implement Match Season Check Logic
        // TODO: Query Database For Match Commentators And Referees
        return Ok(new
        {
            error_code = 100,
            data = new
            {
                is_season_match = true,
                comment = Array.Empty<string>(),  // Array Of Account IDs For Voice Presenters
                referee = Array.Empty<string>()   // Array Of Account IDs For Referees
            }
        });
    }

    /// <summary>
    ///     Updates the status of a match.
    /// </summary>
    /// <remarks>
    ///     Called by the game server when match status changes (e.g., match ends).
    ///     Query Parameters: region, match_id, status
    ///     This is a fire-and-forget request; response is not parsed by the client.
    /// </remarks>
    private IActionResult ChangeMatchStatus()
    {
        string region = Request.Query["region"].ToString();
        string matchID = Request.Query["match_id"].ToString();
        string status = Request.Query["status"].ToString();

        if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(matchID) || string.IsNullOrEmpty(status))
        {
            return BadRequest(new { error_code = 400, message = "Missing required parameters: region, match_id, status" });
        }

        // TODO: Implement Match Status Update Logic
        return Ok();
    }

    /// <summary>
    ///     Receives match result data after a game completes.
    /// </summary>
    /// <remarks>
    ///     Called by the game server with POST data containing match results.
    ///     Query Parameters: hongameclientcookie (format: cookie|region|language)
    ///     POST Body: JSON object with match_id, win, ten_kill, first_kill, first_tower, signature (MD5)
    ///     This is a fire-and-forget request; response is not parsed by the client.
    /// </remarks>
    private IActionResult MatchResult()
    {
        string hongameclientcookie = Request.Query["hongameclientcookie"].ToString();

        if (string.IsNullOrEmpty(hongameclientcookie))
        {
            return BadRequest(new { error_code = 400, message = "Missing required parameter 'hongameclientcookie'" });
        }

        // TODO: Parse POST Body And Process Match Results
        // Expected POST Data: match_id, win, ten_kill, first_kill, first_tower, signature
        return Ok();
    }

    /// <summary>
    ///     Records comprehensive match statistics after game completion.
    /// </summary>
    /// <remarks>
    ///     Called by the game server with detailed match statistics.
    ///     POST Body: JSON data containing match_id, region, replay_time, game_phase, match_time,
    ///                hero kills, team data (player stats, hero used, K/D/A), spectator info,
    ///                building status, win team, chat log, combat log.
    ///     This is a fire-and-forget request; response is not parsed by the client.
    /// </remarks>
    private IActionResult MatchStats()
    {
        // TODO: Parse POST Body And Process Match Statistics
        return Ok();
    }

    /// <summary>
    ///     Logs when a replay or spectate session starts.
    /// </summary>
    /// <remarks>
    ///     Called by the replay manager when spectating begins.
    ///     Query Parameters: region, match_id
    ///     This is a fire-and-forget notification; response is not parsed by the client.
    /// </remarks>
    private IActionResult RecordSpectateStartTime()
    {
        string region = Request.Query["region"].ToString();
        string matchID = Request.Query["match_id"].ToString();

        if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(matchID))
        {
            return BadRequest(new { error_code = 400, message = "Missing required parameters: region, match_id" });
        }

        // TODO: Implement Spectate Start Time Recording Logic
        return Ok();
    }

    /// <summary>
    ///     Verifies a user's role for a match.
    /// </summary>
    /// <remarks>
    ///     Called during client login to verify user role (e.g., referee, player).
    ///     Query Parameters: account_id, region
    ///     Response is actively parsed by the client.
    ///     Role values: "2" = Referee/GM, any other value = not a referee
    /// </remarks>
    private IActionResult CheckUserRole()
    {
        string accountID = Request.Query["account_id"].ToString();
        string region = Request.Query["region"].ToString();

        if (string.IsNullOrEmpty(accountID) || string.IsNullOrEmpty(region))
        {
            return BadRequest(new { error_code = 400, message = "Missing required parameters: account_id, region" });
        }

        // TODO: Implement User Role Verification Logic
        // TODO: Query Database For User Role (GM/Referee Status)
        return Ok(new
        {
            error_code = 100,
            role = "0"  // "2" = Referee/GM, "0" Or Other Values = Normal User
        });
    }
}
