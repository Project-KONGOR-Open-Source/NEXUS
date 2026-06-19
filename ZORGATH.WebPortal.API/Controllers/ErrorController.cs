namespace ZORGATH.WebPortal.API.Controllers;

/// <summary>
///     The terminal handler for the production exception-handling pipeline configured via <c>UseExceptionHandler("/error")</c>.
///     It produces a problem details response so that an unhandled exception surfaces as a clean 500 response, rather than the exception handler itself producing a 404 response (which masks the original exception with a secondary one).
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error() => Problem();
}
