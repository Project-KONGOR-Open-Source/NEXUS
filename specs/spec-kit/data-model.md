# Data Model: Chat Server Entities

**Date**: 2025-01-13
**Feature**: TRANSMUTANSTEIN Chat Server Implementation
**Phase**: Phase 1 Design

## Overview

The chat server uses a hybrid storage approach:
- **In-Memory**: Transient state for active channels, matchmaking groups, and sessions (managed in ConcurrentDictionary collections)
- **Database**: Persistent state for player statistics, friend relationships, and clan membership (Entity Framework Core via MERRICK.DatabaseContext)

**Thread Safety**: All in-memory collections use `ConcurrentDictionary<TKey, TValue>` or `ConcurrentBag<T>` for safe multi-threaded access.

## Storage Strategy

NEXUS uses three persistence layers based on data lifetime requirements:

1. **Database (SQL Server via MERRICK.DatabaseContext)**: Permanent persistence
   - PlayerStatistics: Match statistics and ratings persist indefinitely
   - FriendedPeer: Friend relationships persist across all restarts
   - Clan, ClanMember: Clan data must be permanent
   - Use when: Data must survive application restarts AND service failures

2. **Redis Cache (via Aspire)**: Service-restart-safe temporary data
   - GameServer registry: Must persist while game servers online, even if chat service restarts
   - Potentially: Queue state if we want matchmaking to survive chat server restarts
   - Use when: Data should survive service restarts but not indefinite

3. **In-Memory (ConcurrentDictionary)**: Disposable session data
   - ChatChannel: Recreated as players join channels
   - MatchmakingGroup: Recreated as players form groups
   - ChatSession: Active connections only
   - Use when: Data is session-based and acceptable to lose on service restart

**Rule of Thumb**:
- Permanent data → Database
- Service-restart-safe → Redis
- Disposable/session-based → In-Memory

---

## In-Memory Entities (Transient State)

### ChatChannel

Represents an active chat channel with current members. Exists only while players are joined.

**Properties**:
```csharp
int ChannelID              // Unique identifier (auto-increment or hash-based)
string Name                // Channel name (e.g., "General", "Clan_MyGuild")
int? ClanID                // Clan ID if clan-specific channel, null for public
string Topic               // Channel topic (can be set by admins)
string? Password           // Optional password for restricted channels
ChannelFlags Flags         // Bitfield: Permanent, Hidden, Server, Reserved
DateTime CreatedTime       // When channel was created (in-memory)
```

**Collections**:
```csharp
ConcurrentDictionary<int, ChatChannelMember> Members  // AccountID → Member
```

**Methods**:
```csharp
void AddMember(int accountID, ChatSession session, AdminLevel adminLevel = AdminLevel.None)
void RemoveMember(int accountID)
void BroadcastMessage(ChatBuffer message, int? excludeAccountID = null)
void SetTopic(string topic, int setByAccountID)
void KickMember(int targetAccountID, int kickedByAccountID)
void SilenceMember(int targetAccountID, DateTime until, int silencedByAccountID)
bool IsMember(int accountID)
bool IsAdmin(int accountID)  // Check if user has admin privileges
```

**Lifecycle**:
- Created when first player joins channel
- Exists while Members.Count > 0
- Automatically removed from ChatServer.ActiveChannels when last member leaves
- Clan channels may be marked Permanent (persist even when empty)

**Validation Rules**:
- Name: Required, 1-64 characters, alphanumeric + underscore
- Password: Optional, max 32 characters
- ClanID: Must be valid clan if provided
- Flags: Valid ChannelFlags enum combinations

**Enum Definitions**:
```csharp
[Flags]
public enum ChannelFlags
{
    None = 0,
    Permanent = 1 << 0,  // Channel persists even when empty
    Hidden = 1 << 1,     // Not shown in channel list
    Server = 1 << 2,     // Server-managed channel
    Reserved = 1 << 3    // Reserved for future use
}

public enum AdminLevel
{
    None = 0,
    Officer = 1,
    Leader = 2,
    Administrator = 3,
    Staff = 4
}
```

---

### ChatChannelMember

Represents a player's membership in a specific channel. Tied to ChatChannel lifetime.

**Properties**:
```csharp
int AccountID              // Player account ID
string AccountName         // Player display name (cached from session)
AdminLevel AdminLevel      // Admin privileges in this channel
DateTime? SilencedUntil    // Silence expiration time, null if not silenced
DateTime JoinedTime        // When player joined channel
```

**References**:
```csharp
ChatSession Session        // Reference to active session (for sending messages)
```

**Methods**:
```csharp
bool IsSilenced()          // Returns true if SilencedUntil > DateTime.UtcNow
bool CanSpeak()            // Returns !IsSilenced()
bool IsAdmin()             // Returns AdminLevel != AdminLevel.None
```

**Lifecycle**:
- Created when player joins channel
- Removed when player leaves channel or disconnects
- Removed if player kicked from channel
- SilencedUntil automatically expires (checked on message send)

**Validation Rules**:
- AccountID: Must match authenticated session
- SilencedUntil: Must be future date if set

---

### MatchmakingGroup

Represents a team preparing to queue for matchmaking. Session-based, disbanded after match starts or leader leaves.

**Properties**:
```csharp
int GroupID                // Unique group identifier (auto-increment)
int LeaderAccountID        // Group leader account ID
TMMGameType GameType       // Campaign Normal, Campaign Casual, Midwars, Riftwars, Public
TMMGameMode GameMode       // All Pick, Draft, Random, Banning Draft, etc.
TMMRegion Region           // EU, USE, USW, AU, BR, RU, etc.
QueueStatus QueueStatus    // NotQueued, InQueue, MatchFound
DateTime CreatedTime       // When group was created
DateTime? QueuedTime       // When group entered queue (null if not queued)
```

**Collections**:
```csharp
ConcurrentDictionary<int, MatchmakingGroupMember> Members  // AccountID → Member
```

**Methods**:
```csharp
void AddMember(int accountID, ChatSession session)
void RemoveMember(int accountID)
void SetLeader(int newLeaderAccountID)
bool AllMembersReady()     // Check if all members have ReadyStatus = true
void JoinQueue()           // Enter matchmaking queue
void LeaveQueue()          // Exit matchmaking queue
void Disband()             // Disband group, remove all members
int GetAverageRating()     // Calculate average MMR for matchmaking
```

**Lifecycle**:
- Created when leader issues TMM_GROUP_CREATE command
- Members added via invitation + acceptance
- Joins queue when leader issues TMM_GROUP_JOIN_QUEUE (if all ready)
- Disbanded when:
  - Leader leaves group
  - Last member leaves
  - Match starts (group transitions to match lobby)

**Validation Rules**:
- LeaderAccountID: Must be valid member of group
- GameType: Must be one of 5 supported types
- Region: Must be valid region
- Members.Count: 1-5 players (for 5v5 game modes)

**Enum Definitions**:
```csharp
public enum TMMGameType
{
    Public = 0,
    CampaignNormal = 6,
    CampaignCasual = 7,  // Add if not already in ChatProtocol.cs
    Midwars = 3,
    Riftwars = 4
}

public enum TMMGameMode
{
    AllPick = 0,
    SingleDraft = 3,
    AllRandom = 6,
    KrosMode = 14,
    HeroBan = 22,
    Reborn = 24
}

public enum TMMRegion
{
    USE = 0,
    USW = 1,
    EU = 2,
    AU = 11,
    BR = 15,
    RU = 9
}

public enum QueueStatus
{
    NotQueued = 0,
    InQueue = 1,
    MatchFound = 2
}
```

---

### MatchmakingGroupMember

Represents a player in a matchmaking group. Tied to MatchmakingGroup lifetime.

**Properties**:
```csharp
int AccountID              // Player account ID
int TeamSlot               // Position in team (0-4 for 5v5)
bool ReadyStatus           // Player marked as ready
bool LoadingStatus         // Player loading into match lobby
DateTime JoinedTime        // When player joined group
```

**References**:
```csharp
ChatSession Session        // Reference to active session
```

**Methods**:
```csharp
void SetReady(bool ready)
void SetLoading(bool loading)
```

**Lifecycle**:
- Created when player accepts group invitation
- Removed when player leaves group
- Removed if group disbanded
- TeamSlot assigned when joining (next available slot)

**Validation Rules**:
- AccountID: Must be authenticated session
- TeamSlot: 0-4 for 5v5, adjustable for other team sizes
- ReadyStatus: Required for group to join queue

---

### MatchLobby (Phase 3)

Represents a match lobby after matchmaking found compatible groups. Transient state until match starts.

**Properties**:
```csharp
int MatchLobbyID           // Unique match lobby identifier
int Team1GroupID           // First matched group
int Team2GroupID           // Second matched group
TMMGameType GameType       // Agreed game type
TMMGameMode GameMode       // Agreed game mode
TMMRegion Region           // Agreed region
string MapName             // Map to play (e.g., "caldavar", "midwars")
DateTime CreatedTime       // When match was found
DateTime? MatchStartTime   // When match actually started (null until start)
string? GameServerIP       // Allocated game server IP
int? GameServerPort        // Allocated game server port
string? MatchToken         // Security token for server connection
```

**Collections**:
```csharp
ConcurrentDictionary<int, MatchLobbyPlayer> Players  // AccountID → Player
```

**Methods**:
```csharp
void AddPlayer(int accountID, int teamSlot, int team)
void SetPlayerLoading(int accountID, bool loading)
bool AllPlayersLoaded()
void StartMatch(string serverIP, int serverPort, string matchToken)
void AbortMatch(string reason)
```

**Lifecycle**:
- Created when matchmaking broker finds compatible groups
- Players loaded from MatchmakingGroup members
- Game server allocated via Manager port
- Match starts when all players loaded
- Removed after match starts or aborts (timeout, player decline)

**Validation Rules**:
- Team1GroupID, Team2GroupID: Must be valid groups
- GameServerIP: Required before match start
- AllPlayersLoaded: Required to issue MATCH_START

---

### MatchLobbyPlayer (Phase 3)

Represents a player in a match lobby.

**Properties**:
```csharp
int AccountID
int Team                   // 1 or 2
int TeamSlot               // Position in team (0-4)
bool LoadingStatus         // Player ready to start
DateTime JoinedLobbyTime
```

**References**:
```csharp
ChatSession Session
```

**Methods**:
```csharp
void SetLoading(bool loading)
```

---

## Persistent Database Entities (MERRICK.DatabaseContext)

### PlayerStatistics

Stores player ratings, wins, losses across all 5 game types. Persists across sessions.

**Entity Configuration**:
```csharp
[Table("PlayerStatistics")]
public class PlayerStatistics
{
    [Key]
    public int StatisticsID { get; set; }

    [Required]
    [ForeignKey("Account")]
    public int AccountID { get; set; }

    // Campaign Normal (ranked TMM)
    public float CampaignNormalRating { get; set; } = 1500.0f;
    public int CampaignNormalWins { get; set; } = 0;
    public int CampaignNormalLosses { get; set; } = 0;

    // Campaign Casual (casual TMM)
    public float CampaignCasualRating { get; set; } = 1500.0f;
    public int CampaignCasualWins { get; set; } = 0;
    public int CampaignCasualLosses { get; set; } = 0;

    // Midwars
    public float MidwarsRating { get; set; } = 1500.0f;
    public int MidwarsWins { get; set; } = 0;
    public int MidwarsLosses { get; set; } = 0;

    // Riftwars
    public float RiftwarsRating { get; set; } = 1500.0f;
    public int RiftwarsWins { get; set; } = 0;
    public int RiftwarsLosses { get; set; } = 0;

    // Public (PSR - Public Skill Rating, works like MMR)
    public float PublicRating { get; set; } = 1500.0f;  // PSR (Public Skill Rating)
    public int PublicWins { get; set; } = 0;
    public int PublicLosses { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Account Account { get; set; } = null!;
}
```

**Indexes**:
- Primary Key: StatisticsID
- Unique Index: AccountID (one-to-one with Account)

**Validation Rules**:
- Ratings: Default 1500.0, must be >= 0
- Wins/Losses: Must be >= 0
- LastUpdated: Auto-updated on any rating change

**Rating Calculation** (Phase 3):
- Elo-like algorithm with K-factor (configurable, typically 32)
- Rating change based on match outcome and opponent ratings
- Separate calculation per game type
- NewRating = OldRating + K * (ActualScore - ExpectedScore)

---

### FriendedPeer

Stores persistent friend relationships between players.

**Entity Configuration**:
```csharp
[Table("FriendedPeers")]
public class FriendedPeer
{
    [Key]
    public int FriendshipID { get; set; }

    [Required]
    [ForeignKey("RequesterAccount")]
    public int RequesterAccountID { get; set; }

    [Required]
    [ForeignKey("TargetAccount")]
    public int TargetAccountID { get; set; }

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Account RequesterAccount { get; set; } = null!;
    public virtual Account TargetAccount { get; set; } = null!;
}

public enum FriendshipStatus
{
    Pending = 0,   // Invitation sent, not yet accepted
    Accepted = 1,  // Active friendship
    Blocked = 2    // One player blocked the other
}
```

**Indexes**:
- Primary Key: FriendshipID
- Unique Composite Index: (RequesterAccountID, TargetAccountID)
- Index: RequesterAccountID (for quick lookup)
- Index: TargetAccountID (for quick lookup)

**Validation Rules**:
- RequesterAccountID ≠ TargetAccountID (cannot friend yourself)
- Unique (RequesterAccountID, TargetAccountID) pair
- Status: Valid FriendshipStatus enum value

**Online Status Tracking** (in-memory):
- Friend online status tracked via active ChatSessions
- When player connects, notify all friends (send BUDDY_ONLINE)
- When player disconnects, notify all friends (send BUDDY_OFFLINE)
- Query FriendedPeer table on connect, cache in ChatSession

---

### ClanMember (if not already in MERRICK.DatabaseContext)

Stores player membership in clans. May already exist in MERRICK - check during implementation.

**Entity Configuration**:
```csharp
[Table("ClanMembers")]
public class ClanMember
{
    [Key]
    public int ClanMemberID { get; set; }

    [Required]
    [ForeignKey("Clan")]
    public int ClanID { get; set; }

    [Required]
    [ForeignKey("Account")]
    public int AccountID { get; set; }

    public ClanRole Role { get; set; } = ClanRole.Member;

    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Clan Clan { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
}

public enum ClanRole
{
    Member = 0,
    Officer = 1,
    Leader = 2
}
```

**Indexes**:
- Primary Key: ClanMemberID
- Unique Composite Index: (ClanID, AccountID)
- Index: ClanID (for clan roster queries)
- Index: AccountID (for player's clans query)

**Validation Rules**:
- Unique (ClanID, AccountID) pair
- Role: Valid ClanRole enum value
- Only one Leader per clan (enforced at application level)

**ACTION REQUIRED**: Check if ClanMember already exists in MERRICK.DatabaseContext. If yes, use existing definition.

---

## Entity Relationships

### In-Memory Relationships

```
ChatServer (singleton)
├── ActiveChannels: ConcurrentDictionary<string, ChatChannel>
│   └── ChatChannel
│       └── Members: ConcurrentDictionary<int, ChatChannelMember>
│           └── ChatChannelMember
│               └── Session: ChatSession (reference)
│
└── ActiveGroups: ConcurrentDictionary<int, MatchmakingGroup>
    └── MatchmakingGroup
        └── Members: ConcurrentDictionary<int, MatchmakingGroupMember>
            └── MatchmakingGroupMember
                └── Session: ChatSession (reference)

ChatSession
├── CurrentChannels: List<ChatChannel> (references)
├── CurrentGroup: MatchmakingGroup? (reference)
└── LoadedStatistics: PlayerStatistics (cached from database)
```

### Database Relationships (Entity Framework)

```
Account (existing)
├── PlayerStatistics (1:1)
├── RequestedFriendships (1:N FriendedPeer as Requester)
├── ReceivedFriendships (1:N FriendedPeer as Target)
└── ClanMemberships (1:N ClanMember)

PlayerStatistics
└── Account (N:1)

FriendedPeer
├── RequesterAccount (N:1)
└── TargetAccount (N:1)

Clan (existing)
└── ClanMembers (1:N)

ClanMember
├── Clan (N:1)
└── Account (N:1)
```

---

## State Management Patterns

### ChatServer Singleton

**Central State Manager**:
```csharp
public class ChatServer
{
    // In-memory state (thread-safe)
    private static ConcurrentDictionary<string, ChatChannel> ActiveChannels = new();
    private static ConcurrentDictionary<int, MatchmakingGroup> ActiveGroups = new();
    private static ConcurrentDictionary<int, ChatSession> ActiveSessions = new();

    // Access patterns
    public static ChatChannel GetOrCreateChannel(string channelName)
    {
        return ActiveChannels.GetOrAdd(channelName, name => new ChatChannel(name));
    }

    public static void RemoveChannelIfEmpty(string channelName)
    {
        if (ActiveChannels.TryGetValue(channelName, out ChatChannel? channel))
        {
            if (channel.Members.Count == 0 && !channel.Flags.HasFlag(ChannelFlags.Permanent))
            {
                ActiveChannels.TryRemove(channelName, out _);
            }
        }
    }

    public static MatchmakingGroup CreateGroup(int leaderAccountID)
    {
        int groupID = Interlocked.Increment(ref nextGroupID);
        MatchmakingGroup group = new MatchmakingGroup(groupID, leaderAccountID);
        ActiveGroups.TryAdd(groupID, group);
        return group;
    }

    public static void RemoveGroup(int groupID)
    {
        ActiveGroups.TryRemove(groupID, out _);
    }
}
```

### Cleanup on Disconnect

**Pattern** (in ChatSession.OnDisconnect):
```csharp
public void OnDisconnect()
{
    // 1. Remove from all channels
    foreach (ChatChannel channel in CurrentChannels.ToList())
    {
        channel.RemoveMember(AccountID);
        ChatServer.RemoveChannelIfEmpty(channel.Name);
    }
    CurrentChannels.Clear();

    // 2. Handle group membership
    if (CurrentGroup != null)
    {
        if (CurrentGroup.LeaderAccountID == AccountID)
        {
            // Leader leaving - disband entire group
            CurrentGroup.Disband();
            ChatServer.RemoveGroup(CurrentGroup.GroupID);
        }
        else
        {
            // Member leaving - just remove from group
            CurrentGroup.RemoveMember(AccountID);
        }
        CurrentGroup = null;
    }

    // 3. Notify friends of offline status
    NotifyFriendsOffline();

    // 4. Remove from active sessions
    ChatServer.ActiveSessions.TryRemove(AccountID, out _);
}
```

---

## Database Migrations

### Migration Strategy

**New Entities**:
1. PlayerStatistics (new table)
2. FriendedPeer (new table, or use existing if "Buddy" table exists)
3. ClanMember (check if already exists)

**Migration Commands**:
```powershell
# Create migration for PlayerStatistics
aspire exec --resource database-context -- dotnet ef migrations add AddPlayerStatistics

# Create migration for FriendedPeer (if not exists)
aspire exec --resource database-context -- dotnet ef migrations add AddFriendedPeer

# Apply migrations to development database
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
aspire exec --resource database-context -- dotnet ef database update
```

**DbContext Updates**:
```csharp
public class DatabaseContext : DbContext
{
    // Existing DbSets
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Clan> Clans { get; set; }

    // New DbSets
    public DbSet<PlayerStatistics> PlayerStatistics { get; set; }
    public DbSet<FriendedPeer> FriendedPeers { get; set; }  // Or Buddies
    // public DbSet<ClanMember> ClanMembers { get; set; }  // If not already exists

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PlayerStatistics: One-to-one with Account
        modelBuilder.Entity<PlayerStatistics>()
            .HasOne(ps => ps.Account)
            .WithOne()
            .HasForeignKey<PlayerStatistics>(ps => ps.AccountID);

        modelBuilder.Entity<PlayerStatistics>()
            .HasIndex(ps => ps.AccountID)
            .IsUnique();

        // FriendedPeer: Two foreign keys to Account
        modelBuilder.Entity<FriendedPeer>()
            .HasOne(fp => fp.RequesterAccount)
            .WithMany()
            .HasForeignKey(fp => fp.RequesterAccountID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendedPeer>()
            .HasOne(fp => fp.TargetAccount)
            .WithMany()
            .HasForeignKey(fp => fp.TargetAccountID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendedPeer>()
            .HasIndex(fp => new { fp.RequesterAccountID, fp.TargetAccountID })
            .IsUnique();

        // ClanMember: If not already exists
        // ... (similar pattern)
    }
}
```

---

## Summary

**In-Memory Entities** (4):
- ChatChannel
- ChatChannelMember
- MatchmakingGroup
- MatchmakingGroupMember
- (MatchLobby and MatchLobbyPlayer in Phase 3)

**Persistent Entities** (2-3):
- PlayerStatistics (new)
- FriendedPeer (new, check terminology)
- ClanMember (check if exists)

**Thread Safety**: All in-memory collections use concurrent types
**Cleanup**: Immediate on disconnect (no grace period)
**Database**: Only persistent data, Entity Framework navigation properties

**Next**: Generate protocol contracts (contracts/ directory)
