namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleClaimSeasonRewards()
    {
        // Reference implementation returns ClaimSeasonRewardsResponse with default values.
        Dictionary<string, object> response = new()
        {
            ["vested_threshold"] = 5,
            ["0"] = true
        };
        return Ok(PhpSerialization.Serialize(response));
    }
}
