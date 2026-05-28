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

        // Despite The C# ".Equals" Looking Ordinal, This Translates To A Plain SQL Equality, And SQL Server's Default Collation Is Case-Insensitive, So Any Casing Variant Of A Registered Account Name Matches Here
        // Authentication Therefore Succeeds Regardless Of The Submitted Casing; Any Propagation Beyond This Method Must Use The Canonical "account.Name" Rather Than The User-Supplied "accountName" To Avoid Leaking The Caller's Casing Downstream
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        if (account.Type is AccountType.Disabled)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountIsDisabled, account.NameWithClanTag)));

        User user = account.User;

        string authenticationSessionSalt = SRPAuthenticationSessionDataStageOne.GenerateSRPSessionSalt();

        string verifier = SRPAuthenticationSessionDataStageOne.ComputeVerifier(authenticationSessionSalt, accountName, user.SRPPasswordHash);

        (string serverPrivateEphemeral, string serverPublicEphemeral) = SRPAuthenticationSessionDataStageOne.ComputeServerEphemeral(verifier);

        SRPAuthenticationSessionDataStageOne data = new ()
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

        string? systemInformationHashes = Request.Form["SysInfo"];

        if (systemInformationHashes is null)
            return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));

        SRPAuthenticationSessionDataStageOne? stageOneData = await DistributedCache.GetSRPAuthenticationSessionData(accountName);

        if (stageOneData is null)
        {
            Logger.LogError($@"[BUG] Unable To Retrieve Cached SRP Authentication Session Data For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingCachedSRPData)));
        }

        await DistributedCache.RemoveSRPAuthenticationSessionData(accountName);

        string? systemInformation = await DistributedCache.GetSRPAuthenticationSystemInformation(accountName);

        if (systemInformation is null)
        {
            Logger.LogError($@"[BUG] Unable To Retrieve Cached System Information For Account Name ""{accountName}""");

            return UnprocessableEntity(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        await DistributedCache.RemoveSRPAuthenticationSystemInformation(accountName);

        SRPAuthenticationSessionDataStageTwo stageTwoData = new (stageOneData, clientProof);

        string? serverProof = stageTwoData.ServerProof;

        if (serverProof is null)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectPassword)));

        // Despite The C# ".Equals" Looking Ordinal, This Translates To A Plain SQL Equality, And SQL Server's Default Collation Is Case-Insensitive, So Any Casing Variant Of A Registered Account Name Matches Here
        // Authentication Therefore Succeeds Regardless Of The Submitted Casing; Any Propagation Beyond This Method Must Use The Canonical "account.Name" Rather Than The User-Supplied "accountName" To Avoid Leaking The Caller's Casing Downstream
        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
            .Include(account => account.BannedPeers)
            .Include(account => account.FriendedPeers)
            .Include(account => account.IgnoredPeers)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));

        if (account.Type is AccountType.ServerHost)
            return Unauthorized(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IsServerHostingAccount)));

        if (Request.HttpContext.Connection.RemoteIpAddress is null)
        {
            Logger.LogError($@"[BUG] Remote IP Address For Account Name ""{accountName}"" Is NULL");

            return UnprocessableEntity(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingIPAddress)));
        }

        string remoteIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        if (account.Type is not AccountType.Staff)
        {
            string agent = Request.Headers.UserAgent.SingleOrDefault() ?? string.Empty;

            if (UserAgentRegex().IsMatch(agent).Equals(false))
            {
                Logger.LogError($@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected User Agent ""{agent}""");

                return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.UnexpectedUserAgent)));
            }

            if (account.IPAddressCollection.Contains(remoteIPAddress).Equals(false))
                account.IPAddressCollection.Add(remoteIPAddress);

            /*
                Stage One (pre_auth) System Information Format:
                  - Windows: "MAC|OS|Processor|Video|RAM" (five pipe-separated data points)
                  - Linux/macOS: "not running on windows" (plain string)
            */
            bool isNonWindowsClient = systemInformation.Equals("not running on windows", StringComparison.Ordinal);

            if (isNonWindowsClient)
            {
                if (account.SystemInformationCollection.Contains(systemInformation).Equals(false))
                    account.SystemInformationCollection.Add(systemInformation);
            }

            else
            {
                string[] systemInformationDataPoints = [.. systemInformation.Split('|', StringSplitOptions.RemoveEmptyEntries)];

                if (systemInformationDataPoints.Length is not 5 /* the expected count of data points */)
                {
                    Logger.LogError($@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected System Information ""{systemInformation}""");

                    return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
                }

                if (account.MACAddressCollection.Contains(systemInformationDataPoints.First()).Equals(false))
                    account.MACAddressCollection.Add(systemInformationDataPoints.First());

                if (account.SystemInformationCollection.Contains(string.Join('|', systemInformationDataPoints.Skip(1))).Equals(false))
                    account.SystemInformationCollection.Add(string.Join('|', systemInformationDataPoints.Skip(1)));
            }

            /*
                The HWID hash implementation appears to be bugged in the client.
                The intention was probably for each of the five system information data points to be hashed separately, but instead the same HWID hash is repeated five times.

                Stage Two (srpAuth) System Information Hashes Format:
                  - All Platforms (HWID obtained): "hash|hash|hash|hash|hash" (same HWID hash repeated 5 times, likely a bug)
                  - All Platforms (HWID not obtained): "not obtainable" (plain string)
            */
            string? systemInformationHash = systemInformationHashes.Split('|', StringSplitOptions.RemoveEmptyEntries).Distinct().SingleOrDefault();

            if (systemInformationHash is null)
            {
                Logger.LogError($@"Account ""{account.NameWithClanTag}"" Has Made A Request To Log In Using Unexpected System Information Hashes ""{systemInformationHashes}""");

                return BadRequest(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
            }

            if (account.SystemInformationHashCollection.Contains(systemInformationHash).Equals(false))
                account.SystemInformationHashCollection.Add(systemInformationHash);

            await MerrickContext.SaveChangesAsync();
        }

        // TODO: Resolve Suspensions

        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
            ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerClientConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT")
            ?? throw new NullReferenceException("Chat Server Client Connections Port Is NULL"));

        SRPAuthenticationHandlers.StageTwoResponseParameters parameters = new ()
        {
            Account = account,
            ClanRoster = account.Clan?.Members ?? [],
            ServerProof = serverProof,
            ClientIPAddress = remoteIPAddress,
            ChatServer = (chatServerHost, chatServerClientConnectionsPort)
        };

        SRPAuthenticationResponseStageTwo response = SRPAuthenticationHandlers.GenerateStageTwoResponse(parameters, out string cookie);

        account.TimestampLastActive = DateTimeOffset.UtcNow;

        await MerrickContext.SaveChangesAsync();

        await DistributedCache.SetAccountNameForSessionCookie(cookie, account.Name);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is not null)
            Logger.LogWarning(@"Account ""{AccountName}"" Is Attempting To Use HTTP Client Authentication", accountName);

        string response = PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.SRPAuthenticationDisabled));

        return BadRequest(response);
    }

    [GeneratedRegex(@"(?>S2 Games)\/(?>Heroes [oO]f Newerth)\/(?<version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\/(?<platform>[wlm]a[cs])\/(?<architecture>x86_64|x86-biarch|universal-64)")]
    private static partial Regex UserAgentRegex();
}
