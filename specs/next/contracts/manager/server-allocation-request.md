# Server Allocation Request

**Command**: NET_CHAT_MGR_SERVER_ALLOCATION_REQUEST
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Chat Server → Manager (manager port)
**Response**: Server allocation response with IP/port/token

## Purpose

Chat server requests game server allocation for a match that has been accepted by all players.

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_MGR_SERVER_ALLOCATION_REQUEST]
[int32: Match Lobby ID]
[int8: Game Type]
[int8: Game Mode]
[int8: Region]
[int8: Player Count (10)]
[int32: Average Rating]
```

**Fields**:
- **Match Lobby ID** (int32): Unique match identifier
- **Game Type** (int8): Campaign Normal (6), Campaign Casual (7), Midwars (3), Riftwars (4), Public (0)
- **Game Mode** (int8): All Pick (0), Single Draft (3), All Random (6), etc.
- **Region** (int8): USE (0), USW (1), EU (2), RU (9), AU (11), BR (15)
- **Player Count** (int8): Always 10 for standard match
- **Average Rating** (int32): Average player rating (for server selection optimization)

**Example**:
```
Match Lobby ID: 12345
Game Type: 6 (Campaign Normal)
Game Mode: 0 (All Pick)
Region: 0 (USE)
Player Count: 10
Average Rating: 1650
```

## Response Message

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_MGR_SERVER_ALLOCATION_RESPONSE]
[int8: Success = 1]
[int32: Match Lobby ID]
[int32: Server ID]
[string: Server IP]
[int32: Server Port]
[string: Match Token]
[int64: Allocation Time (Unix epoch milliseconds)]
```

**Failure Response**:
```
[int8: Success = 0]
[int32: Match Lobby ID]
[string: Error Message]
```

## Server Selection Algorithm

**Priority Factors**:
1. **Region match**: Server must be in requested region
2. **Availability**: Server not at capacity (CurrentMatchCount < MaxConcurrentMatches)
3. **Load balancing**: Prefer servers with fewest current matches
4. **Performance**: Prefer servers with low CPU/memory usage

**Algorithm**:
```csharp
private GameServerSession? SelectBestServer(Region region, GameType gameType, int avgRating)
{
    // Get available servers in region
    var candidates = AvailableServersByRegion[region]
        .Where(s => s.Status == GameServerStatus.Available)
        .Where(s => s.CurrentMatchCount < s.MaxConcurrentMatches)
        .ToList();

    if (candidates.Count == 0)
        return null; // No servers available

    // Score each server
    var scored = candidates.Select(s => new
    {
        Server = s,
        Score = CalculateServerScore(s)
    });

    // Select server with highest score
    return scored.OrderByDescending(s => s.Score).First().Server;
}

private float CalculateServerScore(GameServerSession server)
{
    float score = 100.0f;

    // Penalize for current load
    float loadPenalty = (server.CurrentMatchCount / (float)server.MaxConcurrentMatches) * 50.0f;
    score -= loadPenalty;

    // Penalize for CPU usage
    score -= (server.CPUUsage / 100.0f) * 20.0f;

    // Penalize for memory usage
    score -= (server.MemoryUsage / 100.0f) * 10.0f;

    return score;
}
```

## Match Token Generation

**Purpose**: Authenticate players when they connect to game server

**Format**:
```
MATCH_{MatchLobbyID}_{RandomGUID}_{UnixTimestamp}
```

**Example**:
```
MATCH_12345_f7a3b9c2-4d1e-8f6a-9b2c-3e4f5a6b7c8d_1705123456
```

**Generation**:
```csharp
private string GenerateMatchToken(int matchLobbyID)
{
    string guid = Guid.NewGuid().ToString();
    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    return $"MATCH_{matchLobbyID}_{guid}_{timestamp}";
}
```

**Validation**:
- Token expires after 2 minutes (120 seconds)
- Single-use (cannot reuse after match starts)
- Validated by game server against Manager

## Error Cases

| Error | Reason |
|-------|--------|
| "No servers available" | All servers in region at capacity |
| "Region not supported" | Invalid region |
| "Invalid match lobby ID" | Match ID not found |

## Processing Steps (Manager Side)

1. Validate Match Lobby ID
2. Validate region is supported
3. Find available servers in region
4. Select best server using algorithm
5. Generate match token
6. Send allocation command to selected game server
7. Wait for game server acknowledgement (5 second timeout)
8. Send allocation response to chat server
9. Log allocation for telemetry

## Game Server Allocation Command

**Manager → Game Server**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_ALLOCATE_MATCH]
[int32: Match Lobby ID]
[int8: Game Type]
[int8: Game Mode]
[string: Match Token]
```

**Game Server → Manager** (acknowledgement):
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_ALLOCATE_MATCH_ACK]
[int8: Success = 1]
[int32: Match Lobby ID]
```

**Then Game Server → Chat Server**:
```
NET_CHAT_SV_READY_FOR_MATCH (see server-ready-for-match.md)
```

## State Management

```csharp
// Select server
var server = SelectBestServer(region, gameType, avgRating);

if (server == null)
{
    // No servers available
    SendAllocationFailure(chatServer, matchLobbyID, "No servers available");
    return;
}

// Generate token
string matchToken = GenerateMatchToken(matchLobbyID);

// Send allocation command to game server
var allocateCmd = BuildAllocateMatchCommand(matchLobbyID, gameType, gameMode, matchToken);
await server.Connection.SendAsync(allocateCmd);

// Wait for game server acknowledgement (5 second timeout)
bool ack = await WaitForGameServerAck(matchLobbyID, timeout: 5000);

if (!ack)
{
    // Game server didn't respond, try another server
    SendAllocationFailure(chatServer, matchLobbyID, "Server allocation timeout");
    return;
}

// Send success response to chat server
var response = BuildServerAllocationResponse(
    matchLobbyID,
    server.ServerID,
    server.ServerIP,
    server.ServerPort,
    matchToken
);

await chatServer.Connection.SendAsync(response);

// Update server state
server.CurrentMatchCount++;
if (server.CurrentMatchCount >= server.MaxConcurrentMatches)
{
    server.Status = GameServerStatus.Full;
    AvailableServersByRegion[region].Remove(server);
}
```

## Timeout Handling

**Manager → Game Server Timeout**: 5 seconds
- If game server doesn't acknowledge allocation, try next best server
- Maximum 3 retry attempts before returning failure

**Chat Server Timeout**: 30 seconds
- If Manager doesn't respond, abort match
- Notify players of allocation failure

## Performance Targets

- **Allocation time**: <5 seconds (P95)
- **Success rate**: >99.5%
- **Retry rate**: <1%

## Telemetry

**Metrics to Track**:
- Allocation request volume per region
- Average allocation time
- Failure rate by region
- Server selection distribution (load balancing effectiveness)
- Retry frequency

## Related Commands

- **server-allocation-response.md**: Manager's response
- **server-ready-for-match.md** (server/): Game server confirmation
- **match-start.md** (client/): Send players to server
- **manager-handshake.md**: Manager connection setup
