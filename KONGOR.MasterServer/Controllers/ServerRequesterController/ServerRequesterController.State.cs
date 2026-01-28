namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private Task<IActionResult> HandleShutdown()
    {
        string? session = Request.Form["session"];

        if (session is null)
        {
            return Task.FromResult<IActionResult>(BadRequest(@"Missing Value For Form Parameter ""session"""));
        }

        /*
            Match server shutdown logic would normally go here, however, we NOOP (no operation) this action and let the TCP server handle it instead.
            The reason is that there is no HTTP endpoint for match server manager shutdown, while the match server and the match server manager both issue a TCP command for shutdown.
            Thus, we just return OK here and handle shutdown consistently using the NET_CHAT_GS_DISCONNECT and NET_CHAT_SM_DISCONNECT TCP commands as initiators.
        */

        return Task.FromResult<IActionResult>(Ok());
    }
}