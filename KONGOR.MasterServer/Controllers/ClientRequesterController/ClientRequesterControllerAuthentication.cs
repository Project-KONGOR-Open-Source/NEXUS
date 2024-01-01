namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandlePreAuthenticationFunction()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        string? clientPublicEphemeral = Request.Form["A"];

        if (clientPublicEphemeral is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.BadRequest)));

        string? systemInformation = Request.Form["SysInfo"];

        if (systemInformation is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.BadRequest)));

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        if (account.AccountType is AccountType.Disabled)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountIsDisabled, account)));

        User user = account.User;

        SRPAuthenticationSessionData data = new()
        {
            LoginIdentifier = accountName,
            Salt = user.SRPSalt,
            PasswordSalt = user.SRPPasswordSalt,
            PasswordHash = user.SRPPasswordHash,
            ClientPublicEphemeral = clientPublicEphemeral,
            SystemInformation = systemInformation
        };

        Cache.SetSRPAuthenticationSessionData(accountName, data);

        return Ok(PhpSerialization.Serialize(new SRPAuthenticationResponseStageOne(data)));
    }

    private async Task<IActionResult> HandleSRPAuthenticationFunction()
    {
        // TODO: Implement This

        throw new NotImplementedException();
    }
}
