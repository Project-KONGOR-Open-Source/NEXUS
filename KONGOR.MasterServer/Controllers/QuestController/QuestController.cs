using KONGOR.MasterServer.Logging;

namespace KONGOR.MasterServer.Controllers.QuestController;

[ApiController]
[Route("master/quest")]
[Route("master/questserver")]
[Consumes("application/x-www-form-urlencoded")]
public partial class QuestController : ControllerBase
{
    private readonly MerrickContext _dbContext;
    private readonly IDatabase _distributedCache;
    private readonly ILogger<QuestController> _logger;

    public QuestController(MerrickContext dbContext, IDatabase distributedCache, ILogger<QuestController> logger)
    {
        _dbContext = dbContext;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    [HttpPost("getcurrentquests")]
    public async Task<IActionResult> GetCurrentQuests()
    {
        try
        {
            string? cookie = Request.Form["cookie"];
            _logger.LogGetCurrentQuests(cookie != null);

            if (cookie is not null)
            {
                (bool isValid, string? accountName) =
                    await _distributedCache.ValidateAccountSessionCookie(cookie, _dbContext, _logger);
                _logger.LogCookieValidationResult(isValid, accountName);

                if (!isValid)
                {
                    _logger.LogUnauthorizedAccess(cookie);
                    // return Unauthorized("Session Not Found"); // BLOCKED: Causes client logout loop
                }
            }

            // Returns an empty list of quests or a success status.
            // The game expects PHP serialization response.
            Dictionary<string, object> response = new()
            {
                ["quests"] = new Dictionary<string, object>(),
                ["0"] = 1 // Returning Integer 1 as "Success"
            };

            string serialized = PhpSerialization.Serialize(response);
            _logger.LogSuccessResponse(serialized);
            return Ok(serialized);
        }
        catch (Exception ex)
        {
            _logger.LogGetCurrentQuestsError(ex);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("getplayerquests")]
    public async Task<IActionResult> GetPlayerQuests()
    {
        try
        {
            string? cookie = Request.Form["cookie"];

            if (cookie is not null)
            {
                (bool isValid, string? accountName) =
                    await _distributedCache.ValidateAccountSessionCookie(cookie, _dbContext, _logger);
                if (!isValid)
                {
                    _logger.LogUnauthorizedPlayerQuests(cookie);
                    // return Unauthorized("Session Not Found"); // BLOCKED: Causes client logout loop
                }
            }

            Dictionary<string, object> response = new() { ["quests"] = new Dictionary<string, object>(), ["0"] = 1 };

            return Ok(PhpSerialization.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogGetPlayerQuestsError(ex);
            return StatusCode(500, "Internal Server Error");
        }
    }
}