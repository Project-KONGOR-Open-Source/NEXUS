namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleGetAccountMastery()
    {
        string? cookie = Request.Form["cookie"];
        if (string.IsNullOrEmpty(cookie))
        {
            return new UnauthorizedResult();
        }

        (bool accountSessionCookieIsValid, string? sessionAccountName) =
            await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (accountSessionCookieIsValid.Equals(false) || sessionAccountName is null)
        {
            return Unauthorized($@"No Session Found For Cookie ""{cookie}""");
        }

        Account? account = await MerrickContext.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Name == sessionAccountName);

        if (account is null)
        {
            return Unauthorized(@"Account Not Found");
        }

        // "Masteries" table is missing in NEXUS schema, so we cannot query it matching READ ONLY.
        // Returning a mocked structure with empty mastery info to mimic a user with 0 masteries.
        // READ ONLY Reference: response["mastery_info"] = mastery_info.ToString();

        Dictionary<string, object> response = new()
        {
            { "error_code", 0 },
            { "error_msg", string.Empty },
            { "account_id", account.ID },
            { "mastery_info", string.Empty }
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}