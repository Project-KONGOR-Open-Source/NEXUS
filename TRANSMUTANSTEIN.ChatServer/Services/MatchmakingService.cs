namespace TRANSMUTANSTEIN.ChatServer.Services;

using Microsoft.Extensions.Options;

using Configuration;

/// <summary>
///     Background service that manages matchmaking and the match broker.
/// </summary>
public class MatchmakingService : BackgroundService, IDisposable
{
    private readonly IOptions<MatchmakingSettings> _settings;
    private readonly IDatabase _distributedCacheStore;
    private int _matchIndex;

    public MatchmakingService(IOptions<MatchmakingSettings> settings, IDatabase distributedCacheStore)
    {
        _settings = settings;
        _distributedCacheStore = distributedCacheStore;
        _matchIndex = 0;
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
        => new(Groups.Where(group => group.Value.Members.Count == 1));

    public static ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 2));

    public static ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 3));

    public static ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 4));

    public static ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 5));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Matchmaking Service Is Starting");

        await RunMatchBroker(stoppingToken);

        Log.Information("Matchmaking Service Has Started");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Matchmaking Service Is Stopping");

        await base.StopAsync(cancellationToken);

        Log.Information("Matchmaking Service Has Stopped");
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
            await Task.Delay(_settings.Value.MatchmakingCycleInterval, cancellationToken);

            if (_settings.Value.Enabled is false)
                continue;

            // Get All Queued Groups (Groups With A Non-NULL QueueStartTime And Not Already Matched)
            List<MatchmakingGroup> queuedGroups = [.. Groups.Values
                .Where(group => group.QueueStartTime is not null && group.MatchedUp is false)
                .OrderBy(group => group.QueueStartTime)];

            if (queuedGroups.Count == 0)
                continue;

            // Run The Broker Cycle
            List<MatchmakingMatch> matches = RunBrokerCycle(queuedGroups);

            // Spawn Each Match
            foreach (MatchmakingMatch match in matches)
            {
                bool spawned = await SpawnMatch(match);

                if (spawned is false)
                {
                    SendNoServersFound(match);

                    // Return Groups To Queue
                    foreach (MatchmakingGroup group in match.GetAllGroups())
                    {
                        group.MatchedUp = false;
                        group.AssignedMatchGUID = null;
                        group.AssignedTeamGUID = null;
                    }
                }
            }

            // Send Periodic Queue Time Updates (Every 10 Seconds = Every 2 Cycles At 5-Second Intervals)
            BroadcastQueueTimeUpdates(queuedGroups.Where(group => group.MatchedUp is false).ToList());
        }
    }

    /// <summary>
    ///     Runs a single match broker cycle with simple first-in-first-out matching.
    ///     This is a simplified implementation that just pairs groups in queue order.
    /// </summary>
    private List<MatchmakingMatch> RunBrokerCycle(List<MatchmakingGroup> queuedGroups)
    {
        List<MatchmakingMatch> matches = [];
        int playersPerTeam = _settings.Value.PlayersPerTeam;

        // Group By Game Type For Compatibility
        Dictionary<ChatProtocol.TMMGameType, List<MatchmakingGroup>> groupsByGameType = queuedGroups
            .GroupBy(group => group.Information.GameType)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        foreach ((ChatProtocol.TMMGameType gameType, List<MatchmakingGroup> typeGroups) in groupsByGameType)
        {
            // Form Teams From Groups (Simple FIFO Approach)
            List<MatchmakingTeam> teams = FormTeams(typeGroups, playersPerTeam);

            // Pair Teams Into Matches
            for (int teamIndex = 0; teamIndex + 1 < teams.Count; teamIndex += 2)
            {
                MatchmakingTeam legionTeam = teams[teamIndex];
                MatchmakingTeam hellbourneTeam = teams[teamIndex + 1];

                if (legionTeam.IsCompatibleWith(hellbourneTeam))
                {
                    MatchmakingMatch match = MatchmakingMatch.FromTeams(legionTeam, hellbourneTeam, ++_matchIndex);

                    // Set Match Details From First Group's Information
                    MatchmakingGroupInformation information = legionTeam.Groups[0].Information;
                    match.GameType = gameType;
                    match.SelectedMap = information.MapName;
                    match.SelectedMode = information.GameModes.Length > 0 ? information.GameModes[0] : "ap";
                    match.SelectedRegion = information.GameRegions.Length > 0 ? information.GameRegions[0] : "NEWERTH";
                    match.IsRanked = information.Ranked;
                    match.CombineMethod = MatchmakingCombineMethod.FirstInFirstOut;

                    matches.Add(match);

                    Log.Information(
                        @"Match Created: Index={MatchIndex}, GameType={GameType}, Legion={LegionCount} Players (Avg TMR: {LegionTMR:F1}), Hellbourne={HellbourneCount} Players (Avg TMR: {HellbourneTMR:F1}), Prediction={Prediction:P1}",
                        match.MatchIndex,
                        gameType,
                        match.LegionTeam.PlayerCount,
                        match.LegionTeam.AverageTMR,
                        match.HellbourneTeam.PlayerCount,
                        match.HellbourneTeam.AverageTMR,
                        match.MatchupPrediction);
                }
            }
        }

        return matches;
    }

    /// <summary>
    ///     Forms teams from groups using simple first-in-first-out approach.
    ///     Groups are combined until the team reaches the target player count.
    /// </summary>
    private static List<MatchmakingTeam> FormTeams(List<MatchmakingGroup> groups, int playersPerTeam)
    {
        List<MatchmakingTeam> teams = [];
        List<MatchmakingGroup> availableGroups = [.. groups.Where(group => group.MatchedUp is false)];

        while (availableGroups.Count > 0)
        {
            List<MatchmakingGroup> teamGroups = [];
            int currentPlayerCount = 0;

            // Add Groups Until We Reach The Target Player Count
            foreach (MatchmakingGroup group in availableGroups.ToList())
            {
                if (currentPlayerCount + group.Members.Count <= playersPerTeam)
                {
                    teamGroups.Add(group);
                    currentPlayerCount += group.Members.Count;
                    availableGroups.Remove(group);

                    if (currentPlayerCount == playersPerTeam)
                        break;
                }
            }

            // Only Create A Team If We Have The Full Player Count
            if (currentPlayerCount == playersPerTeam)
            {
                MatchmakingTeam team = MatchmakingTeam.FromGroups(teamGroups, playersPerTeam);
                teams.Add(team);
            }
            else
            {
                // Not Enough Players To Form A Full Team, Return Groups To Pool
                break;
            }
        }

        return teams;
    }

    /// <summary>
    ///     Spawns a match by allocating a server and notifying players.
    /// </summary>
    private async Task<bool> SpawnMatch(MatchmakingMatch match)
    {
        // Find An Available Server
        MatchServer? server = await FindAvailableServer(match);

        if (server is null)
        {
            Log.Warning(@"No Available Server Found For Match Index {MatchIndex}", match.MatchIndex);

            return false;
        }

        // Assign Server To Match
        match.AssignedServerID = server.ID;
        match.ServerAddress = server.IPAddress;
        match.ServerPort = (ushort)server.Port;
        match.State = MatchmakingMatchState.ServerAllocated;

        // Store Match In Active Matches Registry
        ActiveMatches.TryAdd(match.GUID, match);

        // Clear Queue Start Time For All Groups
        foreach (MatchmakingGroup group in match.GetAllGroups())
            group.QueueStartTime = null;

        // Notify All Players
        SendMatchFoundUpdate(match);
        SendFoundServerUpdate(match);
        SendAutoMatchConnect(match, server);

        Log.Information(
            @"Match Spawned: Index={MatchIndex}, Server={ServerAddress}:{ServerPort}",
            match.MatchIndex,
            match.ServerAddress,
            match.ServerPort);

        return true;
    }

    /// <summary>
    ///     Finds an available server for a match.
    ///     For now, returns the first idle server or uses a placeholder for testing.
    /// </summary>
    private async Task<MatchServer?> FindAvailableServer(MatchmakingMatch match)
    {
        List<MatchServer> servers = await _distributedCacheStore.GetMatchServers();

        // Find An Idle Server
        MatchServer? idleServer = servers.FirstOrDefault(server => server.Status == ServerStatus.SERVER_STATUS_IDLE);

        if (idleServer is not null)
            return idleServer;

        // For Testing: If No Real Servers Are Available, Create A Placeholder
        // This Allows Testing The Matchmaking Flow Without A Real Game Server
        if (servers.Count == 0)
        {
            Log.Warning(@"No Servers Available For Match Index {MatchIndex}, Using Placeholder For Testing", match.MatchIndex);

            return new MatchServer
            {
                HostAccountID = 0,
                HostAccountName = "PLACEHOLDER",
                ID = 999999,
                Name = "Test Server",
                MatchServerManagerID = null,
                Instance = 1,
                IPAddress = "127.0.0.1",
                Port = 11235,
                Location = "NEWERTH",
                Description = "Placeholder Server For Testing",
                Status = ServerStatus.SERVER_STATUS_IDLE
            };
        }

        return null;
    }

    /// <summary>
    ///     Sends MatchFoundUpdate (0x0D09) to all players in a match.
    /// </summary>
    private static void SendMatchFoundUpdate(MatchmakingMatch match)
    {
        ChatBuffer matchFound = new();

        matchFound.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
        matchFound.WriteString(match.SelectedMap);                           // Map Name
        matchFound.WriteInt32(match.LegionTeam.TeamSize);                     // Team Size
        matchFound.WriteInt8(Convert.ToByte(match.GameType));                 // Game Type
        matchFound.WriteString(match.SelectedMode);                           // Game Mode
        matchFound.WriteString(match.SelectedRegion);                         // Server Region
        matchFound.WriteString($"Match #{match.MatchIndex}");                 // Extra Info (Debug Data)

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(matchFound);
    }

    /// <summary>
    ///     Sends GroupQueueUpdate type=16 (TMM_GROUP_FOUND_SERVER) to all players.
    ///     This triggers "Sound The Horn!" in the client.
    /// </summary>
    private static void SendFoundServerUpdate(MatchmakingMatch match)
    {
        ChatBuffer found = new();

        found.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        found.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_FOUND_SERVER));

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(found);
    }

    /// <summary>
    ///     Sends AutoMatchConnect (0x0062) to all players with server connection details.
    /// </summary>
    private static void SendAutoMatchConnect(MatchmakingMatch match, MatchServer server, bool isReminder = false)
    {
        // Determine The Arranged Match Type Based On Game Type
        byte arrangedMatchType = match.GameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL          => (byte)MatchType.AM_MATCHMAKING,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL          => (byte)MatchType.AM_UNRANKED_MATCHMAKING,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS         => (byte)MatchType.AM_MATCHMAKING_MIDWARS,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS        => (byte)MatchType.AM_MATCHMAKING_RIFTWARS,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => (byte)MatchType.AM_MATCHMAKING_CAMPAIGN,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => (byte)MatchType.AM_MATCHMAKING_CAMPAIGN,
            _                                                      => (byte)MatchType.AM_MATCHMAKING
        };

        ChatBuffer connect = new();

        connect.WriteCommand(ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT);
        connect.WriteInt8(arrangedMatchType);                                         // Arranged Match Type
        connect.WriteInt32(match.MatchIndex);                                         // Matchup ID
        connect.WriteString(server.IPAddress);                                        // Server Address (IP)
        connect.WriteInt16(Convert.ToInt16(server.Port));                             // Server Port
        connect.WriteInt32(isReminder ? unchecked((int)0xFFFFFFFF) : Random.Shared.Next()); // Connection Reminder Flag

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(connect);
    }

    /// <summary>
    ///     Sends TMM_GROUP_NO_SERVERS_FOUND to all players in a match.
    /// </summary>
    private static void SendNoServersFound(MatchmakingMatch match)
    {
        ChatBuffer noServers = new();

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

        ChatBuffer update = new();

        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
        update.WriteInt32(averageQueueTimeSeconds);

        foreach (MatchmakingGroup group in queuedGroups)
            foreach (MatchmakingGroupMember member in group.Members)
                member.Session.Send(update);
    }
}
