# Match Results Submission

**Command**: NET_CHAT_SV_MATCH_RESULTS
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Game Server → Chat Server (server port)
**Response**: Acknowledgement + rating updates

## Purpose

Game server submits final match results for rating calculations and statistics updates.

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_MATCH_RESULTS]
[int32: Server ID]
[int32: Match Lobby ID]
[int8: Winning Team (1 or 2)]
[int32: Match Duration Seconds]
[int8: Team 1 Player Count]
[for each Team 1 player:]
    [int32: Account ID]
    [int32: Kills]
    [int32: Deaths]
    [int32: Assists]
    [int32: Gold Earned]
    [int32: Experience Earned]
    [int8: Disconnect Status (1 if disconnected, 0 if finished)]
[int8: Team 2 Player Count]
[for each Team 2 player:]
    [int32: Account ID]
    [int32: Kills]
    [int32: Deaths]
    [int32: Assists]
    [int32: Gold Earned]
    [int32: Experience Earned]
    [int8: Disconnect Status]
```

**Fields**:
- **Server ID**: Submitting server's ID
- **Match Lobby ID**: Match identifier
- **Winning Team**: 1 or 2 (0 if draw/no result)
- **Match Duration Seconds**: Total match time
- **Player Statistics**: Per-player match stats

**Example**:
```
Server ID: 42
Match Lobby ID: 12345
Winning Team: 1
Match Duration: 2145 seconds (35 minutes 45 seconds)
Team 1: 5 players [stats...]
Team 2: 5 players [stats...]
```

## Response Message

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_MATCH_RESULTS_RESPONSE]
[int8: Success = 1]
[int32: Match Lobby ID]
[int8: Team 1 Player Count]
[for each Team 1 player:]
    [int32: Account ID]
    [float: New Rating]
    [float: Rating Change]
[int8: Team 2 Player Count]
[for each Team 2 player:]
    [int32: Account ID]
    [float: New Rating]
    [float: Rating Change]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Valid Server ID** | Server must be registered |
| **Valid Match Lobby ID** | Match must exist |
| **Server owns match** | Server must be allocated to this match |
| **Player count** | 10 players total (5 per team) |
| **Valid winning team** | 0 (draw), 1, or 2 |
| **Reasonable duration** | 10-180 minutes (reject outliers) |

## Error Cases

| Error | Reason |
|-------|--------|
| "Server not registered" | Server ID not found |
| "Match not found" | Match Lobby ID invalid |
| "Server not allocated to this match" | Wrong server submitting |
| "Results already submitted" | Duplicate submission |
| "Invalid player count" | Not 10 players |
| "Invalid match duration" | Duration out of reasonable range |

## Processing Steps

1. Validate Server ID and Match Lobby ID
2. Verify server owns this match
3. Check results not already submitted
4. Validate player counts and data
5. Calculate rating changes (ELO/TrueSkill)
6. Update PlayerStatistics in database
7. Store match results in MatchHistory table
8. Update player win/loss records
9. Free game server slot (decrement CurrentMatchCount)
10. Send rating updates to server
11. Notify online players of rating changes
12. Remove MatchLobby from memory

## Rating Calculation

**ELO System** (simplified):

```csharp
private float CalculateRatingChange(float playerRating, float opponentAvgRating, bool won, bool disconnected)
{
    // K-factor (rating volatility)
    float kFactor = 32.0f;

    // Disconnects reduce rating gain or increase rating loss
    if (disconnected)
    {
        kFactor *= 1.5f; // 50% harsher penalty
    }

    // Expected score (0.0-1.0)
    float expectedScore = 1.0f / (1.0f + MathF.Pow(10.0f, (opponentAvgRating - playerRating) / 400.0f));

    // Actual score (1.0 for win, 0.0 for loss)
    float actualScore = won ? 1.0f : 0.0f;

    // Rating change
    float ratingChange = kFactor * (actualScore - expectedScore);

    return ratingChange;
}
```

**Application**:
```csharp
// Calculate team average ratings
float team1AvgRating = team1Players.Average(p => p.CurrentRating);
float team2AvgRating = team2Players.Average(p => p.CurrentRating);

// Update each player
foreach (var player in team1Players)
{
    bool won = (winningTeam == 1);
    float ratingChange = CalculateRatingChange(
        player.CurrentRating,
        team2AvgRating,
        won,
        player.Disconnected
    );

    player.NewRating = player.CurrentRating + ratingChange;

    // Update database
    await UpdatePlayerRating(player.AccountID, gameType, player.NewRating);
}
```

## Database Updates

**PlayerStatistics Update**:
```csharp
var stats = await db.PlayerStatistics.FindAsync(accountID);

// Update rating for specific game type
switch (gameType)
{
    case GameType.CampaignNormal:
        stats.CampaignNormalRating += ratingChange;
        stats.CampaignNormalGamesPlayed++;
        if (won) stats.CampaignNormalWins++;
        break;
    // ... other game types
}

stats.TotalGamesPlayed++;
if (won) stats.TotalWins++;

await db.SaveChangesAsync();
```

**MatchHistory Creation** (optional):
```csharp
var matchHistory = new MatchHistory
{
    MatchLobbyID = matchLobbyID,
    GameType = matchLobby.GameType,
    GameMode = matchLobby.GameMode,
    Region = matchLobby.Region,
    WinningTeam = winningTeam,
    MatchDurationSeconds = matchDuration,
    CompletedAt = DateTime.UtcNow,
    Team1Players = JsonSerializer.Serialize(team1PlayerStats),
    Team2Players = JsonSerializer.Serialize(team2PlayerStats)
};

db.MatchHistories.Add(matchHistory);
await db.SaveChangesAsync();
```

## State Management

```csharp
// Validate
if (!RegisteredGameServers.TryGetValue(serverID, out var server))
{
    SendError(session, "Server not registered");
    return;
}

if (!ActiveMatchLobbies.TryGetValue(matchLobbyID, out var matchLobby))
{
    SendError(session, "Match not found");
    return;
}

if (matchLobby.GameServerID != serverID)
{
    SendError(session, "Server not allocated to this match");
    return;
}

// Calculate ratings
var ratingUpdates = await CalculateRatingUpdates(matchLobby, winningTeam, team1Players, team2Players);

// Update database
await UpdatePlayerStatistics(ratingUpdates);
await StoreMatchHistory(matchLobby, winningTeam, matchDuration, team1Players, team2Players);

// Free server slot
server.CurrentMatchCount--;
if (server.Status == GameServerStatus.Full && server.CurrentMatchCount < server.MaxConcurrentMatches)
{
    server.Status = GameServerStatus.Available;
    AvailableServersByRegion[server.Region].Add(server);
}

// Remove match lobby
ActiveMatchLobbies.TryRemove(matchLobbyID, out _);

// Notify online players
await NotifyPlayersOfRatingChange(ratingUpdates);

// Send response
var responseMsg = BuildMatchResultsResponseMessage(matchLobbyID, ratingUpdates);
await session.SendAsync(responseMsg);
```

## Disconnect Penalties

**Impact**:
- **Disconnected players**: Harsher rating loss (1.5x K-factor)
- **Team with disconnects**: Reduced rating loss
- **Leaver tracking**: Increment disconnect counter for repeat offenders

## Related Commands

- **match-start.md** (client/): Begin match
- **server-ready-for-match.md**: Server allocation
- **player-statistics-query.md**: Query updated ratings
