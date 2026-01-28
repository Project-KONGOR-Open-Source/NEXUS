namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private Task<IActionResult> HandleGetQuickStats()
    {
        // Stub implementation to prevent 500 errors.
        // Quick stats likely returns summary data for player profiles.
        Dictionary<string, object> response = new()
        {
            // Empty for now
        };

        return Task.FromResult<IActionResult>(Ok(PhpSerialization.Serialize(response)));
    }
}