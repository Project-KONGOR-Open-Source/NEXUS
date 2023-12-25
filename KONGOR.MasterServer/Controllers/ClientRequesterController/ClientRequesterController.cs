namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

[ApiController]
[Route("client_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ClientRequesterController : ControllerBase
{
    /// <summary>
    ///     Requests hitting the Client Requester endpoint have two types of request identifiers.
    ///     1) Part of the query string, in the format "client_requester.php?f={function}".
    ///     2) Specified as the form data request parameter "f", in the format "f={function}".
    /// </summary>
    [HttpPost(Name = "Client Requester All-In-One")]
    public async Task<IActionResult> ClientRequester()
    {
        return Request.Query["f"].SingleOrDefault() switch
        {
            "pre_auth"  => await HandlePreAuthenticationFunction(),
            "srpAuth"   => await HandleSRPAuthenticationFunction(),
            null        => await HandleNullQueryStringFunctionParameter(),
            _           => throw new NotImplementedException($"Unsupported Client Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }

    private async Task<IActionResult> HandleNullQueryStringFunctionParameter()
    {
        return Request.Form["f"].SingleOrDefault() switch
        {
            _           => throw new NotImplementedException($"Unsupported Client Requester Controller Form Parameter: f={Request.Query["f"].Single()}")
        };
    }
}
