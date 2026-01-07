namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleClientEventsInfo()
    {
        // Stub to prevent 500 error.
        Dictionary<string, object> response = new()
        {
             // Empty events info
        };
        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleGetSpecialMessages()
    {
         // Stub to prevent 500 error.
        Dictionary<string, object> response = new()
        {
             // Empty messages
        };
        return Ok(PhpSerialization.Serialize(response));
    }
}
