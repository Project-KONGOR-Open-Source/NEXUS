# Feature Specification: Chat Server Implementation

**Feature Branch**: `next`
**Created**: 2025-01-13
**Status**: Draft
**Input**: Implement TRANSMUTANSTEIN Chat Server with full protocol support, maintaining parity with HoN/KONGOR legacy implementations

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Player Authentication and Connection (Priority: P1)

Players must be able to connect to the chat server, authenticate their game account, and establish a persistent session for real-time communication.

**Why this priority**: Without authentication and connection management, no other chat features can function. This is the foundational capability that blocks all other user stories.

**Independent Test**: Can be fully tested by launching the game client, attempting to connect to the chat server, and verifying the client receives an authenticated session. Delivers the ability to establish and maintain connections.

**Acceptance Scenarios**:

1. **Given** a valid HoN game client with credentials, **When** the client connects to the chat server TCP endpoint, **Then** the client completes the handshake, receives session confirmation, and maintains a persistent connection
2. **Given** an authenticated player session, **When** the player remains connected for 5 minutes, **Then** the server sends keep-alive packets and maintains the connection without dropping
3. **Given** an invalid authentication credential, **When** a client attempts to connect, **Then** the server rejects the connection with an appropriate error message
4. **Given** a player with an active session, **When** the same account attempts to connect from a different location, **Then** the server gracefully disconnects the existing session and allows the new connection to proceed

---

### User Story 2 - Chat Channel Management (Priority: P2)

Players must be able to join chat channels, view other members in channels, send messages to channels, and receive messages from other channel members in real-time.

**Why this priority**: Channel-based communication is the primary social feature. Players use channels for general chat, finding groups, and community interaction.

**Independent Test**: Can be tested by having multiple authenticated clients join the same channel and exchange messages. Delivers basic public chat functionality.

**Acceptance Scenarios**:

1. **Given** an authenticated player, **When** the player joins a public channel (e.g., "General"), **Then** the player sees the list of current channel members and can send/receive messages
2. **Given** multiple players in a channel, **When** one player sends a message, **Then** all other channel members receive the message in real-time
3. **Given** a player in multiple channels, **When** messages are sent to different channels, **Then** the player receives messages with correct channel identification
4. **Given** a channel administrator, **When** the admin kicks a disruptive user, **Then** the kicked user is removed from the channel and receives notification
5. **Given** a clan-specific channel, **When** a non-clan member attempts to join, **Then** the server rejects the join request with appropriate error

---

### User Story 3 - Matchmaking Group Formation (Priority: P3)

Players must be able to create matchmaking groups, invite other players, form teams, and join the matchmaking queue to find games.

**Why this priority**: Matchmaking is core to the game experience. While dependent on authentication, it represents a complete flow from group formation to queue entry.

**Independent Test**: Can be tested by having players form groups, invite members, and join queues. Delivers the pre-game social and team formation experience.

**Acceptance Scenarios**:

1. **Given** an authenticated player, **When** the player creates a matchmaking group, **Then** the player becomes the group leader and can invite other players
2. **Given** a group leader, **When** the leader invites another player, **Then** the invited player receives the invitation and can accept or decline
3. **Given** a formed group with all players ready, **When** the group joins the matchmaking queue, **Then** the server begins searching for an opponent group
4. **Given** a player in a group, **When** the player marks themselves as ready, **Then** all group members see the updated ready status
5. **Given** a group in queue, **When** the group leader cancels, **Then** all group members are removed from the queue

---

### User Story 4 - Private Messaging and Buddy System (Priority: P4)

Players must be able to send private whisper messages to other players, maintain a buddy/friend list, and see online status of friends.

**Why this priority**: Private communication enhances the social experience but is not blocking for core gameplay. Can be implemented after public channels.

**Independent Test**: Can be tested by having two authenticated players add each other as buddies and exchange whisper messages. Delivers private social features.

**Acceptance Scenarios**:

1. **Given** two authenticated players, **When** Player A sends a whisper to Player B, **Then** Player B receives the private message and can reply
2. **Given** a player's buddy list, **When** a buddy comes online, **Then** the player receives a notification of the buddy's online status
3. **Given** a player, **When** the player adds a new buddy, **Then** the buddy list updates and persists across sessions
4. **Given** a player with DND (Do Not Disturb) mode enabled, **When** another player sends a whisper, **Then** the sender receives a notification that the recipient is unavailable

---

### User Story 5 - Clan Communication and Management (Priority: P5)

Players in clans must be able to access clan-specific channels, send clan-wide messages, view clan member status, and participate in clan activities.

**Why this priority**: Clan features enhance long-term player retention but are not essential for initial gameplay. Builds on channel and messaging foundations.

**Independent Test**: Can be tested by having clan members join their clan channel and exchange clan-specific communications. Delivers clan social features.

**Acceptance Scenarios**:

1. **Given** a clan member, **When** the player connects to the chat server, **Then** the player automatically joins the clan channel
2. **Given** a clan member in the clan channel, **When** the player sends a message, **Then** all online clan members receive the message
3. **Given** a clan leader, **When** the leader promotes a member to officer, **Then** the promoted member gains officer privileges in the clan channel
4. **Given** clan members, **When** viewing the clan roster, **Then** players see accurate online/offline status and current activity

---

### User Story 6 - Game Server Coordination (Priority: P6)

Game servers must be able to connect to the chat server, report match status, coordinate with players, and update player statistics after matches.

**Why this priority**: Required for post-game statistics and match coordination, but can be partially stubbed initially. Critical for production completeness.

**Independent Test**: Can be tested by having a game server connect, report match progress, and submit match results. Delivers game-chat integration.

**Acceptance Scenarios**:

1. **Given** a game server, **When** the server connects to the chat server manager port, **Then** the server authenticates and registers its availability
2. **Given** a match in progress, **When** the game server reports player status updates, **Then** the chat server updates player availability in real-time
3. **Given** a completed match, **When** the game server submits match results, **Then** the chat server updates player statistics and ratings
4. **Given** a match lobby, **When** all players load successfully, **Then** the game server receives confirmation to start the match

---

### User Story 7 - Matchmaking Broker and Queue Management (Priority: P7)

The matchmaking system must pair compatible groups, consider MMR ratings across multiple game modes, balance teams, and allocate game servers for matches.

**Why this priority**: Complex algorithm requiring multiple rating types and sophisticated matching logic. Depends on all previous matchmaking infrastructure.

**Independent Test**: Can be tested by having multiple groups enter queues and verifying appropriate matches are created based on ratings and preferences. Delivers automated match creation.

**Acceptance Scenarios**:

1. **Given** two groups in queue with similar MMR ratings, **When** the matchmaking broker evaluates compatibility, **Then** the broker creates a match and notifies both groups
2. **Given** groups with different preferred game modes, **When** the broker searches for matches, **Then** only groups with compatible preferences are paired
3. **Given** a formed match, **When** the broker allocates a game server, **Then** all players receive server connection information
4. **Given** multiple game types (Campaign Normal, Campaign Casual, Midwars, Riftwars, Public), **When** calculating match compatibility, **Then** the broker uses the appropriate rating for the selected game type

---

### Edge Cases

- **Connection drops**: When a player's connection drops unexpectedly, the system immediately cleans up all state (removes from channels, disbands groups if leader, removes from queue, marks offline). Game client handles reconnection and re-establishes fresh state. This prevents "ghost users" appearing online when actually disconnected.
- **Rapid reconnection attempts**: System must handle connection spam with rate limiting to prevent denial-of-service attacks
- **Channel buffer overflow**: When a protocol message would cause the channel buffer to exceed 16KB limit, the system rejects the entire message with an error response. Sender must retry after buffer clears. Partial message transmission is forbidden to maintain protocol integrity and prevent client connection corruption
- **Chat flooding and spam**: System prevents flooding through message rate limits (default: 5 messages per 2 seconds per user, configurable per deployment). Exceeded limit results in temporary message rejection with cooldown period
- **Game server unavailability**: When game servers become unavailable during matchmaking, queued groups must be notified and matches cannot be formed until servers return
- **Authentication session expiration**: Expired sessions must be detected during handshake and connection rejected with appropriate error
- **Channel limit exceeded**: When a player attempts to join more than 8 channels (HoN protocol limit), system sends CHAT_CMD_MAX_CHANNELS (0x0021) error response to client
- **Clan membership changes**: When a player leaves a clan mid-session, they are immediately removed from clan-specific channels
- **Queue wait time thresholds**: When matchmaking queue wait times exceed acceptable thresholds, system may relax rating requirements or notify players
- **Time zone differences**: For scheduled tournaments, all times stored and compared in UTC to eliminate time zone ambiguity

## Clarifications

### Session 2025-01-13

- Q: When adding new database entities (like Buddy, PlayerStatistics, ClanMember), what is the policy for handling entity relationships? → A: Entity Framework navigation properties matching existing MERRICK patterns
- Q: When a player's connection drops unexpectedly, what should happen to their in-progress activities? → A: Immediate cleanup - all state cleared instantly on disconnect (game client handles reconnection)
- Q: When a second connection attempt is detected from the same account, what enforcement action should be taken? → A: Disconnect existing session gracefully, allow new connection (prevents lockout)
- Q: Should new command processors follow the existing attribute-based pattern (modeled after ASP.NET Core), or is there flexibility? → A: Follow ASP.NET Core-inspired pattern; minor improvements welcome if clearly superior
- Q: How should the three TCP listener ports be structured (clients, game servers, managers)? → A: Single ChatServer class managing three listener instances using existing custom TCP server infrastructure
- Q: Should hardware fingerprinting for suspension tracking (from KONGOR) be included? → A: No, defer suspension system implementation; stub/skip suspension checks for now
- Q: Should specification use British English or defer language conversion? → A: Defer to implementation phase (code, comments, user-facing strings use British English)
- Q: Should all 200+ protocol commands be implemented initially or phased? → A: Phased implementation prioritising: chat channels, matchmaking groups, matchmaking/queue/match start, player communication
- Q: Which game types/rating categories should be supported (not all 9 from KONGOR)? → A: 5 game types: Campaign Normal, Campaign Casual, Midwars, Riftwars, Public (Campaign = internal name for TMM)

### Session 2025-01-14

- Q: How should we approach the existing custom TCP server infrastructure? → A: Document existing TCP patterns now, add low-priority task (post-MVP) to evaluate migration to System.Net.Sockets async patterns or Kestrel-based TCP server for improved maintainability
- Q: When the buffer approaches 16KB limit, how should protocol messages be handled? → A: Reject message entirely (return error, sender must retry later when buffer clears). Protocol messages have specific structures and sending incomplete payloads would corrupt the connection
- Q: What should the default message rate limit be for flood prevention? → A: 5 messages per 2 seconds per user (configurable via deployment settings)
- Q: What is the maximum number of channels a player can join simultaneously? → A: 8 channels maximum (per HoN legacy protocol documentation)
- Q: Does the protocol support manual leadership transfer commands (player voluntarily promotes another member to leader), or only automatic transfer on leader disconnect/leave? → A: [NEEDS RESEARCH - check HoN/KONGOR for NET_CHAT_CL_TMM_GROUP_PROMOTE_LEADER or similar command]

## Requirements *(mandatory)*

### Functional Requirements

#### Core Connection Management

- **FR-001**: System MUST support three separate TCP listener ports for client connections, game server connections, and server manager connections. Implementation should use a single ChatServer class managing three listener instances with shared infrastructure, leveraging the existing custom TCP server base classes (TCPServer, TCPSession)
- **FR-002**: System MUST implement a generic connection handler pattern supporting polymorphic handling of Client, Server, and Manager connection types
- **FR-003**: System MUST implement keep-alive mechanism sending packets every 60 seconds to maintain persistent connections
- **FR-004**: System MUST track last communication timestamp for each connection and remove stale connections. On disconnect, system MUST immediately clean up all player state (remove from channels, disband groups if leader, remove from queues, mark offline) to prevent ghost users appearing as interactable
- **FR-005**: System MUST implement message framing with 2-byte length prefix and 2-byte command code structure

#### Authentication and Session Management

- **FR-006**: System MUST authenticate client connections using session cookie and authentication hash validation
- **FR-007**: System MUST detect concurrent connection attempts from the same account. When detected, system MUST gracefully disconnect the existing session (with notification) and allow the new connection to proceed, preventing account lockout scenarios
- **FR-008**: System MUST maintain client session state including account ID, super ID, client version, OS information, and chat mode status
- **FR-009**: System MUST validate client version against minimum required version (configurable, currently "4.10.1.0")
- **FR-010**: System MUST support chat mode states: Available, AFK (Away From Keyboard), DND (Do Not Disturb), and Invisible

#### Buffer and Message Processing

- **FR-011**: System MUST implement buffer management with dual read/write offset system and automatic capacity expansion
- **FR-012**: System MUST use async/await patterns for message processing to prevent blocking and ensure scalability
- **FR-013**: System MUST use dependency injection for database context and service dependencies in command processors
- **FR-014**: System MUST support message serialization/deserialization for: int8, int16, int32, int64, float, double, string, boolean types
- **FR-015**: System MUST enforce 16KB buffer limit per channel to prevent buffer overflow attacks. When a protocol message would exceed the buffer limit, the system MUST reject the message entirely and return NET_CHAT_CL_CHANNEL_BUFFER_FULL error response (or appropriate protocol-defined error code) to the sender. Partial message transmission is forbidden as it would corrupt the protocol stream and potentially crash client connections

#### Channel Management

- **FR-016**: System MUST support channel creation with properties: channel ID, clan ID, flags (permanent, hidden, server, reserved), topic, password
- **FR-017**: System MUST maintain channel member lists with admin levels: Officer, Leader, Administrator, Staff
- **FR-018**: System MUST broadcast channel events (join, leave, message) to all channel members in real-time
- **FR-019**: System MUST support channel moderation: kick users, silence users (with expiration), ban users from channels
- **FR-020**: System MUST reject duplicate channel join attempts from the same player
- **FR-020a**: System MUST enforce maximum channel limit of 8 channels per player. When a player attempts to join a 9th channel, system MUST send CHAT_CMD_MAX_CHANNELS error response
- **FR-021**: System MUST validate clan channel membership before allowing join operations
- **FR-022**: System MUST support authorized user lists for restricted channels

#### Private Messaging

- **FR-023**: System MUST support whisper (private message) functionality between any two authenticated players
- **FR-024**: System MUST deliver whisper failed notifications when recipient is offline or unavailable
- **FR-025**: System MUST respect chat mode status when delivering whispers (block whispers for DND, mark as away for AFK)
- **FR-026**: System MUST support buddy/friend list management: add buddy, remove buddy, view buddy list
- **FR-027**: System MUST send online/offline notifications when buddies change connection status

#### Matchmaking Group Management

- **FR-028**: System MUST support group creation with designated group leader (maximum 5 members per team as per team size variations in FR-043)
- **FR-029**: System MUST support group invitation system with accept/decline responses
- **FR-030**: System MUST track group member ready status (ready, not ready) per player
- **FR-031**: System MUST track group member loading status during match initialization
- **FR-032**: System MUST automatically transfer group leadership when leader leaves or is removed. Leadership transfers to member at index 0 (after leader removal, all indices shift down by 1)
- **FR-033**: System MUST allow group members to leave groups voluntarily
- **FR-034**: System MUST disband groups when the last member leaves
- **FR-035**: System MUST support group queue operations: join queue, leave queue, queue status updates

#### Matchmaking Broker

- **FR-036**: System MUST implement matchmaking broker service evaluating group compatibility for match creation
- **FR-037**: System MUST support 5 game types with associated ratings per player: Campaign Normal (ranked TMM with MMR), Campaign Casual (casual TMM with MMR), Midwars (MMR), Riftwars (MMR), and Public (PSR - Public Skill Rating). Note: "Campaign" is the internal name for Team Matchmaking (TMM). PSR works like MMR but uses separate rating container
- **FR-038**: System MUST calculate match compatibility based on average group MMR with configurable rating disparity thresholds
- **FR-039**: System MUST filter matchmaking by game mode preferences: All Pick, Draft, Random, Banning Draft
- **FR-040**: System MUST filter matchmaking by game map preferences: Caldavar, Midwars, Riftwars, Prophets
- **FR-041**: System MUST filter matchmaking by region preferences to minimize latency
- **FR-042**: System MUST prioritize queue wait time in matchmaking algorithm to prevent excessive wait times
- **FR-043**: System MUST support team size variations: 1v1, 2v2, 3v3, 4v4, 5v5

#### Clan Features

- **FR-044**: System MUST maintain clan member roster with roles and permissions
- **FR-045**: System MUST support clan-specific whisper broadcasts to all online clan members
- **FR-046**: System MUST support clan channel creation automatically for registered clans
- **FR-047**: System MUST enforce clan channel access restrictions based on clan membership
- **FR-048**: System MUST support clan rank management: member, officer, leader

#### Game Server Coordination

- **FR-049**: System MUST support game server authentication and registration on dedicated manager port
- **FR-050**: System MUST track game server availability and capacity for matchmaking allocation
- **FR-051**: System MUST allocate available game servers to matched groups
- **FR-052**: System MUST coordinate match lobby creation with game servers
- **FR-053**: System MUST process match status updates from game servers (in progress, completed, aborted)
- **FR-054**: System MUST process match results submission including player statistics, winners, and match duration

#### Statistics and Rating Management

- **FR-055**: System MUST update player statistics after match completion: wins, losses, kills, deaths, assists
- **FR-056**: System MUST calculate rating changes based on match outcome and opponent ratings
- **FR-057**: System MUST maintain separate statistics per game type (Campaign Normal, Campaign Casual, Midwars, Riftwars, Public)
- **FR-058**: [DEFERRED POST-MVP] System SHOULD track match history per player with configurable history depth
- **FR-059**: [DEFERRED POST-MVP] System SHOULD track recent win/loss streaks for rating volatility adjustments
- **FR-060**: [DEFERRED POST-MVP] System SHOULD support season statistics resets at configured intervals

#### Moderation and Anti-Abuse

- **FR-061**: System MUST implement chat flood prevention with configurable message rate limits (default: 5 messages per 2 seconds per user, overridable via configuration)
- **FR-062**: [DEFERRED POST-MVP] System SHOULD track leaver strikes per player for abandonment penalties
- **FR-063**: System MUST enforce account bans during authentication. Suspension system deferred - skip suspension checks in initial implementation
- **FR-064**: System MUST support silence durations with automatic expiration
- **FR-065**: System MUST log all moderation actions (kicks, bans, silences) with administrator attribution

#### Performance and Observability

- **FR-066**: System MUST implement performance counter tracking for each message type processed
- **FR-067**: System MUST support configurable logging levels: Debug, Information, Warning, Error, Critical
- **FR-068**: System MUST log all protocol interactions at appropriate levels for debugging
- **FR-069**: System MUST expose health check endpoints for Aspire orchestration
- **FR-070**: System MUST support graceful shutdown with connection draining

### Key Entities *(include if feature involves data)*

**Entity Relationship Patterns**: All database entities follow MERRICK.DatabaseContext conventions using Entity Framework Core navigation properties for related data access. Relationships are defined bidirectionally where appropriate to enable efficient querying and maintain referential integrity.

**Storage Classification**:
- **Database Entities (Permanent)**: PlayerStatistics, FriendedPeer, Clan, ClanMember - Persist indefinitely in MERRICK.DatabaseContext
- **Redis Entities (Service-Restart-Safe)**: GameServer - Persist across service restarts but temporary
- **In-Memory Entities (Disposable)**: ChatSession, ChatChannel, ChatChannelMember, MatchmakingGroup, MatchmakingGroupMember - Cleared on service shutdown

- **ChatSession**: Represents a connected client with authentication state, account information, chat mode, channels, and matchmaking group membership. Navigation properties link to Account, ChatChannels (collection), and MatchmakingGroup.
- **ChatChannel**: Represents a chat channel with members, admin levels, topic, password, flags (permanent, hidden, clan-specific), and moderation state. Navigation properties link to ChatChannelMembers (collection) and Clan (if clan-specific).
- **ChatChannelMember**: Represents a player's membership in a channel with admin level and silence status. Navigation properties link to Account and ChatChannel.
- **MatchmakingGroup**: Represents a team preparing to queue with members, leader, ready states, loading states, and queue status. Navigation properties link to GroupLeader (Account), MatchmakingGroupMembers (collection).
- **MatchmakingGroupMember**: Represents a player in a matchmaking group with team slot assignment and status flags. Navigation properties link to Account and MatchmakingGroup.
- **GameServer**: Represents a connected game server with availability status, capacity, current match count, and region. May link to current Match entities (collection) if match tracking is in database.
- **PlayerStatistics**: Represents player ratings and match history across 5 game types (Campaign Normal, Campaign Casual, Midwars, Riftwars, Public) with separate rating values per type. Navigation property links to Account. May link to MatchHistory (collection) for detailed match records.
- **FriendedPeer**: Represents a friend relationship between two players with online status tracking. Navigation properties link to both Account entities (requester and target) using appropriate foreign keys.
- **Clan**: Represents a clan organization with members, ranks, and associated channels. Navigation properties link to ClanMembers (collection), ClanLeader (Account), and ClanChannels (collection).
- **ClanMember**: Represents a player's membership in a clan with role and join date. Navigation properties link to Account and Clan.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Players can connect, authenticate, and maintain persistent chat sessions with 99.9% uptime
- **SC-002**: Chat messages are delivered to channel members in under 100ms (P95 latency)
- **SC-003**: System supports at least 10,000 concurrent connected players
- **SC-004**: Matchmaking broker creates balanced matches (rating disparity < 300 MMR) within 3 minutes for 90% of groups
- **SC-005**: Game server allocation completes within 5 seconds of match formation
- **SC-006**: Chat flood prevention blocks spam attempts while allowing legitimate rapid communication (default: 5 messages per 2 seconds, configurable)
- **SC-007**: System maintains protocol compatibility with HoN game clients version 4.10.1.0+
- **SC-008**: Connection keep-alive mechanism prevents premature disconnections (< 1% false disconnect rate)
- **SC-009**: Protocol commands implemented in phased approach prioritising core functionality: (1) chat channels, (2) matchmaking groups, (3) matchmaking/queue/match start, (4) player communication. Additional commands implemented as needed
- **SC-010**: Integration tests validate protocol parity with KONGOR legacy system for all implemented features
- **SC-011**: Player statistics and ratings persist correctly across sessions with zero data loss
- **SC-012**: Clan channels automatically provision when clans are created and restrict access to clan members
- **SC-013**: Moderation actions (kicks, bans, silences) take effect immediately and persist correctly

## Assumptions

1. **Database Schema**: MERRICK.DatabaseContext already contains necessary tables for accounts, clans, player statistics, and chat state or will be extended as needed
2. **Authentication Service**: Session cookie validation can be performed against existing account authentication infrastructure
3. **Game Client Compatibility**: HoN game clients expect protocol version 68 as defined in ChatProtocol.cs
4. **Network Infrastructure**: TCP ports for Client, Server, and Manager connections are configurable via environment variables in Aspire
5. **Rating Algorithm**: MMR/TMR calculations follow industry-standard Elo-like algorithms with configurable K-factors
6. **Message Format**: Legacy KONGOR and HoN systems use 2-byte length prefix + 2-byte command code + variable payload format
7. **Performance Targets**: Target infrastructure can handle 10,000+ concurrent connections based on modern server capabilities
8. **Redis Availability**: Redis caching is available via Aspire orchestration for session state and temporary data
9. **Observability Stack**: Aspire provides built-in logging, metrics, and health check infrastructure
10. **Legacy Code Access**: KONGOR and HoN source code remains accessible in LEGACY directory for reference during implementation

## Dependencies

1. **MERRICK.DatabaseContext**: Database schema and Entity Framework context for persistent data
2. **ASPIRE.ApplicationHost**: Aspire orchestration for service registration, configuration, and lifecycle management
3. **ASPIRE.Common**: Shared utilities and common infrastructure components
4. **Redis**: Caching layer for session state and temporary matchmaking data
5. **SQL Server**: Primary database for accounts, clans, statistics, and persistent chat state
6. **KONGOR Legacy Code**: Reference implementation for message processing patterns and protocol handling
7. **HoN Legacy Code**: Authoritative source for protocol definitions and expected behaviors

## Out of Scope

- **Voice Chat**: Voice communication is handled by separate infrastructure outside chat server scope
- **Web Portal Integration**: Web-based chat interfaces are handled by ZORGATH.WebPortal.API
- **Replay Management**: Replay file storage and distribution is handled by separate services
- **Payment Processing**: In-game purchases and currency are handled by Master Server
- **Content Delivery**: Game client updates and asset downloads are handled by separate CDN infrastructure
- **Anti-Cheat**: Cheat detection and prevention is handled by game client and separate security services
- **Forum Integration**: Forum posts and web-based community features are outside chat server scope
- **Email Notifications**: Email-based notifications are handled by separate notification services

## Notes

- This specification focuses on WHAT the chat server must do, not HOW to implement it technically
- Implementation will follow NEXUS constitution principles: HoN as absolute truth, KONGOR as practical reference
- All legacy protocol behaviors must be maintained for client compatibility
- Modern .NET patterns should be used for implementation while preserving protocol compatibility
- Extensive integration testing required to validate parity with legacy systems
- Chat Server is marked as HIGHEST PRIORITY in NEXUS constitution
- **Architecture Pattern**: The existing TRANSMUTANSTEIN.ChatServer command processor framework is modelled after ASP.NET Core but adapted for TCP (attribute-based routing with ChatCommandAttribute, ISynchronousCommandProcessor, IAsynchronousCommandProcessor). New implementations should follow this established pattern. Minor improvements are welcome if they offer clear superiority while maintaining the ASP.NET Core-inspired philosophy
- **TCP Infrastructure**: TRANSMUTANSTEIN.ChatServer already contains custom TCP server infrastructure (TCPServer base class, TCPSession, ChatSession, etc.). MVP implementation should build on this existing foundation to minimize risk and leverage proven patterns. A low-priority post-MVP task should evaluate potential migration to modern .NET async Socket patterns or Kestrel-based TCP server for improved long-term maintainability and ecosystem integration
- **Terminology**: Prefer NEXUS terminology over KONGOR when conflicts arise. Use established NEXUS codebase terms rather than renaming to match legacy implementations
- **Language**: British English should be used in code comments, documentation, and user-facing strings during implementation
- **Feature Scope**: KONGOR contains some features not desired in NEXUS (e.g., hardware fingerprinting). When encountering KONGOR-specific features, verify before implementing. Suspension system deferred - stub/skip suspension checks initially
- **Implementation Phases**: Protocol commands implemented in priority order: (1) chat channels, (2) matchmaking groups, (3) matchmaking/queue/match start, (4) player communication. This avoids implementing all 200+ commands upfront and focuses on core functionality
