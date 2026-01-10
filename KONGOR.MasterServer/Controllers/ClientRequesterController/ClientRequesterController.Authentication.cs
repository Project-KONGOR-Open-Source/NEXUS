namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandlePreAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
        {
            return NotFound(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));
        }

        string? clientPublicEphemeral = Request.Form["A"];

        if (clientPublicEphemeral is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingClientPublicEphemeral)));
        }

        string? systemInformation = Request.Form["SysInfo"];

        if (systemInformation is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.ToLower() == accountName.ToLower());

        if (account is null)
        {
            return NotFound(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));
        }

        if (account.Type is AccountType.Disabled)
        {
            return Unauthorized(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountIsDisabled,
                    account.NameWithClanTag)));
        }

        User user = account.User;

        string authenticationSessionSalt = SRPAuthenticationSessionDataStageOne.GenerateSRPSessionSalt();

        string verifier =
            SRPAuthenticationSessionDataStageOne.ComputeVerifier(authenticationSessionSalt, accountName,
                user.SRPPasswordHash);

        (string serverPrivateEphemeral, string serverPublicEphemeral) =
            SRPAuthenticationSessionDataStageOne.ComputeServerEphemeral(verifier);

        SRPAuthenticationSessionDataStageOne data = new()
        {
            LoginIdentifier = accountName,
            SessionSalt = authenticationSessionSalt,
            PasswordSalt = user.SRPPasswordSalt,
            PasswordHash = user.SRPPasswordHash,
            ClientPublicEphemeral = clientPublicEphemeral,
            Verifier = verifier,
            ServerPrivateEphemeral = serverPrivateEphemeral,
            ServerPublicEphemeral = serverPublicEphemeral
        };

        await DistributedCache.SetSRPAuthenticationSessionData(accountName, data);
        await DistributedCache.SetSRPAuthenticationSystemInformation(accountName, systemInformation);

        return Ok(PhpSerialization.Serialize(new SRPAuthenticationResponseStageOne(data)));
    }

    private async Task<IActionResult> HandleSRPAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
        {
            return NotFound(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));
        }

        string? clientProof = Request.Form["proof"];

        if (clientProof is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSRPClientProof)));
        }

        string? operatingSystemType = Request.Form["OSType"];

        if (operatingSystemType is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingOperatingSystemType)));
        }

        string? majorVersion = Request.Form["MajorVersion"];

        if (majorVersion is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMajorVersion)));
        }

        string? minorVersion = Request.Form["MinorVersion"];

        if (minorVersion is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMinorVersion)));
        }

        string? microVersion = Request.Form["MicroVersion"];

        if (microVersion is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMicroVersion)));
        }

        string? systemInformationHashes = Request.Form["SysInfo"];

        if (systemInformationHashes is null)
        {
            return BadRequest(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        SRPAuthenticationSessionDataStageOne? stageOneData =
            await DistributedCache.GetSRPAuthenticationSessionData(accountName);

        if (stageOneData is null)
        {
            Logger.LogError(
                $@"[BUG] Unable To Retrieve Cached SRP Authentication Session Data For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingCachedSRPData)));
        }

        await DistributedCache.RemoveSRPAuthenticationSessionData(accountName);

        string? systemInformation = await DistributedCache.GetSRPAuthenticationSystemInformation(accountName);

        if (systemInformation is null)
        {
            Logger.LogError($@"[BUG] Unable To Retrieve Cached System Information For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        await DistributedCache.RemoveSRPAuthenticationSystemInformation(accountName);

        SRPAuthenticationSessionDataStageTwo stageTwoData = new(stageOneData, clientProof);

        string? serverProof = stageTwoData.ServerProof;

        if (serverProof is null)
        {
            return Unauthorized(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectPassword)));
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
            .Include(account => account.BannedPeers)
            .Include(account => account.FriendedPeers)
            .Include(account => account.IgnoredPeers)
            .FirstOrDefaultAsync(account => account.Name.ToLower() == accountName.ToLower());

        if (account is null)
        {
            return NotFound(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));
        }

        if (account.Type is AccountType.ServerHost)
        {
            return Unauthorized(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IsServerHostingAccount)));
        }

        if (Request.HttpContext.Connection.RemoteIpAddress is null)
        {
            Logger.LogError($@"[BUG] Remote IP Address For Account Name ""{accountName}"" Is NULL");

            return UnprocessableEntity(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingIPAddress)));
        }

        string remoteIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        if (account.Type is not AccountType.Staff)
        {
            string agent = Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;

            if (UserAgentRegex().IsMatch(agent).Equals(false))
            {
                Logger.LogError(
                    $@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected User Agent ""{agent}""");

                return BadRequest(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.UnexpectedUserAgent)));
            }

            if (account.IPAddressCollection.Contains(remoteIPAddress).Equals(false))
            {
                account.IPAddressCollection.Add(remoteIPAddress);
            }

            string[] systemInformationDataPoints =
                [.. systemInformation.Split('|', StringSplitOptions.RemoveEmptyEntries)];

            if (systemInformationDataPoints.Length is not 5 /* the expected count of data points */)
            {
                Logger.LogError(
                    $@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected System Information ""{systemInformation}""");

                return BadRequest(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(
                        SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
            }

            if (account.MACAddressCollection.Contains(systemInformationDataPoints.First()).Equals(false))
            {
                account.MACAddressCollection.Add(systemInformationDataPoints.First());
            }

            if (account.SystemInformationCollection.Contains(string.Join('|', systemInformationDataPoints.Skip(1)))
                .Equals(false))
            {
                account.SystemInformationCollection.Add(string.Join('|', systemInformationDataPoints.Skip(1)));
            }

            // The set of system information hashes is most likely bugged.
            // The intention probably was for each of the five data points to be hashed separately, but instead the entire system information is hashed five times.
            // NOTE: The value of Request.Form["SysInfo"] for Linux and macOS clients is "not running on windows".
            string? systemInformationHash = systemInformationHashes.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Distinct().FirstOrDefault();

            if (systemInformationHash is null)
            {
                Logger.LogError(
                    $@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected System Information Hashes ""{systemInformationHashes}""");

                return BadRequest(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(
                        SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
            }

            if (account.SystemInformationHashCollection.Contains(systemInformationHash).Equals(false))
            {
                account.SystemInformationHashCollection.Add(systemInformationHash);
            }

            await MerrickContext.SaveChangesAsync();
        }

        // TODO: Resolve Suspensions

        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
                                ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerClientConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT")
                                                        ?? throw new NullReferenceException(
                                                            "Chat Server Client Connections Port Is NULL"));

        SRPAuthenticationHandlers.StageTwoResponseParameters parameters = new()
        {
            Account = account,
            ClanRoster = account.Clan?.Members ?? [],
            ServerProof = serverProof,
            ClientIPAddress = remoteIPAddress,
            ChatServer = (chatServerHost, chatServerClientConnectionsPort)
        };

        SRPAuthenticationResponseStageTwo response =
            SRPAuthenticationHandlers.GenerateStageTwoResponse(parameters, out string cookie);

        account.TimestampLastActive = DateTimeOffset.UtcNow;
        account.Cookie = cookie;

        await MerrickContext.SaveChangesAsync();
        Logger.LogInformation("Persisted Cookie {Cookie} for Account {AccountName} to Database", cookie, accountName);

        await DistributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is not null)
        {
            Logger.LogWarning(@"Account ""{AccountName}"" Is Attempting To Use HTTP Client Authentication",
                accountName);
        }

        string response =
            PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.SRPAuthenticationDisabled));

        return BadRequest(response);
    }

    private async Task<IActionResult> HandleAids2Cookie()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is not null)
        {
            cookie = cookie.Replace("-", string.Empty);
        }

        if (cookie is null)
        {
            Logger.LogError("Missing Cookie In aids2cookie Request");

            return BadRequest(PhpSerialization.Serialize(new { error = "Missing Cookie" }));
        }

        string? accountName = HttpContext.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            // Fallback: Check the database for the cookie to handle cases where the cache has been cleared (e.g. server restart)
            var accountData = await MerrickContext.Accounts
                .Where(account => account.Cookie == cookie)
                .Select(account => new { account.Name, account.ID })
                .FirstOrDefaultAsync();

            if (accountData is not null)
            {
                // Re-populate the cache
                await DistributedCache.SetAccountNameForSessionCookie(cookie, accountData.Name);
                return Ok(PhpSerialization.Serialize(accountData.ID));
            }

            // This should theoretically not be reached if validation passed in the main controller
            Logger.LogError(
                $@"Cookie ""{cookie}"" Validated In Controller But Account Name Not Found In Cache (Context Missing, Redis Check Failed)");

            return Unauthorized(PhpSerialization.Serialize(new { error = "Invalid Session" }));
        }

        int? accountId = await MerrickContext.Accounts
            .Where(account => account.Name.ToLower() == accountName.ToLower())
            .Select(account => (int?) account.ID)
            .FirstOrDefaultAsync();

        if (accountId is null)
        {
            Logger.LogError($@"Account Name ""{accountName}"" From Cookie Not Found In Database");

            return NotFound(PhpSerialization.Serialize(new { error = "Account Not Found" }));
        }

        return Ok(PhpSerialization.Serialize(accountId.Value));
    }

    private async Task<IActionResult> HandleLogout()
    {
        string? cookie = Request.Form["cookie"];
        if (cookie is not null)
        {
            await DistributedCache.RemoveAccountNameForSessionCookie(cookie);
        }

        return Ok(PhpSerialization.Serialize(true));
    }

    [GeneratedRegex(
        @"(?>S2 Games)\/(?>Heroes [oO]f Newerth)\/(?<version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\/(?<platform>[wlm]a[cs])\/(?<architecture>x86_64|x86-biarch|universal-64)")]
    private static partial Regex UserAgentRegex();
}