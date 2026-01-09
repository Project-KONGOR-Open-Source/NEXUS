namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetAccountAllHeroStats()
    {
        // Stub to prevent 500 error.
        Dictionary<string, object> response = new()
        {
             // Empty stats for now
        };
        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleGetCampaignHeroStats()
    {
        // Stub to prevent 400 error.
        Dictionary<string, object> response = new()
        {
             // Empty stats for now
        };
        return Ok(PhpSerialization.Serialize(response));
    }
}
