using MERRICK.Database.Context;
using MERRICK.Database.Entities.Game;
using MERRICK.Database.Enumerations;
using Microsoft.EntityFrameworkCore;
using TRANSMUTANSTEIN.ChatServer.Matchmaking;
using TRANSMUTANSTEIN.ChatServer.Core;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public class MatchService(MerrickContext merrickContext, ILogger<MatchService> logger)
{
    private readonly MerrickContext _merrickContext = merrickContext;
    private readonly ILogger<MatchService> _logger = logger;

    public async Task<Match> CreateMatchAsync(MatchmakingGroup team1, MatchmakingGroup team2, long serverID, string hostAccountName)
    {
        // Generate a unique match ID
        int matchID = await GenerateMatchIDAsync();

        // Calculate team ratings
        int avgPSRTeam1 = (int)team1.AverageRating;
        int avgPSRTeam2 = (int)team2.AverageRating;

        // Determine match type based on game type
        ArrangedMatchType matchType = GetArrangedMatchType(team1.GameType);

        // Create match entity
        var match = new Match
        {
            MatchID = matchID,
            ServerID = serverID,
            HostAccountName = hostAccountName,
            Map = GetMapName(team1.GameType),
            MapVersion = "1.0",
            GameMode = GetGameModeName(team1.GameType),
            MatchType = matchType,
            Region = team1.Regions, // Assuming the first region
            Ranked = team1.Ranked,
            Status = MatchStatus.Created,
            AveragePSRTeamOne = avgPSRTeam1,
            AveragePSRTeamTwo = avgPSRTeam2,
            TimestampCreated = DateTime.UtcNow,
            Participants = []
        };

        // Add participants from both teams
        await AddParticipantsToMatchAsync(match, team1, 1);
        await AddParticipantsToMatchAsync(match, team2, 2);

        // Save to database
        _merrickContext.Matches.Add(match);
        await _merrickContext.SaveChangesAsync();

        _logger.LogInformation("Created match {MatchID} with {ParticipantCount} participants", match.MatchID, match.Participants.Count);

        return match;
    }

    public async Task UpdateMatchStatusAsync(int matchID, MatchStatus status)
    {
        var match = await _merrickContext.Matches
            .FirstOrDefaultAsync(m => m.MatchID == matchID);

        if (match != null)
        {
            match.Status = status;
            
            if (status == MatchStatus.InProgress && match.TimestampStarted == null)
            {
                match.TimestampStarted = DateTime.UtcNow;
            }
            else if (status == MatchStatus.Completed && match.TimestampEnded == null)
            {
                match.TimestampEnded = DateTime.UtcNow;
                if (match.TimestampStarted.HasValue)
                {
                    match.TimePlayed = (int)(match.TimestampEnded.Value - match.TimestampStarted.Value).TotalSeconds;
                }
            }

            await _merrickContext.SaveChangesAsync();
            _logger.LogInformation("Updated match {MatchID} status to {Status}", matchID, status);
        }
    }

    public async Task UpdateParticipantStatusAsync(int matchID, int accountID, bool isReady, bool hasJoined, bool hasDisconnected)
    {
        var participant = await _merrickContext.MatchParticipants
            .FirstOrDefaultAsync(p => p.MatchID == matchID && p.AccountID == accountID);

        if (participant != null)
        {
            participant.IsReady = isReady;
            participant.HasJoined = hasJoined;
            participant.HasDisconnected = hasDisconnected;

            await _merrickContext.SaveChangesAsync();
            _logger.LogDebug("Updated participant {AccountID} in match {MatchID}", accountID, matchID);
        }
    }

    private async Task<int> GenerateMatchIDAsync()
    {
        // Simple approach: find the max match ID and increment
        // In production, you might want a more sophisticated approach
        var lastMatch = await _merrickContext.Matches
            .OrderByDescending(m => m.MatchID)
            .FirstOrDefaultAsync();

        int nextID = lastMatch?.MatchID + 1 ?? 1000000; // Start from 1M for matches
        
        // Make sure it doesn't conflict with existing MatchStatistics
        var existingStats = await _merrickContext.MatchStatistics
            .AnyAsync(ms => ms.MatchID == nextID);

        if (existingStats)
        {
            var maxStatID = await _merrickContext.MatchStatistics
                .MaxAsync(ms => ms.MatchID);
            nextID = Math.Max(nextID, maxStatID + 1);
        }

        return nextID;
    }

    private async Task AddParticipantsToMatchAsync(Match match, MatchmakingGroup group, int team)
    {
        int lobbyPosition = 0;
        foreach (var participant in group.Participants)
        {
            var account = await _merrickContext.Accounts
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.ID == participant.AccountId);

            if (account != null)
            {
                var matchParticipant = new MatchParticipant
                {
                    MatchID = match.MatchID,
                    AccountID = account.ID,
                    AccountName = account.Name,
                    ClanID = account.Clan?.ID,
                    ClanTag = account.Clan?.Tag,
                    Team = team,
                    LobbyPosition = lobbyPosition++,
                    GroupNumber = group.GroupId,
                    IsReady = false,
                    HasJoined = false,
                    HasDisconnected = false
                };

                match.Participants.Add(matchParticipant);
            }
        }
    }

    private static ArrangedMatchType GetArrangedMatchType(ChatProtocol.TMMGameType gameType)
    {
        return gameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL => ArrangedMatchType.Matchmaking,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL => ArrangedMatchType.UnrankedMatchmaking,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => ArrangedMatchType.MatchmakingMidwars,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => ArrangedMatchType.MatchmakingCampaign,
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => ArrangedMatchType.MatchmakingCampaign,
            _ => ArrangedMatchType.Matchmaking
        };
    }

    private static string GetMapName(ChatProtocol.TMMGameType gameType)
    {
        return gameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => "midwars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS => "riftwars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => "caldavar",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL => "caldavar",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => "caldavar_old",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL => "caldavar_old",
            _ => "caldavar"
        };
    }

    private static string GetGameModeName(ChatProtocol.TMMGameType gameType)
    {
        return gameType switch
        {
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL => "Normal",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CASUAL => "Casual",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS => "MidWars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS => "RiftWars",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL => "Campaign Normal",
            ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_CASUAL => "Campaign Casual",
            _ => "Normal"
        };
    }
}
