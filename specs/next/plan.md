# Implementation Plan: Chat Server Implementation

**Branch**: `next` | **Date**: 2025-01-13 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/next/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement the TRANSMUTANSTEIN chat server with full TCP protocol support for player communication, matchmaking, and game coordination. The server maintains protocol compatibility with HoN (absolute truth) and KONGOR (production reference) while using modern .NET 10 patterns. Architecture uses triple TCP listeners (client, game server, manager ports) with in-memory state management for channels and groups, and Entity Framework persistence for player statistics and friend relationships. Implementation follows phased approach: (1) chat channels, (2) matchmaking groups, (3) queue/match lobby/game start, (4) player communication.

## Technical Context

**Language/Version**: C# with .NET 10
**Primary Dependencies**: .NET Aspire, Entity Framework Core, System.Net.Sockets, System.Diagnostics.Metrics
**Storage**: SQL Server via MERRICK.DatabaseContext (EF Core), In-memory ConcurrentDictionary for transient state, Redis caching via Aspire
**Testing**: xUnit in ASPIRE.Tests, NBomber for load testing, SimulatedChatClient for protocol integration tests
**Target Platform**: Cross-platform (Windows/Linux) with Docker support, deployed via Aspire orchestration
**Project Type**: Distributed service (TCP server with multiple listener ports)
**Performance Goals**: 10,000+ concurrent connections, 1000+ messages/second throughput, P95 message latency < 100ms
**Constraints**: Protocol version 68 compatibility (HoN/KONGOR), 16KB channel buffer limit, 60-second keep-alive interval, immediate disconnect cleanup
**Scale/Scope**: 5 game types, 200+ protocol commands (phased implementation), 4 in-memory entity types, 2-3 persistent database entities

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Legacy Parity and Source of Truth
- [x] Feature maintains protocol compatibility with HoN/KONGOR
- [x] Legacy behavior documented and referenced (if applicable)
- [x] HoN source code consulted for absolute truth (if protocol-related)
- [x] KONGOR implementation reviewed for production patterns (if applicable)
- [x] Enhancement rationale documented (if changing legacy behavior)

**Evidence**: research.md documents HoN c_client.h (742 lines), c_channel.h as absolute truth. KONGOR Connection.cs, ChatServer.cs analysed for production patterns. Protocol version 68 compatibility maintained. Modern .NET patterns used for implementation while preserving protocol structures.

### Principle II: Service Architecture
- [x] Service boundaries clearly defined
- [x] Service registered in ASPIRE.ApplicationHost (if new service)
- [x] Uses MERRICK.DatabaseContext for data persistence (if data required)
- [x] Follows Aspire patterns for observability and configuration
- [x] Independent deployability considered in design

**Evidence**: TRANSMUTANSTEIN.ChatServer is distinct service. Will register in ASPIRE.ApplicationHost. data-model.md defines PlayerStatistics and FriendedPeer entities using MERRICK.DatabaseContext. Triple TCP listener architecture supports independent deployment. Aspire health checks and metrics planned.

### Principle III: Chat Server Priority
- [x] Chat Server priority acknowledged (if not chat-related, mark N/A)
- [x] Event flows documented (if chat-related)
- [x] Protocol validated against HoN and KONGOR (if chat-related)
- [x] Real-time performance considered (if chat-related)
- [x] Logging strategy defined for debugging (if chat-related)

**Evidence**: This IS the chat server (highest priority). research.md documents command categorization and flows. Protocol validated against HoN chatserver_protocol.h and KONGOR implementations. Performance targets: 10k connections, <100ms P95 latency. quickstart.md defines logging strategy with .NET metrics.

### Principle IV: Code Style and Formatting
- [x] Will follow C# conventions from copilot-instructions.md
- [x] No "var" usage planned
- [x] Proper acronym casing (UserID, userGUID, HTTPParser)
- [x] Full lambda parameter names planned
- [x] XML summaries planned for public APIs

**Evidence**: quickstart.md examples use explicit types (no var), proper casing (AccountID, GroupID), full lambda parameter names. data-model.md entity definitions follow conventions. Public APIs will have XML summaries per constitution.

### Principle V: Security and Correctness
- [x] OWASP Top 10 vulnerabilities considered
- [x] Input validation strategy defined
- [x] Authentication/authorization requirements identified
- [x] Data protection requirements identified
- [x] Performance optimization strategy balanced with readability

**Evidence**: spec.md FR-011 to FR-015 define buffer limits (16KB), flood prevention, rate limiting. FR-006 to FR-009 define authentication/authorization. quickstart.md shows input validation patterns. Performance via ConcurrentDictionary and async patterns, not premature optimization.

### Principle VI: Simplicity and Maintainability
- [x] Simplest solution that meets requirements selected
- [x] Complexity justified (if introducing new patterns)
- [x] YAGNI principle applied
- [x] No over-engineering in design
- [x] Clear rationale for architectural choices

**Evidence**: In-memory state (not over-engineered persistence). Triple listener is simplest for three port types. Existing TCPServer base class reused. No unnecessary abstractions. research.md documents rationale for all major decisions.

### Principle VII: Testing and Validation
- [x] Integration test strategy defined (protocol, database, chat flows)
- [x] Legacy parity validation approach documented
- [x] Performance testing planned (for real-time components)
- [x] Test location identified (ASPIRE.Tests)
- [x] Success criteria measurable

**Evidence**: quickstart.md defines SimulatedChatClient for protocol testing, 10k connection load tests, P95 latency measurement. research.md section 8 details performance testing harness. Success criteria in spec.md SC-001 to SC-013 are measurable. Tests in ASPIRE.Tests per constitution.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
NEXUS/
├── TRANSMUTANSTEIN.ChatServer/
│   ├── Core/
│   │   ├── ChatServer.cs              # Main server with triple TCP listeners
│   │   ├── ChatSession.cs             # Client connection handler
│   │   ├── GameServerSession.cs       # Game server connection handler
│   │   ├── ManagerSession.cs          # Manager connection handler
│   │   ├── ChatProtocol.cs            # Command codes (protocol v68)
│   │   ├── ChatBuffer.cs              # Binary message serialization
│   │   └── CommandProcessorRegistry.cs # Command routing
│   ├── CommandProcessors/
│   │   ├── Channels/
│   │   │   ├── JoinChannelProcessor.cs
│   │   │   ├── LeaveChannelProcessor.cs
│   │   │   ├── ChannelMessageProcessor.cs
│   │   │   ├── KickFromChannelProcessor.cs
│   │   │   └── SilenceUserProcessor.cs
│   │   ├── Groups/
│   │   │   ├── GroupCreateProcessor.cs
│   │   │   ├── GroupInviteProcessor.cs
│   │   │   ├── GroupJoinProcessor.cs
│   │   │   ├── GroupLeaveProcessor.cs
│   │   │   └── PlayerReadyStatusProcessor.cs
│   │   ├── Matchmaking/
│   │   │   ├── GroupJoinQueueProcessor.cs
│   │   │   ├── GroupLeaveQueueProcessor.cs
│   │   │   ├── MatchFoundProcessor.cs
│   │   │   ├── MatchAcceptProcessor.cs
│   │   │   └── MatchStartProcessor.cs
│   │   ├── Communication/
│   │   │   ├── WhisperProcessor.cs
│   │   │   ├── AddBuddyProcessor.cs
│   │   │   └── RemoveBuddyProcessor.cs
│   │   └── Server/
│   │       ├── ServerHandshakeProcessor.cs
│   │       ├── ServerStatusProcessor.cs
│   │       ├── MatchCompleteProcessor.cs
│   │       └── AllocateServerProcessor.cs
│   ├── InMemory/
│   │   ├── ChatChannel.cs             # In-memory channel state
│   │   ├── ChatChannelMember.cs       # Channel membership
│   │   ├── MatchmakingGroup.cs        # In-memory group state
│   │   ├── MatchmakingGroupMember.cs  # Group membership
│   │   ├── MatchLobby.cs              # Match lobby (Phase 3)
│   │   └── MatchLobbyPlayer.cs        # Lobby player (Phase 3)
│   ├── Services/
│   │   ├── MatchmakingBroker.cs       # Matchmaking algorithm (Phase 3)
│   │   └── KeepAliveService.cs        # 60-second keep-alive timer
│   └── Program.cs                      # Aspire orchestration entry
│
├── MERRICK.DatabaseContext/
│   ├── Entities/
│   │   ├── PlayerStatistics.cs        # NEW: 5 game type ratings
│   │   ├── FriendedPeer.cs            # NEW: Friend relationships
│   │   └── ClanMember.cs              # Check if exists, add if needed
│   └── Migrations/
│       ├── AddPlayerStatistics.cs     # EF Core migration
│       └── AddFriendedPeer.cs         # EF Core migration
│
├── ASPIRE.Tests/
│   ├── ChatServer/
│   │   ├── Integration/
│   │   │   ├── ChannelTests.cs        # Phase 1 tests
│   │   │   ├── GroupTests.cs          # Phase 2 tests
│   │   │   ├── MatchmakingTests.cs    # Phase 3 tests
│   │   │   └── CommunicationTests.cs  # Phase 4 tests
│   │   ├── Performance/
│   │   │   ├── ConcurrentConnectionsTest.cs
│   │   │   ├── MessageLatencyTest.cs
│   │   │   └── SimulatedChatClient.cs # Test helper
│   │   └── Unit/
│   │       ├── ChatBufferTests.cs
│   │       ├── ChatChannelTests.cs
│   │       └── MatchmakingGroupTests.cs
│   └── TestHelpers/
│       └── SimulatedChatClient.cs     # Protocol test client
│
└── ASPIRE.ApplicationHost/
    └── Program.cs                      # Register TRANSMUTANSTEIN.ChatServer
```

**Structure Decision**: NEXUS uses distributed service architecture with .NET Aspire orchestration. TRANSMUTANSTEIN.ChatServer is a dedicated TCP service with clear separation of concerns: Core (server infrastructure), CommandProcessors (protocol handlers organized by phase), InMemory (transient state), Services (business logic). Database entities added to MERRICK.DatabaseContext. Tests organized by type (integration, performance, unit) in ASPIRE.Tests.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
