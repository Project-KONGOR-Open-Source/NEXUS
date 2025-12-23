# Server Allocation Response

**Command**: NET_CHAT_MGR_SERVER_ALLOCATION_RESPONSE
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Manager → Chat Server (manager port)
**Response**: None (this is the response)

## Purpose

Manager responds to chat server's server allocation request with game server connection details.

## Message Structure

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

**Fields**:
- **Success** (int8): 1 for success, 0 for failure
- **Match Lobby ID** (int32): Match identifier from request
- **Server ID** (int32): Allocated game server's ID
- **Server IP** (string): IP address for clients to connect (e.g., "192.168.1.100")
- **Server Port** (int32): Port for clients (e.g., 11235)
- **Match Token** (string): Authentication token for players
- **Allocation Time** (int64): When allocation occurred

**Failure Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_MGR_SERVER_ALLOCATION_RESPONSE]
[int8: Success = 0]
[int32: Match Lobby ID]
[string: Error Message]
```

## Error Messages

| Error | Meaning |
|-------|---------|
| "No servers available" | All servers in region at capacity |
| "Server allocation timeout" | Game server didn't acknowledge |
| "Region not supported" | Invalid region specified |
| "Internal error" | Unexpected Manager error |

## Processing Steps (Chat Server Side)

1. Receive allocation response from Manager
2. Validate Match Lobby ID matches pending request
3. If success:
   - Store server IP, port, token in MatchLobby
   - Mark MatchLobby.ServerAllocated = true
   - Check if all players loaded (if so, send MATCH_START)
   - If not, wait for players to finish loading
4. If failure:
   - Abort match
   - Notify all players of failure
   - Return players to queue or group

## Success Flow

```csharp
// Receive allocation response
public async Task OnServerAllocationResponse(ServerAllocationResponse response)
{
    if (!ActiveMatchLobbies.TryGetValue(response.MatchLobbyID, out var matchLobby))
    {
        Logger.LogWarning($"Received allocation response for unknown match {response.MatchLobbyID}");
        return;
    }

    if (response.Success)
    {
        // Store server details
        matchLobby.GameServerID = response.ServerID;
        matchLobby.GameServerIP = response.ServerIP;
        matchLobby.GameServerPort = response.ServerPort;
        matchLobby.MatchToken = response.MatchToken;
        matchLobby.ServerAllocated = true;
        matchLobby.Status = MatchLobbyStatus.ServerAllocated;

        Logger.LogInformation($"Match {response.MatchLobbyID} allocated to server {response.ServerID} at {response.ServerIP}:{response.ServerPort}");

        // Check if all players already loaded
        if (matchLobby.AllPlayersLoaded())
        {
            await SendMatchStart(matchLobby);
        }
        // Otherwise wait for PLAYER_LOADING_STATUS messages
    }
    else
    {
        // Allocation failed
        Logger.LogError($"Server allocation failed for match {response.MatchLobbyID}: {response.ErrorMessage}");

        await AbortMatch(matchLobby, $"Server allocation failed: {response.ErrorMessage}");
    }
}
```

## Failure Flow

```csharp
private async Task AbortMatch(MatchLobby matchLobby, string reason)
{
    matchLobby.Status = MatchLobbyStatus.Aborted;

    // Notify all players
    var abortMsg = BuildMatchAbortedMessage(matchLobby.MatchLobbyID, reason);

    foreach (var group in matchLobby.GetAllGroups())
    {
        foreach (var member in group.Members.Values)
        {
            await member.Session.SendAsync(abortMsg);
        }

        // Return group to queue or leave queue
        group.QueueStatus = QueueStatus.NotQueued;

        // Optionally re-queue automatically
        // await ReQueueGroup(group);
    }

    // Remove match lobby
    ActiveMatchLobbies.TryRemove(matchLobby.MatchLobbyID, out _);

    Logger.LogInformation($"Match {matchLobby.MatchLobbyID} aborted: {reason}");
}
```

## Match Token Validation

**Game Server validates token when players connect**:

```csharp
// Game server receives player connection
public async Task OnPlayerConnectToMatch(int accountID, string providedToken)
{
    // Validate token
    if (!ActiveMatchTokens.TryGetValue(providedToken, out int matchLobbyID))
    {
        DisconnectPlayer(accountID, "Invalid match token");
        return;
    }

    // Check token not expired (2 minutes)
    if (IsTokenExpired(providedToken))
    {
        DisconnectPlayer(accountID, "Match token expired");
        return;
    }

    // Check player is part of this match
    if (!IsPlayerInMatch(matchLobbyID, accountID))
    {
        DisconnectPlayer(accountID, "Not part of this match");
        return;
    }

    // Accept player connection
    AcceptPlayerConnection(matchLobbyID, accountID);
}
```

## Timing Considerations

**Race Conditions**:

**Scenario 1**: Players finish loading before server allocated
```
1. All players send PLAYER_LOADING_STATUS (true)
2. Chat server waits for server allocation
3. Allocation response arrives
4. Immediately send MATCH_START
```

**Scenario 2**: Server allocated before players finish loading
```
1. Allocation response arrives
2. Chat server waits for players to load
3. 10th player sends PLAYER_LOADING_STATUS (true)
4. Immediately send MATCH_START
```

**Both handled by checking both conditions**:
```csharp
// After allocation response
if (matchLobby.AllPlayersLoaded() && matchLobby.ServerAllocated)
{
    await SendMatchStart(matchLobby);
}

// After player loading status update
if (matchLobby.AllPlayersLoaded() && matchLobby.ServerAllocated)
{
    await SendMatchStart(matchLobby);
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
    public MatchLobbyStatus Status;
}

// Process response
lock (matchLobby.ServerAllocationLock)
{
    if (response.Success)
    {
        matchLobby.GameServerID = response.ServerID;
        matchLobby.GameServerIP = response.ServerIP;
        matchLobby.GameServerPort = response.ServerPort;
        matchLobby.MatchToken = response.MatchToken;
        matchLobby.ServerAllocated = true;
        matchLobby.Status = MatchLobbyStatus.ServerAllocated;

        // Check if ready to start
        if (matchLobby.AllPlayersLoaded())
        {
            await SendMatchStart(matchLobby);
        }
    }
    else
    {
        await AbortMatch(matchLobby, response.ErrorMessage);
    }
}
```

## Performance Targets

- **Response time**: <5 seconds (P95)
- **Success rate**: >99.5%
- **Timeout rate**: <0.1%

## Telemetry

**Metrics to Track**:
- Response time distribution (P50, P95, P99)
- Success vs failure rate
- Failure reasons distribution
- Time from allocation to MATCH_START

## Related Commands

- **server-allocation-request.md**: Chat server's request
- **server-ready-for-match.md** (server/): Game server confirmation
- **match-start.md** (client/): Send players to server
- **player-loading-status.md** (client/): Player readiness
