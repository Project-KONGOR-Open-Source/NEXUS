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
    [HttpPost("getcurrentquests")]
    public IActionResult GetCurrentQuests()
    {
        // Stub implementation to prevent 404 errors.
        // Returns an empty list of quests or a success status.
        // The game expects PHP serialization response.
        
        Dictionary<string, object> response = new()
        {
            ["quests"] = new Dictionary<string, object>()
        };

        return Ok(PhpSerialization.Serialize(response));
    }
    
    [HttpPost("getplayerquests")]
    public IActionResult GetPlayerQuests()
    {
         Dictionary<string, object> response = new()
        {
            ["quests"] = new Dictionary<string, object>()
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
