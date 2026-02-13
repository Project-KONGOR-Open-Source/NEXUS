# Heroes of Newerth Matchmaking Protocol Documentation

## Sources (Authoritative)

### HON Game Client (C++)
- `C:\Users\SADS-810\Source\HON\k2public\chatserver_protocol.h` - Protocol command codes and enumerations
- `C:\Users\SADS-810\Source\HON\k2public\Chatserver Protocol.txt` - Official protocol documentation
- `C:\Users\SADS-810\Source\HON\src\k2\c_chatmanager.h` - Client-side chat manager
- `C:\Users\SADS-810\Source\HON\src\k2\c_chatmanager.cpp` - Client-side TMM implementation

### HON Game Client (LUA)
- `C:\Users\SADS-810\Source\HON\Heroes of Newerth\game\resources0\ui\scripts\matchmaking.lua` - UI logic and flow

### HON Chat Server (C++) - Matchmaking Algorithm
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_matchmaker.cpp` - **Complete matchmaking algorithm**
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_matchmakercommon.h` - Enums and structs
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_matchmakercommon.inl` - Algorithm functions
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_teamfinder.cpp` - Queue management
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_client.cpp` - Client packet handlers
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_group.cpp` - Group management
- `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\chatserver_types.h` - Type definitions

### HON Master Server API (PHP)
- `C:\Users\SADS-810\Source\HON-Zend-Server-API\masterapi-international\client_requester.php` - Client API
- `C:\Users\SADS-810\Source\HON-Zend-Server-API\masterapi-international\server_requester.php` - Server API
- `C:\Users\SADS-810\Source\HON-Zend-Server-API\masterapi-international\stats_requester.php` - Stats API
- `C:\Users\SADS-810\Source\HON-Zend-Server-API\library-international\hon\db\table\stats\` - Stats database tables

### Packet Captures
- `E:\Offline Arcade\Heroes Of Newerth\Project KONGOR\Packet Dumps` - Network packet captures

---

## 1. Protocol Overview

### Packet Structure
All TCP chat protocol packets follow this format:
```
[2 bytes] uint16 LE - packet length (excluding this field)
[2 bytes] uint16 LE - command code
[N bytes] payload data (command-specific)
```

All numeric values are **little-endian**. Strings are **NULL-terminated UTF-8**.

### Chat Server Ports
- **11031** - Stable build (default)
- **11032** - Test build
- **11037/11038** - Release Candidate

---

## 2. TCP Command Codes (Matchmaking)

### Client -> Server

| Code     | Constant                                    | Description                              |
|----------|---------------------------------------------|------------------------------------------|
| `0x0C0A` | `NET_CHAT_CL_TMM_GROUP_CREATE`              | Create matchmaking group                 |
| `0x0C0B` | `NET_CHAT_CL_TMM_GROUP_JOIN`                | Accept invite and join group             |
| `0x0C0C` | `NET_CHAT_CL_TMM_GROUP_LEAVE`               | Leave current group                      |
| `0x0C0D` | `NET_CHAT_CL_TMM_GROUP_INVITE`              | Invite player to group                   |
| `0x0C0F` | `NET_CHAT_CL_TMM_GROUP_REJECT_INVITE`       | Reject group invite                      |
| `0x0D00` | `NET_CHAT_CL_TMM_GROUP_KICK`                | Leader kicks member                      |
| `0x0D01` | `NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE`          | Leader joins matchmaking queue           |
| `0x0D02` | `NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE`         | Leave matchmaking queue                  |
| `0x0D04` | `NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS` | Send loading progress (0-100)          |
| `0x0D05` | `NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS` | Send ready status                        |
| `0x0D07` | `NET_CHAT_CL_TMM_POPULARITY_UPDATE`         | Request popularity data                  |
| `0x0D08` | `NET_CHAT_CL_TMM_GAME_OPTION_UPDATE`        | Leader updates group settings            |
| `0x0E06` | `NET_CHAT_CL_TMM_SWAP_GROUP_TYPE`           | Swap from PvP to bot group or vice versa |
| `0x0E07` | `NET_CHAT_CL_TMM_BOT_GROUP_UPDATE`          | Update bot configuration                 |
| `0x000F` | `CHAT_CMD_JOINING_GAME`                     | Tell server we're joining a game         |
| `0x0010` | `CHAT_CMD_JOINED_GAME`                      | Finished joining a game                  |
| `0x0011` | `CHAT_CMD_LEFT_GAME`                        | Left a game                              |

### Server -> Client

| Code     | Constant                                    | Description                              |
|----------|---------------------------------------------|------------------------------------------|
| `0x0C0D` | `NET_CHAT_CL_TMM_GROUP_INVITE`              | Invite received (bidirectional)          |
| `0x0C0E` | `NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST`    | Broadcast invite to group members        |
| `0x0C0F` | `NET_CHAT_CL_TMM_GROUP_REJECT_INVITE`       | Invite rejected (bidirectional)          |
| `0x0D01` | `NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE`          | Group joined queue (bidirectional)       |
| `0x0D02` | `NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE`         | Group left queue (bidirectional)         |
| `0x0D03` | `NET_CHAT_CL_TMM_GROUP_UPDATE`              | Group state update (with ETMMUpdateType) |
| `0x0D06` | `NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE`        | Queue status/time update                 |
| `0x0D09` | `NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE`        | Match details when found                 |
| `0x0E07` | `NET_CHAT_CL_TMM_BOT_GROUP_UPDATE`          | Bot update (bidirectional)               |
| `0x0E08` | `NET_CHAT_CL_TMM_BOT_GROUP_BOTS`            | Current bots configuration               |
| `0x0E09` | `NET_CHAT_CL_TMM_BOT_NO_BOTS_SELECTED`      | No bots selected error                   |
| `0x0E0A` | `NET_CHAT_CL_TMM_FAILED_TO_JOIN`            | Join failure with reason                 |
| `0x0E0B` | `NET_CHAT_CL_TMM_REGION_UNAVAILABLE`        | Region restriction error                 |
| `0x0E0C` | `NET_CHAT_CL_TMM_GROUP_REJOIN_QUEUE`        | Re-queue with previous time              |
| `0x0E0D` | `NET_CHAT_CL_TMM_GENERIC_RESPONSE`          | Generic response                         |
| `0x0E0F` | `NET_CHAT_CL_TMM_EVENTS_INFO`               | TMM event information                    |
| `0x0F01` | `NET_CHAT_CL_TMM_LEAVER_INFO`               | Leaver status information                |
| `0x0F02` | `NET_CHAT_CL_TMM_REQUEST_READY_UP`          | Request group to ready                   |
| `0x0F03` | `NET_CHAT_CL_TMM_START_LOADING`             | Begin map preloading                     |
| `0x0F04` | `NET_CHAT_CL_TMM_PENDING_MATCH`             | Match waiting for acceptance             |
| `0x0F05` | `NET_CHAT_CL_TMM_ACCEPT_PENDING_MATCH`      | Player accepted match                    |
| `0x0F06` | `NET_CHAT_CL_TMM_FAILED_TO_ACCEPT_PENDING_MATCH` | Player failed to accept            |
| `0x0F07` | `NET_CHAT_CL_TMM_CAMPAIGN_STATS`            | Season/Campaign stats                    |
| `0x0F09` | `NET_CHAT_CL_TMM_LEAVER_STRIKE_WARN`        | Leaver warning popup                     |

### Match Connection

| Code     | Constant                   | Description                    |
|----------|----------------------------|--------------------------------|
| `0x0062` | `CHAT_CMD_AUTO_MATCH_CONNECT` | Server IP/port to connect to |
| `0x0069` | `CHAT_CMD_SERVER_NOT_IDLE` | Server was not available       |

### Game Server <-> Chat Server

| Code     | Constant                   | Description                              |
|----------|----------------------------|------------------------------------------|
| `0x0500` | `NET_CHAT_GS_CONNECT`      | Game server requesting connection        |
| `0x0501` | `NET_CHAT_GS_DISCONNECT`   | Game server disconnecting                |
| `0x0502` | `NET_CHAT_GS_STATUS`       | Game server's current status             |
| `0x0503` | `NET_CHAT_GS_ANNOUNCE_MATCH` | Arranged match is ready for clients    |
| `0x0504` | `NET_CHAT_GS_ABANDON_MATCH` | Match failed to start                   |
| `0x0505` | `NET_CHAT_GS_MATCH_STARTED` | Match started successfully              |
| `0x0506` | `NET_CHAT_GS_REMIND_PLAYER` | Remind missing player                   |
| `0x0508` | `NET_CHAT_GS_NOT_IDLE`     | Server was not idle                      |
| `0x0509` | `NET_CHAT_GS_MATCH_ABORTED` | Match aborted                           |
| `0x1500` | `NET_CHAT_GS_ACCEPT`       | Accept connection from game server       |
| `0x1501` | `NET_CHAT_GS_REJECT`       | Refuse connection from game server       |
| `0x1502` | `NET_CHAT_GS_CREATE_MATCH` | Tell server to host arranged match       |
| `0x1503` | `NET_CHAT_GS_END_MATCH`    | Tell server to end match                 |

---

## 3. Enumerations

### ETMMUpdateType (GroupUpdate sub-type)
```
0  = TMM_CREATE_GROUP                 // Group created
1  = TMM_FULL_GROUP_UPDATE            // Full state sync
2  = TMM_PARTIAL_GROUP_UPDATE         // Loading/ready only
3  = TMM_PLAYER_JOINED_GROUP          // Player joined
4  = TMM_PLAYER_LEFT_GROUP            // Player left
5  = TMM_PLAYER_KICKED_FROM_GROUP     // Player kicked
6  = TMM_GROUP_JOINED_QUEUE           // Entered queue
7  = TMM_GROUP_REJOINED_QUEUE         // Re-entered queue
8  = TMM_GROUP_LEFT_QUEUE             // Left queue
9  = TMM_INVITED_TO_GROUP             // Invite sent
10 = TMM_PLAYER_REJECTED_GROUP_INVITE // Invite rejected
11 = TMM_GROUP_QUEUE_UPDATE           // Queue time update
12 = TMM_GROUP_NO_MATCHES_FOUND       // No matches found
13 = TMM_GROUP_NO_SERVERS_FOUND       // No servers available
14 = TMM_POPULARITY_UPDATE            // Popularity data
15 = TMM_FOUND_MATCH_UPDATE           // Match found
16 = TMM_GROUP_FOUND_SERVER           // Server allocated ("Sound The Horn!")
17 = TMM_MATCHMAKING_DISABLED         // MM disabled
```

### ETMMGameTypes (Game Type)
```
1  = TMM_GAME_TYPE_NORMAL          // Caldavar Ranked (5v5)
2  = TMM_GAME_TYPE_CASUAL          // Caldavar Casual (5v5)
3  = TMM_GAME_TYPE_MIDWARS         // MidWars (5v5)
4  = TMM_GAME_TYPE_RIFTWARS        // RiftWars (5v5)
5  = TMM_GAME_TYPE_CUSTOM          // Custom Maps
6  = TMM_GAME_TYPE_CAMPAIGN_NORMAL // Season Ranked (5v5)
7  = TMM_GAME_TYPE_CAMPAIGN_CASUAL // Season Casual (5v5)
8  = TMM_GAME_TYPE_REBORN_NORMAL   // Caldavar Reborn Ranked
9  = TMM_GAME_TYPE_REBORN_CASUAL   // Caldavar Reborn Casual
10 = TMM_GAME_TYPE_MIDWARS_REBORN  // MidWars Reborn
```

### ETMMTypes (Group Type)
```
1 = TMM_TYPE_SOLO     // Solo queue
2 = TMM_TYPE_PVP      // Player vs Player group (multiplayer)
3 = TMM_TYPE_COOP     // Co-op (vs bots) group
4 = TMM_TYPE_CAMPAIGN // Season/Campaign group
```

### EArrangedMatchType
```
0  = AM_PUBLIC               // Public game
1  = AM_MATCHMAKING          // Ranked Normal/Casual (TMM)
2  = AM_SCHEDULED_MATCH      // Tournament match
3  = AM_UNSCHEDULED_MATCH    // League match
4  = AM_MATCHMAKING_MIDWARS  // MidWars queue
5  = AM_MATCHMAKING_BOTMATCH // Bot co-op
6  = AM_UNRANKED_MATCHMAKING // Unranked queue
7  = AM_MATCHMAKING_RIFTWARS // RiftWars queue
8  = AM_PUBLIC_PRELOBBY      // Public pre-lobby
9  = AM_MATCHMAKING_CUSTOM   // Custom map queue
10 = AM_MATCHMAKING_CAMPAIGN // Season queue
```

### ETMMFailedToJoinReason
```
0  = TMMFTJR_LEAVER                // Player has leaver status
1  = TMMFTJR_DISABLED              // Matchmaking disabled
2  = TMMFTJR_BUSY                  // MM full/unavailable
3  = TMMFTJR_OPTION_UNAVAILABLE    // Selected option unavailable
4  = TMMFTJR_INVALID_VERSION       // Client version mismatch
5  = TMMFTJR_GROUP_FULL            // Group at max capacity
6  = TMMFTJR_BAD_STATS             // Cannot retrieve stats
7  = TMMFTJR_ALREADY_QUEUED        // Group already in queue
8  = TMMFTJR_TRIAL                 // Trial account (deprecated)
9  = TMMFTJR_BANNED                // Banned from matchmaking
10 = TMMFTJR_LOBBY_FULL            // Lobby full
11 = TMMFTJR_WRONG_PASSWORD        // Wrong password
12 = TMMFTJR_CAMPAIGN_NOT_ELIGIBLE // Not eligible for season
```

---

## 4. Packet Formats (Official Documentation)

### GroupCreate (0x0C0A) - Client -> Server

```
[X] string      - HoN client version (e.g., "4.10.1")
[1] byte        - TMM type (1=solo, 2=pvp group, 3=bot match)
[1] ETMMGameTypes - game type
[X] string      - map name ("caldavar", "grimmscrossing", "midwars")
[X] string      - game modes, pipe-delimited (e.g., "ap|sd|ar")
[X] string      - regions, pipe-delimited (e.g., "USE|EU|")
[1] bool        - ranked
[1] bool        - match fidelity (TRUE = higher quality, longer wait)
[1] byte        - bot difficulty (1=easy, 2=medium, 3=hard)
[1] bool        - randomize enemy bots
```

**Notes:**
- Flood protection applies
- If the request fails, server sends `NET_CHAT_CL_TMM_FAILED_TO_JOIN (0x0E0A)`
- The group leader is the client who creates the group

### GroupJoin (0x0C0B) - Client -> Server

```
[X] string      - HoN client version (e.g., "4.10.1")
[X] string      - target client's name (any member of the group, not just leader)
```

**Notes:**
- Target does not have to be the group leader
- Client must have been invited to the group first

### GroupLeave (0x0C0C) - Client -> Server

```
(no data)
```

### GroupInvite (0x0C0D) - Client -> Server

```
[X] string      - client's nickname to invite
```

**Notes:**
- Flood protection applies
- If target has DND (Do Not Disturb) on, fails silently
- Success sends `NET_CHAT_CL_TMM_GROUP_INVITE` to the target

### GroupInvite (0x0C0D) - Server -> Client

```
[X] string      - inviting client's name
[4] uint32      - inviting client's account ID
[1] EChatClientStatus - inviting client's status
[1] byte        - inviting client's flags
[X] string      - inviting client's name colour
[X] string      - inviting client's account icon
[X] string      - map name
[1] ETMMGameTypes - game type
[X] string      - game modes, pipe-delimited
[X] string      - server regions, pipe-delimited
```

### GroupKick (0x0D00) - Client -> Server

```
[1] byte        - team slot of the group member to kick (0-4)
```

**Notes:**
- Must be the group leader
- Cannot kick yourself

### PlayerReadyStatus (0x0D05) - Client -> Server

```
[1] bool        - ready status (0=not ready, 1=ready)
```

**Notes:**
- If group leader readies and others haven't, server sends `NET_CHAT_CL_TMM_REQUEST_READY_UP (0x0F02)` to all
- When all clients ready, server sends `NET_CHAT_CL_TMM_START_LOADING (0x0F03)` to all

### PlayerLoadingStatus (0x0D04) - Client -> Server

```
[1] byte        - loading percent (0-100, hex: 0x00-0x64)
```

**Notes:**
- Once all clients are ready AND send 100% loading, group joins matchmaking queue automatically

### GameOptionUpdate (0x0D08) - Client -> Server

```
[1] ETMMGameTypes - game type
[X] string      - map name
[X] string      - game modes, pipe-delimited
[X] string      - server regions, pipe-delimited
[1] bool        - ranked
[1] bool        - match fidelity
[1] byte        - bot difficulty
[1] bool        - randomize enemy bots
```

**Notes:**
- Must be the group leader

### GroupUpdate (0x0D03) - Server -> Client

```
[1] ETMMUpdateType - update type
[4] uint32      - account ID (context-dependent)
[1] byte        - number of clients in group
[2] uint16      - average MMR of clients in group
[4] uint32      - group leader's account ID
[1] EArrangedMatchType - arranged match type
[1] ETMMGameTypes - game type
[X] string      - map name
[X] string      - game modes, pipe-delimited
[X] string      - server regions, pipe-delimited
[1] bool        - ranked
[1] bool        - match fidelity
[1] byte        - bot difficulty
[1] bool        - randomize bots
[X] string      - country restrictions
[X] string      - player invitation responses
[1] byte        - matchmaking team size (5 for Caldavar, 3 for Grimm's Crossing)
[1] byte        - group type (2=pvp, 3=bot)

FOR EACH member (if update type != TMM_PARTIAL_GROUP_UPDATE):
    [4] uint32  - account ID
    [X] string  - account name
    [1] byte    - team slot (0-4)
    [2] uint16  - MMR (USHORT_MAX if unranked or MidWars)

FOR EACH member (always):
    [1] byte    - loading percent (0-100)
    [1] bool    - ready
    [1] bool    - in game

FOR EACH member (if update type != TMM_PARTIAL_GROUP_UPDATE):
    [1] bool    - eligible for ranked matchmaking
    [X] string  - chat name colour
    [X] string  - account icon
    [X] string  - country
    [1] bool    - has access to all group's game modes
    [X] string  - game mode access, pipe-delimited (e.g., "true|true|false")

FOR EACH member (if update type != TMM_PARTIAL_GROUP_UPDATE):
    [1] bool    - is a buddy
```

### GroupQueueUpdate (0x0D06) - Server -> Client

```
[1] ETMMUpdateType - update type (valid: 11, 12, 13, 16, 17)
    TMM_GROUP_QUEUE_UPDATE (11):     Average queue time updated
    TMM_GROUP_NO_MATCHES_FOUND (12): Too long in queue
    TMM_GROUP_NO_SERVERS_FOUND (13): No servers available
    TMM_GROUP_FOUND_SERVER (16):     Match starting
    TMM_MATCHMAKING_DISABLED (17):   Matchmaking disabled

IF (update type == TMM_GROUP_QUEUE_UPDATE):
    [4] uint32  - average queue time in seconds (0 = no data)
```

### MatchFoundUpdate (0x0D09) - Server -> Client

```
[X] string      - map name (e.g., "caldavar", "midwars")
[1] byte        - team size (5 for Caldavar, 3 for Grimm's Crossing, 1 for 1v1)
[1] ETMMGameTypes - game type
[X] string      - game mode code (e.g., "ap", "sd", "ar" - see Section 9)
[X] string      - server region code (e.g., "USE", "EU" - see Section 10)
[X] string      - extra info (debug data, may be empty)
```

### AutoMatchConnect (0x0062) - Server -> Client

```
[1] EArrangedMatchType - arranged match type
[4] uint32      - matchup ID
[X] string      - server address (IP)
[2] uint16      - server port
[4] uint32      - connection reminder flag
                  0xFFFFFFFF = this is a connection reminder
                  Otherwise = random value (duplicate packet workaround)
```

### JoiningGame (0x000F) - Client -> Server

```
[X] string      - server address and port (e.g., "49.51.172.52:21235")
```

**Notes:**
- Also removes client from all general chat channels (e.g., "HoN 5")

### FailedToJoin (0x0E0A) - Server -> Client

```
[1] ETMMFailedToJoinReason - failure reason

IF (failure reason == TMMFTJR_BANNED):
    [4] uint32  - time banned from matchmaking, in seconds
```

### LeaverInfo (0x0F01) - Server -> Client

```
[4] uint32      - number of matches played
[4] uint32      - number of match disconnects
```

**Notes:**
- Sent when client fails to create/join a group because they're a leaver

---

## 5. Complete Matchmaking Flow (Verified from Packet Capture)

### Observed Sequence (Frames from pcapng)

```
Frame 3136: Client -> Server: GroupCreate (0x0C0A)
            version="4.10.1", type=1 (solo), gameType=3 (midwars)
            map="midwars", modes="hb|ar|sd", regions="USE|EU|"
            ranked=0, fidelity=0, botDiff=1, randomBots=1

Frame 3140: Server -> Client: GroupUpdate (0x0D03)
            updateType=0 (TMM_CREATE_GROUP)
            Group created with leader info

Frame 3143: Client -> Server: PlayerReadyStatus (0x0D05)
            readyStatus=1

Frame 3146: Server -> Client: GroupUpdate (0x0D03)
            updateType=2 (TMM_PARTIAL_GROUP_UPDATE)
            Loading started

Frames 3178-3490: Client -> Server: PlayerLoadingStatus (0x0D04)
            Loading: 6% -> 9% -> 10% -> ... -> 100%
            (Server broadcasts TMM_PARTIAL_GROUP_UPDATE for each)

Frame 3491: Server -> Client: GroupUpdate (0x0D03)
            All players at 100%, auto-queue triggered
            Contains: GroupJoinQueue + QueueUpdate(type=11, time=116s)

Frame 3537: Server -> Client: GroupQueueUpdate (0x0D06)
            updateType=11 (TMM_GROUP_QUEUE_UPDATE)
            avgTime=116 seconds

Frame 3559: Server -> Client: EventsInfo (0x0E0F)
            TMM events data

Frame 3597: Server -> Client: Multiple packets:
            - GroupLeaveQueue (0x0D02)
            - GroupUpdate (0x0D03) - full update
            - MatchFoundUpdate (0x0D09)
              map="midwars", size=5, type=3, mode="ar", region="EU"
            - QueueUpdate (0x0D06) type=16 (TMM_GROUP_FOUND_SERVER)

Frame 3637: Server -> Client: AutoMatchConnect (0x0062)
            type=4 (AM_MATCHMAKING_MIDWARS)
            matchID=119044
            server="49.51.172.52", port=21235

Frame 3974: Client -> Server: JoiningGame (0x000F)
            address="49.51.172.52:21235"
```

---

## 6. Flow Diagram

```
┌─────────────┐                              ┌─────────────┐
│   Client    │                              │ Chat Server │
└──────┬──────┘                              └──────┬──────┘
       │                                            │
       │  GroupCreate (0x0C0A)                      │
       │  version, type, map, modes, regions        │
       │ ──────────────────────────────────────────>│
       │                                            │
       │  GroupUpdate (0x0D03)                      │
       │  type=0 (TMM_CREATE_GROUP)                 │
       │ <──────────────────────────────────────────│
       │                                            │
       │  PlayerReadyStatus (0x0D05) ready=1        │
       │ ──────────────────────────────────────────>│
       │                                            │
       │  StartLoading (0x0F03)                     │
       │ <──────────────────────────────────────────│
       │                                            │
       │  PlayerLoadingStatus (0x0D04) 0%->100%     │
       │ ──────────────────────────────────────────>│
       │                                            │
       │  GroupUpdate (0x0D03)                      │
       │  type=2 (TMM_PARTIAL_GROUP_UPDATE)         │
       │ <──────────────────────────────────────────│
       │                                            │
       │  [100% loading -> auto queue join]         │
       │                                            │
       │  GroupJoinQueue (0x0D01)                   │
       │ <──────────────────────────────────────────│
       │                                            │
       │  GroupQueueUpdate (0x0D06)                 │
       │  type=11 (TMM_GROUP_QUEUE_UPDATE)          │
       │  avgTime=<seconds>                         │
       │ <──────────────────────────────────────────│
       │                                            │
       │          [...waiting in queue...]          │
       │                                            │
       │  MatchFoundUpdate (0x0D09)                 │
       │  map, size, mode, region                   │
       │ <──────────────────────────────────────────│
       │                                            │
       │  GroupQueueUpdate (0x0D06)                 │
       │  type=16 (TMM_GROUP_FOUND_SERVER)          │
       │  "Sound The Horn!"                         │
       │ <──────────────────────────────────────────│
       │                                            │
       │  AutoMatchConnect (0x0062)                 │
       │  IP, port, matchID                         │
       │ <──────────────────────────────────────────│
       │                                            │
       │  JoiningGame (0x000F) to IP:port           │
       │ ──────────────────────────────────────────>│
       │                                            │
       │  [Client connects to game server]          │
       │                                            │
```

---

## 7. Group State Machine

```
[NOT_IN_GROUP]
      │
      │ GroupCreate (0x0C0A)
      v
[WAITING_TO_START] ────> GroupUpdate type=0 (TMM_CREATE_GROUP) sent to client
      │
      │ All players ready (PlayerReadyStatus 0x0D05)
      v
[LOADING_RESOURCES] ───> StartLoading (0x0F03) sent
      │
      │ All players at 100% (PlayerLoadingStatus 0x0D04)
      v
[IN_QUEUE] ────────────> GroupJoinQueue (0x0D01) sent
      │                  GroupQueueUpdate type=11 (periodic updates)
      │
      │ Match broker finds match
      v
[MATCH_FOUND] ─────────> MatchFoundUpdate (0x0D09)
      │                  GroupQueueUpdate type=16 (TMM_GROUP_FOUND_SERVER)
      │
      │ Server allocated
      v
[CONNECTING] ──────────> AutoMatchConnect (0x0062)
      │
      │ Client connects to game server
      v
[IN_GAME]
```

---

## 8. Key Implementation Details

### Readiness Management
- **Leader:** Starts not ready, must explicitly ready up via `0x0D05`
- **When leader readies but others haven't:** Server sends `NET_CHAT_CL_TMM_REQUEST_READY_UP (0x0F02)` to all
- **When all members ready:** Server sends `NET_CHAT_CL_TMM_START_LOADING (0x0F03)` to all

### Loading Management
- Client sends `PlayerLoadingStatus (0x0D04)` with percent 0-100
- Server broadcasts `GroupUpdate type=2 (TMM_PARTIAL_GROUP_UPDATE)` for each update
- When all at 100% -> automatic queue join

### Queue Time Tracking
- `GroupQueueUpdate (0x0D06)` with `type=11` contains average queue time in seconds
- 0 means no data available
- Queue time can be remembered for rejoin (`NET_CHAT_CL_TMM_GROUP_REJOIN_QUEUE` 0x0E0C)

### Match Found Sequence
1. `GroupLeaveQueue (0x0D02)` - group leaves queue
2. `GroupUpdate (0x0D03)` - full state update
3. `MatchFoundUpdate (0x0D09)` - match details (map, mode, region)
4. `GroupQueueUpdate (0x0D06) type=16` - "Sound The Horn!" (server allocated)
5. `AutoMatchConnect (0x0062)` - server IP/port for client to connect

### Connection Reminder
- If client doesn't connect, server sends `AutoMatchConnect` again with `reminder=0xFFFFFFFF`
- The 4th field being `0xFFFFFFFF` indicates this is a reminder, not initial notification

---

## 9. Game Modes Reference

Game mode codes are sent as strings in `MatchFoundUpdate` packets. These mappings are from `CMMCommon::TranslateGameMode()` in the legacy source code:

| Code  | Enum Constant                      | Description                           |
|-------|-----------------------------------|---------------------------------------|
| `ap`  | TMM_GAME_MODE_ALL_PICK            | Standard all pick                     |
| `apg` | TMM_GAME_MODE_ALL_PICK_GATED      | All pick with gated hero pool         |
| `apd` | TMM_GAME_MODE_ALL_PICK_DUPLICATE_HERO | All pick with duplicate heroes    |
| `sd`  | TMM_GAME_MODE_SINGLE_DRAFT        | Pick from 3 random heroes             |
| `bd`  | TMM_GAME_MODE_BANNING_DRAFT       | Ban then pick                         |
| `bp`  | TMM_GAME_MODE_BANNING_PICK        | Ban during pick                       |
| `ar`  | TMM_GAME_MODE_ALL_RANDOM          | Random hero assignment                |
| `lp`  | TMM_GAME_MODE_LOCK_PICK           | 5-player lock pick                    |
| `bb`  | TMM_GAME_MODE_BLIND_BAN           | Blind ban mode                        |
| `bbg` | TMM_GAME_MODE_BLIND_BAN_GATED     | Blind ban with gated hero pool        |
| `bbr` | TMM_GAME_MODE_BLIND_BAN_RAPID_FIRE | Blind ban rapid fire                 |
| `bm`  | TMM_GAME_MODE_BOT_MATCH           | Bot match mode                        |
| `cm`  | TMM_GAME_MODE_CAPTAINS_PICK       | Captain's mode                        |
| `br`  | TMM_GAME_MODE_BALANCED_RANDOM     | Team-balanced random                  |
| `km`  | TMM_GAME_MODE_KROS_MODE           | RiftWars/Kros mode                    |
| `rd`  | TMM_GAME_MODE_RANDOM_DRAFT        | Random draft mode                     |
| `bdr` | TMM_GAME_MODE_BANNING_DRAFT_RAPID_FIRE | Banning draft rapid fire         |
| `cp`  | TMM_GAME_MODE_COUNTER_PICK        | Counter pick mode                     |
| `fp`  | TMM_GAME_MODE_FORCE_PICK          | Force pick mode                       |
| `sp`  | TMM_GAME_MODE_SOCCER_PICK         | Soccer pick mode                      |
| `ss`  | TMM_GAME_MODE_SOLO_SAME           | Solo same hero selection              |
| `sm`  | TMM_GAME_MODE_SOLO_DIFF           | Solo different hero selection         |
| `hb`  | TMM_GAME_MODE_HERO_BAN            | Hero ban mode                         |
| `mwb` | TMM_GAME_MODE_MIDWARS_BETA        | MidWars beta mode                     |
| `rb`  | TMM_GAME_MODE_REBORN              | Reborn mode                           |

---

## 10. Regions Reference

| Code  | Name           |
|-------|----------------|
| `USE` | US East        |
| `USW` | US West        |
| `EU`  | Europe         |
| `SG`  | Singapore      |
| `MY`  | Malaysia       |
| `PH`  | Philippines    |
| `TH`  | Thailand       |
| `ID`  | Indonesia      |
| `VN`  | Vietnam        |
| `RU`  | Russia         |
| `KR`  | Korea          |
| `AU`  | Australia      |
| `LAT` | Latin America  |
| `DX`  | China DX       |
| `CN`  | China          |
| `BR`  | Brazil         |
| `TR`  | Turkey         |

---

## 11. Client UI Commands (from LUA)

The game client's LUA script uses these UI commands to communicate with the C++ engine:

| Command                           | Purpose                          |
|-----------------------------------|----------------------------------|
| `CreateTMMGroup(...)`             | Create matchmaking group         |
| `JoinTMMGroup(playerName)`        | Accept invite and join group     |
| `LeaveTMMGroup()`                 | Leave current group              |
| `InviteToTMMGroup(playerName)`    | Invite player to group           |
| `KickFromTMMGroup(slotNumber)`    | Kick player from group           |
| `LeaveTMMQueue()`                 | Leave matchmaking queue          |
| `SendTMMPlayerReadyStatus(...)`   | Set ready status                 |
| `SendTMMGroupOptionsUpdate(...)`  | Update group settings            |
| `RequestTMMPopularityUpdate()`    | Request queue popularity data    |
| `GetTMMOtherPlayersReady()`       | Check if other players are ready |

---

## 12. Matchmaking Service (Game Finder)

### When The Matchmaking Service Is Involved

The matchmaking service operates **server-side on the Chat Server**. It becomes involved after groups enter the queue:

```
[Client Flow]                          [Server Flow]
     │                                      │
GroupCreate ──────────────────────────────> │
     │                                      │
     │ <────────────────────── GroupUpdate (type=0, group created)
     │                                      │
PlayerReadyStatus ────────────────────────> │
     │                                      │
     │ <────────────────────── StartLoading (0x0F03)
     │                                      │
PlayerLoadingStatus (0%→100%) ────────────> │
     │                                      │
     │ <────────────────────── GroupJoinQueue (0x0D01)
     │                                      │
     │                            ┌─────────┴─────────┐
     │                            │ MATCHMAKING       │
     │                            │ SERVICE ENGAGED   │
     │                            └─────────┬─────────┘
     │                                      │
     │ <────────────────────── GroupQueueUpdate (type=11, avg time)
     │                                      │ (periodic updates)
     │                                      │
     │                            [Match Found!]
     │                                      │
     │ <────────────────────── MatchFoundUpdate (0x0D09)
     │ <────────────────────── GroupQueueUpdate (type=16, server found)
     │ <────────────────────── AutoMatchConnect (0x0062)
     │                                      │
JoiningGame ──────────────────────────────> │
     │                                      │
```

**Key Point:** The matchmaking service only processes groups that are **fully loaded and in the queue**.

### Failure Handling (from Protocol)

| Update Type | Constant                   | Meaning                       |
|-------------|----------------------------|-------------------------------|
| 11          | TMM_GROUP_QUEUE_UPDATE     | Normal queue time update      |
| 12          | TMM_GROUP_NO_MATCHES_FOUND | Too long in queue, no matches |
| 13          | TMM_GROUP_NO_SERVERS_FOUND | No servers available          |
| 16          | TMM_GROUP_FOUND_SERVER     | Match found, server allocated |
| 17          | TMM_MATCHMAKING_DISABLED   | Matchmaking is disabled       |

---

## 13. Matchmaking Algorithm (Official HON Chat Server)

**Source:** `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server\c_matchmaker.cpp`

The matchmaking algorithm runs in periodic "spawn cycles" (approximately 5 seconds each). Each cycle processes all queued groups and attempts to form matches.

### 13.1 Algorithm Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    MATCHMAKING CYCLE                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Receive group data from parent process                      │
│                          ↓                                      │
│  2. PruneGroups() - Remove groups already matched               │
│                          ↓                                      │
│  3. CombineGroups() - Form teams from compatible groups         │
│     • TS5_5_RANDOM (full 5-stacks)                              │
│     • TS5_4_PLUS_1_RANDOM (4+1 combinations)                    │
│     • TS5_3_PLUS_2_RANDOM (3+2 combinations)                    │
│     • TS5_ALL_ONES_RANDOM (solo queue teams)                    │
│                          ↓                                      │
│  4. CreateMatches() - Pair teams against each other             │
│     • FindTeamMatchup() - Find best opposing team               │
│     • BalanceTeams() - Swap groups to improve fairness          │
│                          ↓                                      │
│  5. WriteMatchData() - Send matches back to parent              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 13.2 Team Composition Scoring

Groups are scored based on their composition to ensure fair matchups:

| Composition   | Score | Description                    |
|---------------|-------|--------------------------------|
| 5-stack       | 25    | Full premade team              |
| 4+1           | 17    | 4-stack with 1 solo            |
| 3+2           | 13    | 3-stack with 2-stack           |
| 3+1+1         | 11    | 3-stack with 2 solos           |
| 2+2+1         | 9     | Two 2-stacks with 1 solo       |
| 2+1+1+1       | 7     | 2-stack with 3 solos           |
| 1+1+1+1+1     | 5     | All solo queue                 |

For 3v3 (Grimm's Crossing):

| Composition   | Score |
|---------------|-------|
| 3-stack       | 9     |
| 2+1           | 5     |
| 1+1+1         | 3     |

**Group Makeup Fairness Rule:** Teams with a composition score difference > 2 must wait longer before being matched (default: 3 minutes for ranked, 1.5 minutes for casual/MidWars).

### 13.3 Combine Methods

The algorithm uses different strategies to form teams depending on the cycle. Method names are prefixed with team size (`TS5_`, `TS3_`, `TS1_`) in the source code:

**5v5 Methods (Team Size 5):**

| Method (Source Name)             | Description                                       |
|----------------------------------|---------------------------------------------------|
| `TS5_FULL_OR_2_GROUPS_TIME_QUEUED` | Full teams or 2-group combos, sorted by wait time |
| `TS5_FULL_OR_2_GROUPS_RANDOM`      | Full teams or 2-group combos, randomised          |
| `TS5_5_RANDOM`                     | Only match full 5-stacks                          |
| `TS5_4_PLUS_1_RANDOM`              | Only 4+1 combinations                             |
| `TS5_3_PLUS_2_RANDOM`              | Only 3+2 combinations                             |
| `TS5_ALL_ONES_RANDOM`              | Only solo queue players                           |
| `TS5_ALL_GROUP_SIZES_RANDOM`       | Any combination of group sizes                    |
| `TS5_ALL_EXPERIMENTAL`             | Experimental matching (currently disabled)        |

**3v3 Methods (Team Size 3 - Grimm's Crossing):**

| Method (Source Name)             | Description                                       |
|----------------------------------|---------------------------------------------------|
| `TS3_FULL_OR_2_GROUPS_TIME_QUEUED` | Full teams or 2-group combos, sorted by wait time |
| `TS3_FULL_OR_2_GROUPS_RANDOM`      | Full teams or 2-group combos, randomised          |
| `TS3_ALL_ONES_RANDOM`              | Solo queue only                                   |
| `TS3_ALL_GROUP_SIZES_RANDOM`       | Any combination of group sizes                    |

**1v1 Methods (Team Size 1 - Solo Map):**

| Method (Source Name)             | Description                                       |
|----------------------------------|---------------------------------------------------|
| `TS1_RANDOM`                       | 1v1 matching                                      |

**Special Methods:**

| Method (Source Name)             | Description                                       |
|----------------------------------|---------------------------------------------------|
| `BRUTE_FORCE`                      | Force match for long-waiting groups (any team size) |

### 13.4 Cycle Timing

| Cycle Multiple | Action                                | Timing (at 5s cycles) |
|----------------|---------------------------------------|----------------------|
| Every 2        | Alternate between 5v5 and 3v3         | 10 seconds           |
| Every 4        | Include inexperienced groups          | 20 seconds           |
| Every 6        | Full/two-group matching               | 30 seconds           |
| Every 24       | Any-group-size matching               | 2 minutes            |

### 13.5 Matchup Prediction (ELO-Based)

The algorithm uses a logistic function to predict match outcomes:

```
Prediction = 1 / (1 + 10^(-(AdjustedTMR_A - AdjustedTMR_B) / Scale))
```

Where:
- `Prediction` = Probability that Team A wins (0.0 to 1.0)
- `AdjustedTMR_A` = **Weighted** average TMR of Team A (not simple average)
- `AdjustedTMR_B` = **Weighted** average TMR of Team B
- `Scale` = Logistic prediction scale, configurable via `matchmaker_logisticPredictionScale` (default: 225.0)

**Adjusted TMR Calculation:**

The adjusted TMR uses a weighting constant (`matchmaker_teamRankWeighting`, default: 6.5) to emphasise higher-skilled players:

```cpp
// From source: GetAdjustedTeamTMR()
AdjustedTMR = (Sum of weighted player TMRs) / TeamSize
// Weighting gives more influence to higher TMR players
```

**Example:** If Team A has adjusted TMR of 1500 and Team B has 1400:
```
Prediction = 1 / (1 + 10^(-(1500-1400)/225))
           = 1 / (1 + 10^(-0.444))
           = 1 / (1 + 0.359)
           = 0.736 (73.6% win chance for Team A)
```

**Note:** Higher scale values reduce the impact of TMR differences (flatter curve), making matches more likely to be considered fair.

### 13.6 Wait Time TMR Spread

As groups wait longer, the acceptable TMR range expands. Wait time thresholds are **configurable via CVARs**, not hardcoded:

| CVAR | Default | Wait Value |
|------|---------|------------|
| `matchmaker_waitTime1` | 60s | 2 |
| `matchmaker_waitTime2` | 120s | 3 |
| `matchmaker_waitTime3` | 180s | 4 |
| `matchmaker_waitTime4` | 240s | 5 |
| `matchmaker_waitTime5` | 300s | 6 |
| `matchmaker_waitTime6` | 600s | 7 |

Wait values above threshold 6 use value 10. The defaults shown produce:

| Wait Time | Wait Value | TMR Spread Multiplier |
|-----------|------------|----------------------|
| 0-60s     | 1          | Base range           |
| 60s+      | 2          | 2x base              |
| 120s+     | 3          | 3x base              |
| 180s+     | 4          | 4x base              |
| 240s+     | 5          | 5x base              |
| 300s+     | 6          | 6x base              |
| 600s+     | 7          | 7x base              |
| >600s     | 10         | 10x base             |

**Note:** All thresholds are server-configurable. The values shown are defaults from the legacy HON Chat Server.

**TMR Spread Calculation:**
```
Spread = WaitValue × TMRMultiplier (default: 6)
Low = AverageTMR - Spread
High = AverageTMR + Spread
```

For **outlier players** (TMR < 1200 or > 1750), an additional spread is added:
```
OutlierSpread = WaitValue × TMROutlierValue (default: 40)
```

### 13.7 Win/Loss Percentage Thresholds

The algorithm only creates matches within acceptable fairness bounds:

| Parameter                  | Default | Description                           |
|----------------------------|---------|---------------------------------------|
| Starting Win %             | 51%     | Initial acceptable win probability    |
| Starting Loss %            | 49%     | Initial acceptable loss probability   |
| Win/Loss Multiplier        | 0.015   | Expansion per wait tier               |
| Outlier Multiplier         | 0.240   | Additional expansion for outliers     |

As wait time increases, the acceptable range expands:
```
WinPercent  = 0.51 + (WaitValue × 0.015) + OutlierAdjustment
LossPercent = 0.49 - (WaitValue × 0.015) - OutlierAdjustment
```

### 13.8 Team Balancing Algorithm

When two teams are found, the algorithm attempts to balance them by swapping groups:

**First Pass (Same-Size Swaps):**
1. Try swapping 4-player groups between teams
2. Try swapping 3-player groups
3. Try swapping 2-player groups (all combinations)
4. Try swapping solo players (all combinations)

**Second Pass (Unequal-Size Swaps):**
- Swap 4-stack for 3+1
- Swap 4-stack for 2+2
- Swap 4-stack for 2+1+1
- Swap 4-stack for 1+1+1+1
- Swap 3-stack for 2+1
- Swap 3-stack for 1+1+1
- Swap 2-stack for 1+1

**Balance Validation:**
```cpp
bool IsValidGroupSwap(TeamA, TeamB, GroupA, GroupB) {
    oldPrediction = GetMatchupPrediction(TeamA, TeamB);
    SwapGroups(TeamA, TeamB, GroupA, GroupB);
    newPrediction = GetMatchupPrediction(TeamA, TeamB);

    // Keep swap if new prediction is closer to 50%
    if (abs(0.5 - oldPrediction) < abs(0.5 - newPrediction)) {
        SwapGroups(TeamA, TeamB, GroupB, GroupA);  // Undo
        return false;
    }
    return true;
}
```

The algorithm iterates up to 3 times, shuffling groups between iterations to find optimal balance. Target: 49.5% - 50.5% matchup prediction.

### 13.9 K-Factor (MMR Gain/Loss Calculation)

**Base K-Factor:** 10 (±5 TMR for a 50/50 match)

K-Factor is modified by multipliers, not capped at a maximum value:

**Provisional Players** (< 10 matches in game type AND TMR < 1750):
```
K-Factor = BaseKFactor × ProvisionalMultiplier (default: 2.0)
// Results in K-Factor of 20 for provisional players
```

**High TMR Players** (> 1600 TMR):
```
Reduction = CLAMP((TMR - 1600) / 300, 0, 1) × ReducedKFactorMultiplier (default: 0.20)
K-Factor = BaseKFactor × (1 - Reduction)
// Results in K-Factor of 8-10 for high TMR players
```

**Note:** There is no explicit `maxKFactor` CVAR. The effective maximum is `BaseKFactor × ProvisionalMultiplier` (default: 20). All multipliers are server-configurable.

**Win/Loss Value Calculation:**
```
WinValue  = (1 - Prediction) × KFactor × SkillDifferenceAdjustment
LossValue = -Prediction × KFactor × SkillDifferenceAdjustment
```

**Skill Difference Adjustment** (Gamma Distribution):
Applied when:
- Player TMR > 1750 (outlier), OR
- Group has skill difference > 175 TMR AND win rate < 59%

```
Adjustment = GammaDistribution(PlayerTMR - TeamAvgTMR, k=18, theta=5.0)
```

**Reduced Loss for Small Groups:**
When a team of only 1s and 2s plays against a team with a 4 or 5-stack:
```
LossValue = LossValue × 0.5  // 50% reduced loss
```

### 13.10 Experience Classification

**Inexperienced Player:**
- Total matches < 50, OR
- TMR < 1625

**Provisional Player:**
- Matches in game type < 10, AND
- TMR < 1750

Inexperienced players are matched together for the first few cycles, then mixed with experienced players after waiting 3+ minutes.

### 13.11 Brute Force Matching

When groups have waited too long, the algorithm bypasses normal fairness checks:

| Group Type  | Brute Force Threshold |
|-------------|----------------------|
| Solo Queue  | 10 minutes           |
| Any Group   | 20 minutes           |

Brute force matching:
1. Creates a pool of all compatible groups
2. Randomises sorting (time, TMR ascending, TMR descending, random)
3. Attempts to form two teams from the pool
4. Tries up to 3 different permutations
5. Creates match even if fairness is suboptimal

### 13.12 Special Rules

**5-Stack vs Solos Protection:**
- 5-stacks can NEVER be matched against 5 solo players
- 5-stacks can only match against 2+1+1+1 or higher after waiting 6+ minutes

**Campaign (Seasons) Mode:**
- 5-stacks can only match against: 5, 4+1, 3+2, or 2+2+1
- Custom win/loss values based on prediction:
  - Prediction ≥ 60%: Win +3, Loss -7
  - Prediction ≤ 40%: Win +7, Loss -3
  - Otherwise: Win +5, Loss -5

**Match Fidelity:**
- Players can opt for "higher quality" matches
- Restricts acceptable prediction to 47% - 53%
- Maximum wait value capped at 3

**IP Conflict Checking:**
- Optional: Prevents players with same IP on opposing teams

---

## 14. Configuration Variables (CVARs)

All matchmaking parameters are configurable via CVARs. Defaults from `c_matchmaker.cpp`:

### 14.1 TMR Range and Outliers

| CVAR                        | Default | Description                             |
|-----------------------------|---------|----------------------------------------|
| matchmaker_minimumTMR       | 1000.0  | Minimum possible TMR                    |
| matchmaker_maximumTMR       | 2500.0  | Maximum possible TMR                    |
| matchmaker_lowTMROutlier    | 1200.0  | Below this = low outlier                |
| matchmaker_highTMROutlier   | 1750.0  | Above this = high outlier               |
| matchmaker_TMRMultiplier    | 6       | TMR spread per wait tier                |
| matchmaker_TMROutlierValue  | 40      | Additional spread for outliers          |

### 14.2 K-Factor and MMR

| CVAR                                      | Default | Description                       |
|-------------------------------------------|---------|-----------------------------------|
| matchmaker_baseKFactor                    | 10.0    | Base MMR change (±5 for 50/50)    |
| matchmaker_provisionalKFactorMultiplier   | 2.0     | Multiplier for provisional players (effective max K = 20) |
| matchmaker_provisionalMatchCount          | 10      | Matches for provisional status    |
| matchmaker_provisionalTMRCutoff           | 1750.0  | TMR cap for provisional bonus     |
| matchmaker_reducedKFactorMultiplier       | 0.20    | Max reduction for high TMR (20%)  |
| matchmaker_reducedKFactorTMRCutoff        | 1600.0  | TMR threshold for reduction start |

**Note:** There is no explicit `maxKFactor` CVAR. The effective maximum K-Factor is `baseKFactor × provisionalKFactorMultiplier` (default: 10 × 2 = 20).

### 14.3 Experience Classification

| CVAR                                | Default | Description                        |
|-------------------------------------|---------|-----------------------------------|
| matchmaker_inexperiencedMatchCount  | 50      | Max matches for "inexperienced"    |
| matchmaker_inexperiencedTMRCutoff   | 1625.0  | Max TMR for "inexperienced"        |

### 14.4 Wait Times and Fairness

| CVAR                                    | Default | Description                        |
|-----------------------------------------|---------|-----------------------------------|
| matchmaker_defaultFairWaitTime          | 3.0     | Minutes before unfair group makeup |
| matchmaker_defaultLenientWaitTime       | 1.5     | Minutes for casual/MidWars         |
| matchmaker_defaultFullTeamWaitTime      | 6.0     | Minutes for 5-stack vs lower       |
| matchmaker_bruteForceWaitTime           | 20.0    | Minutes before brute force         |
| matchmaker_bruteForceSoloWaitTime       | 10.0    | Minutes for solo brute force       |
| matchmaker_defaultGroupMakeupDifference | 2       | Max composition score difference   |

**Wait Value Thresholds (for TMR Spread Expansion):**

| CVAR                  | Default | Wait Value |
|-----------------------|---------|------------|
| matchmaker_waitTime1  | 60      | 2          |
| matchmaker_waitTime2  | 120     | 3          |
| matchmaker_waitTime3  | 180     | 4          |
| matchmaker_waitTime4  | 240     | 5          |
| matchmaker_waitTime5  | 300     | 6          |
| matchmaker_waitTime6  | 600     | 7          |

Queue durations below `waitTime1` use wait value 1. Durations above `waitTime6` use wait value 10.

### 14.5 Prediction and Thresholds

| CVAR                                | Default | Description                        |
|-------------------------------------|---------|-----------------------------------|
| matchmaker_logisticPredictionScale  | 225.0   | ELO scale factor                   |
| matchmaker_startingWinPercent       | 0.51    | Initial acceptable win %           |
| matchmaker_startingLossPercent      | 0.49    | Initial acceptable loss %          |
| matchmaker_winLossMultiplier        | 0.015   | Expansion per wait tier            |
| matchmaker_winLossOutlierMultiplier | 0.240   | Additional expansion for outliers  |
| matchmaker_balanceTeamsLowPercent   | 0.80    | Max prediction to attempt balance  |
| matchmaker_balanceTeamsHighPercent  | 0.20    | Min prediction to attempt balance  |

### 14.6 Match Fidelity

| CVAR                                       | Default | Description                      |
|--------------------------------------------|---------|----------------------------------|
| matchmaker_enableMatchFidelity             | true    | Allow match fidelity option      |
| matchmaker_maxMatchFidelityTMRDifference   | 100.0   | Max TMR diff for fidelity option |
| matchmaker_fairLowMatchFidelityWinPercent  | 0.47    | Min win % for fidelity matches   |
| matchmaker_fairHighMatchFidelityWinPercent | 0.53    | Max win % for fidelity matches   |
| matchmaker_matchFidelityMaxWaitValue       | 3       | Max wait value with fidelity     |

### 14.7 Gamma Curve (Skill Difference)

| CVAR                              | Default | Description                         |
|-----------------------------------|---------|-------------------------------------|
| matchmaker_skillDifferenceEnabled | true    | Enable skill difference penalty     |
| matchmaker_gammaCurveRange        | 175.0   | TMR diff triggering adjustment      |
| matchmaker_gammaCurveK            | 18      | Gamma distribution shape parameter  |
| matchmaker_gammaCurveTheta        | 5.0     | Gamma distribution scale parameter  |
| matchmaker_teamRankWeighting      | 6.5     | Team TMR weighting constant         |

### 14.8 Cycle Timing

| CVAR                                         | Default | Description                       |
|----------------------------------------------|---------|-----------------------------------|
| matchmaker_inexperiencedGroupSpawnCycleDelay | 4       | Cycles between inexperienced runs |
| matchmaker_fullOrTwoGroupSpawnCycleDelay     | 6       | Cycles between full/2-group runs  |
| matchmaker_anyGroupsSpawnCycleDelay          | 24      | Cycles between any-group runs     |
| matchmaker_maxMatchingCatchallLoopCount      | 3       | Max loops per catchall routine    |
| matchmaker_maxMatchingTime                   | 15000   | Max algorithm runtime (ms)        |

### 14.9 Feature Flags

| CVAR                                          | Default | Description                      |
|-----------------------------------------------|---------|----------------------------------|
| matchmaker_enableAlternateCombines            | true    | Broader group makeup matching    |
| matchmaker_enableAlternateTeamBalancing       | false   | Unequal-size group swapping      |
| matchmaker_enableAlternateGroupSorting        | true    | TMR-based group sorting          |
| matchmaker_enableAlternateTeamSorting         | true    | TMR-based team sorting           |
| matchmaker_alternateSortingRatio              | 0.90    | Frequency of alternate sorting   |
| matchmaker_enableIPConflictChecking           | false   | Block same-IP opposing players   |
| matchmaker_enableKDRatioUse                   | false   | Use K/D for matching             |
| matchmaker_maxKDDifference                    | 0.75    | Max K/D difference allowed       |
| matchmaker_enableReducedMMRLossForSmallGroups | true    | 50% loss for small vs large      |
| matchmaker_fast1v1                            | true    | Fast 1v1 matching                |

---

## 15. TMR Brackets

The matchmaking system uses 22 TMR brackets for statistical tracking:

| Bracket | TMR Range    |
|---------|--------------|
| 0       | 750 - 999    |
| 1       | 1000 - 1049  |
| 2       | 1050 - 1099  |
| 3       | 1100 - 1149  |
| 4       | 1150 - 1199  |
| 5       | 1200 - 1249  |
| 6       | 1250 - 1299  |
| 7       | 1300 - 1349  |
| 8       | 1350 - 1399  |
| 9       | 1400 - 1449  |
| 10      | 1450 - 1499  |
| 11      | 1500 - 1549  |
| 12      | 1550 - 1599  |
| 13      | 1600 - 1649  |
| 14      | 1650 - 1699  |
| 15      | 1700 - 1749  |
| 16      | 1750 - 1799  |
| 17      | 1800 - 1849  |
| 18      | 1850 - 1899  |
| 19      | 1900 - 1949  |
| 20      | 1950 - 1999  |
| 21      | 2000 - 2500  |

---

## 16. Source File References

### Original HON (C++) - Client Side
**Path:** `C:\Users\SADS-810\Source\HON`
- `k2public/chatserver_protocol.h` - Command codes and enumerations
- `k2public/Chatserver Protocol.txt` - Official protocol documentation
- `src/k2/c_chatmanager.h` - Client-side TMM handling declarations
- `src/k2/c_chatmanager.cpp` - Client-side TMM implementation

### HON Chat Server (C++) - Server Side
**Path:** `C:\Users\SADS-810\Source\HON-Chat-Server\Chat Server`

#### Core Infrastructure
- `chatserver_common.h/.cpp` - Common definitions and macros
- `chatserver_types.h` - Type definitions, constants, game types, maps, modes, regions
- `chat_definitions.h` - Chat-specific definitions
- `c_core.h/.cpp` - Core server class
- `c_cvar.h/.cpp` - Configuration variable system
- `c_command.h/.cpp` - Command processing
- `c_console.h/.cpp` - Server console
- `console_commands.cpp` - Console command handlers
- `c_diagnostics.h/.cpp` - Diagnostics and monitoring

#### Client Management
- `c_client.h/.cpp` - **Client class** (connected user, TMR, stats, group membership, packet handlers)
- `c_clientmanager.h/.cpp` - Client connection lifecycle management

#### Group System
- `c_group.h/.cpp` - **Group class** (players queuing together, invite/join/leave/kick)
- `c_groupmanager.h/.cpp/.inl` - Group lifecycle management

#### Matchmaking Algorithm
- `c_matchmaker.h/.cpp` - **Complete matchmaking algorithm** (CVARs, CombineGroups, CreateMatches, BalanceTeams)
- `c_matchmakercommon.h/.inl` - Enums (ETMMBracketsByTMR, ETMMCombineMethod), structs (SMatch), algorithm functions (GetMatchupPrediction, GetWinLossPercent, UpdateTMRSpread, LookupWaitValue)
- `c_matchmakergroup.h/.cpp` - Lightweight group for matchmaking subprocess
- `c_matchmakerteam.h/.cpp` - Lightweight team for matchmaking subprocess
- `c_matchmakerclient.h/.cpp` - Lightweight client for matchmaking subprocess

#### Team Finder (Queue Management)
- `c_teamfinder.h/.cpp` - **TeamFinder class** (queue management, metrics, leaver tracking, match spawning)

#### Match Management
- `c_match.h/.cpp` - Match class (active game tracking)

#### Game Server Communication
- `c_gameserver.h/.cpp` - Game server connection class and protocol handling
- `c_gameservermanager.h/.cpp` - Game server pool management, region selection, server allocation
- `c_gamemanager.h/.cpp` - Game session management

#### Game Lobby (Pre-Game)
- `c_gamelobbymanager.h/.cpp/.inl` - Pre-game lobby management, hero selection, ready state

#### Scheduled Matches (Tournaments)
- `c_scheduledmatch.h/.cpp` - Tournament/league match class
- `c_event.h/.cpp` - Event/tournament class

#### Chat System
- `c_channel.h/.cpp` - Chat channel class
- `c_channelmanager.h/.cpp` - Channel management
- `c_clan.h/.cpp` - Clan class
- `c_clanmanager.h/.cpp` - Clan management

#### Networking
- `c_messagesocket.h/.cpp` - TCP socket wrapper
- `c_packet.h/.cpp` - Packet serialisation/deserialisation
- `c_netmanager.h/.cpp` - Network manager
- `c_netdriver.h` - Network driver interface
- `c_netdriver_win32.cpp` - Windows network implementation
- `c_netdriver_linux.cpp` - Linux network implementation
- `c_network.h` - Network definitions
- `c_network_win32.cpp` - Windows network implementation
- `c_pendingconnection.h/.cpp` - Pending connection handling
- `c_dnsthread.h/.cpp` - DNS resolution

#### HTTP Communication
- `c_httpmanager.h/.cpp` - HTTP request management
- `c_httprequest.h/.cpp` - HTTP request class
- `c_phpdata.cpp` - PHP data serialisation

#### Peer Communication (Chat Server Clustering)
- `c_peer.h/.cpp` - Peer chat server connection
- `c_peermanager.h/.cpp` - Peer server management

#### Map Settings
- `c_mapsettingmanager.h/.cpp` - Map configuration management

#### Memory Management
- `c_memmanager.h/.cpp` - Memory management
- `c_heap.h/.cpp` - Heap allocation
- `c_outputstream.h/.cpp` - Output stream handling

#### Utilities
- `bitflags.h` - Bit flag utilities

### HON Zend Server API (PHP) - Master Server
**Path:** `C:\Users\SADS-810\Source\HON-Zend-Server-API`

#### API Entry Points (`masterapi-international/`)
- `client_requester.php` - **Client API entry point** (authentication, TMM rating, stats)
- `client_controller.php` - Client request handler
- `client_request_class.php` - Client request processing
- `server_requester.php` - **Game Server API entry point** (match creation, stats submission)
- `server_controller.php` - Server request handler
- `server_request_class.php` - Server request processing
- `server_account_class.php` - Server account operations
- `stats_requester.php` - Statistics API entry point
- `stats_controller.php` - Statistics request handler
- `stats_request_class.php` - Statistics request processing
- `store_requester.php` - Store API entry point

#### Controllers (`library-international/inc/honmaster/controllers/`)
- `client_abstract.php` - Base client controller
- `server_abstract.php` - Base server controller
- `ChatController.php` - Chat-related operations
- `StatsController.php` - Statistics operations
- `SeasonController.php` - Campaign/Season operations
- `QuestController.php` - Quest system
- `QuestserverController.php` - Quest server operations

#### Account Classes (`library-international/inc/`)
- `client_account_class.php` - Client account operations

#### Core Library (`library-international/hon/`)
- `class_account.php` - Account class
- `class_server.php` - Server class
- `class_region.php` - Region definitions
- `class_chatsocket.php` - Chat server communication

#### Database Tables - Stats (`library-international/hon/db/table/stats/`)
- `class_account_stats.php` - Account statistics table
- `class_player_stats.php` - Player statistics
- `class_ranked_stats.php` - Ranked match statistics
- `class_casual_stats.php` - Casual match statistics
- `class_midwars_stats.php` - MidWars statistics
- `class_riftwars_stats.php` - RiftWars statistics
- `class_ranked_ladder_stats.php` - Ranked ladder statistics
- `class_casual_ladder_stats.php` - Casual ladder statistics
- `class_midwars_ladder_stats.php` - MidWars ladder statistics
- `class_match_stats.php` - Per-match statistics
- `class_match_summ.php` - Match summary
- `class_match_options.php` - Match options
- `class_match_type.php` - Match type definitions
- `class_player_match_history.php` - Player match history
- `class_ranked_match_history.php` - Ranked match history
- `class_casual_match_history.php` - Casual match history
- `class_recent_match_history.php` - Recent match history
- `class_player_hero_stats.php` - Per-hero statistics
- `class_ranked_hero_stats.php` - Ranked hero statistics
- `class_casual_hero_stats.php` - Casual hero statistics
- `class_match_banned_heroes.php` - Banned heroes per match

#### Database Tables - Accounts (`library-international/hon/db/table/accounts/`)
- `class_account.php` - Account table
- `class_account_info.php` - Account info table
- `class_server_info.php` - Server info table

#### Database Tables - Chat (`library-international/hon/db/table/chat/`)
- `class_buddies.php` - Buddy list table
- `class_notification.php` - Notifications table

#### Database Servers (`library-international/hon/db/server/`)
- `class_stats_master.php` - Stats database master
- `class_account_master.php` - Account database master
- `class_chat_master.php` - Chat database master

#### Models - Stats (`library-international/hon/models/stats/`)
- `class_match_stats.php` - Match stats model
- `class_match_stats_mapper.php` - Match stats mapper
- `class_match_summ.php` - Match summary model
- `class_match_summ_mapper.php` - Match summary mapper
- `account/class_stats.php` - Account stats model
- `account/class_stats_mapper.php` - Account stats mapper
- `type/ranked/class_stats.php` - Ranked stats model
- `type/casual/class_stats.php` - Casual stats model
- `type/midwars/class_stats.php` - MidWars stats model
- `type/riftwars/class_stats.php` - RiftWars stats model

#### Game Match Processing (`library-international/hon/game/match/`)
- `class_stats.php` - Match statistics processing
- `type/public/class_stats.php` - Public match stats

#### Region Configuration (`library-international/hon/region/`)
- `int/class_stats_level.php` - International stats levels
- `sea/class_stats_level.php` - SEA stats levels
- `lat/class_stats_level.php` - Latin America stats levels
- `cis/class_stats_level.php` - CIS stats levels
- `cn/class_stats_level.php` - China stats levels

### Game Client (LUA)
**Path:** `C:\Users\SADS-810\Source\HON\Heroes of Newerth\game\resources0\ui\scripts`
- `matchmaking.lua` - UI logic and flow control
- `main_options_game.lua` - Game options including matchmaking settings

### Packet Captures
**Path:** `E:\Offline Arcade\Heroes Of Newerth\Project KONGOR\Packet Dumps`
- Network packet captures used to verify protocol documentation

---

## 17. Data Models for Implementation

### 17.1 Client (Player) Model

The client/player model stores all matchmaking-related data per connected user:

```
CClient {
    // Identity
    AccountID           : uint          // Unique account identifier
    Name                : string        // Account name
    Cookie              : string        // Session cookie
    ClientID            : uint          // Connection-specific ID

    // TMR (Team Match Rating) - Separate per game type
    NormalTMR           : float         // Ranked Normal TMR
    CasualTMR           : float         // Ranked Casual TMR
    UnrankedNormalTMR   : float         // Unranked Normal TMR
    UnrankedCasualTMR   : float         // Unranked Casual TMR
    MidwarsTMR          : float         // MidWars TMR
    RiftwarsTMR         : float         // RiftWars TMR
    CampaignNormalTMR   : float         // Season Normal TMR
    CampaignCasualTMR   : float         // Season Casual TMR
    RebornNormalTMR     : float         // Reborn Normal TMR
    RebornCasualTMR     : float         // Reborn Casual TMR
    MidwarsRebornTMR    : float         // MidWars Reborn TMR

    // Match Counts
    TotalMatches        : int           // All matches played
    NormalMatches       : int           // Ranked Normal matches
    CasualMatches       : int           // Ranked Casual matches
    UnrankedMatches     : int           // Unranked matches
    MidwarsMatches      : int           // MidWars matches
    RiftwarsMatches     : int           // RiftWars matches
    CampaignNormalMatches : int         // Season Normal matches
    CampaignCasualMatches : int         // Season Casual matches
    Disconnects         : int           // Total disconnects (for leaver detection)

    // Match History (for recent win/loss tracking)
    MatchHistory        : vector<{MatchID, bWin}>   // Recent match history
    RecentWins          : uint          // Wins in recent history
    RecentLosses        : uint          // Losses in recent history
    MatchHistoryAdjustment : float      // TMR adjustment based on streaks

    // Statistics (for K/D ratio matching)
    Kills               : int
    Assists             : int
    Deaths              : int
    Experience          : int
    Gold                : int
    MinutesPlayed       : float

    // Group State
    Group               : CGroup*       // Current group (NULL if not in group)
    GroupID             : uint          // Current group ID
    TeamSlot            : byte          // Slot in group (0-4)
    TMMReadyStatus      : byte          // 0=not ready, 1=ready
    TMMLoadingStatus    : byte          // 0-100 loading percent

    // Match State
    MatchID             : uint          // Current match ID
    ServerAddressPort   : string        // Current game server
    PendingMatchAccept  : bool          // Waiting for match acceptance

    // Level/Eligibility
    Level               : uint          // Account level
    CampaignEligible    : bool          // Eligible for seasons

    // Calculated Values (set when match is created)
    MatchWinValue       : float         // TMR gained on win
    MatchLossValue      : float         // TMR lost on loss
}
```

### 17.2 Group Model

Groups are collections of players queuing together:

```
CGroup {
    // Identity
    GroupID             : uint          // Unique group identifier
    LeaderAccountID     : uint          // Account ID of group leader
    Name                : wstring       // Group/team name (for scheduled matches)

    // Players
    Players             : vector<CClient*>  // Players in the group
    InvitedPlayers      : vector<uint>      // Account IDs of pending invites
    PlayerResponses     : map<uint, byte>   // Accept=1, Reject=2

    // Queue Options
    TeamSize            : byte          // 5 for Caldavar, 3 for Grimm's Crossing, 1 for 1v1
    GameType            : ETMMGameTypes // Normal, Casual, MidWars, etc.
    Maps                : uint          // Bitmask of selected maps
    GameModes           : uint          // Bitmask of selected game modes
    Regions             : uint          // Bitmask of selected regions
    Ranked              : bool          // Ranked or unranked pool
    MatchFidelity       : byte          // 0=off, 1=on (prefer closer skill matches)

    // Bot Match Options
    BotGroup            : bool          // Is this a bot match group
    BotDifficulty       : byte          // 1=easy, 2=medium, 3=hard
    RandomizeBots       : bool          // Randomize enemy bots
    TeamBots            : vector<wstring>[5]  // Ally bot hero names
    EnemyBots           : vector<wstring>[5]  // Enemy bot hero names

    // Queue State
    Queued              : bool          // Is group in matchmaking queue
    JoinedQueueTime     : uint          // Timestamp when joined queue (ms)
    LastJoinedQueueTime : uint          // Previous queue time (for rejoin)
    MatchedUp           : bool          // Has been matched
    PendingMatch        : bool          // Waiting for match acceptance
    LoadingStatusChanged : bool         // Loading status update needed

    // Match Info (when matched)
    MatchupID           : uint          // Internal match ID
    Challenge           : uint          // Server integrity verification
    MatchAnnounceTime   : uint          // When match was announced
    ServerAddress       : string        // Game server IP
    ServerPort          : ushort        // Game server port

    // Cached Statistics (recalculated when members change)
    AverageTMR          : float
    TotalTMR            : float
    HighestTMR          : float
    LowestTMR           : float
    TotalKills          : int
    TotalDeaths         : int
    AverageKAD          : float
    TotalMatchCount     : int

    // Game Lobby (for pre-game lobbies)
    GameLobby           : GameLobbyHandle
}
```

### 17.3 Matchmaker Group Model

Lightweight group representation sent to the matchmaking subprocess:

```
CMMGroup {
    GroupID             : uint
    JoinedQueueTime     : uint
    TeamSize            : byte
    GameType            : ETMMGameTypes
    Map                 : uint          // Bitmask
    GameModes           : uint          // Bitmask
    Regions             : uint          // Bitmask
    Ranked              : bool
    MatchFidelity       : byte
    Virtual             : bool          // For simulation/testing

    // Cached Stats
    TotalMatchCount     : uint
    TotalKills          : uint
    TotalDeaths         : uint
    AverageKD           : float
    TotalTMR            : float
    AverageTMR          : float
    HighestTMR          : float
    LowestTMR           : float

    // State
    MatchedUp           : bool
    HasTeam             : bool

    // Players
    Players             : vector<CMMClient*>
}
```

### 17.4 Matchmaker Client Model

Lightweight player representation for matchmaking:

```
CMMClient {
    GroupID             : uint
    Name                : string
    AccountID           : uint
    TeamSlot            : byte
    TMR                 : float
    RecentWins          : uint
    RecentLosses        : uint
    MatchHistoryAdjustment : float
    TotalMatchCount     : uint
    NormalMatches       : uint
    CasualMatches       : uint
    Virtual             : bool
    Level               : uint
    Kills               : uint
    Deaths              : uint
    IPAddress           : uint          // For conflict detection

    // Calculated per-match
    MatchWinValue       : float
    MatchLossValue      : float
}
```

### 17.5 Team Model

Teams are formed from groups to create a full side (5 players):

```
CMMTeam {
    TeamID              : uint
    MatchedUp           : bool
    TeamSize            : byte
    GameType            : ETMMGameTypes
    Map                 : ETMMGameMaps      // Selected map for match
    GameMode            : ETMMGameModes     // Selected mode for match
    Region              : ETMMGameRegions   // Selected region for match
    Ranked              : bool
    Virtual             : bool

    // Composition
    Groups              : vector<CMMGroup*>
    GroupMakeup         : uint          // Composition score (5=all solos, 25=5-stack)
    GameModeFlags       : uint          // Compatible game modes
    RegionFlags         : uint          // Compatible regions

    // Statistics
    TotalMatchCount     : uint
    TotalKills          : uint
    TotalDeaths         : uint
    AverageKD           : float
    TotalTMR            : float
    AverageTMR          : float
    AdjustedTMR         : float         // TMR with weighting applied
    HighestTMR          : float
    LowestTMR           : float
}
```

### 17.6 Match Model

Represents a created match between two teams:

```
SMatch {
    MatchIndex          : uint
    CombineMethod       : byte          // ETMMCombineMethod used
    MatchupPercents     : SMatchupPercents {
        MatchupPrediction   : float     // 0.0-1.0 (Team A win probability)
        WinPercent          : float     // Acceptable win threshold
        LossPercent         : float     // Acceptable loss threshold
    }
    MismatchedGroupMakeup : bool        // Teams have very different compositions
    FirstPassBalanced   : bool          // Balanced in first pass
    SecondPassBalanced  : bool          // Balanced in second pass
    MatchmakingAttributes : SMatchmakingAttributes {
        TeamSize            : byte
        GameType            : ETMMGameTypes
        Map                 : ETMMGameMaps
        GameMode            : ETMMGameModes
        Region              : ETMMGameRegions
    }
    GroupInfo           : map<uint, uint>[2]  // GroupID -> PlayerCount per team
    MatchPointValues    : map<uint, SMatchPointValues>  // AccountID -> Win/Loss values
}
```

---

## 18. Leaver System

### 18.1 Leaver Detection

A player is considered a **leaver** if:
```
Disconnects / TotalMatches > leaver_threshold (default: 0.05 = 5%)
```

The `IsLeaver()` check is performed when:
- Creating a group (`TMMFTJR_LEAVER` failure)
- Joining a group
- Entering the queue

### 18.2 Leaver Strike System

The leaver strike system tracks recent abandons:

```
SLeaverBan {
    Strikes         : uint              // Current strike count
    BanTime         : uint              // Ban duration in seconds
    ResetTime       : uint              // When strikes reset
    StrikeHistory   : vector<time_t>    // Timestamp of each strike
}
```

**Strike Escalation:**
| Strikes | Ban Duration           |
|---------|------------------------|
| 1       | Warning only           |
| 2       | 5 minutes              |
| 3       | 15 minutes             |
| 4       | 30 minutes             |
| 5       | 1 hour                 |
| 6+      | Escalating (2x prev)   |

**Strike Decay:**
- Strikes decay after a configurable period (default: 24 hours)
- Good behaviour (completed matches) can reduce strike count

### 18.3 Ban From TMM

```cpp
void BanFromTMM(uint uiAccountID, byte yType) {
    // Type 0: Short ban (minutes)
    // Type 1: Long ban (hours)
    // Type 2: Permanent (until reviewed)
}
```

---

## 19. Match Server Allocation

### 19.1 Server Selection Algorithm

```
FindAvailableServer(TeamA, TeamB):
    1. Get common regions from both teams
    2. For each region (in priority order):
        a. Query GameServerManager for idle servers in region
        b. Filter by: map support, capacity, version compatibility
        c. Return first available server
    3. If no servers in preferred regions:
        a. Try backup regions (configured per-region)
        b. For EU: Try USE, USW as backups
    4. If still no servers:
        a. Return NULL (TMM_GROUP_NO_SERVERS_FOUND sent to clients)
```

### 19.2 Match Spawn Flow

```
SpawnMatch(matchData, pServer, pLegionTeam, pHellbourneTeam):
    1. Validate match data and teams
    2. Generate MatchupID (sequential)
    3. Generate Challenge value (random, for verification)
    4. Set match point values for all players
    5. Send NET_CHAT_GS_CREATE_MATCH to game server
    6. Store MatchReminderInfo for timeout handling
    7. Wait for NET_CHAT_GS_ANNOUNCE_MATCH from server
    8. On success:
        a. Send MatchFoundUpdate (0x0D09) to all players
        b. Send GroupQueueUpdate type=16 (TMM_GROUP_FOUND_SERVER)
        c. Send AutoMatchConnect (0x0062) to all players
    9. On failure (timeout or error):
        a. Return groups to queue
        b. Try with another server
```

### 19.3 Connection Reminder System

If players don't connect to the game server within a timeout:

```
SMatchReminderInfo {
    ArrangedMatchType   : byte
    Address             : string
    Port                : ushort
    ReminderTimeStamp   : uint
    Players             : vector<CClient*>
}
```

The system:
1. Stores match reminder info when match is created
2. Periodically checks if all players have connected
3. Resends `AutoMatchConnect (0x0062)` with `reminder=0xFFFFFFFF`
4. After N reminders, abandons match and potentially bans non-connectors

---

## 20. Pending Match Accept Flow (Optional)

For matches requiring explicit acceptance (e.g., ranked):

```
SPendingMatch {
    MatchData           : SMatch
    TimeMatched         : uint          // When match was created
    NumPlayersAccepted  : uint          // Players who accepted
    NumPlayers          : uint          // Total players needed
}
```

**Flow:**
1. Match found → Send `NET_CHAT_CL_TMM_PENDING_MATCH (0x0F04)` to all players
2. Players send `NET_CHAT_CL_TMM_ACCEPT_PENDING_MATCH (0x0F05)`
3. Server tracks acceptance count
4. If all accept within timeout → Spawn match
5. If any decline or timeout → Cancel match, return to queue

---

## 21. Maps Reference

| Code               | Name                  | Team Size |
|--------------------|-----------------------|-----------|
| `caldavar`         | Forests of Caldavar   | 5         |
| `caldavar_reborn`  | Caldavar Reborn       | 5         |
| `grimmscrossing`   | Grimm's Crossing      | 3         |
| `midwars`          | Mid Wars              | 5         |
| `midwars_reborn`   | Mid Wars Reborn       | 5         |
| `riftwars`         | Rift Wars             | 5         |
| `prophets`         | Prophets              | 5         |
| `thegrimmhunt`     | The Grimm Hunt        | 5         |
| `capturetheflag`   | Capture The Flag      | 5         |
| `devowars`         | Devo Wars             | 5         |
| `soccer`           | Soccer                | 5         |
| `solomap`          | Solo Map (1v1)        | 1         |
| `team_deathmatch`  | Team Deathmatch       | 5         |

---

## 22. Statistics Tracking

### 22.1 Metrics Collected

The TeamFinder tracks extensive metrics for monitoring:

```
SMetrics {
    // Player counts by group fullness
    PlayersInGroupByFull[5][2]      // [TeamSize][Full/Open]
    PlayersQueuedInGroup[5]         // By team size

    // Group counts
    GroupsByFull[5][2]              // [TeamSize][Full/Open]
    GroupsQueued[5]                 // Queued by team size
    GroupsByNumPlayers[5][6]        // [TeamSize][NumPlayers]
    QueuedGroupsByNumPlayers[5][6]
    QueuedGroupsByNumModes[5][GameModes]
    QueuedGroupsByNumRegions[5][Regions]

    // Average queue times
    TimeQueued[5]                   // By team size

    // Option popularity
    GameType[NUM_GAME_TYPES]
    GameMap[NUM_GAME_MAPS]
    GameMode[NUM_GAME_MODES]
    Region[NUM_REGIONS]
}
```

### 22.2 Match Statistics

Per-match statistics tracked:
- **GroupQueueTimes**: Queue time for each group by size
- **TeamQueueTimes**: Queue time for each team by group count
- **MatchupPercent**: Win prediction for each match
- **CombineMethod**: Which algorithm created the match
- **BalancePass**: Which pass balanced the teams

### 22.3 Leaderboard/Season Statistics

For Campaign (Seasons) mode:
- **CampaignNormalMedal**: Medal rank
- **CampaignCasualMedal**: Medal rank
- **CampaignNormalRank**: Leaderboard position
- **CampaignCasualRank**: Leaderboard position
- **CampaignKills/Deaths**: Season-specific stats

---

## 23. HTTP API Endpoints

### 23.1 Required Master Server Endpoints

| Endpoint                                      | Purpose                              |
|-----------------------------------------------|--------------------------------------|
| `POST /client_requester.php?f=get_tmm_rating` | Fetch player's TMR and stats         |
| `POST /client_requester.php?f=start_match`    | Create match in database             |
| `POST /server_requester.php?f=start_match`    | Game server match creation           |
| `POST /server_requester.php?f=match_stats`    | Submit match results                 |
| `POST /events.php`                            | Fetch current events/tournaments     |

### 23.2 TMM Rating Request

**Request:**
```
f=get_tmm_rating
account_id=<int>
tmm_type=<byte>         // ETMMTypes
game_type=<int>         // ETMMGameTypes
team_size=<byte>
maps=<int>              // Bitmask
game_modes=<int>        // Bitmask
regions=<int>           // Bitmask
ranked=<bool>
match_fidelity=<byte>
leader_account_id=<int>
```

**Response:**
```php
[
    'tmr' => <float>,
    'matches' => <int>,
    'wins' => <int>,
    'losses' => <int>,
    'disconnects' => <int>,
    'kills' => <int>,
    'deaths' => <int>,
    'assists' => <int>,
    // ... per game type stats
]
```

### 23.3 Start Match Request

**Request:**
```
f=start_match
session=<cookie>
map=<string>
version=<string>
mname=<string>          // Match name
mstr=<string>           // Host account name
casual=<0|1>
arrangedmatchtype=<int>
match_mode=<string>
```

**Response:**
```php
['match_id' => <int>]
```

---

## 24. Implementation Checklist

### 24.1 Chat Server Components

- [x] Group Manager - Create, join, leave, kick, invite
- [x] Team Finder - Queue management (basic), metrics tracking (stub)
- [x] Matchmaker - Team formation (FIFO), match creation (basic)
- [ ] Matchmaker - Team balancing (2-pass algorithm)
- [ ] Game Server Manager - Server selection, match spawning
- [ ] Leaver Tracker - Strike system, ban management

### 24.2 Data Persistence

- [x] Player TMR (per game type) - AccountStatistics.SkillRating
- [ ] Match history
- [ ] Leaver strikes
- [ ] Queue statistics
- [ ] Season/Campaign rankings

### 24.3 Protocol Messages

- [x] Client → Server commands (Section 2) - All 10 processors implemented
- [x] Server → Client responses (Section 2) - GroupUpdate, MatchFoundUpdate, AutoMatchConnect
- [ ] Game Server ↔ Chat Server (Section 2) - NET_CHAT_GS_CREATE_MATCH, etc.

### 24.4 Algorithm Implementation

- [x] ELO-based matchup prediction - Formula in MatchmakingMatch.CalculateMatchupPrediction()
- [ ] TMR spread calculation
- [x] Team composition scoring - MatchmakingTeam.CalculateGroupMakeup()
- [ ] Group combining (14 methods) - Enums defined, FIFO only active
- [ ] Team balancing (2-pass)
- [ ] K-Factor calculation

### 24.5 Configuration

- [x] Basic CVARs with defaults (MatchmakingSettings.cs)
- [ ] All CVARs from Section 14 (many not yet configurable)
- [ ] Region availability
- [ ] Game mode availability
- [ ] Map availability

---

## 25. Implementation Notes

This section documents findings from the actual implementation that supplement or clarify the protocol documentation.

### 25.1 Game Mode Codes in MatchFoundUpdate

The `MatchFoundUpdate (0x0D09)` packet sends game mode as a string code, not an enum value:

| Code | Mode Name |
|------|-----------|
| `ap` | All Pick |
| `sd` | Single Draft |
| `ar` | All Random |
| `bd` | Banning Draft |
| `bp` | Banning Pick |
| `br` | Balanced Random |
| `cm` | Captain's Mode |
| `hb` | Hero Ban |
| `bb` | Blind Ban |
| `rd` | Random Draft |

**Example packet content:** `map="midwars", mode="ar", region="EU"`

### 25.2 AutoMatchConnect Random Value

The fourth field in `AutoMatchConnect (0x0062)` serves dual purposes:
- Normal connect: Random uint32 value (duplicate packet workaround)
- Reminder: `0xFFFFFFFF` indicates this is a connection reminder

From legacy C++ (`c_teamfinder.cpp`):
```cpp
pkt << (byte)eArrangedMatchType;
pkt << (uint)uiMatchupID;
pkt << sAddress;
pkt << (ushort)usPort;
pkt << (uint)rand();  // Or 0xFFFFFFFF for reminder
```

### 25.3 MatchInformation Redis Caching

When a match is created, `MatchInformation` must be cached in Redis before sending `AutoMatchConnect` to clients. The game server uses this cached data to validate connecting players.

**Redis Key Pattern:** `match:{matchID}:info`

**Required Fields:**
- Match ID
- Player account IDs (both teams)
- Server ID
- Map name
- Game mode
- Expected player count

### 25.4 ActionCampaign Analytics Events

The client sends analytics events during matchmaking that should be handled gracefully:

| Event | When Sent |
|-------|-----------|
| `AC_MATCHMAKING_MATCH_FOUND` | Client receives MatchFoundUpdate |
| `AC_MATCHMAKING_SERVER_FOUND` | Client receives QueueUpdate type=16 |
| `AC_MATCHMAKING_MATCH_READY` | Client is ready to connect |

These are informational events and should be logged at debug level, not treated as errors.

### 25.5 Environment-Specific Configuration

PlayersPerTeam should be environment-specific for testing:
- **Development:** 1 (enables 1v1 local testing)
- **Production:** 5 (standard 5v5 matches)

Configuration in `appsettings.{Environment}.json`:
```json
{
  "Matchmaking": {
    "PlayersPerTeam": 1
  }
}
```

### 25.6 Match Found Packet Sequence

The complete sequence when a match is found:

1. **GroupLeaveQueue (0x0D02)** - Remove group from queue
2. **GroupUpdate (0x0D03)** - Full state update
3. **MatchFoundUpdate (0x0D09)** - Match details (map, mode, region)
4. **GroupQueueUpdate (0x0D06) type=16** - TMM_GROUP_FOUND_SERVER ("Sound The Horn!")
5. **AutoMatchConnect (0x0062)** - Server connection details

All packets must be sent to every member of both teams.

### 25.7 Current Implementation Files

| File | Purpose |
|------|---------|
| `Configuration/MatchmakingSettings.cs` | Algorithm configuration (CVARs) |
| `Domain/Matchmaking/MatchmakingTeam.cs` | Team model with composition scoring |
| `Domain/Matchmaking/MatchmakingMatch.cs` | Match model with state machine |
| `Domain/Matchmaking/MatchmakingGroup.cs` | Extended with TMR calculations |
| `Domain/Matchmaking/MatchmakingGroupMember.cs` | Extended with TMR, match counts |
| `Services/MatchmakingService.cs` | Match broker with FIFO algorithm |
| `CommandProcessors/Actions/TrackPlayerAction.cs` | Handles analytics events |

### 25.8 Verification Notes

This specification has been verified against the following legacy source files:

**Protocol Verification:**
- `c_teamfinder.cpp` - AutoMatchConnect, MatchFoundUpdate packet construction
- `c_matchmakercommon.inl` - TranslateGameMode, TranslateMap, TranslateRegion functions
- `chatserver_protocol.h` - ETMMGameTypes, ETMMGameModes, EArrangedMatchType enums
- `c_matchmakercommon.h` - ETMMCombineMethod enum, TMR brackets

**Key Corrections Made:**
1. MatchFoundUpdate team size field is `byte` (1 byte), not `uint32` (4 bytes)
2. K-Factor maximum is determined by multiplier (`BaseKFactor × ProvisionalKFactorMultiplier`), not a separate `maxKFactor` CVAR
3. Wait time thresholds are configurable via CVARs (`matchmaker_waitTime1` through `matchmaker_waitTime6`), not hardcoded
4. Added missing combine method `TS5_ALL_EXPERIMENTAL`
5. Added missing `TS3_FULL_OR_2_GROUPS_RANDOM` combine method
6. Corrected game mode code descriptions (e.g., `cp` = Counter Pick, not Captains Pick)
