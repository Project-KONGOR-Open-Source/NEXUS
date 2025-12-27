# Match Start

**Command**: NET_CHAT_CL_TMM_MATCH_START
**Phase**: Phase 3 (Queue & Match Lobby)
**Direction**: Server → Client (broadcast to all match players)
**Response**: N/A (final command before players connect to game server)

## Message Structure

**Broadcast to all players in match lobby**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_MATCH_START]
[int32: Match Lobby ID]
[string: Game Server IP]
[int32: Game Server Port]
[string: Match Token]
[int8: Game Type]
[int8: Game Mode]
[string: Map Name]
[int8: Team 1 Player Count]
[for each Team 1 player:]
    [int32: Account ID]
    [string: Account Name]
    [int8: Team Slot]
[int8: Team 2 Player Count]
[for each Team 2 player:]
    [int32: Account ID]
    [string: Account Name]
    [int8: Team Slot]
```

**Fields**:
- **Match Lobby ID**: Unique match identifier
- **Game Server IP**: IP address to connect to (e.g., "192.168.1.100")
- **Game Server Port**: Port number (e.g., 11235)
- **Match Token**: Security token for game server authentication
- **Game Type**: Campaign Normal (6), Campaign Casual (7), Midwars (3), Riftwars (4), Public (0)
- **Game Mode**: All Pick (0), etc.
- **Map Name**: "caldavar", "midwars", "riftwars", etc.
- **Team 1/2 Player Lists**: Complete roster with team slots

## Prerequisites

All players must have **LoadingStatus = true** before this command is sent.

## Processing Flow (Leading to Match Start)

### 1. Matchmaking Algorithm Finds Match
```
Two compatible groups found in queue
→ Create MatchLobby in-memory
→ Notify both groups (MATCH_FOUND)
```

### 2. Players Accept Match
```
All players send MATCH_ACCEPT
→ Allocate game server via Manager port
→ Wait for server allocation response
```

### 3. Game Server Allocation
```
Chat server → Manager: SERVER_ALLOCATION_REQUEST
Manager → Available game server: Allocate for match
Game server → Manager: SERVER_READY_FOR_MATCH
Manager → Chat server: SERVER_ALLOCATION_RESPONSE (IP, port, token)
```

### 4. Players Mark Loading
```
Each player sends TMM_PLAYER_LOADING_STATUS (true)
→ MatchLobby tracks loading status
→ When all players loaded, send MATCH_START
```

### 5. Match Start (This Command)
```
All players loaded
→ Send MATCH_START to all players
→ Players connect to game server IP:port with match token
→ Chat server marks match as started
→ MatchLobby removed from memory (transitioned to game server)
```

## Client Behavior After Receiving

1. Display match details (teams, map, mode)
2. Connect to game server: `tcp://GameServerIP:GameServerPort`
3. Send match token to game server for authentication
4. Begin loading game assets
5. Disconnect from chat server (or maintain connection for post-match)

## Timeout Handling

| Timeout | Duration | Action |
|---------|----------|--------|
| **Player loading timeout** | 60 seconds | Abort match, return players to queue or group |
| **Server allocation timeout** | 30 seconds | Abort match, notify players of failure |
| **Player accept timeout** | 20 seconds | Abort match if any player doesn't accept |

## Match Token Format

**Purpose**: Prevent unauthorized connections to game server

**Format** (example):
```
"MATCH_{MatchLobbyID}_{RandomGUID}_{Timestamp}"
```

**Validation**:
- Game server validates token with chat server
- Token expires after 2 minutes
- Single-use token (cannot reuse)

## Error Cases

| Error | Reason | Action |
|-------|--------|--------|
| "Server allocation failed" | No available game servers | Abort match, return to queue |
| "Player disconnected" | Player left during loading | Abort match, penalize leaver |
| "Loading timeout" | Player took too long to load | Abort match, penalize slow player |

## Related Commands

- **match-found.md**: Initial match notification
- **match-lobby-created.md**: Lobby state after acceptance
- **server-allocation-request.md** (manager/): Request game server
- **server-allocation-response.md** (manager/): Receive server details

## Example Flow

```
1. Matchmaking finds Team1 vs Team2
2. Send MATCH_FOUND to all 10 players
3. All players send MATCH_ACCEPT
4. Send MATCH_LOBBY_CREATED (lobby details)
5. Request server allocation from Manager
6. Receive SERVER_ALLOCATION_RESPONSE (IP: 192.168.1.50, Port: 11235, Token: "MATCH_12345_...")
7. All players send LOADING_STATUS = true
8. Send MATCH_START to all players:
   - Server: 192.168.1.50:11235
   - Token: "MATCH_12345_..."
   - Teams: [Team 1: Players 1-5], [Team 2: Players 6-10]
9. Players connect to game server
10. Match begins
```

## Performance Targets

- **Server allocation**: <5 seconds (SC-005)
- **Total time from match found to match start**: <30 seconds
- **Player connection success rate**: >99%
