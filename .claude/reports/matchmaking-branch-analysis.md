# Matchmaking Branch Analysis Report

**Date:** 2026-01-26
**Branch:** `matchmaking` (since `de4bbc9` / `origin/next`)
**Scope:** 35 files changed, ~3,500 lines added
**Cross-Referenced Against:** HON C++ Source Code, HON Chat Server C++ Source Code, Legacy KONGOR C# Implementation

---

## Verified Correct

| Area | Details |
|------|---------|
| **Protocol constants** | All `ChatProtocol` enum values (`TMMUpdateType`, `TMMFailedToJoinReason`, `EArrangedMatchType`, `ETMMGameTypes`, `ETMMTypes`, `ServerStatus`) match C++ `chatserver_protocol.h` exactly |
| **GroupCreate packet parsing** | Field order and types match C++ `HandleRequestCreateTMMGroup` exactly |
| **GroupUpdate (0x0D03) structure** | Header fields, per-member data layout, conditional full/partial logic all match C++ `BroadcastToGroup` |
| **AutoMatchConnect (0x0062) packet** | Matches `CTeamFinder::AnnounceMatchReady` format: ArrangedMatchType(byte), MatchupID(uint32), Address(string), Port(uint16), RandomInt(int32) |
| **CreateMatch (0x1502) packet** | Extended format matches C++ `CTeamFinder` code path with per-player fields (AccountID, Team, Slot, SocialBonus, WinValue, LossValue, IsProvisional, GroupIndex, BenefitValue) |
| **Premade bonus formula** | `4 * 2^groupSize` matches KONGOR legacy `TMMGroup.cs` |
| **Power mean TMR calculation** | `(Σ rating^6.5)^(1/6.5)` matches C++ `c_match.cpp` with `teamRankWeighting = 6.5` |
| **Combine method enum** | Values match `ETMMCombineMethod` from `c_matchmakercommon.h` |
| **Configuration defaults** | `LogisticPredictionScale = 225.0` matches `c_matchmaker.cpp` CVAR |
| **MatchInformation refactor** | Correctly changed from computed to `required init` to support TMM server-assigned IDs |

---

## Critical Issues

### 1. Matchup Prediction Uses Wrong Exponential Base

- **File:** `MatchmakingMatch.cs:136`
- **Issue:** Uses `Math.Pow(10.0, ...)` (base 10) but C++ uses `pow(M_E, ...)` (base *e*, natural exponential)
- **C++ Source:** `c_match.cpp:170` — `1.0f / (1.0f + pow(M_E, -(fTeamRating - fOtherTeamRating) / matchmaker_logisticPredictionScale))`
- **Impact:** Produces significantly different win probabilities. For TMR diff of 100 with scale 225: base-e gives 60.9%, base-10 gives 73.6%
- **Fix:** Change `Math.Pow(10.0, ...)` to `Math.Exp(...)` (equivalent to `Math.Pow(Math.E, ...)`)

### 2. GroupKick Reads Wrong Data Type From Packet

- **File:** `GroupKick.cs:33` / `GroupKickRequestData`
- **Issue:** Reads `buffer.ReadInt32()` (4-byte account ID) but C++ reads `pktRecv.ReadByte()` (1-byte team slot)
- **C++ Source:** `c_client.cpp:3007` — `const byte yTeamSlot(pktRecv.ReadByte());`
- **Impact:** Reads wrong bytes from the packet, causing either incorrect kicks or packet corruption for subsequent reads
- **Fix:** Change to `buffer.ReadInt8()` and use the team slot to look up the member, not account ID

### 3. Duplicate Match Notifications

- **Files:** `MatchmakingService.cs:448-450` and `MatchAnnounce.cs:64-66`
- **Issue:** `SpawnMatch` sends `FoundServerUpdate`, `MatchFoundUpdate`, and `AutoMatchConnect` immediately. Then `MatchAnnounce` (triggered by the game server's response) sends all three again
- **C++ Source:** `c_teamfinder.cpp` only sends `TMM_GROUP_FOUND_SERVER` at spawn time; `AnnounceMatchReady` sends `CHAT_CMD_AUTO_MATCH_CONNECT` later
- **Impact:** Players receive duplicate match-found, found-server, and auto-connect packets, potentially causing UI glitches or double connection attempts
- **Fix:** Only send `TMM_GROUP_FOUND_SERVER` at spawn time (like C++); send `MatchFoundUpdate` and `AutoMatchConnect` only in `MatchAnnounce`

### 4. MulticastUpdate Buddy Flags Not Per-Recipient

- **File:** `MatchmakingGroup.cs:568-581`
- **Issue:** Writes buddy (IsFriend) flags from the emitter's perspective for ALL recipients, then sends the same buffer to everyone
- **C++ Source:** `c_teamfinder.cpp:3710-3726` — Resets buffer length and writes per-recipient buddy data: each player sees whether **they** are friends with each member
- **Impact:** All group members see the same friendship data (the emitter's), not their own. Cosmetic/social impact
- **Fix:** Build per-recipient packets with individualised buddy flags, similar to C++ pattern

---

## Moderate Issues

### 5. ArrangedMatchType Mapping Incomplete

- **Files:** `MatchAnnounce.cs:123-132`, `MatchmakingService.cs:478-487`, `MatchmakingService.cs:589-598`
- **Issue:** Missing mappings for `REBORN_NORMAL`, `REBORN_CASUAL`, `MIDWARS_REBORN` → `AM_MATCHMAKING_MIDWARS`; `CUSTOM` → `AM_MATCHMAKING_CUSTOM`; bot group → `AM_MATCHMAKING_BOTMATCH`. Also doesn't check `IsRanked()` for NORMAL — always maps to `AM_MATCHMAKING`
- **C++ Source:** `c_group.cpp:962-978` `GetArrangedMatchType()` has full branching logic
- **Impact:** Reborn/custom/bot game types will get the wrong ArrangedMatchType, affecting game server behaviour

### 6. LeaveMatch Operation Order Differs From C++

- **File:** `ChatSession.Client.cs:158-173`
- **Issue:** Calls `UpdateStatus(CONNECTED)` BEFORE `RejoinDefaultChannel()`, but C++ calls `GetDefaultChannel` + `AddClient` BEFORE `UpdateStatus`
- **C++ Source:** `c_client.cpp:1548-1568` — Rejoins default channel first, then calls `UpdateStatus()`
- **Impact:** Friends/clan members briefly see the player as "Connected" without a channel, then the channel join triggers another update. Minor ordering issue

### 7. JoinMatch Missing Channel Cleanup

- **File:** `ChatSession.Client.cs:93-131`
- **Issue:** Does not call `LeaveAllChannels(CHAT_CHANNEL_FLAG_SERVER)` before joining new match channel
- **C++ Source:** `c_client.cpp:1524-1535` — Calls `LeaveAllChannels(CHAT_CHANNEL_FLAG_SERVER)` before joining new match channel
- **Impact:** Stale match channel memberships may accumulate if a player joins a new match without leaving the old one

---

## Noted Differences (Intentional/Acceptable)

| Item | Details |
|------|---------|
| `DEFAULT_CHANNEL_NAME = "KONGOR"` | Intentional rebrand from C++ "HoN" |
| `MatchAbandoned` reads `int32 MatchID` | C++ only reads 1 byte; reimplementation may be using a different match server version |
| `CombineMethod.FirstInFirstOut = 0` | Documented NEXUS extension, not in legacy |
| GroupMakeup scoring constants | NEXUS extension for fairness checking |
| C++ `INT_ROUND` on power mean before prediction | Reimplementation uses raw double; minor precision difference |
