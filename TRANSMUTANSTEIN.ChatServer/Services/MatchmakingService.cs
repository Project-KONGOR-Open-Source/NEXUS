using System.Collections.Concurrent;
using KONGOR.MasterServer.Models.ServerManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OneOf;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IConfiguration configuration, IChatContext chatContext) : BackgroundService, IMatchmakingService
{
    private IConfiguration Configuration { get; } = configuration;
    private IChatContext ChatContext { get; } = chatContext;

    public ConcurrentDictionary<int, MatchmakingGroup> Groups { get; } = [];

    // ... (rest of filtering properties)
    public ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 1));
    
    private ConcurrentDictionary<int, List<MatchmakingGroup>> PendingMatches { get; } = new();

    public ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 2));

    public ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 3));

    public ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 4));

    public ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups
        => new(Groups.Where(group => group.Value.Members.Count == 5));

    public override void Dispose()
    {
        base.Dispose();

        GC.SuppressFinalize(this);
    }

   // ... (GetMatchmakingGroup methods)
    public MatchmakingGroup? GetMatchmakingGroup(OneOf<int, string> memberIdentifier)
    {
        return memberIdentifier.Match(id => GetMatchmakingGroupByMemberID(id),
            name => GetMatchmakingGroupByMemberName(name));
    }

    public MatchmakingGroup? GetMatchmakingGroupByMemberID(int memberID)
    {
        return Groups.Values.SingleOrDefault(group => group.Members.Any(member => member.Account.ID == memberID));
    }

    public MatchmakingGroup? GetMatchmakingGroupByMemberName(string memberName)
    {
        return Groups.Values.SingleOrDefault(group =>
            group.Members.Any(member => member.Account.Name.Equals(memberName)));
    }


    public MatchmakingGroup? GetMatchmakingGroupByInvitedUser(string accountName)
    {
        // Try to find a group where the user is in the PendingInvites list
        return Groups.Values.SingleOrDefault(group => 
            group.PendingInvites.ContainsKey(accountName));
    }

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

    private async Task RunMatchBroker(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            // TODO: Implement Match Broker Logic Here

            # region Match Broker Logic
            
            // 1. Snapshot all active groups from the concurrent dictionary
            List<MatchmakingGroup> allGroups = Groups.Values.ToList();

            // 2. Define our "Bucket" key
            // A bucket is defined by (Region, GameType, Map).
            // Since groups can select multiple regions and modes, we need to iterate and place them into potentially multiple buckets.
            // For simplicity in this iteration: We will group by the FIRST valid permutation found, or just iterate permutations.
            // Better approach:
            // Iterate all groups. For each group, iterate their selected regions and game modes.
            // Add the group to a list for that specific (Region, Mode, Map) tuple.

            Dictionary<(string Region, ChatProtocol.TMMGameType GameType, string Map), List<MatchmakingGroup>> buckets = new();

            foreach (MatchmakingGroup group in allGroups)
            {
                // Skip if not queued
                if (group.QueueStartTime == null) continue;

                // Expand the group into all eligible buckets
                foreach (string region in group.Information.GameRegions)
                {
                    foreach (string modeStr in group.Information.GameModes)
                    {
                        // Parse mode string back to enum if necessary, or simplify if we assume standard modes.
                        // For now, let's use the Group's primary GameType if it aligns, or just use the primary keys.
                        // The 'Information.GameType' is usually the authority on what they queued for (e.g. Normal, Casual).
                        
                        (string Region, ChatProtocol.TMMGameType GameType, string Map) key = (Region: region, GameType: group.Information.GameType, Map: group.Information.MapName);

                        if (!buckets.ContainsKey(key))
                        {
                            buckets[key] = new List<MatchmakingGroup>();
                        }

                        // Avoid adding the same group multiple times to the same bucket (though the set should be unique by key)
                        if (!buckets[key].Contains(group))
                        {
                            buckets[key].Add(group);
                        }
                    }
                }
            }

            // 3. Process each bucket
            int playersPerTeam = Configuration.GetValue<int>("Matchmaking:PlayersPerTeam", 5);
            int playersPerMatch = playersPerTeam * 2;
            
            foreach (KeyValuePair<(string Region, ChatProtocol.TMMGameType GameType, string Map), List<MatchmakingGroup>> kvp in buckets)

            {
                (string Region, ChatProtocol.TMMGameType GameType, string Map) bucketKey = kvp.Key;
                List<MatchmakingGroup> candidates = kvp.Value;
                
                // Debug Bucket State
                if (candidates.Count > 0)
                {
                    Log.Information("Matchmaking Bucket State: Region={Region}, Map={Map}, Mode={Mode}, Count={Count}", 
                        bucketKey.Region, bucketKey.Map, bucketKey.GameType, candidates.Count);
                }

                // We need to find a combination of groups that sums to exactly 'playersPerMatch'
                // For this implementation, we will use a greedy approach:
                // Sort groups by Queue Duration (Longest first) to prioritize older waiters.
                // Then try to fill the room.

                List<MatchmakingGroup> potentialMatch = new();
                int currentCount = 0;

                List<MatchmakingGroup> sortedCandidates = candidates.OrderBy(g => g.QueueStartTime).ToList();

                foreach (MatchmakingGroup group in sortedCandidates)
                {
                    if (currentCount + group.Members.Count <= playersPerMatch)
                    {
                        potentialMatch.Add(group);
                        currentCount += group.Members.Count;
                    }

                    if (currentCount == playersPerMatch)
                    {
                        // MATCH FOUND!
                        await ProcessMatchUnsafe(potentialMatch);
                        
                        // Remove these groups from further consideration in OTHER buckets for this tick
                        // (Requires tracking used groups if we were doing this in a single pass, 
                        // but since we just modify state and let the next tick handle cleanup, it's safer to just break or track used IDs).
                        // For now, we assume ProcessMatchUnsafe dequeues them.
                        break; 
                    }
                }
            }

            # endregion
            
            int interval = Configuration.GetValue<int>("Matchmaking:IntervalInMilliseconds", 1000);
            await Task.Delay(interval, cancellationToken);
        }

        await Task.CompletedTask;
    }

    private async Task ProcessMatchUnsafe(List<MatchmakingGroup> groups)
    {
        // 1. Mark groups as no longer in queue to prevent double-matching
        foreach (MatchmakingGroup group in groups)
        {
            group.QueueStartTime = null;
        }

        // 2. Find Available Match Server
        ChatSession? matchServer = ChatContext.MatchServerChatSessions.Values
            .FirstOrDefault(s => s.ServerMetadata.Status == ChatProtocol.ServerStatus.SERVER_STATUS_IDLE);

        if (matchServer is null)
        {
            Log.Warning("Match Found But No IDLE Match Servers Available!");
            
            // Notify Clients: No Servers
            foreach (MatchmakingGroup group in groups)
            {
                // TMM_GROUP_NO_SERVERS_FOUND = 13 (0x0D) in enum
                group.MulticastUpdate(groups.First().Leader.Account.ID, ChatProtocol.TMMUpdateType.TMM_GROUP_NO_SERVERS_FOUND);
            }
            return;
        }

        Log.Information("Allocating Match to Server ID {ServerID} ({Address}:{Port})", 
            matchServer.ServerMetadata.ServerID, matchServer.ServerMetadata.Address, matchServer.ServerMetadata.Port);

        // 3. Generate Match ID (This is the MatchUp ID used for correlation)
        int matchUpId = Random.Shared.Next(100000, 999999);
        
        // 4. Store Pending Match State
        PendingMatches[matchUpId] = groups;
        
        // Note: Client Notification is deferred until ConfirmMatch is called by the Game Server (NET_CHAT_GS_ANNOUNCE_MATCH)
        
        // 5. Send CREATE_MATCH to Match Server
        ChatBuffer createMatch = new();

        
        // Determine Match Type byte map
        byte matchType = groups.First().Information.GameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => 4,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => 10,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => 10,
            _ => 10 // Default
        };
        
        MatchmakingGroup representativeGroup = groups.First();

        createMatch.WriteCommand(ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_CREATE_MATCH);
        createMatch.WriteInt8(matchType); // MatchType
        createMatch.WriteInt32(matchUpId); // MatchUp / MatchID (This is the Correlation ID)
        createMatch.WriteInt32(0); // Unknown1
        createMatch.WriteInt32(0); // Password (0 = None)
        createMatch.WriteString("TMM Match #"); // Match Name Prefix

        // Player Info List
        List<MatchmakingGroupMember> allPlayers = groups.SelectMany(g => g.Members).ToList();
        
        // Calculate Dynamic Team Size (e.g. 2 players -> TeamSize 1, 10 players -> TeamSize 5)
        int calculatedTeamSize = Math.Max(1, (int)Math.Ceiling(allPlayers.Count / 2.0));

        // Construct Match Settings String (mimic GameFinder.CreateMatchSettings)
        StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"mode:{representativeGroup.Information.GameModes.FirstOrDefault() ?? "normal"}");
        sb.Append($" map:{representativeGroup.Information.MapName}");
        sb.Append($" teamsize:{calculatedTeamSize}");
        sb.Append(" noleaver:true spectators:10"); // Defaults
        if (representativeGroup.Information.GameType == ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL || 
            representativeGroup.Information.GameType == ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL)
        {
             sb.Append(" casual:1");
        }

        createMatch.WriteString(sb.ToString()); // MatchSettings

        createMatch.WriteInt8(0); // UseNewMmrSystem
        createMatch.WriteInt8(0); // Unknown3

        createMatch.WriteInt8((byte)allPlayers.Count); // Player Count

        int currentSlotTeam1 = 0;
        int currentSlotTeam2 = 0;
        int maxTeamSize = calculatedTeamSize;

        // Balanced Team Assignment (Greedy "Snake" Logic for Groups)
        // 1. Sort Groups by Average MMR (Descending) - Strongest groups first
        // 2. Assign to team with fewer players.
        // 3. If players equal, assign to team with lower Total MMR.

        List<MatchmakingGroup> sortedGroups = groups.OrderByDescending(g => g.Members.Average(m => m.Rating)).ToList();

        List<MatchmakingGroup> team1Groups = new();
        List<MatchmakingGroup> team2Groups = new();
        int team1Count = 0;
        int team2Count = 0;
        double team1TotalMmr = 0;
        double team2TotalMmr = 0;

        foreach (MatchmakingGroup group in sortedGroups)
        {
            double groupMmr = group.Members.Sum(m => m.Rating);
            bool addToTeam1;

            // Decision Logic
            if (team1Count + group.Members.Count > maxTeamSize)
            {
                // Won't fit in Team 1, must go to Team 2
                addToTeam1 = false;
            }
            else if (team2Count + group.Members.Count > maxTeamSize)
            {
                 // Won't fit in Team 2, must go to Team 1
                 addToTeam1 = true;
            }
            else
            {
                // Fits in both. Balance!
                if (team1Count < team2Count)
                {
                    addToTeam1 = true; // Fill emptiness
                }
                else if (team2Count < team1Count)
                {
                    addToTeam1 = false;
                }
                else
                {
                    // Counts equal. Balance MMR.
                    addToTeam1 = team1TotalMmr <= team2TotalMmr;
                }
            }

            if (addToTeam1)
            {
                team1Groups.Add(group);
                team1Count += group.Members.Count;
                team1TotalMmr += groupMmr;
            }
            else
            {
                team2Groups.Add(group);
                team2Count += group.Members.Count;
                team2TotalMmr += groupMmr;
            }
        }

        // Now Write to Packet
        // Team 1
        foreach (MatchmakingGroup group in team1Groups)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                 createMatch.WriteInt32(member.Account.ID);
                 createMatch.WriteInt8(1); // Team 1 (Legion)
                 
                 byte slot = (byte)currentSlotTeam1++;
                 createMatch.WriteInt8(slot);
                 createMatch.WriteInt8((byte)group.Members.Count);
                 createMatch.WriteInt32(BitConverter.SingleToInt32Bits(10.0f)); 
                 createMatch.WriteInt32(BitConverter.SingleToInt32Bits(10.0f)); 
                 createMatch.WriteInt8(0); 
                 createMatch.WriteInt8(0); 
                 createMatch.WriteInt8(0); 
            }
        }

        // Team 2
        foreach (MatchmakingGroup group in team2Groups)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                 createMatch.WriteInt32(member.Account.ID);
                 createMatch.WriteInt8(2); // Team 2 (Hellbourne)
                 
                 byte slot = (byte)currentSlotTeam2++;
                 createMatch.WriteInt8(slot);
                 createMatch.WriteInt8((byte)group.Members.Count);
                 createMatch.WriteInt32(BitConverter.SingleToInt32Bits(10.0f)); 
                 createMatch.WriteInt32(BitConverter.SingleToInt32Bits(10.0f)); 
                 createMatch.WriteInt8(0); 
                 createMatch.WriteInt8(0); 
                 createMatch.WriteInt8(0); 
            }
        }
        
        createMatch.WriteInt32(0); // GroupIds Count (Empty for now)

        matchServer.Send(createMatch);
        
        Log.Information("Pending Match Created. MatchUpID: {MatchUpID}. Waiting for Game Server Confirmation...", matchUpId);

        await Task.CompletedTask;
    }
    
    public async Task ConfirmMatch(int matchupId, int matchId, string serverAddress, int serverPort)
    {
        if (!PendingMatches.TryRemove(matchupId, out List<MatchmakingGroup>? groups))
        {
            Log.Warning("Received ConfirmMatch for unknown MatchUpID: {MatchUpID}", matchupId);
            return;
        }

        Log.Information("Match Confirmed! MatchUpID: {MatchUpID} -> MatchID: {MatchID}. Notifying Clients...", matchupId, matchId);

        MatchmakingGroup representativeGroup = groups.First();

        // 1. MATCH_FOUND_UPDATE
        // Calculate Dynamic Team Size again for client packet
        List<MatchmakingGroupMember> allPlayers = groups.SelectMany(g => g.Members).ToList();
        int calculatedTeamSize = Math.Max(1, (int)Math.Ceiling(allPlayers.Count / 2.0));

        ChatBuffer matchFound = new();
        matchFound.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
        matchFound.WriteString(representativeGroup.Information.MapName); 
        matchFound.WriteInt8((byte)calculatedTeamSize); // Team Size (Dynamic)
        matchFound.WriteInt8((byte)representativeGroup.Information.GameType); 
        matchFound.WriteString(string.Join('|', representativeGroup.Information.GameModes)); 
        matchFound.WriteString(string.Join('|', representativeGroup.Information.GameRegions)); 
        matchFound.WriteString(""); 

        // 2. GROUP_QUEUE_UPDATE
        ChatBuffer groupFoundServer = new();
        groupFoundServer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
        groupFoundServer.WriteInt8(16); // GroupFoundServer

        // 3. AUTO_MATCH_CONNECT
        ChatBuffer autoConnect = new();
        autoConnect.WriteCommand(0x0062); 
        
        byte matchType = representativeGroup.Information.GameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => 4,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => 10,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => 10,
            _ => 10 
        };

        autoConnect.WriteInt8(matchType);
        autoConnect.WriteInt32(matchId); // The REAL MatchID from Game Server
        autoConnect.WriteString(serverAddress); 
        autoConnect.WriteInt16((short)serverPort); 
        autoConnect.WriteInt32(new Random().Next()); 

        foreach (MatchmakingGroup group in groups)
        {
            foreach (MatchmakingGroupMember member in group.Members)
            {
                member.Session.Send(matchFound);
                member.Session.Send(groupFoundServer);
                member.Session.Send(autoConnect);
            }
        }
        
        Log.Information("Clients Notified for MatchID {MatchID}", matchId);
        
        await Task.CompletedTask;
    }
}