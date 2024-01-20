namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandlePreAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));

        string? clientPublicEphemeral = Request.Form["A"];

        if (clientPublicEphemeral is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingClientPublicEphemeral)));

        string? systemInformation = Request.Form["SysInfo"];

        if (systemInformation is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        if (account.Type is AccountType.Disabled)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountIsDisabled, account.NameWithClanTag)));

        User user = account.User;

        string verifier = SRPAuthenticationSessionDataStageOne.ComputeVerifier(user.SRPSalt, accountName, user.SRPPasswordHash);

        (string serverPrivateEphemeral, string serverPublicEphemeral) = SRPAuthenticationSessionDataStageOne.ComputeServerEphemeral(verifier);

        SRPAuthenticationSessionDataStageOne data = new()
        {
            LoginIdentifier = accountName,
            Salt = user.SRPSalt,
            PasswordSalt = user.SRPPasswordSalt,
            PasswordHash = user.SRPPasswordHash,
            ClientPublicEphemeral = clientPublicEphemeral,
            Verifier = verifier,
            ServerPrivateEphemeral = serverPrivateEphemeral,
            ServerPublicEphemeral = serverPublicEphemeral
        };

        Cache.SetSRPAuthenticationSessionData(accountName, data);
        Cache.SetSRPAuthenticationSystemInformation(accountName, systemInformation);

        return Ok(PhpSerialization.Serialize(new SRPAuthenticationResponseStageOne(data)));
    }

    private async Task<IActionResult> HandleSRPAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));

        string? clientProof = Request.Form["proof"];

        if (clientProof is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSRPClientProof)));

        string? operatingSystemType = Request.Form["OSType"];

        if (operatingSystemType is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingOperatingSystemType)));

        string? majorVersion = Request.Form["MajorVersion"];

        if (majorVersion is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMajorVersion)));

        string? minorVersion = Request.Form["MinorVersion"];

        if (minorVersion is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMinorVersion)));

        string? microVersion = Request.Form["MicroVersion"];

        if (microVersion is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMicroVersion)));

        SRPAuthenticationSessionDataStageOne? stageOneData = Cache.GetSRPAuthenticationSessionData(accountName);

        if (stageOneData is null)
        {
            Logger.LogError($@"[BUG] Unable To Retrieve Cached SRP Authentication Session Data For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingCachedSRPData)));
        }

        Cache.RemoveSRPAuthenticationSessionData(accountName);

        string? systemInformation = Cache.GetSRPAuthenticationSystemInformation(accountName);

        if (systemInformation is null)
        {
            Logger.LogError($@"[BUG] Unable To Retrieve Cached System Information For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        Cache.RemoveSRPAuthenticationSystemInformation(accountName);

        SRPAuthenticationSessionDataStageTwo stageTwoData = new(stageOneData, clientProof);

        string? serverProof = stageTwoData.ServerProof;

        if (serverProof is null)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectPassword)));

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        // TODO: Resolve Suspensions

        if (account.Type is not AccountType.Staff)
        {
            // TODO: Get Client Information
        }

        // TODO: Resolve Chat Server Information

        SRPHandlers.StageTwoResponseParameters parameters = new()
        {
            ServerProof = serverProof
        };

        SRPAuthenticationResponseStageTwo response = SRPHandlers.GenerateStageTwoResponse(parameters);

        // TODO: Set Cookie

        account.TimestampLastActive = DateTime.UtcNow;

        await MerrickContext.SaveChangesAsync();

        return Ok(PhpSerialization.Serialize(response));
    }

    private BadRequestObjectResult HandleAuthentication()
        => BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.SRPAuthenticationDisabled)));
}
