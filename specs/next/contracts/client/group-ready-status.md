# Group Ready Status

**Command**: NET_CHAT_CL_TMM_GROUP_READY_STATUS
**Phase**: Phase 2 (Matchmaking Groups)
**Direction**: Client → Server
**Response**: Broadcast to all group members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_READY_STATUS]
[int8: Ready Status (1 = ready, 0 = not ready)]
```

**Fields**:
- **Ready Status** (int8): 1 for ready, 0 for not ready

**Example**:
```
Ready Status: 1 (player is ready)
```

## Response Messages

**Success Response (to updating player)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_READY_STATUS_RESPONSE]
[int8: Success = 1]
[int8: Ready Status]
```

**Broadcast to All Group Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_MEMBER_READY_UPDATED]
[int32: Group ID]
[int32: Account ID]
[string: Account Name]
[int8: Ready Status]
[int8: All Ready (1 if all members ready, 0 otherwise)]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Error Cases

| Error | Reason |
|-------|--------|
| "Not in group" | Player not member of any group |
| "Group in queue" | Cannot change ready status while queued |

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Must be in group** | Player must be group member |
| **Not in queue** | Cannot change while QueueStatus != NotQueued |
| **Only affects self** | Cannot set ready status for other players |

## Processing Steps

1. Validate player is member of a group
2. Check group not in queue (QueueStatus = NotQueued)
3. Update player's ready status in group
4. Check if all members now ready
5. Broadcast updated status to all group members
6. Send confirmation to updating player
7. If all ready, optionally enable queue button in UI

## All Ready Detection

**Logic**:
```csharp
bool allReady = group.Members.Values.All(m => m.ReadyStatus);
```

**Use Case**: Client can enable "Join Queue" button when `allReady == true`

**Note**: Ready status does **not** automatically join queue. Leader must explicitly call `group-join-queue.md` command.

## Queue Behaviour

**Ready Status Reset**:
```csharp
// When group joins queue, all ready statuses remain unchanged
// When group leaves queue, optionally reset all to not ready
if (group.QueueStatus == QueueStatus.NotQueued)
{
    foreach (var member in group.Members.Values)
    {
        member.ReadyStatus = false; // Reset after queue leave
    }
}
```

## State Management

```csharp
// Update member ready status
GroupMember member = group.Members[session.AccountID];
member.ReadyStatus = readyStatus;

// Check all ready
bool allReady = group.Members.Values.All(m => m.ReadyStatus);

// Broadcast to all members
var updateNotification = BuildReadyStatusUpdateMessage(
    group.GroupID,
    session.AccountID,
    session.AccountName,
    readyStatus,
    allReady
);

foreach (var groupMember in group.Members.Values)
{
    groupMember.Session.SendAsync(updateNotification);
}
```

## UI Implications

**Client Behaviour**:
1. Player clicks "Ready" checkbox
2. Send `TMM_GROUP_READY_STATUS` with status=1
3. Receive broadcast with updated status
4. Update UI to show player's ready checkmark
5. If `AllReady == 1`, enable "Join Queue" button (leader only)

## Related Commands

- **group-join.md**: Join group before marking ready
- **group-join-queue.md**: Enter queue (requires all ready)
- **group-leave.md**: Leave group (clears ready status)
- **group-create.md**: Create group (all start not ready)
