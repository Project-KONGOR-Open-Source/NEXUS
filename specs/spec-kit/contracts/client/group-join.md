# Group Join

**Command**: NET_CHAT_CL_TMM_GROUP_JOIN
**Phase**: Phase 2 (Matchmaking Groups)
**Direction**: Client → Server
**Response**: Group state to joining player + broadcast to existing members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_JOIN]
[int32: Group ID]
```

**Fields**:
- **Group ID** (int32): Group to join (from invitation)

**Example**:
```
Group ID: 12345
```

## Response Messages

**Success Response (to joining player)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_JOIN_RESPONSE]
[int8: Success = 1]
[int32: Group ID]
[int32: Leader Account ID]
[int8: Game Type]
[int8: Game Mode]
[int8: Region]
[int32: Member Count]
[for each member:]
    [int32: Account ID]
    [string: Account Name]
    [int8: Team Slot]
    [int8: Ready Status]
```

**Broadcast to Existing Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_MEMBER_JOINED]
[int32: Group ID]
[int32: New Member Account ID]
[string: New Member Account Name]
[int8: Team Slot]
[int32: New Member Count]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Valid invitation** | Must have pending invitation for group |
| **Invitation not expired** | Invitation <60 seconds old |
| **Group exists** | Group ID valid and active |
| **Group not full** | <5 members in group |
| **Group not in queue** | Cannot join while group queued |
| **Player not in group** | Player must not already be in a group |
| **Player not in game** | Player must not be in active match |

## Error Cases

| Error | Reason |
|-------|--------|
| "No invitation" | Player not invited to this group |
| "Invitation expired" | Invitation older than 60 seconds |
| "Group not found" | Group ID invalid or disbanded |
| "Group full" | Group already has 5 members |
| "Group in queue" | Group currently in matchmaking queue |
| "Already in group" | Player already member of another group |
| "In active game" | Player currently in match |

## Processing Steps

1. Validate player has pending invitation for group
2. Check invitation not expired (<60 seconds)
3. Verify group exists and not disbanded
4. Check group size (<5 members)
5. Verify group not in queue (QueueStatus = NotQueued)
6. Check player not already in group
7. Check player not in active game
8. Add player to group members
9. Assign team slot (next available 0-4)
10. Remove pending invitation
11. Send full group state to joining player
12. Broadcast join notification to existing members
13. Update ChatSession.CurrentGroup reference

## Team Slot Assignment

**Slots**: 0-4 (5 total slots per group)

**Assignment Logic**:
```csharp
int assignedSlot = 0;
for (int i = 0; i < 5; i++)
{
    if (!group.Members.Values.Any(m => m.TeamSlot == i))
    {
        assignedSlot = i;
        break;
    }
}

var newMember = new GroupMember
{
    AccountID = session.AccountID,
    AccountName = session.AccountName,
    TeamSlot = assignedSlot,
    ReadyStatus = false
};
```

## State Management

```csharp
// Add to group
group.Members.TryAdd(session.AccountID, newMember);
session.CurrentGroup = group;

// Remove invitation
group.PendingInvitations.TryRemove(invitationID, out _);

// Broadcast to existing members
var joinNotification = BuildGroupMemberJoinedMessage(group, newMember);
foreach (var member in group.Members.Values.Where(m => m.AccountID != session.AccountID))
{
    member.Session.SendAsync(joinNotification);
}
```

## Related Commands

- **group-invite.md**: Receive invitation before joining
- **group-leave.md**: Leave group after joining
- **group-ready-status.md**: Mark ready after joining
- **group-join-queue.md**: Queue with group (Phase 3)
