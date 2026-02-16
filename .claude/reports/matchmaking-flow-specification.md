# Matchmaking Flow Specification

**Cross-referenced against:** HON Chat Server C++ Source Code (`c_client.cpp`, `c_teamfinder.cpp`, `c_matchmaker.cpp`, `c_gameserver.cpp`, `c_group.cpp`)

---

## Phase 1: Group Creation

**Client sends:** `NET_CHAT_CL_TMM_GROUP_CREATE` (protocol ID `0x0D00`)
**Packet fields (in order):**
1. Version (wstring)
2. TMMType (byte) — 2 = team, 3 = bot match, 4 = campaign
3. GameType (byte) — `ETMMGameTypes` enum
4. MapNames (wstring) — pipe-delimited
5. GameModes (wstring) — pipe-delimited
6. Regions (wstring) — pipe-delimited
7. Ranked (byte) — 0 or 1
8. MatchFidelity (byte) — 0-255
9. BotDifficulty (byte)
10. RandomizeBots (byte) — 0 or 1

**Chat server validation order (C++ `HandleRequestCreateTMMGroup`):**
1. Packet integrity (`HasFaults`)
2. Rate limiting (`IncrementRequestCounter`)
3. No pending TMM rating request (`HasRecentTMMRatingRequest`)
4. TMM enabled → else send `TMMFTJR_DISABLED`
5. Player not banned → else send `TMMFTJR_BANNED` + ban time (uint32)
6. Not already in group (removes if found, loops to handle multi-group edge case)
7. Not already in scheduled match (removes if found)
8. Max groups not exceeded → else send `TMMFTJR_BUSY`
9. GameType available → else send `TMMFTJR_OPTION_UNAVAILABLE` + TMM info
10. Single map only → else send `TMMFTJR_OPTION_UNAVAILABLE`
11. Each map available → else send `TMMFTJR_OPTION_UNAVAILABLE`
12. Each game mode available → else send `TMMFTJR_OPTION_UNAVAILABLE`
13. Each region available → else send `TMMFTJR_OPTION_UNAVAILABLE`
14. No disabled game mode combos → else send `TMMFTJR_OPTION_UNAVAILABLE`
15. Version compatible → else send `TMMFTJR_INVALID_VERSION`

**Post-validation actions (`CreateAndAddGroup`):**
1. Remove player from any existing groups (loop until none found)
2. Set player team slot to 0
3. Override settings for special maps:
   - Bot match → force `BOT_MATCH` mode, `FORESTS_OF_CALDAVAR` map, `CASUAL` type
   - Midwars/Riftwars → set matching game type, clear match fidelity, force unranked
4. Allocate `CGroup` with unique `GroupIndex`
5. Add player to group, set as leader
6. Leave all non-server channels (`LeaveAllChannels(0, true)`)
7. Create group channel if TMMType is 2, 3, or 4
8. Broadcast `TMM_CREATE_GROUP` to group (full update)
9. If TMM rating not cached: `SpawnTMMInfoRequest` (HTTP request to API), then `CreateAndAddGroup` is called from the response handler

## Phase 1a: Group Invite

**Client sends:** `NET_CHAT_CL_TMM_GROUP_INVITE` (protocol ID `0x0D04`)
**Packet fields:** NickName (wstring)

**Chat server actions (`HandleRequestInviteTMMPlayer`):**
1. Rate limit check
2. Look up target player by name
3. If target is NULL or in DND mode → silently ignore
4. Call `InvitePlayerToGroup` → sends `TMM_INVITED_TO_GROUP` broadcast to group, sends invite packet to target

## Phase 1b: Group Invite Reject

**Client sends:** `NET_CHAT_CL_TMM_GROUP_REJECT_INVITE` (protocol ID `0x0D06`)
**Packet fields:** NickName (wstring)

**Chat server actions:** Look up inviter, call `RejectGroupInvite` → sends `TMM_PLAYER_REJECTED_GROUP_INVITE` to group

## Phase 2: Group Join (Invite Accept)

**Client sends:** `NET_CHAT_CL_TMM_GROUP_JOIN` (protocol ID `0x0D01`)
**Packet fields (in order):**
1. NickName (wstring) — name of group leader to join
2. Version (wstring)

**Chat server validation order (C++ `HandleRequestJoinTMMGroup`):**
1. Packet integrity
2. Rate limit + no pending TMM rating request
3. TMM enabled → else `TMMFTJR_DISABLED`
4. Player not banned → else `TMMFTJR_BANNED` + ban time
5. Remove player from any existing group
6. Remove player from any existing scheduled match
7. Look up target player by name → if NULL, silently return
8. Target has a group → if NULL, silently return
9. Player was actually invited (`HasInvitedPlayer`)
10. Group not already queued → else `TMMFTJR_ALREADY_QUEUED`
11. Group not full → else `TMMFTJR_GROUP_FULL`
12. Version compatible → else `TMMFTJR_INVALID_VERSION`

**Post-validation actions:**
1. If TMM rating not cached: `SpawnTMMInfoRequest` then add on response
2. Else: add player directly, broadcast `TMM_FULL_GROUP_UPDATE`

## Phase 2a: Group Leave

**Client sends:** `NET_CHAT_CL_TMM_GROUP_LEAVE` (protocol ID `0x0D02`)
**No fields.**

**Chat server actions:** `RemovePlayerFromGroup(accountID)` → if leader leaves, group is dissolved; otherwise broadcasts `TMM_PLAYER_LEFT_GROUP`

## Phase 2b: Group Kick

**Client sends:** `NET_CHAT_CL_TMM_GROUP_KICK` (protocol ID `0x0D05`)
**Packet fields:** TeamSlot (byte) — **NOT account ID**

**Chat server validation (C++ `HandleRequestKickTMMPlayer`):**
1. Sender must be group leader
2. Cannot kick self (`HasTeamSlot` check)
3. Calls `RemovePlayerFromGroup(teamSlot, groupID, isBotGroup)`

**⚠️ REIMPLEMENTATION BUG:** Reads `int32` (account ID) instead of `byte` (team slot). Must be fixed.

## Phase 3: Ready Up

**Client sends:** `NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS` (protocol ID `0x0D0A`)
**Packet fields (in order):**
1. ReadyStatus (byte) — 0 = not ready, 1 = ready
2. GameType (byte)

**C++ validation order (`HandleRequestTMMPlayerReadyStatus`):**
1. Player must be in a group
2. If leader is readying AND non-leaders aren't all ready → send `RequestReadyUp` prompt (asks non-ready members), return
3. If leader is readying AND map is no longer available → silently return
4. If leader is readying AND any player is `JOINING_GAME`/`IN_GAME` → silently return
5. If ranked group AND player not eligible for ranked AND not campaign → silently return
6. If player cannot access group's game modes → silently return

**Chat server actions:**
1. `SetTMMReadyStatus(readyStatus)` on player
2. Broadcast `TMM_PARTIAL_GROUP_UPDATE` to group
3. Check if ALL players have `readyStatus == 1`:
   - If yes → broadcast `NET_CHAT_CL_TMM_START_LOADING` to group
   - Clients begin loading game resources upon receiving this

## Phase 4: Loading

**Client sends:** `NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS` (protocol ID `0x0D09`)
**Packet fields:** Percent (byte) — 0 to 100

**C++ flow (`HandleRequestTMMPlayerLoadingUpdate`):**
1. Read previous loading percent
2. Set new loading percent on player
3. Determine if player is in normal group or scheduled match
4. If in normal group:
   - If percent changed → flag group `SetLoadingStatusChanged(true)`
   - If `AreAllPlayersLoadedAndReady()` → call `AddGroupToQueue(pGroup)`
5. If in scheduled match:
   - Find group index (0 or 1) for the player
   - If percent changed → flag the scheduled match group as having loading status changed

**Key detail:** Loading percent 100 + ready status 1 = "loaded and ready". The `AreAllPlayersLoadedAndReady()` check requires BOTH conditions on ALL players.

## Phase 5: Queue Entry

**Triggered by:** `AddGroupToQueue(pGroup)` — called when all players are loaded and ready

**Leader-only direct entry:** `NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE` (protocol ID `0x0D07`) — only the group leader can call this; it calls `AddGroupToQueue` directly

**C++ validation order (`CTeamFinder::AddGroupToQueue`):**
1. Group must have a valid leader
2. CIS region restrictions: if any player lacks Garena ID access, remove CIS region, unload/unready players, send `TMM_REGION_UNAVAILABLE`
3. Turkey region restrictions: similar Garena ID check for TR region
4. Lock Pick requires exactly 5-player group → remove Lock Pick if fewer, fallback to All Pick
5. No disabled game mode combos → if found, unload/unready and broadcast `TMM_GROUP_LEFT_QUEUE`
6. All players must still be loaded and ready

**Post-validation actions:**
1. `UpdateGroupStats(pGroup)` — recalculates group-level TMR and statistics from current player data
2. `UseAppropriatePlayerStats()` — selects correct rating set (e.g. bot groups recalculate via `ForceBotOptions`)
3. `CacheInformation()` — snapshots all group data for the matchmaker process
4. `SetQueued(true)` — marks group as in queue
5. Broadcast `TMM_GROUP_JOINED_QUEUE` to group (sends `NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE`)
6. Broadcast `TMM_GROUP_QUEUE_UPDATE` to group (sends average queue time)
7. Set `JoinedQueueTime`:
   - If leader has recent `LastMatchupTime` on same map within `lastMatchTimeCutoff`: resume from previous queue position → broadcast `TMM_GROUP_REJOINED_QUEUE` with saved queue time
   - Otherwise: set to current time
8. Increment per-size counter `m_auiTotalGroupsThatQueued[playerCount - 1]`
9. If group has few total matches: increment `m_auiNewGroupsThatQueued` counter

**Leave queue:** `NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE` (protocol ID `0x0D08`) → calls `RemoveGroupFromQueue`

## Phase 6: Matchmaker Cycle

**Architecture:** Runs as a separate child process (`CMatchMaker`), communicating with the main chat server via IPC packets. The matchmaker receives group snapshots and returns match decisions.

**Cycle frequency:** Every ~5 seconds (`matchmaker_spawnCycleDelay`), configurable.

**Cycle increment:** `m_uiSpawnCycles++` each frame. Different combine methods run on different cycle modulos.

**Steps per cycle (`CMatchMaker::Frame`):**
1. Receive updated group data from chat server
2. Build group vector for matching
3. Run combine/match phases in priority order
4. Return match decisions to chat server

**Combine methods — Team Size 5 (alternates with size 3 on odd/even cycles):**

| Cycle Condition | Method | Description |
|----------------|--------|-------------|
| Every even cycle | `TS5_5_RANDOM` | 5-stack vs 5-stack |
| Every even cycle | `TS5_4_PLUS_1_RANDOM` | 4+1 vs 4+1 |
| Every even cycle | `TS5_3_PLUS_2_RANDOM` | 3+2 vs 3+2 |
| Every even cycle | `TS5_ALL_ONES_RANDOM` | All solos (5×1 vs 5×1) |
| Every N cycles | `TS5_FULL_OR_2_GROUPS_*` | Full teams or exactly 2 groups |
| Every M cycles | `TS5_ANY_GROUPS_*` | Any combination of group sizes |
| Catchall loop | `TS5_ALL_GROUP_SIZES_RANDOM` | Any sizes, repeated until no matches |

**Team Size 3 and 1:** Similar phases on odd cycles with `TS3_*` and `TS1_*` methods.

**Each combine/match pass:**
1. `PruneGroups(&vGroups)` — remove groups that are no longer valid (disbanded, player disconnected, etc.)
2. `CombineGroups(&vGroups, &vTeams, method, teamSize, matchInexperienced, fairWaitTime)` — form candidate teams from compatible groups
3. `CreateMatches(&vTeams, method)` — pair two teams into a match if they pass all checks

**Match compatibility checks in `CreateMatches`:**
- TMR range overlap (adjusted by queue wait time — ranges widen over time)
- At least one common game mode
- At least one common region
- Same map and game type
- `BalanceTeams` succeeds (assigns players to optimal team sides)
- Fairness: win prediction within configured thresholds (`matchmaker_startingWinPercent` to `matchmaker_startingLossPercent`)
- Match fidelity: if either team has fidelity enabled, prediction must be within `fairLow/HighMatchFidelityWinPercent`
- IP conflict check (optional, prevents players from same IP on same team)

**Brute force fallback:**
- Solo queue ≥ `bruteForceSoloWaitTime` minutes (default 10) → `FindMatch` ignores fairness
- Any group ≥ `bruteForceWaitTime` minutes → `FindMatch` ignores fairness

**Inexperienced group matching:** Only attempted every `inexperiencedGroupSpawnCycleDelay` cycles. Groups with fewer than `newPlayerMatchCount` total matches per team size are treated as inexperienced.

## Phase 7: Match Spawn

**When `CreateMatches` finds a valid team pair, the chat server (`CTeamFinder::SpawnMatch`) executes:**

1. Set game mode and region on both teams
2. `IncrementMatchesCreated()` → generates unique `MatchupID`
3. `SetMatchupIDs(matchesCreated)` on both teams — tags all groups with this matchup
4. `SetLastMatchupInfo()` on both teams — saves queue position for re-queue on failed match
5. `SetMatchPointValues(matchData, legionTeam, hellbourneTeam)` — calculates per-player win/loss rating changes based on `GetMatchupPrediction`
6. `PrintMatchupDebugInfo` → generates `sExtraMatchInfo` string
7. Track combine method statistics
8. `RemoveGroupsFromQueue("matched")` on both teams — groups are out of queue but NOT deleted
9. **Send `NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE`** to all players on both teams via `BroadcastToTeam`:
   - Fields: MapName(string\0), TeamSize(byte), GameType(byte), GameMode(string\0), Region(string\0), ExtraMatchInfo(string\0)
10. `StartRemoteMatch(legionTeam, hellbourneTeam, gameServer)` — see below

**`StartRemoteMatch` (C++ `CTeamFinder::StartRemoteMatch`):**

1. Build match settings string: `"mode:<mode> map:<map> teamsize:<n> allheroes:true noleaver:<bool> spectators:<n>"`
   - Special overrides: APG→AP+gated, BBG→BB+gated, APD→AP+allowduplicate
   - Bot match: adds bot configuration strings
2. Generate random `challenge` integer
3. Build `NET_CHAT_GS_CREATE_MATCH` packet:

   | Field | Type | Description |
   |-------|------|-------------|
   | Command | byte | `NET_CHAT_GS_CREATE_MATCH` (protocol ID `0x1502`) |
   | ArrangedMatchType | byte | From `GetArrangedMatchType()` |
   | MatchupID | uint32 | Unique match identifier |
   | EventID | uint32 | Placeholder (0 for TMM) |
   | Challenge | uint32 | Random validation token |
   | Name | string\0 | "TMM Match #" (server appends match ID) |
   | MatchSettings | string\0 | Mode, map, team size, options |
   | RevampedMM | byte | Whether to use revamped MMR |
   | FromLobby | byte | Whether created from pregame lobby |
   | PlayerCount | byte | Total players in match roster |

4. For each player on each team (sorted by TMR descending, then optionally shuffled):

   | Field | Type | Description |
   |-------|------|-------------|
   | AccountID | uint32 | |
   | Team | byte | 1 = Legion, 2 = Hellbourne |
   | Slot | byte | Team slot index (0-based) |
   | SocialBonus | byte | Premade group bonus |
   | WinValue | float | Rating gain if win |
   | LossValue | float | Rating loss if lose |
   | IsProvisional | byte | Player in placement matches |
   | GroupIndex | byte | Which group this player belongs to |
   | BenefitValue | float | Additional benefit multiplier |

5. Append group ID roster: `GroupIDCount(uint32)` + `GroupID(uint32)[]`
6. `pServer->CreateMatch(packet)` — sends to game server
7. **Send `TMM_GROUP_FOUND_SERVER`** to both teams via `BroadcastToTeam`:
   - Packet: `NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE` + `byte(TMM_GROUP_FOUND_SERVER)`

**ArrangedMatchType mapping (C++ `CGroup::GetArrangedMatchType`):**

| Condition | ArrangedMatchType |
|-----------|-------------------|
| MIDWARS, REBORN_NORMAL, REBORN_CASUAL, MIDWARS_REBORN | `AM_MATCHMAKING_MIDWARS` |
| RIFTWARS | `AM_MATCHMAKING_RIFTWARS` |
| CAMPAIGN_NORMAL, CAMPAIGN_CASUAL | `AM_MATCHMAKING_CAMPAIGN` |
| IsBotGroup() | `AM_MATCHMAKING_BOTMATCH` |
| CUSTOM | `AM_MATCHMAKING_CUSTOM` |
| !IsRanked() | `AM_UNRANKED_MATCHMAKING` |
| Default (ranked normal) | `AM_MATCHMAKING` |

**⚠️ REIMPLEMENTATION BUG:** Missing mappings for REBORN_NORMAL, REBORN_CASUAL, MIDWARS_REBORN, CUSTOM, and bot groups.

## Phase 8: Match Announce (Server Ready)

**Game server responds:** `NET_CHAT_GS_ANNOUNCE_MATCH`
**Packet fields (in order):**
1. MatchupID (uint32)
2. Challenge (uint32) — must match the one sent in CreateMatch
3. GroupCount (uint32) — number of group IDs following
4. MatchID (uint32) — the game server's assigned match ID
5. GroupIDs (uint32[]) — one per group

**C++ flow (`CGameServer::ProcessAnnounceMatch` → `CTeamFinder::AnnounceMatchReady`):**

1. `ValidateMatch(matchupID, challenge, groupIDs, arrangedMatchType)`:
   - Verifies each group still exists
   - Verifies challenge matches what was stored on the group
   - Verifies matchup ID matches
   - If validation fails → log `FAILED_TO_VALIDATE_MATCH1`, no announcement
2. Create `SMatchReminderInfo` — saves address, port, and reminder delay for reconnection reminders
3. For each group ID in the roster:
   - Find the group in normal groups or bot groups
   - For each player in the group:
     - **Send `CHAT_CMD_AUTO_MATCH_CONNECT`** (protocol ID `0x0062`):

       | Field | Type | Description |
       |-------|------|-------------|
       | ArrangedMatchType | byte | From `SMatchReminderInfo` |
       | MatchupID | uint32 | |
       | Address | string\0 | Game server IP |
       | Port | ushort | Game server port |
       | RandomInt | int32 | Random value to prevent NAT packet dedup |

   - `GatherPlayersFromGroup` → stores player list for match reminders
4. If `enableMatchReminders` → store reminder info in `m_mapMatchReminderInfo`
5. **Post-announce group cleanup:**
   - For each group: if `playerCount <= 1` OR `!persistentGroups` → `DeleteGroupAllTypes(groupID)`
   - Multi-player groups with `persistentGroups = true` are preserved (players can re-queue after match)

### Correct C++ Notification Sequence (Critical)

| Step | When | Packet Sent | Purpose |
|------|------|-------------|---------|
| 1 | `SpawnMatch` (before server) | `TMM_MATCH_FOUND_UPDATE` | Show match info (map, mode, region) in UI |
| 2 | `StartRemoteMatch` (after `CreateMatch` sent) | `TMM_GROUP_FOUND_SERVER` | UI transitions to "server found" state |
| 3 | `AnnounceMatchReady` (server responds) | `AUTO_MATCH_CONNECT` | Client auto-connects to game server |

**⚠️ REIMPLEMENTATION BUG:** `SpawnMatch` sends all 3 packets (FoundServer + MatchFound + AutoMatchConnect), then `MatchAnnounce` sends all 3 again → duplicate notifications.

### Match Reminder System

If `enableMatchReminders` is enabled, the chat server periodically checks `m_mapMatchReminderInfo`. If a player hasn't connected within `matchReminderDelay`, the `AUTO_MATCH_CONNECT` packet is re-sent. This handles cases where the initial connect packet was lost.

## Phase 9: Player Joins Game

### 9a: Joining Game

**Client sends:** `CHAT_CMD_JOINING_GAME`
**Packet fields:** AddressPort (wstring) — "ip:port" of the game server

**C++ flow (`HandleJoiningGame`):**
1. `ClearCurrentMatch()` — clears any previous match state
2. `SetServerAddressPort(addressPort)` — stores for reference
3. `SetStatus(CHAT_CLIENT_STATUS_JOINING_GAME)`
4. If `!client_stayInGeneralChannels` → `LeaveAllChannels(CHAT_CHANNEL_FLAG_GENERAL_USE)`
5. `UpdateStatus()` — notifies friends/clan of status change

### 9b: Joined Game

**Client sends:** `CHAT_CMD_JOINED_GAME`
**Packet fields (in order):**
1. GameName (wstring)
2. MatchID (int32, default -1 if missing)
3. JoinMatchChannel (byte) — 0 if spectating/mentoring

**C++ flow (`HandleJoinedGame`):**
1. `SetGameName(gameName)`
2. `SetMatchID(matchID)`
3. If previous status < `IN_GAME` → `IncrementInGameCount()`
4. `SetStatus(CHAT_CLIENT_STATUS_IN_GAME)`
5. **`LeaveAllChannels(CHAT_CHANNEL_FLAG_SERVER)`** — leave old match channels
6. If matchID is valid AND `joinMatchChannel` → create/join match channel
7. `UpdateStatus()` — notifies friends/clan

**⚠️ REIMPLEMENTATION BUG:** Missing `LeaveAllChannels(CHAT_CHANNEL_FLAG_SERVER)` before joining new match channel. Old match channel memberships accumulate.

## Phase 10: Match Started

**Game server sends:** `NET_CHAT_GS_MATCH_STARTED`
**Packet fields:** MatchupID (uint32)

**C++ flow (`CGameServer::ProcessArrangedMatchStarted`):**
1. Log match started with server info, arranged match type, matchup ID
2. If `AM_MATCHMAKING` → `DeleteMatchupStats(matchupID)` (cleanup tracking data)
3. If any matchmaking type → `IncrementMatchesStarted()` (statistics counter)

**Note:** By this point, groups have already been removed from queue (at spawn) and potentially deleted (at announce). This handler is purely for bookkeeping.

## Phase 11: Match Aborted

**Game server sends:** `NET_CHAT_GS_MATCH_ABORTED`
**Packet fields (in order):**
1. MatchupID (uint32)
2. Reason (byte) — `EMatchAbortedReason`

**Abort reasons:**

| Value | Enum | Description |
|-------|------|-------------|
| 0 | `MATCH_ABORTED_UNKNOWN` | Unknown reason |
| 1 | `MATCH_ABORT_CONNECT_TIMEOUT` | Players failed to connect in time |
| 2 | `MATCH_ABORT_START_TIMEOUT` | Match failed to start after connect |
| 3 | `MATCH_ABORT_PLAYER_LEFT` | A player left during loading |

**C++ flow (`CGameServer::ProcessMatchAborted`):**
1. Log abort with reason
2. `AddDisconnectReason(reason)` — maps to disconnect tracking enum:
   - `CONNECT_TIMEOUT` → `DISCONNECT_MM_CONNECT_TIMEOUT`
   - `START_TIMEOUT` → `DISCONNECT_MM_START_TIMEOUT`
   - `PLAYER_LEFT` → `DISCONNECT_MM_PLAYER_LEFT`

**Note:** Groups were already removed from queue at spawn time. Players must manually re-queue.

## Phase 12: Match Ended

**Game server sends:** `NET_CHAT_GS_MATCH_ENDED`
**Packet fields (in order):**
1. MatchupID (uint32)
2. Reason (byte) — `EMatchEndedReason`
3. WinningTeam (byte) — 1 = Legion, 2 = Hellbourne
4. PlayerCount (byte) — number of players still in match
5. AccountIDs (uint32[]) — one per player

**C++ flow (`CGameServer::ProcessArrangedMatchEnded`):**
1. Log match result with all player account IDs
2. If `AM_SCHEDULED_MATCH` AND `MATCH_ENDED_REMADE` → `SendRemakeInfo(matchupID)` to notify players

**Note:** Rating changes were pre-calculated at spawn time (win/loss values). The actual rating update is handled by the game server submitting stats to the master server API, not by the chat server.

## Phase 13: Player Leaves Match

**Client sends:** `CHAT_CMD_LEFT_GAME`
**No fields.**

**C++ flow (`HandleLeftGame`) — order matters:**
1. If previous status ≥ `IN_GAME` → `DecrementInGameCount()`
2. `SetStatus(CHAT_CLIENT_STATUS_CONNECTED)` — back to connected
3. `ClearCurrentMatch()` — clears match ID, game name, server address
4. Rejoin default channel (unless `CHAT_MODE_INVISIBLE`):
   - `GetDefaultChannel(this)` → `AddClient(this)`
5. `UpdateStatus()` — notifies friends/clan of status change

**⚠️ REIMPLEMENTATION BUG:** C# calls `UpdateStatus(CONNECTED)` BEFORE `RejoinDefaultChannel()`, but C++ calls status update AFTER rejoining. The C++ order ensures friends see the player in a channel when the status update fires.

## Phase 14: Disconnect Cleanup

**Triggered by:** Client socket disconnect or forced removal

**C++ flow (`CClientManager::RemoveClient`):**
1. If not a virtual client:
   - `GameLobbyClientDeleted(pClient)` — remove from any game lobbies
   - **Loop to find ALL groups** containing this client (handles edge case of being in multiple groups):
     ```
     while (FindGroupFromClient(pClient) != NULL)
         RemovePlayerFromGroup(accountID)
     ```
   - If in >1 group: log conflict warning
2. If in scheduled match:
   - `BroadcastToScheduledMatch(SM_PLAYER_LEAVE, accountID)` — notify other players
   - `RemovePlayerFromScheduledMatch(pClient)` — remove from match
   - `BroadcastToScheduledMatch(SM_UPDATE)` — refresh match state
3. Remove from all data structures (buddy lists, clan, channels, etc.)
4. `Disconnect()` — sends `CHAT_CMD_DISCONNECTED`, closes socket

**`RemovePlayerFromGroup` effects:**
- If the removed player was the leader: the entire group is dissolved → `DeleteGroupAllTypes`
- If the removed player was not the leader: broadcast `TMM_PLAYER_LEFT_GROUP` to remaining members
- If group was queued: remove from queue first → broadcast `TMM_GROUP_LEFT_QUEUE`

---

## Group Update Packet Structure (0x0D03)

The `BroadcastToGroup` packet is the most complex packet in the TMM system. Its structure depends on the update type.

### Header (always present for types ≤ TMM_PLAYER_KICKED_FROM_GROUP):

| Field | Type | Description |
|-------|------|-------------|
| Command | uint16 | `NET_CHAT_CL_TMM_GROUP_UPDATE` (0x0D03) |
| UpdateType | byte | See below |
| AccountID | uint32 | Context-dependent (joiner, leaver, etc.) |
| PlayerCount | byte | Current group size |
| AverageTMR | uint16 | Group average TMR |
| LeaderAccountID | uint32 | Group leader |
| ArrangedMatchType | byte | From `GetArrangedMatchType()` |
| GameType | byte | |
| MapName | string\0 | |
| GameModes | string\0 | Pipe-delimited |
| Regions | string\0 | Pipe-delimited |
| Ranked | byte | |
| MatchFidelity | byte | |
| BotDifficulty | byte | |
| RandomizeBots | byte | |
| CountryRestrictions | string\0 | |
| PlayerResponseString | string\0 | Semicolon-delimited player info |
| TeamSize | byte | |
| TMMType | byte | 2 = team, 3 = bot |

### Per-Player Data (for full updates: CREATE, FULL_UPDATE, JOIN, LEAVE, KICK):

| Field | Type | Description |
|-------|------|-------------|
| AccountID | uint32 | |
| Name | string\0 | UTF-8 encoded |
| TeamSlot | byte | |
| CampaignNormalMedal | byte | |
| CampaignCasualMedal | byte | |
| CampaignNormalRank | uint16 | |
| CampaignCasualRank | uint16 | |
| CampaignEligible | byte | |
| TMR | uint16 | 0xFFFF if not ranked/visible |

### Per-Player Data (always, after full update fields):

| Field | Type | Description |
|-------|------|-------------|
| LoadingPercent | byte | |
| ReadyStatus | byte | |
| InGame | byte | |

### Per-Player Data (full update only, continued):

| Field | Type | Description |
|-------|------|-------------|
| RankedEligible | byte | |
| ChatNameColor | string\0 | |
| AccountIcon | string\0 | |
| Country | string\0 | |
| GameModeAccess | byte | Can access all group modes? |
| GameModeAccessList | string\0 | Pipe-delimited per-mode access |

### Per-Recipient Buddy Data (full update only, appended individually):

For each recipient, the buffer is reset to the shared length, then per-player buddy flags are appended:

| Field | Type | Count | Description |
|-------|------|-------|-------------|
| IsBuddy | byte | playerCount | Whether THIS recipient is friends with each member |

**⚠️ REIMPLEMENTATION BUG:** Buddy flags are written from the emitter's perspective and shared to all recipients. C++ resets the buffer and writes per-recipient buddy data.

---

## Update Type Enumeration

| Value | Name | Triggers Full Player Data |
|-------|------|---------------------------|
| 0 | `TMM_CREATE_GROUP` | Yes |
| 1 | `TMM_FULL_GROUP_UPDATE` | Yes |
| 2 | `TMM_PARTIAL_GROUP_UPDATE` | No |
| 3 | `TMM_PLAYER_JOINED_GROUP` | Yes |
| 4 | `TMM_PLAYER_LEFT_GROUP` | Yes |
| 5 | `TMM_PLAYER_KICKED_FROM_GROUP` | Yes |
| 6 | `TMM_GROUP_JOINED_QUEUE` | Separate packet |
| 7 | `TMM_GROUP_REJOINED_QUEUE` | Separate packet + queue time |
| 8 | `TMM_GROUP_LEFT_QUEUE` | Separate packet |
| 9 | `TMM_INVITED_TO_GROUP` | Separate packet |
| 10 | `TMM_PLAYER_REJECTED_GROUP_INVITE` | Separate packet |
| 11 | `TMM_GROUP_QUEUE_UPDATE` | Queue update + avg wait time |
| 12 | `TMM_GROUP_NO_MATCHES_FOUND` | Queue update (no data) |
| 13 | `TMM_GROUP_FOUND_SERVER` | Queue update (no data) |
| 14 | `TMM_MATCHMAKING_DISABLED` | Queue update (no data) |

---

## Known Issues In Current Reimplementation

### Critical (Functional Impact)

1. **Wrong exponential base in prediction formula**
   - File: `MatchmakingMatch.cs:136`
   - Uses `Math.Pow(10.0, ...)` but C++ uses `pow(M_E, ...)` (natural exponential)
   - Fix: Change to `Math.Exp(...)`

2. **GroupKick reads wrong data type**
   - File: `GroupKick.cs:33`
   - Reads `int32` (account ID) but C++ reads `byte` (team slot)
   - Fix: Read `byte`, look up member by team slot

3. **Duplicate match notifications**
   - Files: `MatchmakingService.cs:448-450` and `MatchAnnounce.cs:64-66`
   - SpawnMatch sends FoundServer + MatchFound + AutoMatchConnect, then AnnounceMatch sends them again
   - Fix: SpawnMatch sends only MatchFoundUpdate + FoundServer; AnnounceMatch sends only AutoMatchConnect

4. **Buddy flags not per-recipient**
   - File: `MatchmakingGroup.cs:568-581`
   - Writes buddy flags from emitter's perspective for all recipients
   - Fix: Build per-recipient packets with individualised buddy flags

### Moderate (Correctness Impact)

5. **ArrangedMatchType mapping incomplete**
   - Missing: REBORN_NORMAL, REBORN_CASUAL, MIDWARS_REBORN → AM_MATCHMAKING_MIDWARS
   - Missing: CUSTOM → AM_MATCHMAKING_CUSTOM
   - Missing: bot group → AM_MATCHMAKING_BOTMATCH
   - Missing: !IsRanked() check for NORMAL → AM_UNRANKED_MATCHMAKING

6. **LeaveMatch operation order differs from C++**
   - File: `ChatSession.Client.cs:158-173`
   - C# calls UpdateStatus BEFORE RejoinDefaultChannel
   - C++ calls rejoin BEFORE UpdateStatus
   - Fix: Reorder to match C++

7. **JoinMatch missing channel cleanup**
   - File: `ChatSession.Client.cs:93-131`
   - Missing `LeaveAllChannels(CHAT_CHANNEL_FLAG_SERVER)` before joining new match channel
   - Fix: Add channel cleanup call before join
