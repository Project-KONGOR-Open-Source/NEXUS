using KONGOR.MasterServer.Models;
using Microsoft.AspNetCore.Mvc;
using KONGOR.MasterServer.Infrastructure;

namespace KONGOR.MasterServer.Controllers;

[ApiController]
[Route("master/quest")]
[Route("master/questserver")]
[Consumes("application/x-www-form-urlencoded")]
public class QuestController : ControllerBase
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
            _logger.LogInformation($"[QuestController] GetCurrentQuests called. Cookie present: {cookie != null}");
            
            if (cookie is not null)
            {
                (bool isValid, string? accountName) = await _distributedCache.ValidateAccountSessionCookie(cookie, _dbContext, _logger);
                _logger.LogInformation($"[QuestController] Cookie validation result: IsValid={isValid}, Account={accountName}");
                
                if (!isValid) 
                {
                    _logger.LogWarning($"[QuestController] Unauthorized access attempt. Invalid cookie: {cookie}");
                    return Unauthorized("Session Not Found");
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
            _logger.LogInformation($"[QuestController] Returning success response: {serialized}");
            return Ok(serialized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuestController] Unhandled exception in GetCurrentQuests");
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
                (bool isValid, string? accountName) = await _distributedCache.ValidateAccountSessionCookie(cookie, _dbContext, _logger);
                if (!isValid) return Unauthorized("Session Not Found");
            }

             Dictionary<string, object> response = new()
            {
                ["quests"] = new Dictionary<string, object>(),
                ["0"] = 1
            };

            return Ok(PhpSerialization.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QuestController] Unhandled exception in GetPlayerQuests");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
