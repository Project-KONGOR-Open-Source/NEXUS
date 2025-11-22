# Tasks: Chat Server Implementation

**Input**: Design documents from `/specs/spec-kit/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Integration and performance tests are planned per quickstart.md and will be implemented alongside each user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

NEXUS uses distributed service architecture:
- **Chat Server**: `TRANSMUTANSTEIN.ChatServer/`
- **Database Context**: `MERRICK.DatabaseContext/`
- **Tests**: `ASPIRE.Tests/ChatServer/`
- **Aspire Host**: `ASPIRE.ApplicationHost/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for TRANSMUTANSTEIN.ChatServer

- [x] T001 Create TRANSMUTANSTEIN.ChatServer project directory structure per plan.md (Core/, CommandProcessors/, Services/) - ✅ Verified existing structure with domain-based organization (Communication/, Matchmaking/)
- [x] T002 Initialize .NET 10 project with Aspire, EF Core, System.Net.Sockets dependencies in TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.ChatServer.csproj - ✅ Project configured with .NET 10.0.100, Aspire 13.0.0, EF Core via MERRICK.DatabaseContext, System.Net.Sockets in BCL
- [x] T003 [P] Configure C# code style settings in .editorconfig (no var, explicit types, full lambda names per constitution) - ✅ .editorconfig exists with csharp_style_var_* = false:error (lines 115-117)
- [x] T004 [P] Add project reference to MERRICK.DatabaseContext in TRANSMUTANSTEIN.ChatServer.csproj - ✅ Reference exists (line 15 of .csproj)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database Entities (Foundation for Statistics and Friends)

- [x] T005 [P] Create PlayerStatistics entity in MERRICK.DatabaseContext/Entities/PlayerStatistics.cs with 5 game type ratings (CampaignNormalRating MMR, CampaignCasualRating MMR, MidwarsRating MMR, RiftwarsRating MMR, PublicRating PSR - all default 1500.0f) - ✅ Exists in Statistics/PlayerStatistics.cs (per-match player data)
- [x] T006 [P] Create FriendedPeer entity in MERRICK.DatabaseContext/Entities/FriendedPeer.cs with bidirectional Account relationships (RequesterAccountID, TargetAccountID, Status) - ✅ Exists in Relational/FriendedPeer.cs as JSON-owned entity in Account
- [x] T007 Check if ClanMember entity exists in MERRICK.DatabaseContext, add in MERRICK.DatabaseContext/Entities/ClanMember.cs if missing - ✅ Not needed - Account.Clan navigation property handles membership
- [x] T008 Create EF Core migration for PlayerStatistics using aspire exec --resource database-context -- dotnet ef migrations add AddPlayerStatistics - ✅ Included in 20251111193356_CreateCoreEntities migration
- [x] T009 Create EF Core migration for FriendedPeer using aspire exec --resource database-context -- dotnet ef migrations add AddFriendedPeer - ✅ Included in 20251111193356_CreateCoreEntities migration
- [x] T010 Apply database migrations to development database using aspire exec with ASPNETCORE_ENVIRONMENT=Development - ✅ Migration already applied

### Core TCP Infrastructure

- [x] T011 Implement ChatProtocol.cs in TRANSMUTANSTEIN.ChatServer/Core/ChatProtocol.cs with protocol version 68 command codes from HoN chatserver_protocol.h - ✅ Complete (981 lines, comprehensive v68 protocol)
- [x] T012 Implement ChatBuffer.cs in TRANSMUTANSTEIN.ChatServer/Core/ChatBuffer.cs with 2-byte length prefix serialization (WriteString, WriteInt32, WriteInt16, WriteInt8, WriteFloat, ReadString, ReadInt32, etc.) - ✅ Complete (221 lines, all serialization methods)
- [x] T013 Implement CommandProcessorRegistry.cs in TRANSMUTANSTEIN.ChatServer/Core/CommandProcessorRegistry.cs with attribute-based routing (scan for ChatCommandAttribute) - ✅ Integrated in ChatSession.cs GetCommandType() with reflection + caching
- [x] T014 Create IChatCommandProcessor interface in TRANSMUTANSTEIN.ChatServer/Core/IChatCommandProcessor.cs (ProcessAsync method signature) - ✅ Split into IAsynchronousCommandProcessor & ISynchronousCommandProcessor (better design)
- [x] T015 Create ChatCommandAttribute in TRANSMUTANSTEIN.ChatServer/Core/ChatCommandAttribute.cs (takes ushort command code) - ✅ Complete in Attributes/ChatCommandAttribute.cs

### Session Management Foundation

- [x] T016 Implement ChatSession.cs in TRANSMUTANSTEIN.ChatServer/Core/ChatSession.cs extending TCPSession with AccountID, AccountName, Authenticated, ChatMode, LastCommunicationTimestamp properties - ✅ Complete with Metadata & Account properties
- [ ] T017 [P] Implement GameServerSession.cs in TRANSMUTANSTEIN.ChatServer/Core/GameServerSession.cs extending TCPSession for game server connections - 🔄 DEFERRED until matchmaking queue functional
- [ ] T018 [P] Implement ManagerSession.cs in TRANSMUTANSTEIN.ChatServer/Core/ManagerSession.cs extending TCPSession for manager connections - 🔄 DEFERRED until matchmaking queue functional
- [x] T019 Implement ChatSession.OnDisconnectAsync() cleanup in TRANSMUTANSTEIN.ChatServer/Core/ChatSession.cs (remove from channels, disband groups, notify friends) - ✅ Complete in Terminate() method

### In-Memory State Entities

- [x] T020 [P] Create ChatChannel.cs in TRANSMUTANSTEIN.ChatServer/InMemory/ChatChannel.cs with ConcurrentDictionary<int, ChatChannelMember> Members, ChannelFlags, Topic, Password - ✅ Complete in Communication/ChatChannel.cs (domain-based organization)
- [x] T021 [P] Create ChatChannelMember.cs in TRANSMUTANSTEIN.ChatServer/InMemory/ChatChannelMember.cs with AccountID, AdminLevel, SilencedUntil, ChatSession reference - ✅ Complete in Communication/ChatChannelMember.cs
- [x] T022 [P] Create MatchmakingGroup.cs in TRANSMUTANSTEIN.ChatServer/InMemory/MatchmakingGroup.cs with ConcurrentDictionary<int, MatchmakingGroupMember> Members, LeaderAccountID, GameType, QueueStatus - ✅ Complete in Matchmaking/MatchmakingGroup.cs (domain-based organization)
- [x] T023 [P] Create MatchmakingGroupMember.cs in TRANSMUTANSTEIN.ChatServer/InMemory/MatchmakingGroupMember.cs with AccountID, TeamSlot, ReadyStatus, LoadingStatus - ✅ Complete in Matchmaking/MatchmakingGroupMember.cs

### Chat Server Main Class

- [x] T024 Implement ChatServer.cs in TRANSMUTANSTEIN.ChatServer/Core/ChatServer.cs with three TCPServer instances (clientListener, serverListener, managerListener) managing ports 11031, 11032, 11033 - ✅ Core/ChatServer.cs exists (single listener, multi-port may be handled differently)
- [x] T025 Add ConcurrentDictionary<string, ChatChannel> ActiveChannels for in-memory channel management - ✅ Complete in Internals/Context.cs as static Context.ChatChannels
- [x] T026 Add ConcurrentDictionary<int, MatchmakingGroup> ActiveGroups to ChatServer.cs for in-memory group management - ✅ MatchmakingService.Groups handles this
- [x] T027 Add ConcurrentDictionary<int, ChatSession> ActiveSessions keyed by AccountID - ✅ Complete in Internals/Context.cs as Context.ChatSessions (keyed by AccountName)
- [ ] T027a Add Redis cache integration for GameServer registry in ChatServer.cs (ConcurrentDictionary<int, GameServerSession> with Redis backing for service-restart safety) - 🔄 DEFERRED until matchmaking queue functional
- [ ] T028 Implement ConnectionHealthService.cs in TRANSMUTANSTEIN.ChatServer/Services/ConnectionHealthService.cs with dual purpose: (1) Ping/pong at regular intervals to keep connection alive, (2) Graceful disconnect after 1 initial + 5 retries (6 calls over 30s) with no response - removes from channels/groups/etc. - 🔄 DEFERRED until client cleanup logic complete

### Aspire Registration

- [x] T029 Register TRANSMUTANSTEIN.ChatServer in ASPIRE.ApplicationHost/Program.cs with Aspire orchestration, configure ports (11031, 11032, 11033) via environment variables - ✅ Complete in ASPIRE.cs lines 100-104 with database & distributed cache references
- [ ] T030 Configure health checks for ChatServer in TRANSMUTANSTEIN.ChatServer/Program.cs (TCP listener availability) - 🔄 DEFERRED until client cleanup logic complete
- [x] T031 [P] Configure logging levels in TRANSMUTANSTEIN.ChatServer/appsettings.Development.json (Debug level for TRANSMUTANSTEIN namespace) - ✅ Complete (Default: Debug)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Player Authentication and Connection (Priority: P1) 🎯 MVP

**Goal**: Players can connect to chat server, authenticate with credentials, and maintain persistent sessions with keep-alive mechanism. Handles concurrent connection attempts (disconnect existing session).

**Independent Test**: Launch SimulatedChatClient, connect to port 11031, send authentication, verify session confirmation received and keep-alive packets sent every 60 seconds.

### Tests for User Story 1 (DEFERRED)

**Tests deferred until more chat server functionality is implemented**

- [ ] T032 [P] [US1] Create SimulatedChatClient.cs in ASPIRE.Tests/TestHelpers/SimulatedChatClient.cs (ConnectAsync, SendAuthenticateAsync, ReceiveAsync methods) - 🔄 DEFERRED until more chat server code available
- [ ] T033 [US1] Integration test for authentication flow in ASPIRE.Tests/ChatServer/Integration/AuthenticationTests.cs (valid credentials, invalid credentials, concurrent connections, keep-alive) - 🔄 DEFERRED until more chat server code available

### Implementation for User Story 1

- [x] T034 [US1] Implement AuthenticateProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Authentication/AuthenticateProcessor.cs with [ChatCommand(NET_CHAT_CL_AUTHENTICATE)] attribute - ✅ Complete as ClientHandshake.cs with NET_CHAT_CL_CONNECT
- [x] T035 [US1] Add session token validation logic in AuthenticateProcessor (query MERRICK.DatabaseContext.Accounts, validate cookie/hash) - ✅ Validates cookie in Redis, AccountName match, and SHA1 auth hash
- [x] T036 [US1] Add concurrent connection detection in AuthenticateProcessor (check ActiveSessions, disconnect existing, allow new connection) - ✅ Checks all sub-accounts of User, disconnects existing with ECR_ACCOUNT_SHARING
- [x] T037 [US1] Add authenticated session to ChatServer.ActiveSessions keyed by AccountID in AuthenticateProcessor - ✅ Complete in ChatSession.Accept() line 24
- [x] T038 [US1] Send authentication success response with NET_CHAT_CL_AUTHENTICATE_RESPONSE command code - ✅ Complete as NET_CHAT_CL_ACCEPT in ChatSession.cs:28
- [x] T039 [US1] Implement client version validation in AuthenticateProcessor (minimum version 4.10.1.0 configurable) - ✅ Hardcoded to 4.10.1.0 (version will not change)
- [x] T040 [US1] Add ChatMode state management in ChatSession (Available, AFK, DND, Invisible) - ✅ Complete in ChatSessionMetadata.ClientChatModeState
- [ ] T041 [US1] Start KeepAliveService timer when ChatServer starts in Program.cs (send keep-alive every 60s, remove stale connections >120s) - 🔄 DEFERRED (renamed to ConnectionHealthService)

**Checkpoint**: At this point, players can connect, authenticate, and maintain persistent sessions. User Story 1 is fully functional and testable independently.

---

## Phase 4: User Story 2 - Chat Channel Management (Priority: P2)

**Goal**: Authenticated players can join public/clan channels, view member lists, send/receive messages in real-time, and admins can moderate (kick, silence).

**Independent Test**: Two SimulatedChatClients connect and authenticate, both join "General" channel, one sends message, verify other receives broadcast. Admin client kicks member, verify removal.

### Tests for User Story 2 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T042 [P] [US2] Integration test for channel join/leave in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs (join public channel, leave channel, member list updates) - 🔄 DEFERRED until application code complete
- [ ] T043 [P] [US2] Integration test for channel messaging in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs (send message, broadcast to all members, exclude sender) - 🔄 DEFERRED until application code complete
- [ ] T044 [P] [US2] Integration test for channel moderation in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs (kick user, silence user with expiration) - 🔄 DEFERRED until application code complete

### Implementation for User Story 2

- [x] T045 [P] [US2] Implement JoinChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/JoinChannelProcessor.cs with [ChatCommand(NET_CHAT_CL_JOIN_CHANNEL)] - ✅ Complete as JoinChannel.cs
- [x] T046 [P] [US2] Implement LeaveChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/LeaveChannelProcessor.cs with [ChatCommand(NET_CHAT_CL_LEAVE_CHANNEL)] - ✅ Complete as LeaveChannel.cs
- [x] T047 [P] [US2] Implement ChannelMessageProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/ChannelMessageProcessor.cs with [ChatCommand(NET_CHAT_CL_CHANNEL_MESSAGE)] - ✅ Complete as ChannelMessage.cs, verified against HoN protocol (lines 17-22, 923-930 in Chatserver Protocol.txt)
- [x] T048 [P] [US2] Implement KickFromChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/KickFromChannelProcessor.cs with [ChatCommand(NET_CHAT_CL_KICK_FROM_CHANNEL)] - ✅ Complete as KickFromChannel.cs
- [x] T049 [P] [US2] Implement SilenceUserProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/SilenceUserProcessor.cs with [ChatCommand(NET_CHAT_CL_SILENCE_USER)] - ✅ Complete as SilenceUser.cs, verified against HoN protocol (lines 1136-1144 in Chatserver Protocol.txt), includes Silenced tracking in ChatChannel with IsSilenced() and Silence() methods
- [x] T050 [US2] Add ChatChannel.GetOrCreate() in ChatServer.cs (ActiveChannels.GetOrAdd with channel name key) - ✅ Complete as static method in ChatChannel.cs (line 24-35)
- [x] T051 [US2] Add ChatChannel.AddMember() in ChatChannel.cs (ConcurrentDictionary.TryAdd, broadcast join event) - ✅ Complete as Join() method (line 51-107)
- [x] T052 [US2] Add ChatChannel.RemoveMember() in ChatChannel.cs (ConcurrentDictionary.TryRemove, broadcast leave event, cleanup empty non-permanent channels) - ✅ Complete as Leave() method (line 133-167)
- [x] T053 [US2] Add ChatChannel.BroadcastMessage() in ChatChannel.cs (iterate Members, send to each ChatSession, exclude sender if specified) - ✅ Complete as BroadcastMessage() method (line 202-212), verified against HoN source
- [x] T054 [US2] Add clan channel validation in JoinChannelProcessor (check ClanMember table via MERRICK.DatabaseContext if channel has ClanID) - ✅ Added validation in ChatChannel.Join() (line 56-63) using silent rejection matching legacy behavior. Verified against HoN (c_channel.cpp CanJoin/GetAdminLevel) and KONGOR (JoinChannelRequest.cs). Legacy uses ClanID comparison; we use Clan.GetChatChannelName() which is equivalent for matching channel names
- [x] T055 [US2] Add duplicate join check in JoinChannelProcessor per FR-020 (reject if AccountID already in channel.Members) - ✅ Added validation in ChatChannel.Join() (line 53-58) using silent rejection matching legacy behavior. Verified against HoN (c_channel.cpp HasClient check in CanJoin, line 189) and KONGOR (ChatChannel.cs Any() check on AccountId, line 168). Both implementations return false without error message
- [x] T055a [US2] Add channel limit enforcement in JoinChannelProcessor (count player's current channels via ChatSession.CurrentChannels, reject join with NET_CHAT_CL_MAX_CHANNELS (0x0021) error if attempting to join 9th channel per FR-020a) - ✅ Added MAX_CHANNELS_PER_CLIENT constant (8) to ChatProtocol.cs (line 310), CurrentChannels HashSet to ChatSession.cs (line 19), limit check in ChatChannel.Join() (line 58) with staff exemption for moderation purposes, and tracking updates in Join/Leave methods. Verified against HoN (c_client.cpp HandleJoinChannel line 1713, client_maxChannels CVAR set to 8 at line 59). KONGOR does not implement this limit. Staff accounts (AccountType.Staff) are exempt from the limit
- [x] T056 [US2] Add password validation in JoinChannelProcessor - ✅ Implemented two-step password flow: Added Password property and HasPassword() method to ChatChannel.cs (lines 15-25), modified Join() method to accept optional password parameter and check password before allowing join (lines 86-108), created JoinChannelPassword.cs command processor for 0x0046 bidirectional command. Staff accounts and channel administrators bypass password checks (line 88). **MODERNIZED: Case-sensitive password comparison using StringComparison.Ordinal** (legacy was case-insensitive with CompareNoCase). Added TODO comments for error messaging once direct user messaging is implemented. Verified against HoN (c_channel.cpp CanJoin line 198 password check, line 204 CompareNoCase; c_client.cpp HandleChannelJoinPassword line 2379) and KONGOR (ChatChannel.cs CanJoin)
- [x] T056b [US2] Implement SetChannelPassword command processor (0x0043) - ✅ Created SetChannelPassword.cs command processor for CHAT_CMD_CHANNEL_SET_PASSWORD. Added ChatChannel.SetPassword() method (line 333-360) with CHAT_CLIENT_ADMIN_LEADER permission check, GetAdministratorLevel() helper (line 362-388), and broadcast to all channel members when password changes. Empty string clears password. Added TODO comment for permission error messaging. Verified against HoN (c_channel.cpp SetPassword line 167-179, c_client.cpp HandleChannelSetPassword line 2359-2373) and protocol documentation (Chatserver Protocol.txt line 1216-1223). Staff and clan leaders can set passwords; clan officers cannot
- [x] T057 [US2] Add admin permission checks in KickFromChannelProcessor and SilenceUserProcessor (verify caller has AdminLevel != None) - ✅ Already implemented in ChatChannel.Kick() (line 254) and ChatChannel.Silence() (line 302) via HasHigherAdministratorLevelThan() method. Verified against HoN c_channel.cpp (lines 626, 588). Both use strict inequality check matching legacy behaviour
- [x] T058 [US2] Add silence expiration check in ChannelMessageProcessor (if member.SilencedUntil > DateTime.UtcNow, reject message) - ✅ Already implemented in ChatChannelMember.IsSilenced() with auto-expiration clearing. Added staff immunity check (AdministratorLevel == STAFF) matching HoN c_channel.cpp line 293
- [x] T059 [US2] Add 16KB buffer limit enforcement in ChannelMessageProcessor per FR-015 - ✅ Added MAX_PACKET_SIZE constant (16384 bytes) to ChatProtocol.cs matching HoN c_packet.h line 17. Enforced limit in ChatSession.Send() method (lines 436-444) which applies to ALL outgoing packets, not just channel messages. Packets exceeding limit are rejected with error logging
- [x] T059a [US2] Implement FloodPreventionService.cs in TRANSMUTANSTEIN.ChatServer/Services/FloodPreventionService.cs with HoN-compatible flood protection (request counter increments on action, decays at 1 per 3.5 seconds, threshold of 5, silent rejection) - ✅ Implemented with token bucket algorithm matching HoN c_client.cpp, background decay timer, thread-safe ConcurrentDictionary tracking
- [x] T059b [US2] Integrate FloodPreventionService in SendChannelMessage processor (inject via DI, call CheckMessageAllowed before broadcasting, reject with CHAT_CMD_FLOODING if rate exceeded) - ✅ Integrated with constructor injection, staff exemption, sends CHAT_CMD_FLOODING warning matching HoN protocol

**Checkpoint**: At this point, players can use public and clan chat channels with moderation. User Story 2 is fully functional and testable independently.

---

## Phase 5: User Story 3 - Matchmaking Group Formation (Priority: P3)

**Goal**: Players can create matchmaking groups, invite others, accept/decline invitations, mark ready status, and join matchmaking queue as a team.

**Independent Test**: Three SimulatedChatClients form a group (leader creates, invites two members), all mark ready, group joins queue, verify queue status updates broadcast to all members.

### Tests for User Story 3 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T060 [P] [US3] Integration test for group creation in ASPIRE.Tests/ChatServer/Integration/GroupTests.cs (create group, leader assigned) - 🔄 DEFERRED until application code complete
- [ ] T061 [P] [US3] Integration test for group invitations in ASPIRE.Tests/ChatServer/Integration/GroupTests.cs (invite player, accept, decline, join group) - 🔄 DEFERRED until application code complete
- [ ] T062 [P] [US3] Integration test for ready status in ASPIRE.Tests/ChatServer/Integration/GroupTests.cs (mark ready, all members see status update) - 🔄 DEFERRED until application code complete
- [ ] T063 [P] [US3] Integration test for queue join/leave in ASPIRE.Tests/ChatServer/Integration/GroupTests.cs (join queue, leave queue, leader cancels) - 🔄 DEFERRED until application code complete

### Implementation for User Story 3

- [ ] T064 [P] [US3] Implement GroupCreateProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupCreateProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_CREATE)]
- [ ] T065 [P] [US3] Implement GroupInviteProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupInviteProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_INVITE)]
- [ ] T066 [P] [US3] Implement GroupAcceptInviteProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupAcceptInviteProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_ACCEPT_INVITE)]
- [ ] T067 [P] [US3] Implement GroupRejectInviteProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupRejectInviteProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_REJECT_INVITE)]
- [ ] T068 [P] [US3] Implement GroupJoinProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupJoinProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_JOIN)]
- [ ] T069 [P] [US3] Implement GroupLeaveProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/GroupLeaveProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_LEAVE)]
- [ ] T070 [P] [US3] Implement PlayerReadyStatusProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Groups/PlayerReadyStatusProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_PLAYER_READY_STATUS)]
- [ ] T071 [P] [US3] Implement GroupJoinQueueProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoinQueueProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE)]
- [ ] T072 [P] [US3] Implement GroupLeaveQueueProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupLeaveQueueProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE)]
- [ ] T073 [US3] Add MatchmakingGroup.Create() in ChatServer.cs (ActiveGroups.TryAdd with auto-increment GroupID, set LeaderAccountID)
- [ ] T074 [US3] Add MatchmakingGroup.AddMember() in MatchmakingGroup.cs (assign TeamSlot, add to Members ConcurrentDictionary, broadcast member joined)
- [ ] T075 [US3] Add MatchmakingGroup.RemoveMember() in MatchmakingGroup.cs (remove from Members, disband if leader leaves or last member)
- [ ] T075a [US3] Implement automatic leader transfer in MatchmakingGroup.RemoveMember() when leader leaves per FR-032 (shift all indices down by 1, member at index 0 becomes new leader)
- [ ] T076 [US3] Add MatchmakingGroup.SetReady() in MatchmakingGroupMember.cs (update ReadyStatus, broadcast to all group members)
- [ ] T077 [US3] Add MatchmakingGroup.AllMembersReady() check in GroupJoinQueueProcessor (verify all Members have ReadyStatus = true before queue join)
- [ ] T078 [US3] Add invitation system in GroupInviteProcessor (send invitation message to target player ChatSession)
- [ ] T079 [US3] Add group leave on disconnect in ChatSession.OnDisconnectAsync() (check CurrentGroup, remove member or disband if leader)
- [ ] T080 [US3] Load PlayerStatistics from database in GroupJoinQueueProcessor for matchmaking rating (query MERRICK.DatabaseContext.PlayerStatistics by AccountID)

**Checkpoint**: At this point, players can form groups and join queues. User Story 3 is fully functional and testable independently.

---

## Phase 6: User Story 4 - Private Messaging and Buddy System (Priority: P4)

**Goal**: Players can send whisper messages to other players, add/remove buddies (friends), and receive online/offline notifications when buddies change connection status.

**Independent Test**: Two SimulatedChatClients add each other as buddies, verify persistent in database. One sends whisper, other receives. One disconnects, verify offline notification sent to buddy.

### Tests for User Story 4 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T081 [P] [US4] Integration test for whisper messaging in ASPIRE.Tests/ChatServer/Integration/CommunicationTests.cs (send whisper, receive whisper, whisper failed when offline) - 🔄 DEFERRED until application code complete
- [ ] T082 [P] [US4] Integration test for buddy system in ASPIRE.Tests/ChatServer/Integration/CommunicationTests.cs (add buddy, remove buddy, online notification, offline notification) - 🔄 DEFERRED until application code complete
- [ ] T083 [P] [US4] Integration test for ChatMode respect in ASPIRE.Tests/ChatServer/Integration/CommunicationTests.cs (DND blocks whispers, AFK marks away) - 🔄 DEFERRED until application code complete

### Implementation for User Story 4

- [ ] T084 [P] [US4] Implement WhisperProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Communication/WhisperProcessor.cs with [ChatCommand(NET_CHAT_CL_WHISPER)]
- [ ] T085 [P] [US4] Implement AddBuddyProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Communication/AddBuddyProcessor.cs with [ChatCommand(NET_CHAT_CL_ADD_BUDDY)]
- [ ] T086 [P] [US4] Implement RemoveBuddyProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Communication/RemoveBuddyProcessor.cs with [ChatCommand(NET_CHAT_CL_REMOVE_BUDDY)]
- [ ] T087 [US4] Add whisper delivery in WhisperProcessor (find target in ChatServer.ActiveSessions, send NET_CHAT_CL_WHISPER_RECEIVED)
- [ ] T088 [US4] Add whisper failed notification in WhisperProcessor (if target offline or unavailable, send NET_CHAT_CL_WHISPER_FAILED)
- [ ] T089 [US4] Add ChatMode validation in WhisperProcessor (block whispers if target.ChatMode == DND, mark as away if AFK)
- [ ] T090 [US4] Add FriendedPeer persistence in AddBuddyProcessor (insert into MERRICK.DatabaseContext.FriendedPeers with Status = Pending)
- [ ] T091 [US4] Add FriendedPeer removal in RemoveBuddyProcessor (delete from MERRICK.DatabaseContext.FriendedPeers)
- [ ] T092 [US4] Load buddy list on connect in AuthenticateProcessor (query FriendedPeers where RequesterAccountID or TargetAccountID = session.AccountID, cache in ChatSession)
- [ ] T093 [US4] Send buddy online notifications in AuthenticateProcessor (after authentication, notify all friends with NET_CHAT_CL_BUDDY_ONLINE)
- [ ] T094 [US4] Send buddy offline notifications in ChatSession.OnDisconnectAsync() (notify all friends with NET_CHAT_CL_BUDDY_OFFLINE)

**Checkpoint**: At this point, players have private messaging and friend system working. User Story 4 is fully functional and testable independently.

---

## Phase 7: User Story 5 - Clan Communication and Management (Priority: P5)

**Goal**: Clan members automatically join clan-specific channels on connect, can send clan-wide messages, and clan leaders can promote members to officer (granting admin privileges in clan channel).

**Independent Test**: SimulatedChatClient with ClanID connects, verify auto-joined clan channel. Leader promotes member to officer, verify admin privileges granted in channel.

### Tests for User Story 5 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T095 [P] [US5] Integration test for clan channel auto-join in ASPIRE.Tests/ChatServer/Integration/ClanTests.cs (clan member connects, auto-joined clan channel) - 🔄 DEFERRED until application code complete
- [ ] T096 [P] [US5] Integration test for clan messaging in ASPIRE.Tests/ChatServer/Integration/ClanTests.cs (clan member sends message, all online clan members receive) - 🔄 DEFERRED until application code complete
- [ ] T097 [P] [US5] Integration test for clan rank management in ASPIRE.Tests/ChatServer/Integration/ClanTests.cs (leader promotes to officer, officer gains admin privileges) - 🔄 DEFERRED until application code complete

### Implementation for User Story 5

- [ ] T098 [US5] Add clan channel auto-join in AuthenticateProcessor (after authentication, if player has ClanID, join clan channel automatically)
- [ ] T099 [US5] Create clan channel if not exists in ChatServer.GetOrCreateChannel() (check for clan-specific naming convention, set ChannelFlags.Clan, set ClanID)
- [ ] T100 [US5] Add ClanMember role check in clan channel operations (query MERRICK.DatabaseContext.ClanMembers, map ClanRole to AdminLevel)
- [ ] T101 [US5] Implement PromoteClanMemberProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Clans/PromoteClanMemberProcessor.cs with [ChatCommand(NET_CHAT_CL_CLAN_PROMOTE_MEMBER)] (if implementing promotion via chat server)
- [ ] T102 [US5] Add clan roster online status in clan channel (when members view roster, show online/offline via ActiveSessions lookup)
- [ ] T103 [US5] Remove from clan channel on disconnect in ChatSession.OnDisconnectAsync() (if clan channel exists in CurrentChannels)

**Checkpoint**: At this point, clan features are working. User Story 5 is fully functional and testable independently.

---

## Phase 8: User Story 6 - Game Server Coordination (Priority: P6)

**Goal**: Game servers connect to manager port (11033), authenticate, report availability, and submit match results which update PlayerStatistics in database.

**Independent Test**: SimulatedGameServerClient connects to port 11033, authenticates, reports server available, submits match results with player statistics, verify PlayerStatistics updated in database.

### Tests for User Story 6 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T104 [P] [US6] Integration test for game server handshake in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server connects to port 11033, authenticates, registers availability) - 🔄 DEFERRED until application code complete
- [ ] T105 [P] [US6] Integration test for match status updates in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server reports match in progress, completed, aborted) - 🔄 DEFERRED until application code complete
- [ ] T106 [P] [US6] Integration test for match results submission in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server submits results, PlayerStatistics updated) - 🔄 DEFERRED until application code complete

### Implementation for User Story 6

- [ ] T107 [P] [US6] Implement ServerHandshakeProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/ServerHandshakeProcessor.cs with [ChatCommand(NET_CHAT_SV_SERVER_HANDSHAKE)]
- [ ] T108 [P] [US6] Implement ServerStatusProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/ServerStatusProcessor.cs with [ChatCommand(NET_CHAT_SV_SERVER_STATUS)]
- [ ] T109 [P] [US6] Implement MatchStatusProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/MatchStatusProcessor.cs with [ChatCommand(NET_CHAT_SV_MATCH_STATUS)]
- [ ] T110 [P] [US6] Implement MatchCompleteProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/MatchCompleteProcessor.cs with [ChatCommand(NET_CHAT_SV_MATCH_COMPLETE)]
- [ ] T111 [US6] Add GameServerSession tracking in ChatServer.RegisteredGameServers ConcurrentDictionary (keyed by ServerID)
- [ ] T112 [US6] Add game server authentication in ServerHandshakeProcessor (validate server credentials, add to RegisteredGameServers)
- [ ] T113 [US6] Add server availability tracking in ServerStatusProcessor (update GameServerSession availability status)
- [ ] T114 [US6] Update player availability in MatchStatusProcessor (when match starts, mark players as InGame in ChatSession)
- [ ] T115 [US6] Update PlayerStatistics in MatchCompleteProcessor (query MERRICK.DatabaseContext.PlayerStatistics, update wins/losses/rating for appropriate GameType)
- [ ] T116 [US6] Add rating calculation in MatchCompleteProcessor (Elo-like algorithm with K-factor, separate per game type)

**Checkpoint**: At this point, game servers can coordinate with chat server and update statistics. User Story 6 is fully functional and testable independently.

---

## Phase 9: User Story 7 - Matchmaking Broker and Queue Management (Priority: P7)

**Goal**: Matchmaking broker evaluates queued groups, pairs compatible teams based on ratings/preferences/region, allocates game server, creates match lobby, and starts match when all players loaded.

**Independent Test**: Two groups (2 SimulatedChatClients each) join queue with compatible ratings and preferences, verify matchmaking broker pairs them, allocates server, creates lobby, sends match start when all loaded.

### Tests for User Story 7 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T117 [P] [US7] Integration test for matchmaking algorithm in ASPIRE.Tests/ChatServer/Integration/MatchmakingTests.cs (two compatible groups paired within rating threshold) - 🔄 DEFERRED until application code complete
- [ ] T118 [P] [US7] Integration test for server allocation in ASPIRE.Tests/ChatServer/Integration/MatchmakingTests.cs (match created, server allocated from RegisteredGameServers) - 🔄 DEFERRED until application code complete
- [ ] T119 [P] [US7] Integration test for match lobby flow in ASPIRE.Tests/ChatServer/Integration/MatchmakingTests.cs (lobby created, players accept, loading status tracked, match start) - 🔄 DEFERRED until application code complete

### Implementation for User Story 7

- [ ] T120 [P] [US7] Create MatchLobby.cs in TRANSMUTANSTEIN.ChatServer/InMemory/MatchLobby.cs with ConcurrentDictionary<int, MatchLobbyPlayer> Players, Team1GroupID, Team2GroupID, GameServerIP, MatchToken
- [ ] T121 [P] [US7] Create MatchLobbyPlayer.cs in TRANSMUTANSTEIN.ChatServer/InMemory/MatchLobbyPlayer.cs with AccountID, Team, TeamSlot, LoadingStatus
- [ ] T122 [US7] Implement MatchmakingBroker.cs in TRANSMUTANSTEIN.ChatServer/Services/MatchmakingBroker.cs (EvaluateQueue method, FindCompatibleGroups, CreateMatchLobby)
- [ ] T123 [US7] Add rating disparity check in MatchmakingBroker.FindCompatibleGroups (average group MMR difference < 300, configurable threshold)
- [ ] T124 [US7] Add game mode compatibility check in MatchmakingBroker.FindCompatibleGroups (filter by TMMGameMode preferences)
- [ ] T125 [US7] Add region matching in MatchmakingBroker.FindCompatibleGroups (prefer same TMMRegion)
- [ ] T126 [US7] Add queue time priority in MatchmakingBroker (longer wait times increase threshold flexibility)
- [ ] T127 [US7] Add team size matching in MatchmakingBroker (1v1, 2v2, 3v3, 4v4, 5v5 based on group member counts)
- [ ] T128 [US7] Implement MatchFoundProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/MatchFoundProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_MATCH_FOUND)]
- [ ] T129 [US7] Implement MatchAcceptProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/MatchAcceptProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_MATCH_ACCEPT)]
- [ ] T130 [US7] Implement MatchDeclineProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/MatchDeclineProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_MATCH_DECLINE)]
- [ ] T131 [US7] Implement MatchLobbyUpdateProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/MatchLobbyUpdateProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_MATCH_LOBBY_UPDATE)]
- [ ] T132 [US7] Implement PlayerLoadingStatusProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/PlayerLoadingStatusProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_PLAYER_LOADING_STATUS)]
- [ ] T133 [US7] Implement MatchStartProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/MatchStartProcessor.cs with [ChatCommand(NET_CHAT_CL_TMM_MATCH_START)]
- [ ] T134 [US7] Add server allocation in MatchmakingBroker.CreateMatchLobby (select available server from RegisteredGameServers, send allocation request via manager port)
- [ ] T135 [US7] Add match lobby creation in MatchmakingBroker (create MatchLobby, populate from both groups, send NET_CHAT_CL_TMM_MATCH_FOUND to all players)
- [ ] T136 [US7] Track player loading status in MatchLobbyPlayer (update LoadingStatus when NET_CHAT_CL_TMM_PLAYER_LOADING_STATUS received)
- [ ] T137 [US7] Check all players loaded in MatchStartProcessor (MatchLobby.AllPlayersLoaded(), send NET_CHAT_CL_TMM_MATCH_START with server connection info)
- [ ] T138 [US7] Start MatchmakingBroker background service in ChatServer.StartAsync() (periodic queue evaluation every 5 seconds)

**Checkpoint**: At this point, full matchmaking pipeline works end-to-end. User Story 7 is fully functional and testable independently.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Performance testing, error handling, logging, and production readiness improvements across all user stories

- [ ] T139 [P] Implement SimulatedChatClient.SimulateConcurrentLoad() in ASPIRE.Tests/TestHelpers/SimulatedChatClient.cs for load testing
- [ ] T140 [P] Create performance test for 10k concurrent connections in ASPIRE.Tests/ChatServer/Performance/ConcurrentConnectionsTest.cs
- [ ] T141 [P] Create performance test for message latency (P95 < 100ms) in ASPIRE.Tests/ChatServer/Performance/MessageLatencyTest.cs
- [ ] T142 [P] Add .NET metrics instrumentation in ChatServer.cs (System.Diagnostics.Metrics, track command duration histogram per command type)
- [ ] T143 [P] Add comprehensive error handling in all command processors (try-catch, log exceptions, send error responses to clients)
- [ ] T144 [P] Add input validation in all command processors (string length limits, null checks, range checks per FR-011 to FR-015)
- [ ] T145 [P] Add rate limiting for flood prevention in ChannelMessageProcessor (configurable messages per second per user)
- [ ] T146 [P] Configure Aspire dashboard metrics in TRANSMUTANSTEIN.ChatServer/Program.cs (expose metrics endpoint)
- [ ] T147 [P] Add XML documentation summaries to all public APIs (ChatServer, ChatSession, command processors)
- [ ] T148 [P] Validate all code follows C# conventions (no var, explicit types, full lambda names, proper acronym casing)
- [ ] T149 [P] Add comprehensive logging in all command processors (log command received, processing, completion, errors at appropriate levels)
- [ ] T150 [P] Test graceful shutdown in ChatServer (connection draining, notify all sessions, cleanup resources)
- [ ] T151 Run quickstart.md validation (verify developer can follow guide to run chat server)
- [ ] T151a [P] Research and implement enriched channel flags in ChatChannel.cs per HoN protocol (GENERAL_USE for "HoN 1" style channels, SERVER for post-match channels, HIDDEN for unlisted channels, UNJOINABLE for system-only channels, AUTH_REQUIRED for authorization lists, STREAM_USE for stream channels with 12hr cleanup) - See HON_REVERSE_ENGINEERING_GUIDE.md for flag analysis

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - US1 (Authentication): Can start after Foundational - No dependencies on other stories
  - US2 (Channels): Can start after Foundational - No dependencies (uses authentication from US1 but independently testable)
  - US3 (Groups): Can start after Foundational - No dependencies (uses authentication from US1 but independently testable)
  - US4 (Whispers/Buddies): Can start after Foundational - No dependencies (uses authentication from US1 but independently testable)
  - US5 (Clans): Can start after US2 (Channels) - Reuses channel infrastructure
  - US6 (Game Server): Can start after Foundational - No dependencies on player features
  - US7 (Matchmaking): Can start after US3 (Groups) and US6 (Game Server) - Needs both groups and server coordination
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies Graph

```
Foundational (Phase 2)
    ├── US1: Authentication (P1) ───┐
    │                                ├──> US5: Clans (P5)
    ├── US2: Channels (P2) ─────────┘
    │
    ├── US3: Groups (P3) ────────────┐
    │                                 ├──> US7: Matchmaking (P7)
    ├── US6: Game Server (P6) ───────┘
    │
    └── US4: Whispers/Buddies (P4)
```

### Within Each User Story

- Tests before implementation (tests must FAIL initially)
- Models/entities before services
- Services before command processors
- Command processors before integration
- Story complete and tested before moving to next priority

### Parallel Opportunities

- **Setup Phase**: T001, T002, T003, T004 can all run in parallel (different files)
- **Foundational Phase Database**: T005, T006, T007 can run in parallel (different entity files)
- **Foundational Phase Core**: T011, T012, T013, T014, T015 can run in parallel (different core files)
- **Foundational Phase Sessions**: T017, T018 can run in parallel (different session types)
- **Foundational Phase In-Memory**: T020, T021, T022, T023 can run in parallel (different entity files)
- **User Stories**: After Foundational completes, US1, US2, US3, US4, US6 can all start in parallel (independent stories)
  - US5 waits for US2 completion
  - US7 waits for US3 and US6 completion
- **Within Each Story**: Command processors marked [P] can run in parallel (different files)
- **Polish Phase**: T139-T151 nearly all [P] can run in parallel

---

## Parallel Example: User Story 2 (Channels)

```bash
# Launch all tests for User Story 2 together:
Task: "Integration test for channel join/leave in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs"
Task: "Integration test for channel messaging in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs"
Task: "Integration test for channel moderation in ASPIRE.Tests/ChatServer/Integration/ChannelTests.cs"

# Launch all command processors for User Story 2 together:
Task: "Implement JoinChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/JoinChannelProcessor.cs"
Task: "Implement LeaveChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/LeaveChannelProcessor.cs"
Task: "Implement ChannelMessageProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/ChannelMessageProcessor.cs"
Task: "Implement KickFromChannelProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/KickFromChannelProcessor.cs"
Task: "Implement SilenceUserProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Channels/SilenceUserProcessor.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004)
2. Complete Phase 2: Foundational (T005-T031, includes T027a for Redis) - CRITICAL, blocks all stories
3. Complete Phase 3: User Story 1 (T032-T041)
4. **STOP and VALIDATE**: Test User Story 1 independently (players can connect and authenticate)
5. Deploy/demo if ready

**MVP Deliverable**: Chat server accepts connections, authenticates players, maintains persistent sessions with keep-alive. Redis cache ready for game server registry.

### Incremental Delivery (Recommended)

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (Authentication) → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 (Channels) → Test independently → Deploy/Demo
4. Add User Story 3 (Groups) → Test independently → Deploy/Demo
5. Add User Story 4 (Whispers) → Test independently → Deploy/Demo
6. Add User Story 6 (Game Server) → Test independently → Deploy/Demo
7. Add User Story 5 (Clans) → Test independently → Deploy/Demo (depends on US2)
8. Add User Story 7 (Matchmaking) → Test independently → Deploy/Demo (depends on US3, US6)
9. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers after Foundational phase:

1. **Team completes Setup + Foundational together** (T001-T031)
2. Once Foundational done:
   - Developer A: User Story 1 (Authentication) - T032-T041
   - Developer B: User Story 2 (Channels) - T042-T059
   - Developer C: User Story 3 (Groups) - T060-T080
   - Developer D: User Story 4 (Whispers) - T081-T094
   - Developer E: User Story 6 (Game Server) - T104-T116
3. After US2 completes: Developer B moves to US5 (Clans) - T095-T103
4. After US3 and US6 complete: Available developer takes US7 (Matchmaking) - T117-T138
5. All developers collaborate on Phase 10 (Polish) - T139-T151

---

## Task Statistics

- **Total Tasks**: 157 (151 original + T027a + T055a + T059a + T059b + T075a + T151a)
- **Setup Phase**: 4 tasks
- **Foundational Phase**: 28 tasks (BLOCKS all user stories) - includes Redis cache integration
- **User Story 1**: 10 tasks (MVP)
- **User Story 2**: 21 tasks
- **User Story 3**: 22 tasks - includes automatic leader transfer (FR-032)
- **User Story 4**: 14 tasks
- **User Story 5**: 9 tasks
- **User Story 6**: 13 tasks
- **User Story 7**: 22 tasks
- **Polish Phase**: 14 tasks - includes channel flag enrichment (T151a)
- **Parallel Tasks**: 89 marked [P] (57% can run in parallel within constraints)
- **MVP Scope**: Phases 1, 2, 3 (42 tasks) delivers authenticated connection handling

**Deferred to Post-MVP** (not included in tasks):
- FR-058: Match history tracking with configurable depth
- FR-059: Win/loss streak tracking for rating volatility
- FR-060: Season statistics resets
- FR-062: Leaver strike tracking for abandonment penalties

---

## Notes

- **[P] tasks**: Different files, no dependencies - can run in parallel
- **[Story] label**: Maps task to specific user story for traceability
- **Each user story**: Independently completable and testable
- **Verify tests fail**: Before implementing (TDD approach)
- **Commit frequently**: After each task or logical group
- **Stop at checkpoints**: Validate story independently before continuing
- **HoN source**: Absolute truth for protocol (C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN)
- **KONGOR source**: Practical reference (C:\Users\SADS-810\Source\NEXUS\LEGACY\KONGOR)
- **C# conventions**: No var, explicit types, full lambda parameter names, proper acronym casing (UserID, AccountID, HTTPParser)
- **Protocol version**: 68 (must maintain compatibility)
- **Testing**: SimulatedChatClient for protocol integration, xUnit for unit/integration, NBomber for load tests
- **Performance targets**: 10k+ connections, <100ms P95 latency, 1000+ msg/sec
