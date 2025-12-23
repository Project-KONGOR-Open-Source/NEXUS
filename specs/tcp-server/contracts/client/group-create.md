# Group Create

**Command**: NET_CHAT_CL_TMM_GROUP_CREATE
**Phase**: Phase 2 (Matchmaking Groups)
**Direction**: Client → Server
**Response**: Group ID + initial group state

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_CREATE]
[int8: Game Type]
[int8: Game Mode]
[int8: Region]
```

**Fields**:
- **Game Type** (int8):
  - 0 = Public
  - 3 = Midwars
  - 4 = Riftwars
  - 6 = Campaign Normal
  - 7 = Campaign Casual (add if not in ChatProtocol.cs)
- **Game Mode** (int8):
  - 0 = All Pick
  - 3 = Single Draft
  - 6 = All Random
  - 14 = Kros Mode
  - 22 = Hero Ban
  - 24 = Reborn
- **Region** (int8):
  - 0 = USE (US East)
  - 1 = USW (US West)
  - 2 = EU (Europe)
  - 9 = RU (Russia)
  - 11 = AU (Australia)
  - 15 = BR (Brazil)

## Response Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_CREATE_RESPONSE]
[int8: Success]
[int32: Group ID]
[int32: Leader Account ID]
[int8: Game Type]
[int8: Game Mode]
[int8: Region]
[int32: Member Count (1)]
[int32: Leader Account ID]
[string: Leader Account Name]
[int8: Team Slot (0)]
[int8: Ready Status (0)]
```

**Fields**:
- **Success**: 1 for success, 0 for failure
- **Group ID**: Unique group identifier
- **Leader Account ID**: Creator's account ID
- **Game Type/Mode/Region**: Confirmed settings
- **Member Count**: Initially 1 (just leader)
- **Member List**: Leader's info

## Error Cases

| Error | Reason |
|-------|--------|
| "Already in group" | User already in a matchmaking group |
| "Invalid game type" | Game type not in supported list |
| "Invalid game mode" | Game mode not supported |
| "Invalid region" | Region not available |

## Processing Steps

1. Check if player already in group (CurrentGroup != null)
2. Validate game type, game mode, region
3. Generate unique group ID (Interlocked.Increment)
4. Create MatchmakingGroup in-memory
5. Add creator as leader and first member
6. Add group to ChatServer.ActiveGroups
7. Set ChatSession.CurrentGroup reference
8. Send group state to leader

## State Management

**In-Memory**:
```csharp
MatchmakingGroup group = new MatchmakingGroup
{
    GroupID = nextGroupID++,
    LeaderAccountID = session.AccountID,
    GameType = gameType,
    GameMode = gameMode,
    Region = region,
    QueueStatus = QueueStatus.NotQueued,
    CreatedTime = DateTime.UtcNow
};

ChatServer.ActiveGroups.TryAdd(group.GroupID, group);
session.CurrentGroup = group;
```

## Related Commands

- **group-invite.md**: Invite players to group
- **group-join.md**: Join group after invitation
- **group-leave.md**: Leave group
- **group-join-queue.md**: Enter matchmaking queue (Phase 3)
