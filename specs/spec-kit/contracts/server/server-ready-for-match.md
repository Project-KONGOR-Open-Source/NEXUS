# Server Ready for Match

**Command**: NET_CHAT_SV_READY_FOR_MATCH
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Game Server → Chat Server (server port)
**Response**: Confirmation

## Purpose

Game server confirms it's ready to host a specific match after receiving allocation request.

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_READY_FOR_MATCH]
[int32: Server ID]
[int32: Match Lobby ID]
[string: Match Token]
```

**Fields**:
- **Server ID** (int32): This server's ID
- **Match Lobby ID** (int32): Match being hosted
- **Match Token** (string): Token for player authentication

**Example**:
```
Server ID: 42
Match Lobby ID: 12345
Match Token: "MATCH_12345_f7a3b9c2-4d1e-8f6a-9b2c-3e4f5a6b7c8d_1705123456"
```

## Response Message

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_READY_FOR_MATCH_RESPONSE]
[int8: Success = 1]
[int32: Match Lobby ID]
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
| **Valid Match Token** | Token must match expected value |
| **Server allocated** | Server must be assigned to this match |

## Error Cases

| Error | Reason |
|-------|--------|
| "Server not registered" | Server ID not found |
| "Match not found" | Match Lobby ID invalid |
| "Invalid match token" | Token doesn't match |
| "Server not allocated to this match" | Server not assigned to match |

## Processing Steps

1. Validate Server ID exists
2. Validate Match Lobby ID exists
3. Validate match token matches
4. Verify server is allocated to this match
5. Update match status to ServerReady
6. Store game server connection details in match lobby
7. Send confirmation to game server
8. Check if ready to send MATCH_START to players

## Server Allocation Flow

### 1. Chat Server Requests Allocation (via Manager)

```csharp
// Chat server sends allocation request to Manager
var request = new ServerAllocationRequest
{
    MatchLobbyID = matchLobby.MatchLobbyID,
    GameType = matchLobby.GameType,
    Region = matchLobby.Region
};

await ManagerClient.SendAsync(request);
```

### 2. Manager Selects Game Server

```csharp
// Manager picks best available server
var server = SelectBestServer(region, gameType);

// Manager sends allocation command to game server
var allocationCmd = new AllocateMatchCommand
{
    MatchLobbyID = matchLobby.MatchLobbyID,
    GameType = matchLobby.GameType,
    GameMode = matchLobby.GameMode,
    MatchToken = GenerateMatchToken(matchLobby.MatchLobbyID)
};

await GameServerClient.SendAsync(server, allocationCmd);
```

### 3. Game Server Prepares and Responds (This Command)

```csharp
// Game server receives allocation command
// - Initializes match state
// - Validates token
// - Sends READY_FOR_MATCH to chat server

var readyMsg = BuildReadyForMatchMessage(
    serverID,
    matchLobbyID,
    matchToken
);

await ChatServerClient.SendAsync(readyMsg);
```

### 4. Chat Server Receives Confirmation

```csharp
// Process this command
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

if (matchLobby.MatchToken != matchToken)
{
    SendError(session, "Invalid match token");
    return;
}

// Update match lobby
matchLobby.GameServerID = serverID;
matchLobby.GameServerIP = server.ServerIP;
matchLobby.GameServerPort = server.ServerPort;
matchLobby.ServerAllocated = true;
matchLobby.Status = MatchLobbyStatus.ServerReady;

// Check if can send MATCH_START
if (matchLobby.AllPlayersLoaded() && matchLobby.ServerAllocated)
{
    SendMatchStart(matchLobby);
}
```

## State Management

```csharp
public class MatchLobby
{
    public int? GameServerID;
    public string? GameServerIP;
    public int? GameServerPort;
    public string? MatchToken;
    public bool ServerAllocated = false;
}

// Process ready
lock (matchLobby.ServerAllocationLock)
{
    matchLobby.GameServerID = serverID;
    matchLobby.GameServerIP = server.ServerIP;
    matchLobby.GameServerPort = server.ServerPort;
    matchLobby.ServerAllocated = true;
    matchLobby.Status = MatchLobbyStatus.ServerReady;

    // Send confirmation
    var responseMsg = BuildReadyForMatchResponseMessage(matchLobbyID);
    await session.SendAsync(responseMsg);

    // Check if ready to start match
    if (matchLobby.AllPlayersLoaded())
    {
        await SendMatchStart(matchLobby);
    }
}
```

## Match Start Conditions

**Both conditions must be met**:
1. **All 10 players loaded** (LoadingStatus = true)
2. **Server ready** (SERVER_READY_FOR_MATCH received)

**Either can happen first**:
- Players load fast → wait for server
- Server ready fast → wait for players

## Timeout Handling

**Server Allocation Timeout**: 30 seconds

```csharp
// Set timeout when allocation requested
_ = Task.Delay(30000).ContinueWith(_ =>
{
    lock (matchLobby.ServerAllocationLock)
    {
        if (!matchLobby.ServerAllocated)
        {
            AbortMatch(matchLobby, "Server allocation timeout");
        }
    }
});
```

## Related Commands

- **server-handshake.md**: Server registration
- **server-allocation-request.md** (manager/): Request allocation
- **server-allocation-response.md** (manager/): Allocation response
- **match-start.md** (client/): Send players to server
