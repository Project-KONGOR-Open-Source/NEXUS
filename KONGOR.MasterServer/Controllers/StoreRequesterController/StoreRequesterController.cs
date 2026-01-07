using KONGOR.MasterServer.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace KONGOR.MasterServer.Controllers;

[ApiController]
[Route("store_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public class StoreRequesterController : ControllerBase
{
    [HttpPost]
    public IActionResult Handle()
    {
        // Stub implementation to prevent 404 errors.
        // The store requester handles various micro-transactions and catalog requests.
        // For now, returning a generic success or empty response is sufficient.
        
        Dictionary<string, object> response = new()
        {
            // Empty response
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}
