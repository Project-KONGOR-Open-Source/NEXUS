# Server Handshake

**Command**: NET_CHAT_SV_HANDSHAKE
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Game Server → Chat Server (server port)
**Response**: Authentication confirmation

## Purpose

Game servers authenticate with chat server on startup to register availability for match hosting.

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_HANDSHAKE]
[string: Server Token]
[string: Server IP]
[int32: Server Port]
[int8: Region]
[int32: Max Concurrent Matches]
```

**Fields**:
- **Server Token** (string): Pre-shared authentication token
- **Server IP** (string): Public IP address for client connections
- **Server Port** (int32): Port for game clients (e.g., 11235)
- **Region** (int8): Server region (USE=0, USW=1, EU=2, RU=9, AU=11, BR=15)
- **Max Concurrent Matches** (int32): Maximum simultaneous matches (typically 1-10)

**Example**:
```
Server Token: "GAME_SERVER_SECRET_TOKEN_12345"
Server IP: "192.168.1.100"
Server Port: 11235
Region: 0 (USE)
Max Concurrent Matches: 5
```

## Response Message

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_HANDSHAKE_RESPONSE]
[int8: Success = 1]
[int32: Server ID]
[int64: Handshake Time (Unix epoch milliseconds)]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Valid token** | Server token must match expected value |
| **Valid IP** | IP address format validation |
| **Valid port** | Port in range 1024-65535 |
| **Valid region** | Region must be in supported list |
| **No duplicate** | Server IP:Port combination unique |

## Error Cases

| Error | Reason |
|-------|--------|
| "Invalid token" | Server token doesn't match |
| "Invalid IP address" | Malformed IP |
| "Invalid port" | Port out of range |
| "Invalid region" | Region not supported |
| "Server already registered" | IP:Port already in use |

## Processing Steps

1. Validate server token matches expected value
2. Validate IP address format
3. Validate port range (1024-65535)
4. Validate region is supported
5. Check IP:Port combination not already registered
6. Generate unique Server ID
7. Create GameServerSession in memory
8. Add to available server pool for region
9. Send success response with Server ID
10. Start heartbeat monitoring

## State Management

```csharp
public class GameServerSession
{
    public int ServerID;
    public string ServerIP;
    public int ServerPort;
    public Region Region;
    public int MaxConcurrentMatches;
    public int CurrentMatchCount = 0;
    public DateTime ConnectedAt;
    public DateTime LastHeartbeat;
    public GameServerStatus Status = GameServerStatus.Available;
}

// Register server
var serverSession = new GameServerSession
{
    ServerID = Interlocked.Increment(ref nextServerID),
    ServerIP = serverIP,
    ServerPort = serverPort,
    Region = region,
    MaxConcurrentMatches = maxConcurrentMatches,
    ConnectedAt = DateTime.UtcNow,
    LastHeartbeat = DateTime.UtcNow
};

RegisteredGameServers.TryAdd(serverSession.ServerID, serverSession);
AvailableServersByRegion[region].Add(serverSession);
```

## Heartbeat Monitoring

**Heartbeat Interval**: 10 seconds

**Timeout**: 30 seconds (3 missed heartbeats)

```csharp
// Background task monitors heartbeats
private async Task MonitorServerHeartbeats()
{
    while (true)
    {
        var now = DateTime.UtcNow;

        foreach (var server in RegisteredGameServers.Values)
        {
            if ((now - server.LastHeartbeat).TotalSeconds > 30)
            {
                // Server timed out
                RemoveGameServer(server.ServerID, "Heartbeat timeout");
            }
        }

        await Task.Delay(10000); // Check every 10 seconds
    }
}
```

## Related Commands

- **server-heartbeat.md**: Keep-alive messages
- **server-ready-for-match.md**: Signal match readiness
- **server-allocation-request.md** (manager/): Allocate server for match
