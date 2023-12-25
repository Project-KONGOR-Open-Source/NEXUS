namespace KONGOR.MasterServer.Controllers.ClientRequesterController.Base;

[ApiController]
[Route("client_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public class BaseClientRequesterController : ControllerBase
{
    /// <summary>
    ///     Requests hitting the Client Requester endpoint have two types of request identifiers.
    ///     1) Part of the query string, in the format "client_requester.php?f={endpoint}".
    ///     2) Specified as the form data request parameter "f", in the format "f={endpoint}".
    /// </summary>
    [HttpPost(Name = "Client Requester")]
    public async Task<IActionResult> ClientRequester([FromForm] Dictionary<string, string> formData)
    {
        string? functionName;
        string[] f = [.. Request.Query["f"]];

        return Ok();
    }
}
