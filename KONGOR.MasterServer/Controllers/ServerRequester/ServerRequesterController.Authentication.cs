namespace KONGOR.MasterServer.Controllers.ServerRequester;

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

        // The Built-In Host Account Which Ships With A Publicly-Known Password And Is Intended For Usage By Self-Hosters Must Not Be Used To Host On The Production Server
        if (HostEnvironment.IsProduction() && account.Name.Equals(OOTB.Accounts.OPERATOR.Name))
        {
            Logger.LogWarning(@"Rejected Server Manager Authentication For Host Account ""{HostAccountName}"": The Built-In ""{OOTBHostAccountName}"" Account Cannot Host On The Production Server", account.Name, OOTB.Accounts.OPERATOR.Name);

            return Unauthorized($@"The Built-In ""{OOTB.Accounts.OPERATOR.Name}"" Account Cannot Host On The Production Server; Create A Dedicated Host Account");
        }

        int matchServerManagerID = hostAccountName.GetDeterministicInt32Hash();

        // Re-Adopt Any Of This Manager's Match Servers Which Are Still Registered, So That A Manager Re-Authenticating After Being Reaped Recovers Its Child List Instead Of Resetting It And Losing Track Of Servers Which Are Still Live
        HashSet<int> matchServerIDs = [.. (await DistributedCache.GetMatchServersByAccountName(hostAccountName))
            .Where(matchServer => matchServer.MatchServerManagerID == matchServerManagerID).Select(matchServer => matchServer.ID)];

        MatchServerManager matchServerManager = new ()
        {
            HostAccountID = account.ID,
            HostAccountName = account.Name,
            ID = matchServerManagerID,
            MatchServerIDs = matchServerIDs,
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

        // These Tell The Server Manager Where To Upload Match Replay Clips After A Game (The Authoritative API Returns The Same Pair From Its Spectate Configuration)
        // TODO: Decide The Replay Hosting Model: Either Have Each Host Serve Replay Files Directly From Its Own Hosting Machine, Or Upload To The CDN (Which Requires Researching CDN Cost And Granting Server Launchers Write Access); The Current Values Point At The CDN Upload Host As A Placeholder Until This Is Settled
        response["cdn_upload_host"] = Configuration.CDN.Host;
        response["cdn_upload_target"] = "upload";

        Logger.LogInformation(@"Server Manager ID ""{MatchServerManagerID}"" Was Registered At ""{MatchServerManagerAddress}"" With Cookie ""{MatchServerManagerCookie}""",
            matchServerManager.ID, matchServerManager.IPAddress, matchServerManager.Cookie);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleServerAuthentication()
    {
        string serverIdentifier = Request.Form["login"].ToString();

        string[] serverIdentifierComponents = serverIdentifier.Split(':');

        if (serverIdentifierComponents.Length is not 2)
            return BadRequest(@"Missing Or Incorrect Value For Form Parameter ""login""");

        string hostAccountName = serverIdentifierComponents[0];
        string serverInstance = serverIdentifierComponents[1];

        // Linux Match Server Managers Start A Copy-On-Write Master (HON's "cowmaster") Which Authenticates With An Empty Server Instance, And Then Fork Every Match Server From It
        bool isCopyOnWriteMaster = serverInstance.Length is 0;

        // Match Server Managers Number Their Match Servers From One Without Padding, So Zero And Zero-Padded Numbers Are Not Valid Server Instances, And The Server ID Is Derived From The Raw Login
        bool serverInstanceIsValid = int.TryParse(serverInstance, NumberStyles.None, CultureInfo.InvariantCulture, out int serverInstanceNumber) && serverInstanceNumber > 0 && serverInstance.StartsWith('0') is false;

        if (isCopyOnWriteMaster is false && serverInstanceIsValid is false)
            return BadRequest($@"Invalid Server Instance ""{serverInstance}"" In Form Parameter ""login"": Expected A Positive Integer Without Leading Zeros, Or An Empty Server Instance For A Copy-On-Write Master");

        string? accountPasswordHash = Request.Form["pass"];

        if (accountPasswordHash is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        string? serverPort = Request.Form["port"];

        if (serverPort is null)
            return BadRequest(@"Missing Value For Form Parameter ""port""");

        if (ushort.TryParse(serverPort, NumberStyles.None, CultureInfo.InvariantCulture, out ushort serverPortNumber) is false || serverPortNumber is 0 || serverPort.StartsWith('0'))
            return BadRequest($@"Invalid Value ""{serverPort}"" For Form Parameter ""port"": Expected A Port Number Between 1 And 65535 Without Leading Zeros");

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

        // The Built-In Host Account Which Ships With A Publicly-Known Password And Is Intended For Usage By Self-Hosters Must Not Be Used To Host On The Production Server
        if (HostEnvironment.IsProduction() && account.Name.Equals(OOTB.Accounts.OPERATOR.Name))
        {
            Logger.LogWarning(@"Rejected Server Authentication For Host Account ""{HostAccountName}"": The Built-In ""{OOTBHostAccountName}"" Account Cannot Host On The Production Server", account.Name, OOTB.Accounts.OPERATOR.Name);

            return Unauthorized($@"The Built-In ""{OOTB.Accounts.OPERATOR.Name}"" Account Cannot Host On The Production Server; Create A Dedicated Host Account");
        }

        MatchServerManager? matchServerManager = (await DistributedCache.GetMatchServerManagersByAccountName(hostAccountName)).SingleOrDefault();

        // The Copy-On-Write Master Is A Non-Joinable Match Server: It Needs A Session To Finish Starting Up, But It Never Hosts A Match, So It Is Registered Apart From The Match Servers, Where No Server List, Matchmaking, Or Match Server Look-Up Finds It; Each Match Server Forked From It Authenticates With Its Own Server Instance
        if (isCopyOnWriteMaster)
        {
            // Only A Match Server Manager Starts A Copy-On-Write Master, But The Manager Is Not Required, Because The Master Server May Lose The Manager's Registration While The Manager Keeps Running, And A Manager Whose Chat Connection Stays Up Never Authenticates Again, So Refusing The Copy-On-Write Master Would Stall It Indefinitely
            if (matchServerManager is null)
            {
                Logger.LogWarning(@"Copy-On-Write Master For Host Account ""{HostAccountName}"" Authenticated With An Empty Server Instance While No Server Manager Is Registered For That Host Account; If This Is A Standalone Match Server, It Needs An Instance Number After The Colon, For Example ""{ExampleServerLogin}""",
                    account.Name, $"{account.Name}:1");
            }

            CopyOnWriteMaster copyOnWriteMaster = new ()
            {
                HostAccountID = account.ID,
                HostAccountName = account.Name,
                ID = serverIdentifier.GetDeterministicInt32Hash(),
                Name = serverName,
                MatchServerManagerID = matchServerManager?.ID,
                IPAddress = serverIPAddress,
                Port = serverPortNumber,
                Location = serverLocation,
                Description = serverDescription
            };

            CopyOnWriteMaster? replacedCopyOnWriteMaster = (await DistributedCache.GetCopyOnWriteMasters()).SingleOrDefault(master => master.HostAccountName.Equals(copyOnWriteMaster.HostAccountName));

            await DistributedCache.SetCopyOnWriteMaster(copyOnWriteMaster);

            // A Copy-On-Write Master Renews Its Session From The Same Address And Port, Which Is Expected And Not Warned About
            // A Replacement From A Different Address Or Port Means That Two Copy-On-Write Masters Share One Host Account, And Each Then Replaces The Other's Session
            if (replacedCopyOnWriteMaster is not null && (replacedCopyOnWriteMaster.IPAddress.Equals(copyOnWriteMaster.IPAddress) is false || replacedCopyOnWriteMaster.Port != copyOnWriteMaster.Port))
            {
                Logger.LogWarning(@"Copy-On-Write Master ID ""{CopyOnWriteMasterID}"" For Host Account ""{HostAccountName}"" Replaced A Session Registered At ""{ReplacedCopyOnWriteMasterAddress}"":""{ReplacedCopyOnWriteMasterPort}"" With One At ""{CopyOnWriteMasterAddress}"":""{CopyOnWriteMasterPort}""",
                    copyOnWriteMaster.ID, copyOnWriteMaster.HostAccountName, replacedCopyOnWriteMaster.IPAddress, replacedCopyOnWriteMaster.Port, copyOnWriteMaster.IPAddress, copyOnWriteMaster.Port);
            }

            Logger.LogInformation(@"Copy-On-Write Master ID ""{CopyOnWriteMasterID}"" For Host Account ""{HostAccountName}"" Was Registered At ""{CopyOnWriteMasterAddress}"":""{CopyOnWriteMasterPort}""",
                copyOnWriteMaster.ID, copyOnWriteMaster.HostAccountName, copyOnWriteMaster.IPAddress, copyOnWriteMaster.Port);

            return Ok(ServerAuthenticationResponse(copyOnWriteMaster.Cookie, copyOnWriteMaster.ID));
        }

        MatchServer matchServer = new ()
        {
            HostAccountID = account.ID,
            HostAccountName = account.Name,
            ID = serverIdentifier.GetDeterministicInt32Hash(),
            Name = serverName,
            MatchServerManagerID = matchServerManager?.ID,
            Instance = serverInstanceNumber,
            IPAddress = serverIPAddress,
            Port = serverPortNumber,
            Location = serverLocation,
            Description = serverDescription
        };

        MatchServer? replacedMatchServer = await DistributedCache.GetMatchServerByID(matchServer.ID);

        await DistributedCache.SetMatchServer(hostAccountName, matchServer);

        // A Match Server Renews Its Session From The Same Address And Port After Every Match, Which Is Expected And Not Warned About
        // A Replacement From A Different Address Or Port Means That Two Processes Share One Login, And Each Then Replaces The Other's Session, So That They Take Turns Being Told To Authenticate Again
        if (replacedMatchServer is not null && (replacedMatchServer.IPAddress.Equals(matchServer.IPAddress) is false || replacedMatchServer.Port != matchServer.Port))
        {
            Logger.LogWarning(@"Server ID ""{MatchServerID}"" For Host Account ""{HostAccountName}"" And Instance ""{MatchServerInstance}"" Replaced A Session Registered At ""{ReplacedMatchServerAddress}"":""{ReplacedMatchServerPort}"" With One At ""{MatchServerAddress}"":""{MatchServerPort}""",
                matchServer.ID, matchServer.HostAccountName, matchServer.Instance, replacedMatchServer.IPAddress, replacedMatchServer.Port, matchServer.IPAddress, matchServer.Port);
        }

        // A Match Server Authenticates Again After Every Match And Whenever Its Session Is Renewed, So It Is Only Added To Its Manager When It Is Not Already Listed
        if (matchServerManager is not null && matchServerManager.MatchServerIDs.Add(matchServer.ID))
            await DistributedCache.SetMatchServerManager(hostAccountName, matchServerManager);

        // TODO: Implement Verifier In Description (If The Server Is A COMPEL Server, It Will Have A Verifier In The Description)
        // INFO: The Server Manager Doesn't Send Descriptions, So Use Name Instead Since We Can Override Them Anyway Via Remote Command (svr_name) On TCP Handshake

        Logger.LogInformation(@"Server ID ""{MatchServerID}"" Was Registered At ""{MatchServerAddress}"":""{MatchServerPort}"" With Cookie ""{MatchServerCookie}""",
            matchServer.ID, matchServer.IPAddress, matchServer.Port, matchServer.Cookie);

        return Ok(ServerAuthenticationResponse(matchServer.Cookie, matchServer.ID));
    }

    /// <summary>
    ///     Builds the "new_session" response, which gives a match server, or a copy-on-write master, its session cookie, its server ID, and the chat server to connect to.
    /// </summary>
    private static string ServerAuthenticationResponse(string sessionCookie, int serverID)
    {
        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
            ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerMatchServerConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_MATCH_SERVER")
            ?? throw new NullReferenceException("Chat Server Match Server Connections Port Is NULL"));

        Dictionary<string, object> response = new ()
        {
            ["session"] = sessionCookie,
            ["server_id"] = serverID,
            ["chat_address"] = chatServerHost,
            ["chat_port"] = chatServerMatchServerConnectionsPort,
            ["leaverthreshold"] = 0.05
        };

        return PhpSerialization.Serialize(response);
    }

    /// <summary>
    ///     Builds the "auth_failure" response, which tells a match server that its session is not registered, so that it discards the session and authenticates again.
    ///     A match server which has loaded a world for a match which has not started yet also ends that match and disconnects its players, as it does for the retail master server, which is kept because "start_game" refuses the unregistered session, so that match could never start anyway.
    ///     The match server reads this response whatever the HTTP status code, so it is sent with a success status code, as the protocol answer which it is, rather than as an error.
    /// </summary>
    private static string ServerAuthenticationFailureResponse()
        => PhpSerialization.Serialize(new List<string> { "auth_failure" });

    private async Task<IActionResult> HandleConnectClient()
    {
        // The Session Is Deliberately Not Checked Against The Registered Match Servers, So That A Player Reconnecting To A Match In Progress Is Still Authenticated After The Master Server Has Lost The Match Server's Session
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
            Logger.LogError(@"[BUG] No Account Could Be Found For Account Name ""{AccountName}"" With Session Cookie ""{SessionCookie}""", accountNameForSessionCookie, cookie);

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

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID).ToDictionaryAsync(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        response.Add("infos", new List<DataPoint> { DataPoint.FromAccount(account, aggregates, statisticsByType) });
        response.Add("game_cookie", "00005072-6f6a-6563-7420-4b4f4e474f52"); // TODO: Generate And Store This Cookie Per Match (Must Exist, But The Value Doesn't Really Matter)
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

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{session}""");

        // TODO: Validate And Consume The One-Time Account Key Once The Server List Registers It For Look-Up

        Dictionary<string, object> response = new ()
        {
            ["server_id"] = matchServer.ID,
            ["official"] = 1 // 0 = Unofficial; 1 = Official With Stats; 2 = Official Without Stats
        };

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

        /*
            "c_state" Values:
                0 = Sleeping (Host.IsSleeping() Is TRUE)
                1 = Idle (No World Loaded)
                2 = Lobby/Picking Phase (World Loaded, "m_bMatchStarted" Is FALSE)
                3 = In-Game (World Loaded, "m_bMatchStarted" Is TRUE)
        */
        string? connectionState = Request.Form["c_state"];

        if (connectionState is null)
            return BadRequest(@"Missing Value For Form Parameter ""c_state""");

        string? previousConnectionState = Request.Form["prev_c_state"];

        if (previousConnectionState is null)
            return BadRequest(@"Missing Value For Form Parameter ""prev_c_state""");

        // The Match Server Is Linked To Its Manager At New-Session Authentication (Via The Match Server's "MatchServerManagerID"), So There Is No Linking To Perform On The Heartbeat

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(session);

        if (matchServer is null)
        {
            CopyOnWriteMaster? copyOnWriteMaster = await DistributedCache.GetCopyOnWriteMasterBySessionCookie(session);

            // The Copy-On-Write Master Sends Heartbeats Too, But It Never Hosts A Match, So Its Heartbeat Only Refreshes Its Own Record
            // It Has No Chat Session, So Its Last Heartbeat Is What Keeps It From Being Reaped As Stale
            if (copyOnWriteMaster is not null)
            {
                copyOnWriteMaster.Status = (ServerStatus) int.Parse(connectionState);
                copyOnWriteMaster.TimestampLastHeartbeat = DateTimeOffset.UtcNow;

                await DistributedCache.SetCopyOnWriteMaster(copyOnWriteMaster);

                return Ok();
            }

            // A Session Which Is Not Registered Belongs To A Match Server Which Was Reaped Or Replaced, Or Whose Registration Was Lost, So The Match Server Is Told To Authenticate Again
            // A Match Server In A Lobby ("c_state" 2: World Loaded, Match Not Started) Also Ends That Match And Disconnects Its Players, As It Does For The Retail Master Server; This Is Kept, Because "start_game" Refuses The Unregistered Session, So That Match Could Never Start Anyway
            return Ok(ServerAuthenticationFailureResponse());
        }

        matchServer.Status = (ServerStatus) int.Parse(connectionState);

        // TODO: Put All The Other Data In The Server Model

        await DistributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);

        /*
            The "new" Parameter Is Only Sent On The First Heartbeat After "StartGame" Loads A New World
            All Subsequent Heartbeats For The Same In-Progress Match Omit "new"
            "m_bInitializeMatchHeartbeat" Is Set To TRUE Once By "StartGame", Then Reset To FALSE After The Initialisation Heartbeat Succeeds
            
            "new" Values (1-Indexed "EArrangedMatchType" + 1):
                1  = AM_PUBLIC (Public Match)
                2  = AM_MATCHMAKING (Ranked Normal/Casual)
                3  = AM_SCHEDULED_MATCH (Tournament)
                4  = AM_UNSCHEDULED_MATCH (League)
                5  = AM_MATCHMAKING_MIDWARS (MidWars)
                6  = AM_MATCHMAKING_BOTMATCH (Bot Co-Op)
                7  = AM_UNRANKED_MATCHMAKING (Unranked Normal/Casual)
                8  = AM_MATCHMAKING_RIFTWARS (RiftWars)
                9  = AM_PUBLIC_PRELOBBY (Public Pre-Lobby)
                10 = AM_MATCHMAKING_CUSTOM (Custom Maps)
                11 = AM_MATCHMAKING_CAMPAIGN (Ranked Season Normal/Casual)
        */
        string? newMatch = Request.Form["new"];

        if (newMatch is null)
        {
            // Regular Heartbeat — The Server May Be Idle (Sleeping/No World) Or In An Active Match That Was Already Initialised
            MatchInformation? matchInformation = await DistributedCache.GetMatchInformationByMatchServerSessionCookie(session);

            if (matchInformation is not null)
            {
                matchInformation.ConnectedPlayersCount = int.Parse(connectionsCount);
                matchInformation.Map = map;

                await DistributedCache.SetMatchInformation(matchInformation);
            }
        }

        if (newMatch is not null)
        {
            string? matchID = Request.Form["match_id"];

            if (matchID is null)
                return BadRequest(@"Invalid Value For Form Parameter ""match_id""");

            MatchInformation? matchInformation = await DistributedCache.GetMatchInformation(int.Parse(matchID));

            if (matchInformation is null)
            {
                Logger.LogError(@"[BUG] Received Match Initialisation Heartbeat For Match ID ""{MatchID}"", But No MatchInformation Found In Cache", matchID);

                return Ok();
            }

            matchInformation.ConnectedPlayersCount = int.Parse(connectionsCount);
            matchInformation.Map = map;

            string? maximumPlayersCount = Request.Form["max_players"];

            if (maximumPlayersCount is null)
                return BadRequest(@"Invalid Value For Form Parameter ""max_players""");

            matchInformation.MaximumPlayersCount = int.Parse(maximumPlayersCount);

            string? league = Request.Form["league"];

            if (league is null)
                return BadRequest(@"Invalid Value For Form Parameter ""league""");

            matchInformation.League = int.Parse(league);

            string? matchMode = Request.Form["mode"];

            if (matchMode is null)
                return BadRequest(@"Invalid Value For Form Parameter ""mode""");

            matchInformation.MatchMode = PublicMatchModeExtensions.GetPublicMatchModeFromCode(matchMode)
                ?? throw new InvalidDataException($@"Invalid Match Mode Code ""{matchMode}""");

            string? matchName = Request.Form["mname"];

            if (matchName is null)
                return BadRequest(@"Invalid Value For Form Parameter ""mname""");

            matchInformation.MatchName = matchName;

            MatchOptions options = MatchOptions.None;

            if (Request.Form.ContainsKey("option[ap]"))                 options |= MatchOptions.AllPick;
            if (Request.Form.ContainsKey("option[ar]"))                 options |= MatchOptions.AllRandom;
            if (Request.Form.ContainsKey("option[alt_pick]"))           options |= MatchOptions.AlternateHeroPicking;
            if (Request.Form.ContainsKey("option[ab]"))                 options |= MatchOptions.AutoBalanced;
            if (Request.Form.ContainsKey("option[br]"))                 options |= MatchOptions.BalancedRandom;
            if (Request.Form.ContainsKey("option[veto]"))               options |= MatchOptions.BanPhase;
            if (Request.Form.ContainsKey("option[rapidfire]"))          options |= MatchOptions.BlitzMode;
            if (Request.Form.ContainsKey("option[cas]"))                options |= MatchOptions.CasualMode;
            if (Request.Form.ContainsKey("option[dev_heroes]"))         options |= MatchOptions.DevelopmentHeroes;
            if (Request.Form.ContainsKey("option[drp_itm]"))            options |= MatchOptions.DropItems;
            if (Request.Form.ContainsKey("option[dup_h]"))              options |= MatchOptions.DuplicateHeroes;
            if (Request.Form.ContainsKey("option[em]"))                 options |= MatchOptions.EasyMode;
            if (Request.Form.ContainsKey("option[gated]"))              options |= MatchOptions.Gated;
            if (Request.Form.ContainsKey("option[hardcore]"))           options |= MatchOptions.Hardcore;
            if (Request.Form.ContainsKey("option[no_agi]"))             options |= MatchOptions.NoAgilityHeroes;
            if (Request.Form.ContainsKey("option[no_repick]"))          options |= MatchOptions.NoHeroRepick;
            if (Request.Form.ContainsKey("option[no_swap]"))            options |= MatchOptions.NoHeroSwap;
            if (Request.Form.ContainsKey("option[no_int]"))             options |= MatchOptions.NoIntelligenceHeroes;
            if (Request.Form.ContainsKey("option[nl]"))                 options |= MatchOptions.NoLeavers;
            if (Request.Form.ContainsKey("option[no_pups]"))            options |= MatchOptions.NoPowerUps;
            if (Request.Form.ContainsKey("option[no_timer]"))           options |= MatchOptions.NoRespawnTimer;
            if (Request.Form.ContainsKey("option[no_stats]"))           options |= MatchOptions.NoStatistics;
            if (Request.Form.ContainsKey("option[no_str]"))             options |= MatchOptions.NoStrengthHeroes;
            if (Request.Form.ContainsKey("option[officl]"))             options |= MatchOptions.Official;
            if (Request.Form.ContainsKey("option[rev_hs]"))             options |= MatchOptions.ReverseHeroSelection;
            if (Request.Form.ContainsKey("option[rs]"))                 options |= MatchOptions.ReverseSelection;
            if (Request.Form.ContainsKey("option[shuffleabilities]"))   options |= MatchOptions.ShuffleAbilities;
            if (Request.Form.ContainsKey("option[shuf]"))               options |= MatchOptions.ShuffleTeams;
            if (Request.Form.ContainsKey("option[tr]"))                 options |= MatchOptions.TournamentRules;
            if (Request.Form.ContainsKey("option[verified_only]"))      options |= MatchOptions.VerifiedOnly;

            matchInformation.Options = options;

            await DistributedCache.SetMatchInformation(matchInformation);

            Logger.LogInformation("Captured Match Mode And Options For Match ID {MatchID}: MatchMode={MatchMode}, ArrangedMatchType={ArrangedMatchType}",
                matchID, matchInformation.MatchMode, matchInformation.MatchType);
        }

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
