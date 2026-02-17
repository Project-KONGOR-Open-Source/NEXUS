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
            "api/match/checkmatch" => CheckMatch(),
            _                      => throw new NotImplementedException($"Unsupported Ascension Controller Query String Parameter: r={Request.Query["r"].Single()}")
        };
    }

    /// <summary>
    ///     Checks if a match is a season match before it starts.
    ///     The server uses this information to determine whether statistics should be recorded or not for the match.
    /// </summary>
    /// <remarks>
    ///     Called by the game server before match initialisation.
    ///     Error code 100 indicates success.
    /// </remarks>
    private IActionResult CheckMatch()
    {
        string matchID = Request.Query["match_id"].ToString();

        if (string.IsNullOrEmpty(matchID))
            return BadRequest(new { error_code = 400, message = @"Missing Required Parameter ""match_id""" });

        return Ok(new { error_code = 100, data = new { is_season_match = true, comment = Array.Empty<string>(), referee = Array.Empty<string>() } });
    }
}
