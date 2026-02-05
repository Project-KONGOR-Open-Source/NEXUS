namespace KONGOR.MasterServer.Controllers.Ascension;

using global::KONGOR.MasterServer.Extensions.Cache;
using global::KONGOR.MasterServer.Models.ServerManagement;

using StackExchange.Redis;

[ApiController]
[Route(TextConstant.EmptyString)]
public class AscensionController : ControllerBase
{
    /// <summary>
    /// Routes ascension API requests based on the "r" query parameter.
    /// This endpoint handles all client.sea.heroesofnewerth.com requests.
    /// </summary>
    /// <remarks>
    /// The address is hard-coded in the game client, and needs to be patched either in memory or in the executable file.
    /// <code>
    /// c l i e n t . s e a . h e r o e s o f n e w e r t h . c o m
    /// 63 6C 69 65 6E 74 2E 73 65 61 2E 68 65 72 6F 65 73 6F 66 6E 65 77 65 72 74 68 2E 63 6F 6D
    /// </code>
    /// </remarks>
    [HttpGet("/", Name = "Ascension Root")]
    [HttpGet("index.php", Name = "Ascension Index")]
    [HttpPost("/", Name = "Ascension Root Post")]
    [HttpPost("index.php", Name = "Ascension Index Post")]
    public async Task<IActionResult> RouteAscensionRequest([FromQuery(Name = "r")] string? route)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new NotImplementedException(@"Ascension Controller Query String Parameter ""r"" Is NULL");
        }

        return route switch
        {
            "api/match/checkmatch" => await CheckMatch(),
            // Barebones Implementation: Other endpoints are not yet implemented.
            // "api/match/changematchstatus" => ChangeMatchStatus(),
            // "api/game/matchresult" => MatchResult(),
            // "api/game/matchstats" => MatchStats(),
            // "api/MasterServer/RecordSpectateStartTime" => RecordSpectateStartTime(),
            // "api/match/checkuserrole" => CheckUserRole(),
            _ => throw new NotImplementedException($"Unsupported Ascension Controller Query String Parameter: r={route}")
        };
    }

    private readonly IDatabase _distributedCache;

    public AscensionController(IConnectionMultiplexer redis)
    {
        _distributedCache = redis.GetDatabase();
    }

    /// <summary>
    /// Checks if a match is a season match before it starts.
    /// The server uses this information to determine whether statistics should be recorded or not for the match.
    /// </summary>
    /// <remarks>
    /// Called by the game server before match initialisation.
    /// Response is actively parsed by the client.
    /// Error code 100 indicates success.
    /// </remarks>
    private async Task<IActionResult> CheckMatch()
    {
        if (!int.TryParse(Request.Query["match_id"], out int matchID))
        {
            return BadRequest(new { error_code = 400, message = "Missing or invalid required parameter 'match_id'" });
        }

        return await CheckMatchAsync(matchID);
    }

    private async Task<IActionResult> CheckMatchAsync(int matchID)
    {
        MatchStartData? matchStartData = await _distributedCache.GetMatchStartData(matchID);

        bool isSeasonMatch = false;

        if (matchStartData is not null)
        {
            // If the match exists in our cache, it's a valid match authorized by the Master Server.
            // We can treat it as a "Season Match" (Stats Enabled) by default for now.
            // In the future, we can check matchStartData.League or matchStartData.Options.
            isSeasonMatch = true;
        }

        // TODO: Query Database For Match Commentators And Referees (Schema Update Required)

        return Ok(new
        {
            error_code = 100,
            data = new
            {
                is_season_match = isSeasonMatch,
                comment = Array.Empty<string>(), // Array Of Account IDs For Voice Presenters
                referee = Array.Empty<string>() // Array Of Account IDs For Referees
            }
        });
    }
}