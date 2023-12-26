namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

[ApiController]
[Route("client_requester.php")]
[Consumes("application/x-www-form-urlencoded")]
public partial class ClientRequesterController : ControllerBase
{
    # region Client Requester Controller Description
    /*
        Requests hitting the Client Requester endpoint are uniquely identified by one of two ways to specify the PHP function that they are trying to invoke.
        The PHP function identifier is specified as either 1) the query string parameter "f", in the format "client_requester.php?f={function}", or 2) the form parameter "f", in the format "f={function}".

        Additionally, the Client Requester controller is massive due to the Master Server API not being RESTful.
        This creates an integration complication, due to the fact that MVC action methods cannot be overloaded to the degree that the Master Server API requires it.
        The problem is that MVC action methods are uniquely identified by the combination of route and request method.
        And while the query string and/or the form data of requests hitting this endpoint can be different, the route is always "client_requester.php" and the method is always POST.
        This means that it is not possible to write individual action methods that uniquely identify the request by query string parameter or form data parameter.
        And instead the Client Requester controller needs to be handled using switch pattern matching, or equivalent, on these request data.
    */
    # endregion

    [HttpPost(Name = "Client Requester All-In-One")]
    public async Task<IActionResult> ClientRequester()
    {
        return Request.Query["f"].SingleOrDefault() switch
        {
            "pre_auth"  => await HandlePreAuthenticationFunction(),
            "srpAuth"   => await HandleSRPAuthenticationFunction(),
            null        => await HandleNullQueryStringFunction(),
            _           => throw new NotImplementedException($"Unsupported Client Requester Controller Query String Parameter: f={Request.Query["f"].Single()}")
        };
    }

    private async Task<IActionResult> HandleNullQueryStringFunction()
    {
        return Request.Form["f"].SingleOrDefault() switch
        {
            _           => throw new NotImplementedException($"Unsupported Client Requester Controller Form Parameter: f={Request.Query["f"].Single()}")
        };
    }
}
