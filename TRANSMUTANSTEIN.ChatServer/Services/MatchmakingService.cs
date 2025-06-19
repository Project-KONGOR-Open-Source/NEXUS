using MERRICK.Database.Context;
using MERRICK.Database.Entities.Game;
using Microsoft.Extensions.DependencyInjection;
using TRANSMUTANSTEIN.ChatServer.Matchmaking;
using TRANSMUTANSTEIN.ChatServer.Core;
using System.Collections.Concurrent;
using TRANSMUTANSTEIN.ChatServer.Models;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchmakingService(IServiceProvider serviceProvider) : IHostedService, IDisposable
{
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<MatchmakingService>>();
    private Timer? _matchmakingTimer;
    private readonly object _queueLock = new();

    // Group storage
    public static ConcurrentDictionary<int, MatchmakingGroup> SoloPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> TwoPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> ThreePlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> FourPlayerGroups { get; set; } = [];
    public static ConcurrentDictionary<int, MatchmakingGroup> FivePlayerGroups { get; set; } = [];

    // Matchmaking queues for each game type
    public static ConcurrentQueue<MatchmakingGroup> NormalQueue { get; set; } = new();
    public static ConcurrentQueue<MatchmakingGroup> CasualQueue { get; set; } = new();
    public static ConcurrentQueue<MatchmakingGroup> MidwarsQueue { get; set; } = new();
    public static ConcurrentQueue<MatchmakingGroup> CampaignQueue { get; set; } = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Started");

        // Start matchmaking timer (runs every 5 seconds)
        _matchmakingTimer = new Timer(ProcessMatchmakingQueues, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Matchmaking Service Has Stopped");

        _matchmakingTimer?.Dispose();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _matchmakingTimer?.Dispose();
        SoloPlayerGroups.Clear();
        TwoPlayerGroups.Clear();
        ThreePlayerGroups.Clear();
        FourPlayerGroups.Clear();
        FivePlayerGroups.Clear();

        // Clear queues
        while (NormalQueue.TryDequeue(out _)) { }
        while (CasualQueue.TryDequeue(out _)) { }
        while (MidwarsQueue.TryDequeue(out _)) { }
        while (CampaignQueue.TryDequeue(out _)) { }
    }

    public static void AddGroupToQueue(MatchmakingGroup group)
    {
        var queue = GetQueueForGameType(group.GameType);
        queue.Enqueue(group);
    }

    public static void RemoveGroupFromQueue(MatchmakingGroup group)
    {
        // Note: ConcurrentQueue doesn't have a direct remove method
        // In a real implementation, you might want to use a different data structure
        // For now, we'll rely on the group's state to filter out removed groups
    }

    private static ConcurrentQueue<MatchmakingGroup> GetQueueForGameType(ChatProtocol.TMMGameType gameType)
    {
        return gameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL => NormalQueue,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL => CasualQueue,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => MidwarsQueue,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => CampaignQueue,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => CampaignQueue,
            _ => NormalQueue
        };
    }

    private void ProcessMatchmakingQueues(object? state)
    {
        lock (_queueLock)
        {
            try
            {
                ProcessQueue(NormalQueue, "Normal");
                ProcessQueue(CasualQueue, "Casual");
                ProcessQueue(MidwarsQueue, "Midwars");
                ProcessQueue(CampaignQueue, "Campaign");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing matchmaking queues");
            }
        }
    }

    private void ProcessQueue(ConcurrentQueue<MatchmakingGroup> queue, string queueName)
    {
        List<MatchmakingGroup> queuedGroups = [];
        
        // Collect all groups from queue
        while (queue.TryDequeue(out var group))
        {
            // Only include groups that are still in queue state
            if (group.State == MatchmakingGroup.GroupState.InQueue)
            {
                queuedGroups.Add(group);
            }
        }

        if (queuedGroups.Count < 2)
        {
            // Re-queue the groups if we don't have enough for a match
            foreach (var group in queuedGroups)
            {
                queue.Enqueue(group);
            }
            return;
        }

        // Try to create matches
        var matches = FindMatches(queuedGroups);
        
        foreach (var match in matches)
        {
            CreateMatch(match.team1, match.team2, queueName);
        }

        // Re-queue groups that weren't matched
        var matchedGroups = matches.SelectMany(m => new[] { m.team1, m.team2 }).ToHashSet();
        foreach (var group in queuedGroups.Where(g => !matchedGroups.Contains(g)))
        {
            queue.Enqueue(group);
        }
    }

    private List<(MatchmakingGroup team1, MatchmakingGroup team2)> FindMatches(List<MatchmakingGroup> groups)
    {
        var matches = new List<(MatchmakingGroup, MatchmakingGroup)>();
        var availableGroups = new List<MatchmakingGroup>(groups);

        // Sort by queue time (prioritize longer waiting groups)
        availableGroups.Sort((a, b) => a.QueueJoinTime.CompareTo(b.QueueJoinTime));

        for (int i = 0; i < availableGroups.Count; i++)
        {
            var group1 = availableGroups[i];
            if (group1 == null) continue;

            for (int j = i + 1; j < availableGroups.Count; j++)
            {
                var group2 = availableGroups[j];
                if (group2 == null) continue;

                // Check if groups can be matched
                if (CanGroupsMatch(group1, group2))
                {
                    matches.Add((group1, group2));
                    availableGroups[i] = null!; // Mark as used
                    availableGroups[j] = null!; // Mark as used
                    break;
                }
            }
        }

        return matches;
    }

    private bool CanGroupsMatch(MatchmakingGroup group1, MatchmakingGroup group2)
    {
        // Basic matchmaking criteria
        
        // 1. Total players should be 10 (5v5)
        int totalPlayers = group1.ParticipantCount + group2.ParticipantCount;
        if (totalPlayers != 10) return false;

        // 2. Check rating difference (allow wider range for longer queue times)
        float ratingDiff = Math.Abs(group1.AverageRating - group2.AverageRating);
        float maxRatingDiff = GetMaxRatingDifference(group1, group2);
        if (ratingDiff > maxRatingDiff) return false;

        // 3. Check game mode compatibility
        if (!AreGameModesCompatible(group1, group2)) return false;

        // 4. Check region compatibility
        if (!AreRegionsCompatible(group1, group2)) return false;

        return true;
    }

    private float GetMaxRatingDifference(MatchmakingGroup group1, MatchmakingGroup group2)
    {
        // Get the longest queue time between the two groups
        var longestQueueTime = Math.Max(
            (DateTime.UtcNow - group1.QueueJoinTime).TotalMinutes,
            (DateTime.UtcNow - group2.QueueJoinTime).TotalMinutes
        );

        // Start with base rating difference and increase over time
        float baseRatingDiff = 100f;
        float timeMultiplier = (float)(longestQueueTime / 2.0); // Increase by 50 rating per minute
        
        return baseRatingDiff + (timeMultiplier * 50f);
    }

    private bool AreGameModesCompatible(MatchmakingGroup group1, MatchmakingGroup group2)
    {
        // For now, just check if they have any overlapping game modes
        // In a real implementation, you'd want more sophisticated logic
        return true; // Simplified for now
    }

    private bool AreRegionsCompatible(MatchmakingGroup group1, MatchmakingGroup group2)
    {
        // For now, just check if they have any overlapping regions
        // In a real implementation, you'd want to consider ping/latency
        return true; // Simplified for now
    }    private void CreateMatch(MatchmakingGroup team1, MatchmakingGroup team2, string queueName)
    {
        Logger.LogInformation($"Creating match in {queueName} queue: Team 1 ({team1.ParticipantCount} players, avg rating {team1.AverageRating:F0}) vs Team 2 ({team2.ParticipantCount} players, avg rating {team2.AverageRating:F0})");

        // Transition both groups to loading state
        team1.StartLoadingResources();
        team2.StartLoadingResources();

        // Create match asynchronously
        _ = Task.Run(async () => await CreateMatchAsync(team1, team2, queueName));
    }

    private async Task CreateMatchAsync(MatchmakingGroup team1, MatchmakingGroup team2, string queueName)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var matchService = scope.ServiceProvider.GetRequiredService<MatchService>();
            var gameServerService = scope.ServiceProvider.GetRequiredService<GameServerAllocationService>();

            // 1. Allocate a game server
            GameServer? server = await gameServerService.AllocateServerAsync(team1.Regions);
            if (server == null)
            {
                Logger.LogError("Failed to allocate server for match - abandoning match creation");
                team1.LeaveQueue(true);
                team2.LeaveQueue(true);
                return;
            }

            // 2. Reserve the server
            await gameServerService.ReserveServerAsync(server.ID);

            // 3. Create match record in database
            Match match = await matchService.CreateMatchAsync(team1, team2, server.ID, server.HostAccountName);

            // 4. Send match details to game server
            await SendMatchDetailsToGameServer(server, match, team1, team2);

            // 5. Notify players about the match
            await NotifyPlayersOfMatch(team1, team2, server, match);

            Logger.LogInformation("Successfully created match {MatchID} on server {ServerID}", match.MatchID, server.ID);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during match creation");
            
            // Revert groups back to waiting state on error
            team1.LeaveQueue(true);
            team2.LeaveQueue(true);
        }
    }

    private async Task SendMatchDetailsToGameServer(GameServer server, Match match, MatchmakingGroup team1, MatchmakingGroup team2)
    {
        // TODO: Implement game server communication
        // This would typically involve sending a NET_CHAT_GS_CREATE_MATCH command to the game server
        // For now, just log the action
        Logger.LogInformation("Would send match {MatchID} details to server {ServerID} at {Address}:{Port}", 
            match.MatchID, server.ID, server.IPAddress, server.Port);
        
        await Task.CompletedTask; // Placeholder for actual implementation
    }

    private async Task NotifyPlayersOfMatch(MatchmakingGroup team1, MatchmakingGroup team2, GameServer server, Match match)
    {
        // TODO: Implement player notifications
        // This would involve sending match found notifications to all players with server details
        // For now, just log the action
        Logger.LogInformation("Would notify {PlayerCount} players about match {MatchID} on server {ServerID}", 
            team1.ParticipantCount + team2.ParticipantCount, match.MatchID, server.ID);
        
        await Task.CompletedTask; // Placeholder for actual implementation
    }
}
