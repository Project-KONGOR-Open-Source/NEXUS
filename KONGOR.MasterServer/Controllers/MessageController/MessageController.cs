namespace KONGOR.MasterServer.Controllers.MessageController;

[ApiController]
[Route("message")]
[Consumes("application/x-www-form-urlencoded")]
public class MessageController : ControllerBase
{
    [HttpPost("list/{id}")]
    public IActionResult List(int id)
    {
        // Stub implementation to prevent 404 errors.
        // Likely used for MOTD or system messages.

        Dictionary<string, object> response = new()
        {
            // Empty message list
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}