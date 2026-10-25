namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles a client logout request by invalidating the session cookie in the distributed cache, and by notifying the chat server so that it ends only the chat session which used the session cookie.
    ///     The game client fires this request with <c>SetReleaseOnCompletion(true)</c> and does not read the response, but the response format is retained for protocol fidelity.
    /// </summary>
    private async Task<IActionResult> HandleLogout()
    {
        string cookie = Request.Form["cookie"].ToString();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is not null)
        {
            await DistributedCache.RemoveAccountNameForSessionCookie(cookie);

            // Notify The Chat Server To Force-Terminate The Chat Session Which Used This Session Cookie
            // The Client Normally Closes Its Own Chat Socket On Logout, But This Guards Against Cases Where That Does Not Happen Cleanly
            // The Cookie Is Published Because A Newer Login Of The Same Account Has A Chat Session Which The Logout Of An Older Machine Must Not End
            await DistributedCache.PublishAccountLogout(new AccountLogout { AccountName = accountName, SessionCookie = cookie });

            // Clearing The Latest-Login Record Is Last And Best-Effort, Because A Database Outage Must Never Stop The Chat Session From Ending, And A Stale Record Is Tolerated By The Next Login's Supersession
            try
            {
                int? accountID = await MerrickContext.Accounts.Where(account => account.Name.Equals(accountName)).Select(account => (int?) account.ID).SingleOrDefaultAsync();

                if (accountID is not null)
                    await DistributedCache.RemoveLatestSessionCookie(accountID.Value, cookie);
            }

            catch (Exception exception)
            {
                Logger.LogWarning(exception, @"Unable To Clear The Latest Session Cookie Of Account ""{AccountName}"" On Logout", accountName);
            }

            Logger.LogInformation(@"Account ""{AccountName}"" Has Logged Out", accountName);
        }

        return Ok(PhpSerialization.Serialize(new LogoutResponse()));
    }
}
