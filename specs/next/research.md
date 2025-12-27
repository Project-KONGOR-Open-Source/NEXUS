# Phase 0 Research: Chat Server Protocol Analysis

**Date**: 2025-01-13
**Feature**: TRANSMUTANSTEIN Chat Server Implementation
**Status**: Complete

## Executive Summary

This document consolidates research findings from analysing HoN (absolute source of truth) and KONGOR (production reference) chat server implementations. The analysis covers protocol structures, message flows, architectural patterns, and design decisions for implementing the NEXUS chat server with .NET 10.

**Key Findings**:
- Protocol version 68 with 200+ commands categorised for phased implementation
- Triple-listener architecture (client, server, manager connections)
- In-memory state management for channels and groups (no database persistence)
- 5 game types confirmed: Campaign Normal, Campaign Casual, Midwars, Riftwars, Public
- Performance testing requires simulated concurrent connection harness

---

## 1. Protocol Structure Analysis

### Message Framing (HoN/KONGOR Compatible)

**Decision**: Use 2-byte length prefix + 2-byte command code structure

**Format**:
```
[2 bytes: Message Length (excludes length field itself)]
[2 bytes: Command Code]
[Variable: Payload data]
```

**Rationale**:
- HoN chatserver_protocol.h defines this as the standard message format
- KONGOR Connection.cs OnDataReceivedImpl() validates this pattern (lines 200-220)
- NEXUS ChatBuffer.cs already implements this format
- Maximum message size: 65535 bytes (enforced by 2-byte length field)
- 16KB channel buffer limit prevents memory exhaustion attacks

**Implementation Notes**:
- ChatBuffer.ReadCommandBytes() extracts 2-byte command code
- ChatBuffer size-prefixed string format: [2-byte length][UTF-8 string data]
- Command processors use ChatCommandAttribute with specific command codes from ChatProtocol.cs

### Keep-Alive Mechanism

**Decision**: 60-second keep-alive timer with dummy packets

**Pattern** (from KONGOR ChatServer.cs):
```csharp
private Timer KeepAliveTimer = new Timer(
    KeepAliveTimerCallback,
    this,
    60 * 1000,  // Initial delay: 60 seconds
    60 * 1000   // Interval: 60 seconds
);

void KeepAliveTimerCallback(object? state)
{
    // Send dummy packet to all connected clients
    // Remove connections where LastCommunicationTimestamp exceeds threshold
}
```

**Rationale**:
- Prevents NAT timeout disconnections
- Detects stale connections (client crashed without sending disconnect)
- KONGOR production system uses 60-second interval successfully
- HoN c_client.h tracks m_uiLastRecvTime for timeout detection

**Implementation Notes**:
- Track LastCommunicationTimestamp on each ChatSession
- Send minimal keep-alive packet (empty command or ping)
- Remove sessions with >120 seconds since last communication
- Immediate cleanup on disconnect (no reconnection grace period per spec clarification)

---

## 2. Game Types and Ratings

### Five Game Types Confirmed

**Decision**: Implement 5 game types with separate ratings

**Game Types** (from PopularityUpdate.cs analysis):
1. **Campaign Normal** (TMM_GAME_TYPE_CAMPAIGN_NORMAL = 6)
   - Ranked team matchmaking on Caldavar map
   - Competitive rating tracked
   - Default starting rating: 1500.0

2. **Campaign Casual** (Not currently in PopularityUpdate.cs - needs addition)
   - Unranked team matchmaking on Caldavar map
   - Casual rating tracked separately
   - Lower stakes, same map as Campaign Normal

3. **Midwars** (TMM_GAME_TYPE_MIDWARS = 3)
   - Smaller map, faster gameplay
   - Separate rating pool
   - Currently in PopularityUpdate.cs

4. **Riftwars** (TMM_GAME_TYPE_RIFTWARS = 4)
   - Alternative map/mode
   - Separate rating pool
   - Currently in PopularityUpdate.cs

5. **Public** (TMM_GAME_TYPE_PUBLIC = 0)
   - Open matchmaking, possibly unranked
   - **NEEDS CLARIFICATION**: Does Public have separate rating or is it unranked?
   - **ASSUMPTION**: Public is unranked/casual play without rating changes

**Rationale**:
- HoN c_client.h defines 11+ rating types (including Reborn variants) - too many
- NEXUS constitution clarification specified exactly 5 types
- PopularityUpdate.cs currently lists 3 (Campaign Normal, Midwars, Riftwars) + need to add Campaign Casual and Public
- Simplified from KONGOR while maintaining core gameplay variety

**PlayerStatistics Entity Fields**:
```csharp
// Campaign Normal (ranked TMM)
float CampaignNormalRating;
int CampaignNormalWins;
int CampaignNormalLosses;

// Campaign Casual (casual TMM)
float CampaignCasualRating;
int CampaignCasualWins;
int CampaignCasualLosses;

// Midwars
float MidwarsRating;
int MidwarsWins;
int MidwarsLosses;

// Riftwars
float RiftwarsRating;
int RiftwarsWins;
int RiftwarsLosses;

// Public (possibly unranked - clarify if rating needed)
float PublicRating;  // Or omit if unranked
int PublicWins;
int PublicLosses;
```

**Action Item**: Verify with user if Public game type needs separate rating or is unranked-only.

---

## 3. In-Memory vs Database Persistence

### In-Memory State Management

**Decision**: ChatChannel, MatchmakingGroup, and all transient state managed in-memory only

**Rationale**:
- User clarification: "classes such as chatchannel, matchmaking group, etc. should not be in the database and should live in memory"
- Channels are session-based - no need to persist when no one is joined
- Matchmaking groups are ephemeral - disbanded after match starts or leader leaves
- Reduces database load and complexity
- Faster access times for real-time operations

**In-Memory Entities**:
- ChatChannel (transient, exists while players joined)
- ChatChannelMember (transient, tied to ChatChannel lifetime)
- MatchmakingGroup (transient, exists until match starts or group disbands)
- MatchmakingGroupMember (transient, tied to MatchmakingGroup lifetime)

**Thread Safety Pattern**:
```csharp
// ChatServer maintains concurrent dictionaries for thread-safe access
private ConcurrentDictionary<string, ChatChannel> ActiveChannels = new();
private ConcurrentDictionary<int, MatchmakingGroup> ActiveGroups = new();

// Access pattern
var channel = ActiveChannels.GetOrAdd(channelName, name => new ChatChannel(name));
channel.Members.TryAdd(accountID, new ChatChannelMember(...));
```

**Cleanup on Disconnect**:
1. Remove player from all channels (ChatChannel.RemoveMember)
2. If channel empty, remove from ActiveChannels
3. If player is group leader, disband group
4. If player is group member, remove from group
5. Update friend online status (if FriendedPeer data loaded)

### Persistent Database Entities

**Decision**: Only PlayerStatistics and FriendedPeer persist to database

**Rationale**:
- Player statistics must persist across sessions (ratings, wins/losses)
- Friend relationships must persist (player friendships permanent until removed)
- ClanMember may already exist in MERRICK.DatabaseContext (check during implementation)
- Reduces database migrations and EF Core overhead

**Database Entities** (MERRICK.DatabaseContext):
1. **PlayerStatistics** - 5 game type ratings and match counts
2. **FriendedPeer** - Friend relationships (check NEXUS terminology)
3. **ClanMember** - If not already exists

**NEXUS Terminology Check**:
- **ACTION REQUIRED**: Search NEXUS codebase for existing "Buddy" or "Friend" terminology
- KONGOR uses "Buddy", but user prefers NEXUS-specific terms
- Check if FriendedPeer, Friend, or another term already exists
- Use existing NEXUS term, fallback to FriendedPeer if creating new

---

## 4. Triple-Listener Architecture

### KONGOR Pattern Analysis

**Pattern** (from KONGOR ChatServer.cs):
```csharp
public class ChatServer : IChatServer
{
    private TcpListener? ClientListener;
    private TcpListener? ServerListener;
    private TcpListener? ManagerListener;

    public void Start()
    {
        ClientListener = new TcpListener(IPAddress.Any, ClientPort);
        ServerListener = new TcpListener(IPAddress.Any, ServerPort);
        ManagerListener = new TcpListener(IPAddress.Any, ManagerPort);

        ClientListener.Start();
        ServerListener.Start();
        ManagerListener.Start();

        // Accept connections on separate threads
    }
}
```

**Decision**: Single ChatServer class managing 3 TCP listener instances

**Rationale**:
- User clarification: "option b sounds like what i have in mind" (single server, three listeners)
- Follows modern .NET service patterns (single host, multiple endpoints)
- Centralizes lifecycle management and configuration
- Allows shared state/services across connection types
- Maintains logical separation through different handlers

**NEXUS Implementation** (building on existing TCPServer):
```csharp
public class ChatServer
{
    private TCPServer? ClientTCPServer;
    private TCPServer? GameServerTCPServer;
    private TCPServer? ManagerTCPServer;

    // Shared state
    private ConcurrentDictionary<string, ChatChannel> ActiveChannels = new();
    private ConcurrentDictionary<int, MatchmakingGroup> ActiveGroups = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Start three separate TCP servers on different ports
        ClientTCPServer = new TCPServer(clientPort);
        GameServerTCPServer = new TCPServer(serverPort);
        ManagerTCPServer = new TCPServer(managerPort);

        await Task.WhenAll(
            ClientTCPServer.StartAsync(cancellationToken),
            GameServerTCPServer.StartAsync(cancellationToken),
            ManagerTCPServer.StartAsync(cancellationToken)
        );
    }
}
```

**Connection Type Differentiation**:
- **Client Connections**: ChatSession (existing) for player connections
- **Game Server Connections**: GameServerSession (new) for game server connections
- **Manager Connections**: ManagerSession (new) for server manager connections
- Each session type implements appropriate command processors

---

## 5. Command Processor Categorization

### Phase 1: Chat Channels (Highest Priority)

**Commands** (from ChatProtocol.cs):
- NET_CHAT_CL_JOIN_CHANNEL - Join public or clan channel
- NET_CHAT_CL_LEAVE_CHANNEL - Leave channel
- NET_CHAT_CL_CHANNEL_MESSAGE - Send message to channel
- NET_CHAT_CL_CHANNEL_EMOTE - Send emote to channel
- NET_CHAT_CL_SET_CHANNEL_TOPIC - Set channel topic (admin)
- NET_CHAT_CL_KICK_FROM_CHANNEL - Kick user from channel (admin)
- NET_CHAT_CL_SILENCE_USER - Silence user in channel (admin)
- NET_CHAT_CL_CHANNEL_BAN - Ban user from channel (admin)
- NET_CHAT_CL_CHANNEL_UNBAN - Unban user from channel (admin)
- NET_CHAT_CL_CHANNEL_LIST_BANS - List banned users

**Priority Commands** (implement first):
1. JOIN_CHANNEL - Most fundamental
2. LEAVE_CHANNEL - Required for cleanup
3. CHANNEL_MESSAGE - Core communication
4. KICK_FROM_CHANNEL - Basic moderation

**Deferred Commands** (Phase 1 later):
- Emotes, topic setting, ban management

### Phase 2: Matchmaking Groups

**Commands**:
- NET_CHAT_CL_TMM_GROUP_CREATE - Create matchmaking group
- NET_CHAT_CL_TMM_GROUP_INVITE - Invite player to group
- NET_CHAT_CL_TMM_GROUP_ACCEPT_INVITE - Accept group invitation
- NET_CHAT_CL_TMM_GROUP_REJECT_INVITE - Reject group invitation
- NET_CHAT_CL_TMM_GROUP_JOIN - Join group (after invitation)
- NET_CHAT_CL_TMM_GROUP_LEAVE - Leave group
- NET_CHAT_CL_TMM_GROUP_KICK - Kick member from group (leader only)
- NET_CHAT_CL_TMM_GROUP_PROMOTE_LEADER - Transfer leadership
- NET_CHAT_CL_TMM_PLAYER_READY_STATUS - Set ready status
- NET_CHAT_CL_TMM_PLAYER_LOADING_STATUS - Set loading status

**Priority Commands**:
1. GROUP_CREATE
2. GROUP_INVITE / ACCEPT_INVITE
3. GROUP_JOIN / GROUP_LEAVE
4. PLAYER_READY_STATUS

### Phase 3: Matchmaking, Queue, Match Lobby & Getting Players In-Game

**Queue Commands**:
- NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE - Enter matchmaking queue
- NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE - Exit queue
- NET_CHAT_CL_TMM_POPULARITY_UPDATE - Request queue popularity data

**Match Lobby Commands**:
- NET_CHAT_CL_TMM_MATCH_FOUND - Notify groups of match
- NET_CHAT_CL_TMM_MATCH_ACCEPT - Accept match
- NET_CHAT_CL_TMM_MATCH_DECLINE - Decline match
- NET_CHAT_CL_TMM_MATCH_LOBBY_UPDATE - Update lobby state
- NET_CHAT_CL_TMM_MATCH_START - Start match (all players loaded)

**Game Server Commands** (Server port):
- NET_CHAT_SV_SERVER_HANDSHAKE - Game server authentication
- NET_CHAT_SV_SERVER_STATUS - Report server availability
- NET_CHAT_SV_MATCH_STATUS - Update match progress
- NET_CHAT_SV_MATCH_COMPLETE - Submit match results
- NET_CHAT_SV_ALLOCATE_SERVER - Request server allocation

**Matchmaking Algorithm Requirements**:
1. **Rating Disparity Check**: Average group MMR difference < 300 (configurable)
2. **Queue Time Priority**: Longer wait times increase match threshold flexibility
3. **Region Matching**: Prefer same region, expand if queue time excessive
4. **Game Mode Compatibility**: Only match groups with compatible mode preferences
5. **Team Size Matching**: 1v1, 2v2, 3v3, 4v4, 5v5

**Match Lobby Flow**:
1. Two compatible groups found by matchmaking algorithm
2. Create match lobby in-memory (MatchLobby class)
3. Notify both groups (MATCH_FOUND)
4. Players accept match
5. Allocate game server (via Manager port)
6. Send server connection info to all players (MATCH_START)
7. Players connect to game server
8. Chat server tracks match status until completion

### Phase 4: Player Communication

**Commands**:
- NET_CHAT_CL_WHISPER - Send private message
- NET_CHAT_CL_WHISPER_FAILED - Notify whisper delivery failure
- NET_CHAT_CL_ADD_BUDDY - Add friend
- NET_CHAT_CL_REMOVE_BUDDY - Remove friend
- NET_CHAT_CL_BUDDY_ONLINE - Notify friend came online
- NET_CHAT_CL_BUDDY_OFFLINE - Notify friend went offline
- NET_CHAT_CL_BUDDY_LIST - Send full friend list

**Friend System** (check NEXUS terminology):
- FriendedPeer entity (or existing NEXUS term)
- Persistent in database
- Online status tracked in-memory (via active ChatSessions)
- Notifications sent when friends connect/disconnect

---

## 6. HoN Protocol Truth Analysis

### Client State Management (c_client.h)

**Key Fields** (742 lines analysed):
```cpp
// Connection
CMessageSocket* m_pSocket;
uint m_uiLastRecvTime;

// Status
EChatClientStatus m_eStatus;  // Connecting, Connected, InGame, etc.
EAccountType m_eAccountType;
byte m_yFlags;

// Account
uint m_uiAccountID;
uint m_uiSuperID;
string m_sCookie;
u8string m_sName;

// Matchmaking
byte m_yTMMReadyStatus;
byte m_yTMMLoadingStatus;
CGroup* m_pGroup;
uint m_uiGroupID;

// Ratings (11 types in HoN - simplified to 5 in NEXUS)
float m_fTMR;
float m_fNormalTMR;
float m_fCasualTMR;
float m_fMidwarsTMR;
float m_fRiftwarsTMR;
// ... (6 more Reborn/Campaign variants)

// Clan
CClan* m_pClan;
uint m_uiClanID;
```

**NEXUS ChatSession Mapping**:
- Account, AccountID, SuperID → Existing in ChatSession
- TMM ratings → Loaded from PlayerStatistics entity (database)
- Group membership → Reference to MatchmakingGroup (in-memory)
- Clan → Reference to Clan entity (existing in MERRICK)

### Channel Management (c_channel.h)

**Key Fields**:
```cpp
uint m_uiChannelID;
uint m_uiClanID;
uint m_uiFlags;  // Permanent, Hidden, Server, Reserved
wstring m_sNameUTF8;
wstring m_sTopicUTF8;
ClientSet* m_pClients;  // Active members
AdminMap m_mapAdmins;   // AccountID → AdminLevel
set<wstring> m_setAuthed;  // Authorized users
map<uint, uint> m_mapSilenced;  // AccountID → SilenceExpiryTime
wstring m_sPassword;
```

**NEXUS ChatChannel Mapping**:
- ChannelID, ClanID, Flags, Name, Topic, Password → Direct properties
- m_pClients → ConcurrentDictionary<int, ChatChannelMember> Members
- m_mapAdmins → AdminLevel property on ChatChannelMember
- m_mapSilenced → SilencedUntil property on ChatChannelMember

### Protocol Version

**Confirmed**: Protocol version 68
- HoN chatserver_protocol.h defines version 68
- NEXUS ChatProtocol.cs implements version 68
- All command codes match between HoN and NEXUS definitions

---

## 7. KONGOR Production Patterns

### Generic Connection Handler (Connection.cs - 470 lines)

**Pattern**:
```csharp
public class Connection<T> where T : ConnectedSubject
{
    private ByteBuffer ReceiveBuffer, SendBuffer;
    private Socket Socket;
    private long LastCommunicationTimestamp;
    private ProtocolRequestFactory<T> RequestFactory;

    private void OnDataReceivedImpl()
    {
        while (ReceiveBuffer.HasEnoughData(4))  // 2-byte length + 2-byte command
        {
            ushort length = ReceiveBuffer.ReadUInt16();
            ushort command = ReceiveBuffer.ReadUInt16();

            if (!ReceiveBuffer.HasEnoughData(length))
                break;  // Incomplete message

            ProtocolRequest<T> request = RequestFactory.DecodeProtocolRequest(command);
            request.BeforeProcess();  // Database setup
            request.Process();        // Core logic
            request.AfterProcess();   // Database cleanup
        }
    }
}
```

**NEXUS Equivalent**:
- TCPSession base class provides socket management
- ChatBuffer replaces ByteBuffer with NEXUS-specific serialization
- Command processors implement ISynchronousCommandProcessor / IAsynchronousCommandProcessor
- ChatCommandAttribute for routing (ASP.NET Core-inspired)

**Message Processing Pipeline**:
1. Receive raw data into buffer
2. Extract 2-byte length, 2-byte command
3. Validate complete message available
4. Route to command processor via attribute
5. Execute processor (async or sync)
6. Send response via ChatSession.Send()

### Performance Counters (KONGOR Pattern)

**KONGOR Implementation**:
```csharp
private static Dictionary<ushort, PerformanceCounter> MessageCounters = new();

void TrackMessagePerformance(ushort command, TimeSpan duration)
{
    if (!MessageCounters.ContainsKey(command))
    {
        MessageCounters[command] = new PerformanceCounter(
            "ChatServer",
            $"Command_{command}",
            false
        );
    }

    MessageCounters[command].Increment();
    // Track duration, throughput, etc.
}
```

**NEXUS Alternative** (using .NET 10 metrics):
```csharp
private static Meter ChatServerMeter = new("NEXUS.ChatServer", "1.0.0");
private static Histogram<double> CommandDuration =
    ChatServerMeter.CreateHistogram<double>("command.duration", "ms");

void TrackCommand(string commandName, TimeSpan duration)
{
    CommandDuration.Record(duration.TotalMilliseconds,
        new KeyValuePair<string, object?>("command", commandName));
}
```

**Rationale**: .NET 10 built-in metrics integrate with Aspire dashboard automatically.

---

## 8. Performance Testing with Simulated Concurrency

### Requirement

User clarification: "regarding testing it would be really interesting to be able to test performance under simulated concurrency"

**Goal**: Test 10,000+ concurrent connections with simulated game clients

### Test Harness Design

**Decision**: Create SimulatedChatClient test helper

**Pattern**:
```csharp
public class SimulatedChatClient
{
    private TcpClient client;
    private NetworkStream stream;
    private int accountID;

    public async Task ConnectAsync(string host, int port)
    {
        client = new TcpClient();
        await client.ConnectAsync(host, port);
        stream = client.GetStream();
    }

    public async Task AuthenticateAsync(int accountID, string cookie)
    {
        // Send NET_CHAT_CL_HANDSHAKE with credentials
        // Wait for response
        // Track authenticated state
    }

    public async Task JoinChannelAsync(string channelName)
    {
        // Send NET_CHAT_CL_JOIN_CHANNEL
        // Wait for member list response
    }

    public async Task SendMessageAsync(string channelName, string message)
    {
        // Send NET_CHAT_CL_CHANNEL_MESSAGE
    }

    public async Task SimulateConcurrentLoad(TimeSpan duration)
    {
        // Send random messages, join/leave channels
        // Simulate realistic player behaviour
    }
}
```

**Load Test Pattern**:
```csharp
[Fact]
[Trait("Category", "Performance")]
public async Task Test_10k_Concurrent_Connections()
{
    const int concurrentClients = 10_000;
    List<SimulatedChatClient> clients = new();

    // Spawn 10k simulated clients
    var connectTasks = Enumerable.Range(0, concurrentClients)
        .Select(async i =>
        {
            var client = new SimulatedChatClient();
            await client.ConnectAsync("localhost", chatServerPort);
            await client.AuthenticateAsync(i, $"cookie_{i}");
            clients.Add(client);
        });

    await Task.WhenAll(connectTasks);

    // Measure message latency
    var stopwatch = Stopwatch.StartNew();
    await clients[0].SendMessageAsync("General", "Test message");
    // Measure time until all 9,999 other clients receive broadcast
    stopwatch.Stop();

    Assert.True(stopwatch.ElapsedMilliseconds < 100); // P95 < 100ms
}
```

**Metrics to Track**:
1. Connection establishment time (P50, P95, P99)
2. Message delivery latency (P50, P95, P99)
3. Memory usage at 10k connections
4. CPU usage under load
5. Concurrent messages per second throughput
6. False disconnect rate (< 1% target)

### Load Testing Tools

**Options Considered**:
1. **NBomber** - .NET load testing framework
2. **Custom SimulatedChatClient** - Full protocol control
3. **k6** - External load testing (requires protocol implementation)

**Decision**: Custom SimulatedChatClient + NBomber for scenarios

**Rationale**:
- Custom client provides full HoN protocol compatibility
- NBomber provides scenario orchestration and metrics
- Integration with xUnit for CI/CD pipeline
- Can simulate realistic player behaviour patterns

---

## 9. Design Decisions Summary

### 1. In-Memory State Architecture

| Entity | Storage | Rationale |
|--------|---------|-----------|
| ChatChannel | In-memory (ConcurrentDictionary) | Transient, no persistence needed |
| MatchmakingGroup | In-memory (ConcurrentDictionary) | Session-based, disbanded after match |
| PlayerStatistics | Database (MERRICK.DatabaseContext) | Persistent ratings and match history |
| FriendedPeer | Database (MERRICK.DatabaseContext) | Persistent friend relationships |

### 2. Game Type Ratings

| Game Type | Internal Name | Rating Tracked | Notes |
|-----------|---------------|----------------|-------|
| Campaign Normal | TMM_GAME_TYPE_CAMPAIGN_NORMAL (6) | Yes | Ranked TMM |
| Campaign Casual | (Add to PopularityUpdate.cs) | Yes | Casual TMM |
| Midwars | TMM_GAME_TYPE_MIDWARS (3) | Yes | Fast gameplay |
| Riftwars | TMM_GAME_TYPE_RIFTWARS (4) | Yes | Alternative map |
| Public | TMM_GAME_TYPE_PUBLIC (0) | TBD | Possibly unranked |

**ACTION REQUIRED**: Confirm if Public game type needs rating or is unranked-only.

### 3. Terminology

| Concept | HoN/KONGOR Term | NEXUS Term | Status |
|---------|-----------------|------------|--------|
| Friend system | Buddy | FriendedPeer | Check existing NEXUS codebase |
| Team Matchmaking | TMM | Campaign | Confirmed (internal name) |
| Match Rating | TMR | Rating | Use "Rating" generically |

**ACTION REQUIRED**: Search NEXUS codebase for existing friend/buddy terminology.

### 4. Phase 3 Scope Expansion

**Confirmed Inclusions**:
- Matchmaking algorithm (rating disparity, queue time, region)
- Match lobby creation and management
- Player loading/ready status tracking
- Game server allocation via Manager port
- Match start coordination (getting players in-game)

### 5. Performance Testing Approach

**Confirmed Approach**:
- Custom SimulatedChatClient for protocol compatibility
- 10,000+ concurrent connection testing
- Message latency benchmarks (P95 < 100ms target)
- Integration with xUnit and Category=Performance attribute
- Metrics tracked via .NET 10 System.Diagnostics.Metrics

---

## 10. Open Questions & Action Items

### ✅ RESOLVED (2025-01-14)

1. **Public Game Type Rating**: ✅ RESOLVED
   - **Decision**: Public game type tracks PSR (Public Skill Rating) - separate rating container that works like MMR
   - **Implementation**: PublicRating field in PlayerStatistics (non-nullable, default 1500.0f)
   - **Impact**: FR-037 updated, data-model.md updated

2. **NEXUS Friend Terminology**: ✅ RESOLVED
   - **Decision**: Use "FriendedPeer" consistently (not "Buddy")
   - **Rationale**: More descriptive, shows bidirectional relationship, follows NEXUS naming conventions
   - **Impact**: spec.md updated, data-model.md updated, tasks.md updated

3. **Pipeline Pattern (FR-013)**: ✅ RESOLVED
   - **Decision**: Use modern ASP.NET Core DI pattern (NOT BeforeProcess/AfterProcess)
   - **Rationale**: Legacy pattern is anti-pattern with modern DI, dependencies injected via constructor
   - **Impact**: FR-013 reworded to "use dependency injection for database context"

4. **Recursive Depth Tracking (FR-012)**: ✅ RESOLVED
   - **Decision**: Use async/await patterns (NOT recursive depth counter)
   - **Rationale**: Modern async/await handles this, legacy concern no longer applicable
   - **Impact**: FR-012 reworded, keep FR-015 buffer limit (16KB) for safety

5. **Group Leader Transfer (FR-032)**: ✅ RESOLVED
   - **Decision**: Automatic transfer when leader leaves (index-based)
   - **Implementation**: When leader removed, shift all indices down by 1, member at index 0 becomes new leader
   - **Impact**: FR-032 clarified, task T075a added

6. **Storage Strategy**: ✅ RESOLVED
   - **Database**: Permanent data (PlayerStatistics, FriendedPeer, Clan, ClanMember)
   - **Redis**: Service-restart-safe (GameServer registry, potentially queue state)
   - **In-Memory**: Disposable (ChatChannel, MatchmakingGroup, ChatSession)
   - **Rule**: Permanent → DB, Service-restart-safe → Redis, Disposable → In-Memory
   - **Impact**: data-model.md updated with storage strategy, task T027a added for Redis

7. **PopularityUpdate.cs Update**: ✅ RESOLVED
   - Current: Campaign Normal, Midwars, Riftwars
   - Needed: Add Campaign Casual and Public
   - **Action**: Update PopularityUpdate.cs game types list during implementation

### Deferred to Post-MVP

1. **Match History Tracking (FR-058)**: DEFERRED POST-MVP
2. **Win/Loss Streak Tracking (FR-059)**: DEFERRED POST-MVP
3. **Season Statistics Resets (FR-060)**: DEFERRED POST-MVP
4. **Leaver Strike Tracking (FR-062)**: DEFERRED POST-MVP

### Deferred Decisions (Implementation Phase)

1. Redis key structure for distributed session state
2. Matchmaking algorithm exact thresholds (rating disparity, queue time weights)
3. Channel buffer overflow handling (reject vs drop oldest messages)
4. Performance counter vs .NET metrics choice

---

## 11. Next Steps

**Phase 1 Artifacts to Generate**:
1. **data-model.md**: Define in-memory classes (ChatChannel, MatchmakingGroup) and persistent entities (PlayerStatistics, FriendedPeer)
2. **contracts/**: Protocol message schemas for all command categories
3. **quickstart.md**: Developer onboarding with .NET 10, concurrent testing, in-memory patterns
4. **Agent context update**: Update .specify/templates/agent-file-template.md

**Ready for Implementation**:
- Phase 0 research complete
- Architecture decisions documented
- Protocol analysis comprehensive
- Performance testing approach defined

**Branch**: next
**Next Command**: Generate Phase 1 artifacts (data-model.md, contracts/, quickstart.md)
