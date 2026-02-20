namespace TRANSMUTANSTEIN.ChatServer.Services;

/// <summary>
///     Background service that manages matchmaking and the match broker.
/// </summary>
public class MatchmakingService : BackgroundService, IDisposable
{
    private readonly IOptions<MatchmakingSettings> _settings;
    private readonly IDatabase _distributedCacheStore;

    public MatchmakingService(IOptions<MatchmakingSettings> settings, IDatabase distributedCacheStore)
    {
        _settings = settings;
        _distributedCacheStore = distributedCacheStore;
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

            // Spawn Bot Matches Immediately (Bot Groups Bypass The Regular Match Broker Cycles)
            List<MatchmakingGroup> botGroups = [.. queuedGroups.Where(group => group.Information.GroupType == ChatProtocol.TMMType.TMM_TYPE_COOP)];

            foreach (MatchmakingGroup botGroup in botGroups)
            {
                MatchmakingMatch botMatch = MatchmakingMatch.FromBotGroup(botGroup);

                bool spawned = await SpawnMatch(botMatch);

                if (spawned is false)
                {
                    SendNoServersFound(botMatch);

                    botGroup.MatchedUp = false;
                    botGroup.AssignedMatchGUID = null;
                    botGroup.AssignedTeamGUID = null;
                }
            }

            // Run The Regular Broker Cycle For Non-Bot Groups
            List<MatchmakingGroup> regularGroups = [.. queuedGroups
                .Where(group => group.Information.GroupType != ChatProtocol.TMMType.TMM_TYPE_COOP && group.MatchedUp is false)];

            if (regularGroups.Count == 0)
                continue;

            List<MatchmakingMatch> matches = RunMatchBrokerCycle(regularGroups);

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
    ///     Runs a single match broker cycle with TMR-aware matching.
    ///     Uses adaptive TMR spread based on queue time and enforces group makeup rules.
    /// </summary>
    private List<MatchmakingMatch> RunMatchBrokerCycle(List<MatchmakingGroup> queuedGroups)
    {
        List<MatchmakingMatch> matches = [];

        int playersPerTeam = _settings.Value.PlayersPerTeam;
        double maxTMRDifference = _settings.Value.MaximumTeamTMRDifference;

        // Group By Game Type For Compatibility
        Dictionary<ChatProtocol.TMMGameType, List<MatchmakingGroup>> groupsByGameType = queuedGroups
            .GroupBy(group => group.Information.GameType)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        foreach ((ChatProtocol.TMMGameType gameType, List<MatchmakingGroup> typeGroups) in groupsByGameType)
        {
            // Form Teams From Groups (Using TMR-Aware Grouping)
            List<MatchmakingTeam> teams = FormTeams(typeGroups, playersPerTeam);

            // Sort Teams By Effective Rating For Better Matching (Power Mean + Premade Bonus)
            teams = [.. teams.OrderBy(team => team.EffectiveTeamRating)];

            // Pair Teams Into Matches (Matching Adjacent Teams By TMR)
            HashSet<int> matchedTeamIndices = [];

            for (int teamIndex = 0; teamIndex < teams.Count; teamIndex++)
            {
                if (matchedTeamIndices.Contains(teamIndex))
                    continue;

                MatchmakingTeam legionTeam = teams[teamIndex];

                // Find The Best Opponent From Remaining Teams
                MatchmakingTeam? bestOpponent = null;
                int bestOpponentIndex = -1;
                double bestTMRDifference = double.MaxValue;

                for (int opponentIndex = teamIndex + 1; opponentIndex < teams.Count; opponentIndex++)
                {
                    if (matchedTeamIndices.Contains(opponentIndex))
                        continue;

                    MatchmakingTeam candidateOpponent = teams[opponentIndex];

                    if (legionTeam.IsCompatibleWith(candidateOpponent) is false)
                        continue;

                    // Calculate TMR Difference Using Effective Rating (Power Mean + Premade Bonus)
                    double tmrDifference = Math.Abs(legionTeam.EffectiveTeamRating - candidateOpponent.EffectiveTeamRating);

                    // Get Maximum Acceptable TMR Spread Based On Queue Time
                    double maxAcceptableSpread = GetMaxAcceptableTMRSpread(legionTeam, candidateOpponent, maxTMRDifference);

                    if (tmrDifference > maxAcceptableSpread)
                        continue;

                    int groupMakeupDifference = Math.Abs(legionTeam.GroupMakeup - candidateOpponent.GroupMakeup);

                    // Check Group Makeup Difference (Enforce Fairness)
                    if (groupMakeupDifference > 2)
                    {
                        Log.Debug(@"Skipping Match Due To Group Makeup Mismatch: {Legion} vs {Hellbourne} (Diff: {Diff})",
                            legionTeam.GroupMakeupString, candidateOpponent.GroupMakeupString, groupMakeupDifference);
                        continue;
                    }

                    // Check For +0/-1 Rating Outcomes (Frustrating For High-Rated Players)
                    if (ProducesPlusZeroMinusOne(legionTeam) || ProducesPlusZeroMinusOne(candidateOpponent))
                    {
                        Log.Debug(@"Skipping Match Due To +0/-1 Rating Outcome Risk");
                        continue;
                    }

                    if (tmrDifference < bestTMRDifference)
                    {
                        bestOpponent = candidateOpponent;
                        bestOpponentIndex = opponentIndex;
                        bestTMRDifference = tmrDifference;
                    }
                }

                if (bestOpponent is null)
                    continue;

                // Mark Both Teams As Matched
                matchedTeamIndices.Add(teamIndex);
                matchedTeamIndices.Add(bestOpponentIndex);

                // Create The Match
                MatchmakingMatch match = MatchmakingMatch.FromTeams(legionTeam, bestOpponent, _settings.Value.LogisticPredictionScale);

                // Set Match Details From First Group's Information
                MatchmakingGroupInformation information = legionTeam.Groups[0].Information;
                match.GameType = gameType;
                match.SelectedMap = information.MapName;
                match.SelectedMode = information.GameModes.Length > 0 ? information.GameModes[0] : "ap";
                match.SelectedRegion = information.GameRegions.Length > 0 ? information.GameRegions[0] : "NEWERTH";
                match.IsRanked = information.Ranked;
                match.CombineMethod = MatchmakingCombineMethod.FirstInFirstOut;

                matches.Add(match);

                if (match.HellbourneTeam is not null)
                {
                    Log.Information(@"Match Created: GUID={MatchGUID}, GameType={GameType}, Legion={LegionCount}p ({LegionMakeup}, TeamRating: {LegionTMR:F1}), Hellbourne={HellbourneCount}p ({HellbourneMakeup}, TeamRating: {HellbourneTMR:F1}), TeamRatingDifference={TMRDifference:F1}, Prediction={Prediction:P1}",
                        match.GUID, gameType, match.LegionTeam.PlayerCount, match.LegionTeam.GroupMakeupString, match.LegionTeam.EffectiveTeamRating, match.HellbourneTeam.PlayerCount, match.HellbourneTeam.GroupMakeupString, match.HellbourneTeam.EffectiveTeamRating, bestTMRDifference, match.MatchupPrediction);
                }

                else
                {
                    Log.Information(@"Bot Match Created: GUID={MatchGUID}, GameType={GameType}, Legion={LegionCount}p ({LegionMakeup}, TeamRating: {LegionTMR:F1}), TeamRatingDifference={TMRDifference:F1}",
                        match.GUID, gameType, match.LegionTeam.PlayerCount, match.LegionTeam.GroupMakeupString, match.LegionTeam.EffectiveTeamRating, bestTMRDifference);
                }
            }
        }

        return matches;
    }

    /// <summary>
    ///     Gets the maximum acceptable TMR spread for matching two teams.
    ///     Uses the adaptive spread based on the longest-waiting group.
    /// </summary>
    private static double GetMaxAcceptableTMRSpread(MatchmakingTeam team1, MatchmakingTeam team2, double baseMaxDifference)
    {
        // Get The Longest Queue Time From Either Team
        double longestQueueMinutes = Math.Max(
            team1.Groups.Max(group => group.QueuedTimeInMinutes),
            team2.Groups.Max(group => group.QueuedTimeInMinutes));

        // Base Spread Starts At Config Value
        double spread = baseMaxDifference;

        // Expand By 50 TMR Per Minute After 2 Minutes Of Waiting
        if (longestQueueMinutes > 2.0)
        {
            spread += (longestQueueMinutes - 2.0) * 50.0;
        }

        // Cap At A Reasonable Maximum (1000 TMR Difference)
        return Math.Min(spread, 1000.0);
    }

    /// <summary>
    ///     Checks if a team would produce "+0/-1" rating outcomes.
    ///     This happens when the highest-rated player is so far above their teammates that they would gain 0 MMR for winning but lose MMR for losing.
    /// </summary>
    /// <param name="team">The team to check.</param>
    /// <returns>TRUE if the team would produce +0/-1 outcomes, FALSE otherwise.</returns>
    private static bool ProducesPlusZeroMinusOne(MatchmakingTeam team)
    {
        int teamSize = team.TeamSize;
        int playerCount = team.PlayerCount;

        // Only Applies To Full Teams With More Than 1 Player
        if (playerCount != teamSize || teamSize <= 1)
            return false;

        double totalTMR = team.TotalTMR;
        double highestTMR = team.HighestTMR;

        // Calculate The Combined TMR Of The Bottom N-1 Players
        double bottomMembersTMR = totalTMR - highestTMR;

        // If The Highest Player Is More Than 151 Above The Average Of The Rest, Reject
        // A Difference Of 200 Produces +0/-1 Outcomes, So We Use 151 As The Threshold
        double averageOfOthers = bottomMembersTMR / (teamSize - 1);
        double difference = highestTMR - averageOfOthers;

        return difference > 151.0;
    }

    /// <summary>
    ///     Forms teams from groups using a phased combine method.
    ///     Prioritises full teams first, then larger group combinations, then smaller.
    /// </summary>
    private static List<MatchmakingTeam> FormTeams(List<MatchmakingGroup> groups, int playersPerTeam)
    {
        List<MatchmakingTeam> teams = [];
        List<MatchmakingGroup> availableGroups = [.. groups.Where(group => group.MatchedUp is false)];

        // Phase 1: Full Teams (5-Stacks For 5v5)
        teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [playersPerTeam]));

        // Phase 2: Two-Group Combinations (Largest First)
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [4, 1])); // 4+1
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [3, 2])); // 3+2
        }

        else if (playersPerTeam == 3)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 1])); // 2+1
        }

        // Phase 3: Three-Group Combinations
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [3, 1, 1])); // 3+1+1
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 2, 1])); // 2+2+1
        }

        // Phase 4: Four-Group Combinations
        if (playersPerTeam == 5)
        {
            teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, [2, 1, 1, 1])); // 2+1+1+1
        }

        // Phase 5: All Solo Players
        teams.AddRange(FormTeamsWithPattern(availableGroups, playersPerTeam, Enumerable.Repeat(1, playersPerTeam).ToArray()));

        return teams;
    }

    /// <summary>
    ///     Forms teams matching a specific group size pattern.
    ///     For example, pattern [4, 1] looks for a 4-player group and a 1-player group.
    /// </summary>
    private static List<MatchmakingTeam> FormTeamsWithPattern(List<MatchmakingGroup> availableGroups, int playersPerTeam, int[] pattern)
    {
        List<MatchmakingTeam> teams = [];

        // Sort Pattern Descending For Greedy Matching
        int[] sortedPattern = [.. pattern.OrderByDescending(size => size)];

        while (true)
        {
            List<MatchmakingGroup> teamGroups = [];
            List<MatchmakingGroup> usedGroups = [];

            // Try To Find Groups Matching Each Size In The Pattern
            foreach (int requiredSize in sortedPattern)
            {
                // Find A Group With Exactly This Size (That Hasn't Been Used Yet)
                MatchmakingGroup? matchingGroup = availableGroups
                    .Except(usedGroups)
                    .Where(group => group.Members.Count == requiredSize)
                    .OrderBy(group => group.QueueStartTime) // FIFO Within Same Size
                    .FirstOrDefault();

                if (matchingGroup is null)
                    break; // Can't Complete This Pattern

                teamGroups.Add(matchingGroup);
                usedGroups.Add(matchingGroup);
            }

            // Check If We Found A Complete Pattern
            if (teamGroups.Count == sortedPattern.Length && teamGroups.Sum(group => group.Members.Count) == playersPerTeam)
            {
                // Create The Team
                MatchmakingTeam team = MatchmakingTeam.FromGroups(teamGroups, playersPerTeam);
                teams.Add(team);

                // Remove Used Groups From Available Pool
                foreach (MatchmakingGroup group in teamGroups)
                    availableGroups.Remove(group);

                // Track Combine Method
                string patternString = string.Join("+", sortedPattern);

                Log.Debug(@"Formed Team With Pattern {Pattern}: {GroupCount} Groups, {PlayerCount} Players",
                    patternString, teamGroups.Count, team.PlayerCount);
            }

            else
            {
                // Can't Form More Teams With This Pattern
                break;
            }
        }

        return teams;
    }

    /// <summary>
    ///     Spawns a match by allocating a server and sending CreateMatch to the game server.
    ///     Player notifications are sent immediately - we don't wait for AnnounceMatch because some game server configurations use the HTTP path instead.
    /// </summary>
    private async Task<bool> SpawnMatch(MatchmakingMatch match)
    {
        // Find An Available Server And Its Chat Session
        (MatchServer? server, MatchServerChatSession? serverSession) = await FindAvailableServerWithSession(match);

        if (server is null || serverSession is null)
        {
            Log.Warning(@"No Available Server Found For Match GUID {MatchGUID}", match.GUID);

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

        Log.Information(@"CreateMatch Sent: MatchGUID={MatchGUID}, ServerID={ServerID}, Server={ServerAddress}:{ServerPort}",
            match.GUID, server.ID, match.ServerAddress, match.ServerPort);

        // Send Leave Queue Notification To Dismiss The Client's Queue Timer
        ChatBuffer leaveQueue = new ();

        leaveQueue.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Session.Send(leaveQueue);

        // Send Player Notifications
        SendMatchFoundUpdate(match, match.CorrelationID);
        SendFoundServerUpdate(match);

        // Mark All Members As In-Game
        foreach (MatchmakingGroup group in match.GetAllGroups())
        {
            group.QueueStartTime = null;

            foreach (MatchmakingGroupMember member in group.Members)
                member.IsInGame = true;
        }

        match.State = MatchmakingMatchState.WaitingForPlayers;

        Log.Information(@"Match Notifications Sent: GUID={MatchGUID}, Server={ServerAddress}:{ServerPort}",
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
    /// </summary>
    /// <summary>
    ///     Creates a MatchInformation object for caching, enabling player join tracking.
    ///     Called from MatchAnnounce when the game server provides the real match ID.
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
        bool isCasual = match.GameType is ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL
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
    ///     Finds an available server for a match along with its chat session.
    ///     Returns null if no idle server with an active session is found.
    /// </summary>
    private async Task<(MatchServer? Server, MatchServerChatSession? Session)> FindAvailableServerWithSession(MatchmakingMatch match)
    {
        List<MatchServer> servers = await _distributedCacheStore.GetMatchServers();

        // Find An Idle Server That Also Has An Active Chat Session
        foreach (MatchServer server in servers.Where(server => server.Status == ServerStatus.SERVER_STATUS_IDLE))
        {
            if (Context.MatchServerChatSessions.TryGetValue(server.ID, out MatchServerChatSession? session))
            {
                Log.Debug(@"Found Idle Server With Session: ServerID={ServerID}, ServerName={ServerName}", server.ID, server.Name);

                return (server, session);
            }

            Log.Debug(@"Idle Server Has No Active Chat Session: ServerID={ServerID}", server.ID);
        }

        // No Idle Server With Active Session Found
        if (servers.Count == 0)
            Log.Warning(@"No Servers Available For Match GUID {MatchGUID}", match.GUID);

        else
            Log.Warning(@"No Idle Servers With Active Sessions For Match GUID {MatchGUID} (Total Servers: {ServerCount})", match.GUID, servers.Count);

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
