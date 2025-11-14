# Server Heartbeat

**Command**: NET_CHAT_SV_HEARTBEAT
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Game Server → Chat Server (server port)
**Response**: Acknowledgement

## Purpose

Game servers send periodic heartbeats to indicate they're alive and report current load.

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_HEARTBEAT]
[int32: Server ID]
[int32: Current Match Count]
[float: CPU Usage Percent]
[float: Memory Usage Percent]
```

**Fields**:
- **Server ID** (int32): ID from handshake response
- **Current Match Count** (int32): Active matches on this server
- **CPU Usage Percent** (float): CPU utilization (0.0-100.0)
- **Memory Usage Percent** (float): Memory utilization (0.0-100.0)

**Example**:
```
Server ID: 42
Current Match Count: 3
CPU Usage: 45.2%
Memory Usage: 62.8%
```

## Response Message

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_SV_HEARTBEAT_ACK]
[int8: Success = 1]
[int64: Server Time (Unix epoch milliseconds)]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Heartbeat Interval

**Frequency**: Every 10 seconds

**Timeout**: 30 seconds (3 missed heartbeats = dead server)

## Error Cases

| Error | Reason |
|-------|--------|
| "Server not registered" | Server ID not found |
| "Server disconnected" | Server marked as offline |

## Processing Steps

1. Validate Server ID exists in RegisteredGameServers
2. Update server's LastHeartbeat timestamp
3. Update server's CurrentMatchCount
4. Update server's CPU/Memory metrics
5. Update server availability status (Available if <MaxConcurrentMatches)
6. Send acknowledgement with current server time (for clock sync)

## State Management

```csharp
if (!RegisteredGameServers.TryGetValue(serverID, out var server))
{
    SendError(session, "Server not registered");
    return;
}

// Update heartbeat
server.LastHeartbeat = DateTime.UtcNow;
server.CurrentMatchCount = currentMatchCount;
server.CPUUsage = cpuUsage;
server.MemoryUsage = memoryUsage;

// Update availability
if (currentMatchCount >= server.MaxConcurrentMatches)
{
    server.Status = GameServerStatus.Full;
    AvailableServersByRegion[server.Region].Remove(server);
}
else if (server.Status == GameServerStatus.Full)
{
    server.Status = GameServerStatus.Available;
    AvailableServersByRegion[server.Region].Add(server);
}

// Send acknowledgement
var ackMsg = BuildHeartbeatAckMessage(DateTime.UtcNow);
await session.SendAsync(ackMsg);
```

## Server Status Enum

```csharp
public enum GameServerStatus
{
    Available = 0,    // Accepting new matches
    Full = 1,         // At capacity
    Draining = 2,     // Finishing matches, no new allocations
    Offline = 3       // Disconnected or timed out
}
```

## Heartbeat Timeout Handling

**Background Monitor**:
```csharp
private async Task MonitorServerHeartbeats()
{
    while (true)
    {
        var now = DateTime.UtcNow;

        foreach (var server in RegisteredGameServers.Values.ToList())
        {
            var timeSinceHeartbeat = (now - server.LastHeartbeat).TotalSeconds;

            if (timeSinceHeartbeat > 30)
            {
                // Server timed out
                Logger.LogWarning($"Game server {server.ServerID} timed out (last heartbeat {timeSinceHeartbeat:F1}s ago)");

                server.Status = GameServerStatus.Offline;
                RegisteredGameServers.TryRemove(server.ServerID, out _);
                AvailableServersByRegion[server.Region].Remove(server);

                // Abort any matches being allocated to this server
                AbortMatchesForServer(server.ServerID);
            }
        }

        await Task.Delay(10000); // Check every 10 seconds
    }
}
```

## Telemetry

**Metrics to Track**:
- Server uptime distribution
- Average CPU/memory usage per region
- Heartbeat latency (measure round-trip time)
- Timeout frequency
- Match capacity utilization

## Clock Synchronization

**Purpose**: Server time in response allows game servers to sync clocks

```csharp
// Game server side
long serverTime = response.ServerTime;
long localTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
long clockOffset = serverTime - localTime;

// Adjust for network latency (half of round-trip time)
clockOffset -= (roundTripTime / 2);
```

## Related Commands

- **server-handshake.md**: Initial server registration
- **server-ready-for-match.md**: Signal match readiness
- **server-allocation-request.md** (manager/): Allocate server for match
