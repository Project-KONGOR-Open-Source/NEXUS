# TRANSMUTANSTEIN Chat Server - Developer Quickstart

**Date**: 2025-01-13
**Feature**: Chat Server Implementation
**Target Framework**: .NET 10

## Overview

This guide helps developers get started implementing the TRANSMUTANSTEIN chat server for NEXUS. The chat server provides channels, matchmaking groups, queue management, and player communication for the game.

## Prerequisites

### Required Software

- **.NET 10 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Visual Studio 2025** or **VS Code** with C# extension
- **SQL Server** (LocalDB, Express, or full version)
- **Git**: For version control

### Verify Installation

```powershell
# Check .NET version
dotnet --version  # Should show 10.x.x

# Check SQL Server
sqlcmd -S (localdb)\MSSQLLocalDB -Q "SELECT @@VERSION"
```

## Project Structure

```
NEXUS/
├── TRANSMUTANSTEIN.ChatServer/
│   ├── Core/
│   │   ├── ChatServer.cs          # Main server class
│   │   ├── ChatSession.cs         # Per-client connection
│   │   ├── ChatProtocol.cs        # Command codes and constants
│   │   └── ChatBuffer.cs          # Binary message serialisation
│   ├── CommandProcessors/
│   │   ├── Channels/
│   │   │   ├── JoinChannelProcessor.cs
│   │   │   ├── LeaveChannelProcessor.cs
│   │   │   └── ChannelMessageProcessor.cs
│   │   ├── Groups/
│   │   │   ├── GroupCreateProcessor.cs
│   │   │   ├── GroupInviteProcessor.cs
│   │   │   └── GroupJoinProcessor.cs
│   │   └── Matchmaking/
│   │       ├── GroupJoinQueueProcessor.cs
│   │       ├── MatchAcceptProcessor.cs
│   │       └── PlayerLoadingStatusProcessor.cs
│   ├── InMemory/
│   │   ├── ChatChannel.cs         # In-memory channel state
│   │   ├── MatchmakingGroup.cs    # In-memory group state
│   │   └── MatchLobby.cs          # In-memory match lobby state
│   ├── Database/
│   │   └── (Uses MERRICK.DatabaseContext from shared project)
│   └── Program.cs                 # Aspire orchestration entry point
├── specs/
│   └── tcp-server/
│       ├── spec.md                # Feature specification
│       ├── plan.md                # Implementation plan
│       ├── research.md            # Protocol research
│       ├── data-model.md          # Entity definitions
│       ├── contracts/             # Protocol contracts
│       └── quickstart.md          # This file
```

## Quick Start

### 1. Clone and Setup

```powershell
# Navigate to repo
cd C:\Users\SADS-810\Source\NEXUS

# Ensure you're on tcp-server branch
git checkout tcp-server

# Restore dependencies
dotnet restore

# Verify build
dotnet build
```

### 2. Database Setup

The chat server uses **MERRICK.DatabaseContext** from the shared database project for persistent entities.

```powershell
# Apply migrations
cd MERRICK
dotnet ef database update

# Verify tables exist
sqlcmd -S (localdb)\MSSQLLocalDB -d NEXUS -Q "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('PlayerStatistics', 'FriendedPeer')"
```

### 3. Run Chat Server

```powershell
# Run using Aspire orchestration
cd TRANSMUTANSTEIN.ChatServer
dotnet run
```

**Expected Output**:
```
info: TRANSMUTANSTEIN.ChatServer.Core.ChatServer[0]
      Chat Server listening on port 11031 (client)
info: TRANSMUTANSTEIN.ChatServer.Core.ChatServer[0]
      Chat Server listening on port 11032 (game server)
info: TRANSMUTANSTEIN.ChatServer.Core.ChatServer[0]
      Chat Server listening on port 11033 (manager)
```

### 4. Test Connection

Use a TCP client to verify connectivity:

```powershell
# Test using telnet or custom client
telnet localhost 11031
```

## Architecture Patterns

### 1. Triple TCP Listener

The chat server runs **three separate TCP listeners** on different ports:

```csharp
public class ChatServer
{
    private TcpListener clientListener;    // Port 11031 - Game clients
    private TcpListener serverListener;    // Port 11032 - Game servers
    private TcpListener managerListener;   // Port 11033 - Manager port

    public async Task StartAsync()
    {
        clientListener = new TcpListener(IPAddress.Any, 11031);
        serverListener = new TcpListener(IPAddress.Any, 11032);
        managerListener = new TcpListener(IPAddress.Any, 11033);

        clientListener.Start();
        serverListener.Start();
        managerListener.Start();

        _ = Task.Run(() => AcceptClientsAsync(clientListener, ClientSessionType.GameClient));
        _ = Task.Run(() => AcceptClientsAsync(serverListener, ClientSessionType.GameServer));
        _ = Task.Run(() => AcceptClientsAsync(managerListener, ClientSessionType.Manager));
    }
}
```

### 2. Command Processor Pattern

**ASP.NET Core-inspired attribute routing** for TCP commands:

```csharp
// Define processor with attribute
[ChatCommand(ChatProtocol.NET_CHAT_CL_JOIN_CHANNEL)]
public class JoinChannelProcessor : ICommandProcessor
{
    private readonly ILogger<JoinChannelProcessor> logger;
    private readonly DatabaseContext db;

    public JoinChannelProcessor(ILogger<JoinChannelProcessor> logger, DatabaseContext db)
    {
        this.logger = logger;
        this.db = db;
    }

    public async Task ProcessAsync(ChatSession session, ChatBuffer buffer)
    {
        // Read message fields
        string channelName = buffer.ReadString();
        string password = buffer.ReadString();

        // Validate input
        if (string.IsNullOrWhiteSpace(channelName))
        {
            await session.SendErrorAsync("Invalid channel name");
            return;
        }

        // Process command
        var channel = GetOrCreateChannel(channelName);

        // Validate password
        if (!string.IsNullOrEmpty(channel.Password) && channel.Password != password)
        {
            await session.SendErrorAsync("Incorrect password");
            return;
        }

        // Add member
        channel.AddMember(session.AccountID, session, AdminLevel.None);

        // Send response
        var response = BuildJoinChannelResponse(channel);
        await session.SendAsync(response);

        // Broadcast join to existing members
        channel.BroadcastMessage(BuildUserJoinedMessage(session), excludeAccountID: session.AccountID);
    }
}
```

**Register processors in DI**:

```csharp
// Program.cs
services.AddSingleton<CommandProcessorRegistry>();

// Register all processors
services.AddSingleton<JoinChannelProcessor>();
services.AddSingleton<LeaveChannelProcessor>();
services.AddSingleton<ChannelMessageProcessor>();
// ... etc

// Build registry
var registry = serviceProvider.GetRequiredService<CommandProcessorRegistry>();
registry.ScanAndRegister(typeof(Program).Assembly);
```

**Dispatch commands**:

```csharp
public class ChatSession
{
    public async Task ProcessMessageAsync(ChatBuffer buffer)
    {
        ushort commandCode = buffer.ReadUInt16();

        var processor = commandProcessorRegistry.GetProcessor(commandCode);

        if (processor == null)
        {
            logger.LogWarning($"Unknown command code: 0x{commandCode:X4}");
            return;
        }

        await processor.ProcessAsync(this, buffer);
    }
}
```

### 3. In-Memory State Management

**Thread-safe in-memory collections** for transient state:

```csharp
public class ChatServer
{
    // Active channels (not persisted)
    private ConcurrentDictionary<string, ChatChannel> ActiveChannels = new();

    // Active matchmaking groups (not persisted)
    private ConcurrentDictionary<int, MatchmakingGroup> ActiveGroups = new();

    // Active match lobbies (not persisted)
    private ConcurrentDictionary<int, MatchLobby> ActiveMatchLobbies = new();

    // Active sessions
    private ConcurrentDictionary<int, ChatSession> ActiveSessions = new();

    // Registered game servers
    private ConcurrentDictionary<int, GameServerSession> RegisteredGameServers = new();
}
```

**Channel management example**:

```csharp
public ChatChannel GetOrCreateChannel(string channelName)
{
    return ActiveChannels.GetOrAdd(channelName, name => new ChatChannel
    {
        ChannelID = Interlocked.Increment(ref nextChannelID),
        Name = name,
        Flags = ChannelFlags.None,
        Members = new ConcurrentDictionary<int, ChatChannelMember>()
    });
}
```

### 4. Message Serialisation

**Binary protocol** with 2-byte length + 2-byte command + payload:

```csharp
public class ChatBuffer
{
    private readonly MemoryStream stream;
    private readonly BinaryWriter writer;
    private readonly BinaryReader reader;

    // Writing
    public void WriteString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((ushort)bytes.Length);  // 2-byte length prefix
        writer.Write(bytes);
    }

    public void WriteInt32(int value) => writer.Write(value);
    public void WriteInt8(byte value) => writer.Write(value);
    public void WriteFloat(float value) => writer.Write(value);

    // Reading
    public string ReadString()
    {
        ushort length = reader.ReadUInt16();
        byte[] bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    public int ReadInt32() => reader.ReadInt32();
    public byte ReadInt8() => reader.ReadByte();
    public float ReadFloat() => reader.ReadSingle();

    // Finalise message with length header
    public byte[] ToArray()
    {
        var data = stream.ToArray();
        var finalBuffer = new byte[data.Length + 2];

        // Write length (excludes length field itself)
        BitConverter.GetBytes((ushort)data.Length).CopyTo(finalBuffer, 0);

        // Copy payload
        data.CopyTo(finalBuffer, 2);

        return finalBuffer;
    }
}
```

**Usage example**:

```csharp
var buffer = new ChatBuffer();
buffer.WriteUInt16(ChatProtocol.NET_CHAT_CL_WHISPER_RECEIVED);
buffer.WriteInt32(senderAccountID);
buffer.WriteString(senderAccountName);
buffer.WriteString(messageText);
buffer.WriteInt64(timestamp);

await session.SendAsync(buffer.ToArray());
```

## Common Implementation Patterns

### Pattern 1: Authentication

```csharp
[ChatCommand(ChatProtocol.NET_CHAT_CL_AUTHENTICATE)]
public class AuthenticateProcessor : ICommandProcessor
{
    public async Task ProcessAsync(ChatSession session, ChatBuffer buffer)
    {
        string username = buffer.ReadString();
        string sessionToken = buffer.ReadString();

        // Validate token (from master server)
        var account = await ValidateSessionToken(username, sessionToken);

        if (account == null)
        {
            await session.SendErrorAsync("Authentication failed");
            await session.DisconnectAsync();
            return;
        }

        // Check for existing connection (allow only one per account)
        if (ActiveSessions.TryGetValue(account.AccountID, out var existingSession))
        {
            await existingSession.DisconnectAsync("New connection from same account");
        }

        // Set session properties
        session.AccountID = account.AccountID;
        session.AccountName = account.AccountName;
        session.Authenticated = true;

        // Add to active sessions
        ActiveSessions.TryAdd(account.AccountID, session);

        // Send success response
        var response = new ChatBuffer();
        response.WriteUInt16(ChatProtocol.NET_CHAT_CL_AUTHENTICATE_RESPONSE);
        response.WriteInt8(1); // Success
        response.WriteInt32(account.AccountID);
        response.WriteString(account.AccountName);

        await session.SendAsync(response.ToArray());

        // Notify friends of online status
        await BroadcastFriendOnlineStatus(session, isOnline: true);
    }
}
```

### Pattern 2: Channel Access Control

```csharp
public bool CanAccessChannel(ChatSession session, ChatChannel channel)
{
    // Clan channels require clan membership
    if (channel.Flags.HasFlag(ChannelFlags.Clan) && channel.ClanID.HasValue)
    {
        // Query database for clan membership
        bool isMember = db.ClanMembers
            .Any(cm => cm.AccountID == session.AccountID && cm.ClanID == channel.ClanID);

        if (!isMember)
            return false;
    }

    // Check ban list
    if (channel.BannedUsers.ContainsKey(session.AccountID))
    {
        var bannedUntil = channel.BannedUsers[session.AccountID];

        if (bannedUntil > DateTime.UtcNow || bannedUntil == DateTime.MaxValue)
            return false; // Still banned

        // Ban expired, remove
        channel.BannedUsers.TryRemove(session.AccountID, out _);
    }

    return true;
}
```

### Pattern 3: Broadcasting Messages

```csharp
public class ChatChannel
{
    public void BroadcastMessage(byte[] message, int? excludeAccountID = null)
    {
        var tasks = new List<Task>();

        foreach (var member in Members.Values)
        {
            if (member.AccountID == excludeAccountID)
                continue; // Skip excluded user

            tasks.Add(member.Session.SendAsync(message));
        }

        // Await all sends concurrently
        Task.WaitAll(tasks.ToArray());
    }
}
```

### Pattern 4: Disconnect Cleanup

```csharp
public class ChatSession
{
    public async Task OnDisconnectAsync()
    {
        if (!Authenticated)
            return;

        // Remove from all channels
        foreach (var channel in ActiveChannels.Values.Where(c => c.Members.ContainsKey(AccountID)))
        {
            channel.Members.TryRemove(AccountID, out _);

            // Broadcast departure
            channel.BroadcastMessage(BuildUserLeftMessage(AccountID, AccountName));

            // Cleanup empty non-permanent channels
            if (channel.Members.IsEmpty && !channel.Flags.HasFlag(ChannelFlags.Permanent))
            {
                ActiveChannels.TryRemove(channel.Name, out _);
            }
        }

        // Leave group
        if (CurrentGroup != null)
        {
            await LeaveGroup(CurrentGroup);
        }

        // Abort match if in lobby
        if (CurrentMatchLobby != null)
        {
            await AbortMatch(CurrentMatchLobby, $"{AccountName} disconnected");
        }

        // Remove from active sessions
        ActiveSessions.TryRemove(AccountID, out _);

        // Notify friends of offline status
        await BroadcastFriendOnlineStatus(this, isOnline: false);
    }
}
```

## Testing

### Unit Testing

```csharp
public class JoinChannelProcessorTests
{
    [Fact]
    public async Task JoinChannel_ValidRequest_AddsPlayerToChannel()
    {
        // Arrange
        var mockSession = CreateMockSession(accountID: 123, accountName: "TestUser");
        var processor = new JoinChannelProcessor(logger, db);

        var buffer = new ChatBuffer();
        buffer.WriteString("General");
        buffer.WriteString(""); // No password

        // Act
        await processor.ProcessAsync(mockSession, buffer);

        // Assert
        var channel = chatServer.ActiveChannels["General"];
        Assert.Contains(123, channel.Members.Keys);
    }
}
```

### Integration Testing with Simulated Clients

```csharp
public class SimulatedChatClient
{
    private TcpClient client;
    private NetworkStream stream;

    public async Task ConnectAsync(string host, int port)
    {
        client = new TcpClient();
        await client.ConnectAsync(host, port);
        stream = client.GetStream();
    }

    public async Task SendAuthenticateAsync(string username, string token)
    {
        var buffer = new ChatBuffer();
        buffer.WriteUInt16(ChatProtocol.NET_CHAT_CL_AUTHENTICATE);
        buffer.WriteString(username);
        buffer.WriteString(token);

        await stream.WriteAsync(buffer.ToArray());
    }

    public async Task<ChatBuffer> ReceiveAsync()
    {
        // Read 2-byte length
        byte[] lengthBytes = new byte[2];
        await stream.ReadAsync(lengthBytes, 0, 2);
        ushort length = BitConverter.ToUInt16(lengthBytes);

        // Read payload
        byte[] payload = new byte[length];
        await stream.ReadAsync(payload, 0, length);

        return new ChatBuffer(payload);
    }
}
```

**Load Testing**:

```csharp
[Fact]
public async Task LoadTest_10000ConcurrentConnections()
{
    var clients = new List<SimulatedChatClient>();

    // Connect 10,000 clients
    for (int i = 0; i < 10000; i++)
    {
        var client = new SimulatedChatClient();
        await client.ConnectAsync("localhost", 11031);
        await client.SendAuthenticateAsync($"user{i}", $"token{i}");

        clients.Add(client);

        if (i % 100 == 0)
            await Task.Delay(10); // Slight delay every 100 connections
    }

    // All clients join same channel
    foreach (var client in clients)
    {
        await client.SendJoinChannelAsync("LoadTest");
    }

    // Send 1000 messages
    for (int i = 0; i < 1000; i++)
    {
        var randomClient = clients[Random.Shared.Next(clients.Count)];
        await randomClient.SendChannelMessageAsync("LoadTest", $"Message {i}");
    }

    // Measure P95 latency
    Assert.True(p95Latency < 100); // Under 100ms
}
```

## Performance Monitoring

### Metrics to Track

```csharp
public class ChatServerMetrics
{
    public int ActiveConnections { get; set; }
    public int ActiveChannels { get; set; }
    public int ActiveGroups { get; set; }
    public int ActiveMatchLobbies { get; set; }
    public long MessagesSentPerSecond { get; set; }
    public double AverageLatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public double P99LatencyMs { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CPUUsagePercent { get; set; }
}
```

### Aspire Dashboard

Use .NET Aspire dashboard for real-time monitoring:

```powershell
# Run with Aspire orchestration
dotnet run --project TRANSMUTANSTEIN.ChatServer

# Open browser to Aspire dashboard
# https://localhost:5001
```

## Debugging Tips

### Enable Verbose Logging

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "TRANSMUTANSTEIN.ChatServer": "Trace"
    }
  }
}
```

### Inspect Binary Messages

```csharp
private void LogMessage(byte[] message, string direction)
{
    var hex = BitConverter.ToString(message).Replace("-", " ");
    logger.LogDebug($"{direction}: {hex}");

    // Also log parsed structure
    var buffer = new ChatBuffer(message);
    ushort commandCode = buffer.ReadUInt16();
    logger.LogDebug($"Command Code: 0x{commandCode:X4} ({GetCommandName(commandCode)})");
}
```

## Next Steps

1. **Phase 1**: Implement chat channels (join, leave, message, kick, silence)
2. **Phase 2**: Implement matchmaking groups (create, invite, join, leave, ready)
3. **Phase 3**: Implement queue and match lobby (join queue, matchmaking algorithm, server allocation, match start)
4. **Phase 4**: Implement player communication (whisper, add friend, friend notifications)

## Additional Resources

- **Spec**: `specs/tcp-server/spec.md`
- **Plan**: `specs/tcp-server/plan.md`
- **Research**: `specs/tcp-server/research.md`
- **Data Model**: `specs/tcp-server/data-model.md`
- **Contracts**: `specs/tcp-server/contracts/`
- **HoN Source**: `C:\HoN\` (absolute source of truth)
- **KONGOR Source**: `C:\KONGOR\` (practical reference)

## Getting Help

- Review protocol contracts in `specs/tcp-server/contracts/`
- Check HoN source code for reference implementations
- Consult KONGOR codebase for practical patterns
- Ask team members for clarification

---

**Remember**: HoN is the **absolute source of truth**, KONGOR is the **practical reference**. When in doubt, consult HoN source code first.
