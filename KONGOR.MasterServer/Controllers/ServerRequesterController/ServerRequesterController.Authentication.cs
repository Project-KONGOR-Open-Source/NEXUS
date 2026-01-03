namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private async Task<IActionResult> HandleServerManagerAuthentication()
    {
        string? hostAccountName = Request.Form["login"];

        if (hostAccountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""login""");

        hostAccountName = hostAccountName.TrimEnd(':'); // The Semicolon Is Used To Separate The Account Name From The Server Instance, So We Need To Remove It Because It Is Not Needed For The Server Manager

        string? accountPasswordHash = Request.Form["pass"];

        if (accountPasswordHash is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(hostAccountName));

        if (account is null)
            return NotFound($@"Account ""{hostAccountName}"" Was Not Found");

        if (account.Type is not AccountType.ServerHost)
            return Unauthorized($@"Account ""{hostAccountName}"" Is Not A Server Host");

        string srpPasswordHash = SRPAuthenticationHandlers.ComputeSRPPasswordHash(accountPasswordHash, account.User.SRPPasswordSalt);

        if (srpPasswordHash.Equals(account.User.SRPPasswordHash) is false)
            return Unauthorized("Incorrect Password");

        if (Request.HttpContext.Connection.RemoteIpAddress is null)
        {
            Logger.LogError(@"[BUG] Remote IP Address For Server Manager With Host Account Name ""{HostAccountName}"" Is NULL", hostAccountName);

            return BadRequest("Unable To Resolve Remote IP Address");
        }

        MatchServerManager matchServerManager = new ()
        {
            HostAccountID = account.ID,
            HostAccountName = account.Name,
            ID = hostAccountName.GetDeterministicInt32Hash(),
            MatchServerIDs = [],
            IPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        };

        await DistributedCache.SetMatchServerManager(hostAccountName, matchServerManager);

        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
            ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerMatchServerManagerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER_MANAGER")
            ?? throw new NullReferenceException("Chat Server Match Server Manager Connections Port Is NULL"));

        Dictionary<string, object> response = new ()
        {
            ["server_id"] = matchServerManager.ID,
            ["official"] = 1, // If Not Official, It Is Considered To Be Un-Authorized
            ["session"] = matchServerManager.Cookie,
            ["chat_address"] = chatServerHost,
            ["chat_port"] = chatServerMatchServerManagerConnectionsPort,
        };

        // TODO: Investigate How These Are Used
        response["cdn_upload_host"] = Configuration.CDN.Host;
        response["cdn_upload_target"] = "upload";

        Logger.LogInformation(@"Server Manager ID ""{MatchServerManagerID}"" Was Registered At ""{MatchServerManagerIPAddress}"" With Cookie ""{MatchServerManagerCookie}""",
            matchServerManager.ID, matchServerManager.IPAddress, matchServerManager.Cookie);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleServerAuthentication()
    {
        string serverIdentifier = Request.Form["login"].ToString();

        if (serverIdentifier.Split(':').Length is not 2)
            return BadRequest(@"Missing Or Incorrect Value For Form Parameter ""login""");

        string hostAccountName = serverIdentifier.Split(':').First();
        string serverInstance = serverIdentifier.Split(':').Last();

        string? accountPasswordHash = Request.Form["pass"];

        if (accountPasswordHash is null) 
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        string? serverPort = Request.Form["port"];

        if (serverPort is null)
            return BadRequest(@"Missing Value For Form Parameter ""port""");

        string? serverName = Request.Form["name"];

        if (serverName is null)
            return BadRequest(@"Missing Value For Form Parameter ""name""");

        string? serverDescription = Request.Form["desc"];

        if (serverDescription is null)
            return BadRequest(@"Missing Value For Form Parameter ""desc""");

        string? serverLocation = Request.Form["location"];

        if (serverLocation is null)
            return BadRequest(@"Missing Value For Form Parameter ""location""");

        string? serverIPAddress = Request.Form["ip"];

        if (serverIPAddress is null)
            return BadRequest(@"Missing Value For Form Parameter ""ip""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(hostAccountName));

        if (account is null)
            return NotFound($@"Account ""{hostAccountName}"" Was Not Found");

        if (account.Type is not AccountType.ServerHost)
            return Unauthorized($@"Account ""{hostAccountName}"" Is Not A Server Host");

        string srpPasswordHash = SRPAuthenticationHandlers.ComputeSRPPasswordHash(accountPasswordHash, account.User.SRPPasswordSalt);
        
        if (srpPasswordHash.Equals(account.User.SRPPasswordHash) is false)
            return Unauthorized("Incorrect Password");

        // TODO: Verify Whether The Server Version Matches The Client Version (Or Disallow Servers To Be Started If They Are Not On The Latest Version)

        MatchServerManager? matchServerManager = (await DistributedCache.GetMatchServerManagersByAccountName(hostAccountName)).SingleOrDefault();

        MatchServer matchServer = new ()
        {
            HostAccountID = account.ID,
            HostAccountName = account.Name,
            ID = serverIdentifier.GetDeterministicInt32Hash(),
            Name = serverName,
            MatchServerManagerID = matchServerManager?.ID,
            Instance = int.Parse(serverInstance),
            IPAddress = serverIPAddress,
            Port = int.Parse(serverPort),
            Location = serverLocation,
            Description = serverDescription
        };

        await DistributedCache.SetMatchServer(hostAccountName, matchServer);

        if (matchServerManager is not null)
        {
            matchServerManager.MatchServerIDs.Add(matchServer.ID);

            await DistributedCache.SetMatchServerManager(hostAccountName, matchServerManager);
        }

        // TODO: Implement Verifier In Description (If The Server Is A COMPEL Server, It Will Have A Verifier In The Description)
        // INFO: The Server Manager Doesn't Send Descriptions, So Use Name Instead Since We Can Override Them Anyway Via Remote Command (svr_name) On TCP Handshake

        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
            ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerMatchServerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER")
            ?? throw new NullReferenceException("Chat Server Match Server Connections Port Is NULL"));

        Dictionary<string, object> response = new ()
        {
            ["session"] = matchServer.Cookie,
            ["server_id"] = matchServer.ID,
            ["chat_address"] = chatServerHost,
            ["chat_port"] = chatServerMatchServerConnectionsPort,
            ["leaverthreshold"] = 0.05
        };

        Logger.LogInformation(@"Server ID ""{MatchServerID}"" Was Registered At ""{MatchServerIPAddress}"":""{MatchServerPort}"" With Cookie ""{MatchServerCookie}""",
            matchServer.ID, matchServer.IPAddress, matchServer.Port, matchServer.Cookie);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleConnectClient()
    {
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? ip = Request.Form["ip"];

        if (ip is null)
            return BadRequest(@"Missing Value For Form Parameter ""ip""");

        string? casual = Request.Form["cas"];

        if (casual is null)
            return BadRequest(@"Missing Value For Form Parameter ""cas""");

        // This value is the ChatProtocol.ArrangedMatchType value plus 1. This enum seems to be 1-indexed on the client-side.
        // Example 1: a value of 1 means AM_PUBLIC, which is ChatProtocol.ArrangedMatchType value 0.
        // Example 2: a value of 2 means AM_MATCHMAKING, which is ChatProtocol.ArrangedMatchType value 1.
        string? arrangedMatchType = Request.Form["new"];

        if (arrangedMatchType is null)
            return BadRequest(@"Missing Value For Form Parameter ""new""");

        string? accountNameForSessionCookie = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountNameForSessionCookie is null)
            return Unauthorized("No Valid Client Session Cookie Could Be Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountNameForSessionCookie));

        if (account is null)
        {
            Logger.LogError(@"[BUG] No Account Could Be Found For Account Name ""{AccountName}"" With Session Cookie ""{Cookie}""", accountNameForSessionCookie, cookie);

            return BadRequest($@"Account With Name ""{accountNameForSessionCookie}"" Could Not Be Found");
        }

        Dictionary<string, object> response = new ()
        {
            { "cookie", cookie },
            { "account_id", account.ID },
            { "nickname", account.Name },
            { "super_id", account.User.Accounts.Single(record => record.IsMain).ID },
            { "account_type", account.Type },
            { "level", account.User.TotalLevel }
        };

        if (account.Clan is not null)
        {
            response.Add("clan_id", account.Clan.ID);
            response.Add("tag", account.Clan.Tag);
        }

        /*
            public static List<Info> InfoForAccount(AccountDetails accountDetails, float tournamentRatingForActiveTeam)
           {
               Info info = new ()
               {
                   AccountId = accountDetails.AccountId.ToString(),
                   Standing = "3",
                   Level = "1",
                   LevelExp = "0",
                   // AllTimeTotalDisconnects: appears to be ignored
                   // PossibleDisconnects: appears to be ignored
                   // AllTimeGamesPlayed: appears to be ignored
                   // NumBotGamesWon: appears to be ignored
                   PSR = accountDetails.PublicRating,
                   // PublicGameWins = publicStats.Wins,
                   // PublicGameLosses = publicStats.Losses,
                   PublicGamesPlayed = accountDetails.PublicGamesPlayed,
                   PublicGameDisconnects = accountDetails.PublicTimesDisconnected,
                   // NormalRankedGamesMMR: unused in KONGOR
                   // NormalRankedGameWins: unused in KONGOR
                   // NormalRankedGameLosses: unused in KONGOR
                   // NormalRankedGamesPlayed: unused in KONGOR
                   // NormalRankedGameDisconnects: unused in KONGOR
                   // CasualModeMMR: unused in KONGOR
                   // CasualModeWins: unused in KONGOR
                   // CasualModeLosses: unused in KONGOR
                   // CasualModeGamesPlayed: unused in KONGOR
                   // CasualModeDisconnects: unused in KONGOR
                   MidWarsMMR = accountDetails.MidWarsRating,
                   MidWarsGamesPlayed = accountDetails.MidWarsGamesPlayed,
                   MidWarsTimesDisconnected = accountDetails.MidWarsTimesDisconnected,

                   // Number of Tournament matches played. Note: rift wars is used as a piggy-back.
                   RiftWarsGamesPlayed = accountDetails.TournamentGamesPlayed,
                   RiftWarsDisconnects = accountDetails.TournamentTimesDisconnected,
                   RiftWarsRating = tournamentRatingForActiveTeam,

                   IsNew = 0,
                   ChampionsOfNewerthNormalMMR = accountDetails.CoNNormalRating,
                   ChampionsOfNewerthNormalRank = accountDetails.CoNNormalRank,
                   ChampionsOfNewerthGamesPlayed = accountDetails.CoNNormalGamesPlayed,
                   ChampionsOfNewerthGameDisconnects = accountDetails.CoNNormalTimesDisconnected,

                   ChampionsOfNewerthCasualMMR = accountDetails.CoNCasualRating,
                   ChampionsOfNewerthCasualRank = accountDetails.CoNCasualRank,
                   ChampionsOfNewerthCasualGamesPlayed = accountDetails.CoNCasualGamesPlayed,
                   ChampionsOfNewerthCasualGameDisconnects = accountDetails.CoNCasualTimesDisconnected,

                   // Additional Public Games info requested by server_requester.php?f=c_conn
                   // Unclear if used or not.
                   // PublicHeroKills = account.PlayerSeasonStatsPublic.HeroKills,
                   // PublicHeroAssists = account.PlayerSeasonStatsPublic.HeroAssists,
                   // PublicDeaths = account.PlayerSeasonStatsPublic.Deaths,
                   // PublicWardsPlaced = account.PlayerSeasonStatsPublic.Wards,
                   // PublicGoldEarned = account.PlayerSeasonStatsPublic.Gold,
                   // PublicExpEarned = account.PlayerSeasonStatsPublic.Exp,
                   // PublicSecondsPlayed = account.PlayerSeasonStatsPublic.Secs,
                   // PublicTimeEarningExp = account.PlayerSeasonStatsPublic.TimeEarningExp,

                   // Additional TMM info requested by server_requester.php?f=c_conn

                   // Additional unknown fields requested by server_requester.php?f=c_conn
                   // Unclear if used or not.
                   rnk_amm_solo_conf = 0,
                   rnk_amm_team_conf = 0,
               };

               return new List<Info>() { info };
           }
         */

        // TODO: Create Proper Response Model

        response.Add("infos", ""); // TODO: Set These Stats
        response.Add("game_cookie", "16cb3211-5253-45a8-bcb9-10d037ec9303"); // Must Exist, But The Value Doesn't Really Matter; TODO: Generate And Store This Cookie Per Match?
        response.Add("my_upgrades", account.User.OwnedStoreItems);
        response.Add("selected_upgrades", account.SelectedStoreItems);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleAcceptKey()
    {
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        string? accountKey = Request.Form["acc_key"];

        if (accountKey is null)
            return BadRequest(@"Missing Value For Form Parameter ""acc_key""");

        //GameServer? server = MerrickContext.GameServers.SingleOrDefault(server => server.Cookie.Equals(formData["session"]));
        //if (server is null) return Unauthorized();

        //if (KongorContext.InvalidateGameHostingPermissionToken(formData["acc_key"]).Equals(false))
        //    return Unauthorized($@"NOT AUTHORISED: Invalid Account Key ""{formData["acc_key"]}""");

        /*
        Dictionary<string, object> response = new ()
        {
            { "server_id", server.GameServerId },
            { "official", server.Official ? 1 : 0 } // 0 = Unofficial; 1 = Official With Stats; 2 = Official Without Stats;
        };
        */

        // TODO: Fix This Mess !!! (Maybe Just Use The Cookie As The Account Key?)

        Dictionary<string, object> response = new ()
        {
            { "server_id", 666 },
            { "official", 1 } // 0 = Unofficial; 1 = Official With Stats; 2 = Official Without Stats;
        };

        // TODO: Fully Inspect Response Model

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleSetOnline()
    {
        string? session = Request.Form["session"];

        if (session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        string? connectionsCount = Request.Form["num_conn"];

        if (connectionsCount is null)
            return BadRequest(@"Missing Value For Form Parameter ""num_conn""");

        string? gameTime = Request.Form["cgt"];

        if (gameTime is null)
            return BadRequest(@"Missing Value For Form Parameter ""cgt""");

        string? map = Request.Form["map"];

        if (map is null)
            return BadRequest(@"Missing Value For Form Parameter ""map""");

        string? isPrivate = Request.Form["private"];

        if (isPrivate is null)
            return BadRequest(@"Missing Value For Form Parameter ""private""");

        string? isVIP = Request.Form["vip"];

        if (isVIP is null)
            return BadRequest(@"Missing Value For Form Parameter ""vip""");

        string? connectionState = Request.Form["c_state"];

        if (connectionState is null)
            return BadRequest(@"Missing Value For Form Parameter ""c_state""");

        string? previousConnectionState = Request.Form["prev_c_state"];

        if (previousConnectionState is null)
            return BadRequest(@"Missing Value For Form Parameter ""prev_c_state""");

        // TODO: Maybe Use This To Link The Server To The Server Manager? (Or Maybe Just Do That On Server New Session)

        // TODO: Maybe Make The Servers And Managers Expire From The Cache After A Certain Amount Of Time, And Use This Call To Refresh The Expiration Time

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{session}""");

        matchServer.Status = Enum.Parse<ServerStatus>(connectionState);

        // TODO: Put All The Other Data In The Server Model

        await DistributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

        return Ok();
    }

    private async Task<IActionResult> HandleAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is not null)
            Logger.LogWarning(@"Account ""{AccountName}"" Is Attempting To Use HTTP Server Authentication", accountName);

        string response = PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.SRPAuthenticationDisabled));

        return BadRequest(response);
    }
}
