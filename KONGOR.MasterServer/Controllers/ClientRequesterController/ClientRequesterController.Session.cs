namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles a client logout request by invalidating the session cookie in the distributed cache.
    ///     The game client fires this request with <c>SetReleaseOnCompletion(true)</c> and does not read the response, but the response format is retained for protocol fidelity.
    /// </summary>
    private async Task<IActionResult> HandleLogout()
    {
        string cookie = Request.Form["cookie"].ToString();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is not null)
        {
            await DistributedCache.RemoveAccountNameForSessionCookie(cookie);

            // Notify The Chat Server To Force-Terminate Any Active Chat Session For This Account
            // The Client Normally Closes Its Own Chat Socket On Logout, But This Guards Against Cases Where That Does Not Happen Cleanly
            await DistributedCache.PublishAccountLogout(accountName);

            Logger.LogInformation(@"Account ""{AccountName}"" Has Logged Out", accountName);
        }

        return Ok(PhpSerialization.Serialize(new LogoutResponse()));
    }
}
