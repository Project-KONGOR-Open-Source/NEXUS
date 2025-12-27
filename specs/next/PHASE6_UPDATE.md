# Phase 6 Updated Section (for reference)

This is a draft of the updated Phase 6 section with reprioritized tasks.

## Phase 6: User Story 6 - Game Server Coordination (Priority: P6) 🎯 MATCHMAKING CRITICAL

**Goal**: Game servers connect to manager port (11033), authenticate, report availability, and submit match results which update PlayerStatistics in database with MMR/PSR ratings. Enables server handshakes and stats recording for matchmaking flow.

**Independent Test**: SimulatedGameServerClient connects to port 11033, authenticates, reports server available, submits match results with player statistics, verify PlayerStatistics updated in database with correct MMR/PSR calculations.

### Tests for User Story 6 (DEFERRED)

**Tests deferred until application code is implemented**

- [ ] T081 [P] [US6] Integration test for game server handshake in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server connects to port 11033, authenticates, registers availability) - 🔄 DEFERRED until application code complete
- [ ] T082 [P] [US6] Integration test for match status updates in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server reports match in progress, completed, aborted) - 🔄 DEFERRED until application code complete
- [ ] T083 [P] [US6] Integration test for match results submission in ASPIRE.Tests/ChatServer/Integration/GameServerTests.cs (server submits results, PlayerStatistics updated with MMR/PSR) - 🔄 DEFERRED until application code complete

### ✅ Completed Implementation for User Story 6

- [x] T084 [P] [US6] Implement ServerHandshakeProcessor.cs - ✅ Complete as ServerHandshake.cs
- [x] T085 [P] [US6] Implement ServerStatusProcessor.cs - ✅ Complete as ServerStatus.cs
- [x] T089 [US6] Game server authentication - ✅ Complete with AccountType.ServerHost check
- [x] T089a [P] [US6] ServerDisconnect.cs - ✅ Complete with TCP disconnect handling
- [x] T089b [P] [US6] ServerManagerDisconnect.cs - ✅ Complete with TCP disconnect handling
- [x] T089c [P] [US6] ServerManagerStatus.cs - ✅ Complete with status update handling
- [x] T089d [P] [US6] Ping.cs - ✅ Complete with bidirectional ping/pong

### 🎯 TOP PRIORITY: End-of-Match Stats Recording & MMR/PSR Updates

- [ ] T086 [P] [US6] **[PRIORITY]** Implement MatchStatusProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/MatchStatusProcessor.cs with [ChatCommand(NET_CHAT_SV_MATCH_STATUS)] for tracking match progress (in progress, paused, resumed)

- [ ] T087 [P] [US6] **[PRIORITY]** Implement MatchCompleteProcessor.cs in TRANSMUTANSTEIN.ChatServer/CommandProcessors/Server/MatchCompleteProcessor.cs with [ChatCommand(NET_CHAT_SV_MATCH_COMPLETE)] for receiving match completion event from game server

- [ ] T092 [US6] **[PRIORITY]** Update PlayerStatistics in match completion flow - Query MERRICK.DatabaseContext.PlayerStatistics by AccountID, update wins/losses counters for appropriate GameType (CampaignNormalWins/Losses, CampaignCasualWins/Losses, MidwarsWins/Losses, RiftwarsWins/Losses, PublicWins/Losses)

- [ ] T092a [US6] **[PRIORITY]** Enhance StatsRequesterController.HandleStatsSubmission() in KONGOR.MasterServer/Controllers/StatsRequesterController/StatsRequesterController.Submit.cs to update MMR/PSR ratings after match stats are recorded - After PlayerStatistics entity creation (line 45-47), call RatingCalculationService to calculate new ratings and update entity before SaveChangesAsync

- [ ] T093 [US6] **[PRIORITY]** Implement RatingCalculationService.cs in KONGOR.MasterServer/Services/RatingCalculationService.cs with Elo rating algorithm:
  - Method signature: CalculateNewRating(float currentRating, float opponentAverageRating, bool won, int kFactor = 32)
  - Formula: NewRating = OldRating + K * (ActualScore - ExpectedScore)
  - Where ActualScore = 1.0 (win), 0.5 (draw), 0.0 (loss)
  - ExpectedScore = 1 / (1 + 10^((OpponentRating - PlayerRating) / 400))
  - K-factor configurable via appsettings (default 32)
  - Separate calculations per game type (CampaignNormalRating MMR, CampaignCasualRating MMR, MidwarsRating MMR, RiftwarsRating MMR, PublicRating PSR)

- [ ] T093a [US6] **[PRIORITY]** Update PlayerStatistics entity rating fields in StatsRequesterController after rating calculation:
  - Update CampaignNormalRating/CampaignCasualRating/MidwarsRating/RiftwarsRating/PublicRating based on GameType
  - Increment appropriate wins/losses counter
  - Update LastUpdated timestamp
  - Call MerrickContext.SaveChangesAsync() to persist

### 🎯 TOP PRIORITY: Server Management Refactoring (Separation of Concerns)

**Current Issue**: ChatSession.Terminate() handles both TCP connection cleanup AND distributed cache removal. This violates separation of concerns - TCP events should only manage TCP connections, distributed cache should be managed by HTTP endpoints.

**Solution**: Create HTTP endpoints for graceful shutdown, move distributed cache logic there, keep TCP events focused on connection management only.

- [ ] T089e [US6] **[PRIORITY]** Create ServerRequesterController.Shutdown.cs in KONGOR.MasterServer/Controllers/ServerRequesterController/ServerRequesterController.Shutdown.cs with new endpoint for graceful match server shutdown:
  - Route: server_requester.php?f=shutdown
  - Parameters: session (server session cookie), server_id (match server ID)
  - Logic: Call DistributedCache.RemoveMatchServerByID(serverID), handle cleanup of pending matches, return Ok()
  - Validate session cookie before allowing shutdown

- [ ] T089f [US6] **[PRIORITY]** Add server manager shutdown endpoint in ServerRequesterController.Shutdown.cs:
  - Route: server_requester.php?f=shutdown_manager
  - Parameters: session (manager session cookie), manager_id (server manager ID)
  - Logic: Call DistributedCache.RemoveMatchServerManagerByID(managerID), handle cleanup of orphaned match servers under this manager, return Ok()
  - Validate session cookie before allowing shutdown

- [ ] T089g [US6] **[PRIORITY]** Refactor ChatSession.MatchServer.Terminate() in TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.MatchServer.cs:
  - REMOVE line 20: `await distributedCacheStore.RemoveMatchServerByID(Metadata.ServerID);`
  - REMOVE distributedCacheStore parameter from Terminate() method signature
  - Keep only TCP connection management: send disconnect acknowledgement, remove from Context.MatchServerChatSessions, disconnect and dispose TCP session
  - Add comment: "Distributed cache cleanup is handled by ServerRequesterController HTTP endpoint (f=shutdown), TCP event only manages connection"

- [ ] T089h [US6] **[PRIORITY]** Refactor ChatSession.MatchServerManager.Terminate() in TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.MatchServerManager.cs:
  - REMOVE line 20: `await distributedCacheStore.RemoveMatchServerManagerByID(Metadata.ServerManagerID);`
  - REMOVE distributedCacheStore parameter from Terminate() method signature
  - Keep only TCP connection management: send disconnect acknowledgement, remove from Context.MatchServerManagerChatSessions, disconnect and dispose TCP session
  - Add comment: "Distributed cache cleanup is handled by ServerRequesterController HTTP endpoint (f=shutdown_manager), TCP event only manages connection"

- [ ] T089i [US6] **[PRIORITY]** Update ServerDisconnect.cs and ServerManagerDisconnect.cs command processors:
  - After receiving disconnect command, check if server still exists in distributed cache
  - If server still in cache: Log warning "Ungraceful shutdown detected - server did not call HTTP shutdown endpoint", attempt to call ServerRequesterController HTTP endpoint synchronously as fallback cleanup
  - If server not in cache: Normal flow (already removed by HTTP endpoint)
  - Call refactored Terminate() method (without distributed cache parameter)

- [ ] T089j [US6] **[PRIORITY]** Document new graceful shutdown flow in comments and quickstart.md:
  - Proper shutdown sequence: 1) Game server POSTs to server_requester.php?f=shutdown, 2) HTTP endpoint removes from distributed cache, 3) Game server sends TCP disconnect command, 4) TCP event cleans up connection only
  - Ungraceful shutdown fallback: TCP disconnect without HTTP call triggers warning and fallback cleanup attempt
  - Update quickstart.md with new shutdown protocol

### Lower Priority: Server Tracking and Availability

- [ ] T088 [US6] Add GameServerSession tracking in ChatServer.RegisteredGameServers ConcurrentDictionary (keyed by ServerID) for quick lookup of available servers during matchmaking

- [ ] T090 [US6] Add server availability tracking in ServerStatusProcessor (update GameServerSession availability status based on server load, mark as available/busy/full)

- [ ] T091 [US6] Update player availability in MatchStatusProcessor (when match starts, mark players as InGame in ChatSession, when match ends mark as Available)

**Checkpoint**: At this point, end-of-match stats recording with MMR/PSR updates is complete, server management follows proper separation of concerns (HTTP for distributed cache, TCP for connections only), and the full match lifecycle is functional. User Story 6 is fully functional and testable independently.
