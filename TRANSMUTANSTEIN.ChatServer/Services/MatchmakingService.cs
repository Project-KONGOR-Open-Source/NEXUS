namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Background service that manages matchmaking and the match broker.
///     The broker cycle, team formation, pool-tier resolution, and pairing logic live in <see cref="MatchmakingAlgorithm"/> as pure functions; this service is responsible for orchestration, server allocation, and player notifications.
/// </summary>
public class MatchmakingService : BackgroundService, IDisposable
{
    private readonly IOptions<MatchmakingSettings> _settings;
    private readonly IDatabase _distributedCacheStore;
    private readonly ILogger<MatchmakingService> _logger;

    public MatchmakingService(IOptions<MatchmakingSettings> settings, IDatabase distributedCacheStore, ILogger<MatchmakingService> logger)
    {
        _settings = settings;
        _distributedCacheStore = distributedCacheStore;
        _logger = logger;
    }

    /// <summary>
    ///     Registry of all active matchmaking groups, keyed by the leader's account ID.
    /// </summary>
    public static ConcurrentDictionary<int, MatchmakingGroup> Groups { get; set; } = [];

    /// <summary>
    ///     Registry of all active matches, keyed by the match GUID.
    /// </summary>
    public static ConcurrentDictionary<Guid, MatchmakingMatch> ActiveMatches { get; set; } = [];

    public static MatchmakingGroup? GetMatchmakingGroup(OneOf<int, string> memberIdentifier)
        => memberIdentifier.Match(id => GetMatchmakingGroupByMemberID(id), name => GetMatchmakingGroupByMemberName(name));

    public static MatchmakingGroup? GetMatchmakingGroupByMemberID(int memberID)
        => Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.ID == memberID));

    public static MatchmakingGroup? GetMatchmakingGroupByMemberName(string memberName)
        => Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.Name.Equals(memberName)));

    public static ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 1));

    public static ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 2));

    public static ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 3));

    public static ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 4));

    public static ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups
        => new (Groups.Where(group => group.Value.Members.Count == 5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Matchmaking Service Has Started");

        await RunMatchBroker(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Matchmaking Service Is Stopping");

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("Matchmaking Service Has Stopped");
    }

    public override void Dispose()
    {
        Groups.Clear();
        ActiveMatches.Clear();

        base.Dispose();

        GC.SuppressFinalize(this);
    }

    private async Task RunMatchBroker(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                await Task.Delay(_settings.Value.MatchmakingCycleInterval, cancellationToken);
            }

            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ProcessBrokerCycle();
            }

            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // A Single Broker Cycle Must Never Tear Down The Host (As Per The Default BackgroundServiceExceptionBehavior == StopHost)
            catch (Exception exception)
            {
                // Log And Continue With The Next Cycle
                _logger.LogError(exception, "Matchmaking Broker Cycle Failed; Continuing With The Next Cycle");
            }
        }
    }

    /// <summary>
    ///     Runs a single match broker cycle.
    ///     Each match is spawned in isolation via <see cref="TrySpawnMatch"/> so that the failure of one match cannot abort the others or the cycle.
    /// </summary>
    private async Task ProcessBrokerCycle()
    {
        if (_settings.Value.Enabled is false)
            return;

        // Get All Queued Groups (Groups With A Non-NULL QueueStartTime And Not Already Matched)
        List<MatchmakingGroup> queuedGroups = [.. Groups.Values
            .Where(group => group.QueueStartTime is not null && group.MatchedUp is false)
            .OrderBy(group => group.QueueStartTime)];

        if (queuedGroups.Count == 0)
            return;

        // Spawn Bot Matches Immediately (Bot Groups Bypass The Regular Match Broker Cycles)
        List<MatchmakingGroup> botGroups = [.. queuedGroups.Where(group => group.Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_COOP)];

        foreach (MatchmakingGroup botGroup in botGroups)
            await TrySpawnMatch(MatchmakingMatch.FromBotGroup(botGroup));

        // Run The Regular Broker Cycle For Non-Bot Groups
        List<MatchmakingGroup> regularGroups = [.. queuedGroups
            .Where(group => group.Information.GroupType != ChatProtocol.TMMType.TMM_TYPE_COOP && group.MatchedUp is false)];

        if (regularGroups.Count == 0)
            return;

        int queuedPlayerCount = regularGroups.Sum(group => group.Members.Count);

        PoolSizeParameters poolSizeParameters = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount, _settings.Value);

        _logger.LogDebug(@"Broker Cycle: {PlayerCount} Queued Players, Pool Tier = {PoolTier}", queuedPlayerCount, poolSizeParameters.Tier);

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(regularGroups, _settings.Value, poolSizeParameters);

        // Spawn Each Match In Isolation
        int spawnedMatchCount = 0;

        foreach (MatchmakingMatch match in matches)
        {
            if (await TrySpawnMatch(match))
                spawnedMatchCount++;
        }

        if (matches.Count > 0)
            _logger.LogInformation(@"Broker Cycle Complete: {SpawnedMatchCount} Of {MatchCount} Match(es) Spawned", spawnedMatchCount, matches.Count);

        // Send Periodic Queue Time Updates (Every 10 Seconds = Every 2 Cycles At 5-Second Intervals)
        BroadcastQueueTimeUpdates(queuedGroups.Where(group => group.MatchedUp is false).ToList());
    }

    /// <summary>
    ///     Spawns a single match, isolating any failure so that it cannot abort sibling matches or the broker cycle.
    ///     When no server is available, the players are notified with a "No Servers Found" message and their groups are returned to the queue.
    ///     When the spawn fails unexpectedly, the failure is logged and the match's groups are returned to the queue so the broker re-matches them on a later cycle, without misreporting the failure to the players as a server-availability problem.
    /// </summary>
    private async Task<bool> TrySpawnMatch(MatchmakingMatch match)
    {
        try
        {
            bool spawned = await SpawnMatch(match);

            if (spawned is false)
            {
                SendNoServersFound(match);

                ReturnMatchGroupsToQueue(match);
            }

            return spawned;
        }

        catch (Exception exception)
        {
            _logger.LogError(exception, @"Failed To Spawn Match GUID {MatchGUID} With {PlayerCount} Player(s); Returning Its Groups To Queue", match.GUID, match.GetAllPlayers().Count());

            ActiveMatches.TryRemove(match.GUID, out _);

            ReturnMatchGroupsToQueue(match);

            return false;
        }
    }

    /// <summary>
    ///     Returns every group in a match to the queue by clearing its matched-up state, so that the broker re-matches it on a subsequent cycle.
    /// </summary>
    private static void ReturnMatchGroupsToQueue(MatchmakingMatch match)
    {
        foreach (MatchmakingGroup group in match.GetAllGroups())
        {
            group.MatchedUp = false;
            group.AssignedMatchGUID = null;
            group.AssignedTeamGUID = null;
        }
    }

    /// <summary>
    ///     Spawns a match by allocating a server and sending CreateMatch to the game server.
    ///     Player notifications are sent immediately. We don't wait for AnnounceMatch because some game server configurations use the HTTP path instead.
    /// </summary>
    private async Task<bool> SpawnMatch(MatchmakingMatch match)
    {
        // Find An Available Server And Its Chat Session
        (MatchServer? server, MatchServerChatSession? serverSession) = await FindAvailableServerWithSession(match);

        if (server is null || serverSession is null)
        {
            _logger.LogWarning(@"No Available Server Found For Match GUID {MatchGUID}", match.GUID);

            return false;
        }

        // Assign Server To Match
        match.AssignedServerID = server.ID;
        match.ServerAddress = server.IPAddress;
        match.ServerPort = (ushort)server.Port;
        match.State = MatchmakingMatchState.ServerAllocating;

        // Store Match In Active Matches Registry
        ActiveMatches.TryAdd(match.GUID, match);

        // Send CreateMatch Command To The Game Server
        SendCreateMatch(match, serverSession);

        _logger.LogInformation(@"CreateMatch Sent: MatchGUID={MatchGUID}, ServerID={MatchServerID}, Server={MatchServerAddress}:{MatchServerPort}",
            match.GUID, server.ID, match.ServerAddress, match.ServerPort);

        // Send Leave Queue Notification To Dismiss The Client's Queue Timer
        ChatBuffer leaveQueue = new ();

        leaveQueue.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(leaveQueue);

        // Send Player Notifications
        SendMatchFoundUpdate(match, match.CorrelationID);
        SendFoundServerUpdate(match);

        // Reset Each Matched Group's Readiness And Loading State And Mark Its Members As In-Game
        // A Party Group Stays Alive On The Client After A Match Is Found (Only Solo Queues Self-Disband), So The Chat Server Must Reset The Leader To Not-Ready And Broadcast The Update
        // Without This, The Client's Matchmaking Loading Overlay Remains Visible And Covers The Match Lobby Interface
        // The Match Roster Has Already Been Committed To The Match Server (As Per CreateMatch), So This Loop Is Purely Post-Commit Book-Keeping; A Player Who Drops Out Now Is Handled Downstream By The Match Server's Wait-For-Players Logic And The Resulting MatchAbandoned Signal
        // Each Group's Notification Is Wrapped Individually So That An Exception While Notifying One Group Does Not Skip The Notifications For The Remaining Groups
        foreach (MatchmakingGroup group in match.GetAllGroups())
        {
            try
            {
                group.QueueStartTime = null;

                group.UnloadAndUnreadyMembers();

                foreach (MatchmakingGroupMember member in group.Members)
                    member.IsInGame = true;

                // If Every Member Disconnected During Server Allocation The Group Will Have Been Disbanded, Leaving No One To Notify; Skip The Update In This Case
                // A Group That Still Has Members Always Has Exactly One Leader (Leadership Is Reassigned Whenever A Member Leaves)
                if (group.Members.Count is not 0)
                    group.MulticastUpdate(group.Leader.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);

                else
                    _logger.LogWarning(@"Skipped Post-Matchup Update For Disbanded Group GUID {GroupGUID}", group.GUID);
            }

            catch (Exception exception)
            {
                _logger.LogError(exception, @"Failed Post-Matchup Handling For Group GUID {GroupGUID} In Match GUID {MatchGUID}", group.GUID, match.GUID);
            }
        }

        match.State = MatchmakingMatchState.WaitingForPlayers;

        _logger.LogInformation(@"Match Notifications Sent: GUID={MatchGUID}, Server={MatchServerAddress}:{MatchServerPort}",
            match.GUID, server.IPAddress, server.Port);

        return true;
    }

    /// <summary>
    ///     Sends NET_CHAT_GS_CREATE_MATCH (0x1502) to the game server to set up the match.
    /// </summary>
    private static void SendCreateMatch(MatchmakingMatch match, MatchServerChatSession serverSession)
    {
        // Determine Match Type Using The Centralised Mapping On MatchmakingMatch
        byte matchType = (byte)match.ArrangedMatchType;

        // Build Match Settings String In Expected Format: mode:<string> map:<string> teamsize:<int> allheroes:true noleaver:<bool> spectators:<int>
        string matchSettings;

        if (match.IsBotMatch)
        {
            int humanPlayerCount = match.LegionTeam.PlayerCount;
            int botPlayerCount = match.TeamSize - humanPlayerCount;

            matchSettings = $"mode:botmatch casual:true map:{match.SelectedMap} teamsize:{match.TeamSize} allheroes:true noleaver:false spectators:{humanPlayerCount} randombots:{botPlayerCount}|{match.TeamSize}";
        }

        else
        {
            matchSettings = $"mode:{match.SelectedMode} map:{match.SelectedMap} teamsize:{match.TeamSize} allheroes:true noleaver:true spectators:{match.TeamSize * 2}";
        }

        ChatBuffer createMatch = new ();

        createMatch.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_CREATE_MATCH);
        createMatch.WriteInt8(matchType);             // Match Type
        createMatch.WriteInt32(match.CorrelationID);  // Correlation ID (Returned In AnnounceMatch For Validation)
        createMatch.WriteInt32(0);                    // Unknown1
        createMatch.WriteInt32(Random.Shared.Next()); // Password
        createMatch.WriteString($"TMM Match #");      // Match Name Prefix
        createMatch.WriteString(matchSettings);       // Match Settings
        createMatch.WriteInt8(0);                     // Use New MMR System (FALSE)
        createMatch.WriteInt8(0);                     // Unknown3

        // Write Player Count
        int totalPlayers = match.LegionTeam.PlayerCount + (match.HellbourneTeam?.PlayerCount ?? 0);
        createMatch.WriteInt8(Convert.ToByte(totalPlayers));

        // Build A Lookup From Each Member To Their Group Index (Continuous Across Both Teams)
        Dictionary<MatchmakingGroupMember, byte> memberGroupIndices = [];
        byte groupIndex = 0;

        foreach (MatchmakingGroup group in match.LegionTeam.Groups)
        {
            foreach (MatchmakingGroupMember member in group.Members)
                memberGroupIndices[member] = groupIndex;

            groupIndex++;
        }

        if (match.HellbourneTeam is not null)
        {
            foreach (MatchmakingGroup group in match.HellbourneTeam.Groups)
            {
                foreach (MatchmakingGroupMember member in group.Members)
                    memberGroupIndices[member] = groupIndex;

                groupIndex++;
            }
        }

        // Write Players Sorted By TMR Ascending So That The Lowest-Rated Player Gets Slot 0 (Picks First)
        // And The Highest-Rated Player Gets The Last Slot (Picks Last)

        // Write Legion Players (Team 1)
        byte legionSlot = 0;

        foreach (MatchmakingGroupMember member in match.LegionTeam.GetAllMembers().OrderBy(member => member.TMR))
        {
            createMatch.WriteInt32(member.Account.ID);              // Account ID
            createMatch.WriteInt8(1);                               // Team (1 = Legion)
            createMatch.WriteInt8(legionSlot++);                    // Slot (Continuous Within Team)
            createMatch.WriteInt8(0);                               // Social Bonus (0 = None)
            createMatch.WriteFloat32((float)member.MatchWinValue);  // Win MMR Delta
            createMatch.WriteFloat32((float)member.MatchLossValue); // Loss MMR Delta
            createMatch.WriteInt8(0);                               // Is Provisional (FALSE)
            createMatch.WriteInt8(memberGroupIndices[member]);      // Group Index (Continuous Across Teams)
            createMatch.WriteInt8(0);                               // Benefit Value (0 = Normal)
        }

        // Write Hellbourne Players (Team 2) — Skipped For Bot Matches
        if (match.HellbourneTeam is not null)
        {
            byte hellbourneSlot = 0;

            foreach (MatchmakingGroupMember member in match.HellbourneTeam.GetAllMembers().OrderBy(member => member.TMR))
            {
                createMatch.WriteInt32(member.Account.ID);              // Account ID
                createMatch.WriteInt8(2);                               // Team (2 = Hellbourne)
                createMatch.WriteInt8(hellbourneSlot++);                // Slot (Continuous Within Team)
                createMatch.WriteInt8(0);                               // Social Bonus (0 = None)
                createMatch.WriteFloat32((float)member.MatchWinValue);  // Win MMR Delta
                createMatch.WriteFloat32((float)member.MatchLossValue); // Loss MMR Delta
                createMatch.WriteInt8(0);                               // Is Provisional (FALSE)
                createMatch.WriteInt8(memberGroupIndices[member]);      // Group Index (Continuous Across Teams)
                createMatch.WriteInt8(0);                               // Benefit Value (0 = Normal)
            }
        }

        // Write Group IDs (Count And List Of Group IDs)
        // The C++ Code Sends These For The Chat Server To Track Which Groups To Notify
        // We Use The Deterministic Hash Of The GUID As The Group ID
        List<int> groupIDs = match.GetAllGroups().Select(group => group.GUID.GetDeterministicInt32Hash()).ToList();

        createMatch.WriteInt32(groupIDs.Count);

        foreach (int groupID in groupIDs)
            createMatch.WriteInt32(groupID);

        Log.Debug(@"CreateMatch Packet: MatchType={MatchType}, CorrelationID={CorrelationID}, PlayerCount={PlayerCount}, GroupCount={GroupCount}, PacketSize={PacketSize}",
            matchType, match.CorrelationID, totalPlayers, groupIDs.Count, createMatch.Size);

        // Send To Game Server
        serverSession.Send(createMatch);
    }

    /// <summary>
    ///     Creates a MatchInformation object for caching, enabling player join tracking.
    ///     Called from "MatchAnnounce" when the game server provides the real match ID.
    /// </summary>
    public static MatchInformation CreateMatchInformation(MatchmakingMatch match, MatchServer server, int matchID)
    {
        // Determine Match Type Using The Centralised Mapping On MatchmakingMatch
        MatchType matchType = match.ArrangedMatchType;

        // Determine Match Mode From Mode Code
        PublicMatchMode matchMode = match.SelectedMode.ToLowerInvariant() switch
        {
            "ap" => PublicMatchMode.GAME_MODE_NORMAL,
            "nm" => PublicMatchMode.GAME_MODE_NORMAL,
            "sd" => PublicMatchMode.GAME_MODE_SINGLE_DRAFT,
            "rd" => PublicMatchMode.GAME_MODE_RANDOM_DRAFT,
            "bd" => PublicMatchMode.GAME_MODE_BANNING_DRAFT,
            "ar" => PublicMatchMode.GAME_MODE_ALL_RANDOM,
            "lp" => PublicMatchMode.GAME_MODE_LOCKPICK,
            "bb" => PublicMatchMode.GAME_MODE_BLIND_BAN,
            "hb" => PublicMatchMode.GAME_MODE_HEROBAN,
            "rb" => PublicMatchMode.GAME_MODE_REBORN,
            _    => PublicMatchMode.GAME_MODE_NORMAL
        };

        // Determine If Casual Mode
        bool isCasual = match.GameType
            is ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL
            or ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL
            or ChatProtocol.TMMGameType.TMM_GAME_TYPE_REBORN_CASUAL;

        return new MatchInformation
        {
            MatchID = matchID,
            MatchName = $"TMM Match #{matchID}",
            ServerID = server.ID,
            ServerName = server.Name,
            HostAccountName = server.HostAccountName,
            Map = match.SelectedMap,
            Version = "4.10.1.0", // TODO: Get Actual Client Version From Group Information
            IsCasual = isCasual,
            MatchType = matchType,
            MatchMode = matchMode,
            MaximumPlayersCount = match.TeamSize * 2
        };
    }

    /// <summary>
    ///     The maximum age of a match server's last status update before its chat session is treated as stale and the server is skipped for selection.
    ///     A healthy match server sends a status heartbeat at least once per minute, so this threshold tolerates a couple of missed heartbeats before excluding a server whose connection has gone quiet without being detected at the transport level (for example, a hung server process whose socket is still open).
    /// </summary>
    private static readonly TimeSpan MatchServerStatusFreshnessThreshold = TimeSpan.FromSeconds(150);

    /// <summary>
    ///     Finds an available server for a match along with its chat session.
    ///     Returns <see langword="null"/> if no idle server with an active and recently-active session is found.
    /// </summary>
    private async Task<(MatchServer? Server, MatchServerChatSession? Session)> FindAvailableServerWithSession(MatchmakingMatch match)
    {
        List<MatchServer> servers = await _distributedCacheStore.GetMatchServers();

        // Find An Idle Server That Also Has An Active Chat Session
        foreach (MatchServer server in servers.Where(server => server.Status == ServerStatus.SERVER_STATUS_IDLE))
        {
            if (Context.MatchServerChatSessions.TryGetValue(server.ID, out MatchServerChatSession? session))
            {
                // Skip A Server Whose Chat Session Has Gone Quiet (No Recent Status Heartbeat), Which Indicates A Stale Connection Not Yet Detected At The Transport Level
                if (DateTimeOffset.UtcNow - session.Metadata.LastStatusUpdate > MatchServerStatusFreshnessThreshold)
                {
                    _logger.LogWarning(@"Skipping Idle Server With A Stale Chat Session: ServerID={MatchServerID}, LastStatusUpdate={LastStatusUpdate}", server.ID, session.Metadata.LastStatusUpdate);

                    continue;
                }

                _logger.LogDebug(@"Found Idle Server With Session: ServerID={MatchServerID}, ServerName={MatchServerName}", server.ID, server.Name);

                return (server, session);
            }

            _logger.LogDebug(@"Idle Server Has No Active Chat Session: ServerID={MatchServerID}", server.ID);
        }

        // No Idle Server With Active Session Found
        if (servers.Count == 0)
            _logger.LogWarning(@"No Servers Available For Match GUID {MatchGUID}", match.GUID);

        else
            _logger.LogWarning(@"No Idle Servers With Active Sessions For Match GUID {MatchGUID} (Total Servers: {MatchServerCount})", match.GUID, servers.Count);

        return (null, null);
    }

    /// <summary>
    ///     Sends TMM_GROUP_NO_SERVERS_FOUND to all players in a match.
    /// </summary>
    private static void SendNoServersFound(MatchmakingMatch match)
    {
        ChatBuffer noServers = new ();

        noServers.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        noServers.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_NO_SERVERS_FOUND));

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(noServers);
    }

    /// <summary>
    ///     Broadcasts queue time updates to all queued groups.
    /// </summary>
    private static void BroadcastQueueTimeUpdates(List<MatchmakingGroup> queuedGroups)
    {
        if (queuedGroups.Count == 0)
            return;

        // Calculate Average Queue Time
        int averageQueueTimeSeconds = (int)queuedGroups.Average(group => group.QueueDuration.TotalSeconds);

        ChatBuffer update = new ();

        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
        update.WriteInt32(averageQueueTimeSeconds);

        foreach (MatchmakingGroup group in queuedGroups)
            foreach (MatchmakingGroupMember member in group.Members)
                member.Session.Send(update);
    }

    /// <summary>
    ///     Sends MatchFoundUpdate (0x0D09) to all players in a match.
    /// </summary>
    private static void SendMatchFoundUpdate(MatchmakingMatch match, int matchID)
    {
        ChatBuffer matchFound = new ();

        matchFound.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
        matchFound.WriteString(match.SelectedMap);
        matchFound.WriteInt8(Convert.ToByte(match.TeamSize));
        matchFound.WriteInt8(Convert.ToByte(match.GameType));
        matchFound.WriteString(match.SelectedMode);
        matchFound.WriteString(match.SelectedRegion);
        matchFound.WriteString($"Match #{matchID}");

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(matchFound);
    }

    /// <summary>
    ///     Sends GroupQueueUpdate type=16 (TMM_GROUP_FOUND_SERVER) to all players.
    /// </summary>
    private static void SendFoundServerUpdate(MatchmakingMatch match)
    {
        ChatBuffer found = new ();

        found.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        found.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_FOUND_SERVER));

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(found);
    }
}
