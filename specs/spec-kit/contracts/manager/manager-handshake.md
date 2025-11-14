# Manager Handshake

**Command**: NET_CHAT_MGR_HANDSHAKE
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Chat Server ↔ Manager (manager port)
**Response**: Authentication confirmation

## Purpose

Chat server and Manager port authenticate on startup to establish server allocation communication.

## Request Message (Chat Server → Manager)

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_MGR_HANDSHAKE]
[string: Service Token]
[string: Chat Server ID]
[int8: Supported Region Count]
[for each region:]
    [int8: Region]
```

**Fields**:
- **Service Token** (string): Pre-shared authentication token
- **Chat Server ID** (string): Unique identifier for this chat server instance
- **Supported Region Count** (int8): Number of regions chat server handles
- **Region List**: Regions this chat server manages

**Example**:
```
Service Token: "CHAT_MANAGER_SECRET_TOKEN_67890"
Chat Server ID: "chat-server-01"
Supported Regions: 3 (USE, USW, EU)
```

## Response Message (Manager → Chat Server)

**Success Response**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_MGR_HANDSHAKE_RESPONSE]
[int8: Success = 1]
[int64: Handshake Time (Unix epoch milliseconds)]
[int32: Available Game Server Count]
[for each available server:]
    [int32: Server ID]
    [string: Server IP]
    [int32: Server Port]
    [int8: Region]
    [int32: Max Concurrent Matches]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Valid token** | Service token must match expected value |
| **Valid Chat Server ID** | ID format validation |
| **Valid regions** | All regions must be supported |
| **No duplicate Chat Server ID** | Unique per instance |

## Error Cases

| Error | Reason |
|-------|--------|
| "Invalid token" | Service token doesn't match |
| "Invalid Chat Server ID" | Malformed ID |
| "Invalid region" | Region not supported |
| "Chat Server already registered" | Duplicate ID |

## Processing Steps (Manager Side)

1. Validate service token
2. Validate Chat Server ID format
3. Validate all regions are supported
4. Check Chat Server ID not already registered
5. Create ChatServerConnection in memory
6. Send current list of available game servers
7. Send success response
8. Start heartbeat monitoring

## Processing Steps (Chat Server Side)

1. Connect to Manager port on startup
2. Send handshake with authentication
3. Receive list of available game servers
4. Register game servers in local AvailableServersByRegion
5. Ready to process server allocation requests

## State Management (Manager)

```csharp
public class ChatServerConnection
{
    public string ChatServerID;
    public List<Region> SupportedRegions;
    public DateTime ConnectedAt;
    public DateTime LastHeartbeat;
    public TcpConnection Connection;
}

// Register chat server
var chatServerConn = new ChatServerConnection
{
    ChatServerID = chatServerID,
    SupportedRegions = regions,
    ConnectedAt = DateTime.UtcNow,
    LastHeartbeat = DateTime.UtcNow,
    Connection = connection
};

RegisteredChatServers.TryAdd(chatServerID, chatServerConn);
```

## State Management (Chat Server)

```csharp
// Receive game server list from Manager
foreach (var serverInfo in response.AvailableServers)
{
    var serverSession = new GameServerSession
    {
        ServerID = serverInfo.ServerID,
        ServerIP = serverInfo.ServerIP,
        ServerPort = serverInfo.ServerPort,
        Region = serverInfo.Region,
        MaxConcurrentMatches = serverInfo.MaxConcurrentMatches,
        Status = GameServerStatus.Available
    };

    RegisteredGameServers.TryAdd(serverSession.ServerID, serverSession);
    AvailableServersByRegion[serverSession.Region].Add(serverSession);
}
```

## Heartbeat Monitoring

**Heartbeat Interval**: 30 seconds (less frequent than game server heartbeats)

**Timeout**: 90 seconds (3 missed heartbeats)

```csharp
// Background task monitors chat server heartbeats
private async Task MonitorChatServerHeartbeats()
{
    while (true)
    {
        var now = DateTime.UtcNow;

        foreach (var chatServer in RegisteredChatServers.Values.ToList())
        {
            if ((now - chatServer.LastHeartbeat).TotalSeconds > 90)
            {
                // Chat server timed out
                Logger.LogWarning($"Chat server {chatServer.ChatServerID} timed out");

                RegisteredChatServers.TryRemove(chatServer.ChatServerID, out _);
                chatServer.Connection.Disconnect();
            }
        }

        await Task.Delay(30000); // Check every 30 seconds
    }
}
```

## Startup Sequence

**1. Manager starts first**:
```
Manager listens on manager port (e.g., 11031)
Waits for chat servers and game servers to connect
```

**2. Game servers connect to Manager**:
```
Game Server → Manager: SERVER_HANDSHAKE
Manager registers game server
```

**3. Chat server connects to Manager**:
```
Chat Server → Manager: MGR_HANDSHAKE
Manager responds with list of available game servers
Chat server now ready to allocate servers
```

**4. Normal operation**:
```
Chat Server ↔ Manager: Server allocation requests/responses
Game Server ↔ Manager: Heartbeats, status updates
```

## High Availability Considerations

**Multiple Chat Servers**:
- Each chat server has unique ID
- Manager routes allocation requests to appropriate region
- Chat servers can share regions (load balancing)

**Manager Redundancy**:
- Primary/secondary Manager instances
- Chat servers reconnect to secondary on primary failure
- Game server registrations replicated across Managers

## Related Commands

- **manager-heartbeat.md**: Keep-alive for Manager connection
- **server-allocation-request.md**: Request game server
- **server-allocation-response.md**: Receive server details
- **server-handshake.md** (server/): Game server registration
