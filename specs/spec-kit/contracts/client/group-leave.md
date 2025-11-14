# Group Leave

**Command**: NET_CHAT_CL_TMM_GROUP_LEAVE
**Phase**: Phase 2 (Matchmaking Groups)
**Direction**: Client → Server
**Response**: Broadcast to remaining members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_LEAVE]
```

**Fields**: None (implicit from session)

## Response Messages

**Success Response (to leaving player)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_LEAVE_RESPONSE]
[int8: Success = 1]
```

**Broadcast to Remaining Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_MEMBER_LEFT]
[int32: Group ID]
[int32: Left Member Account ID]
[string: Left Member Account Name]
[int32: New Member Count]
[int32: New Leader Account ID (if leadership transferred)]
```

**Group Disbanded Notification** (if last member):
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_DISBANDED]
[int32: Group ID]
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
| "Cannot leave while queued" | Group currently in matchmaking queue |

## Queue Handling

**Rule**: Players **cannot** leave group while in queue

**Enforcement**:
```csharp
if (group.QueueStatus != QueueStatus.NotQueued)
{
    SendError(session, "Cannot leave group while in queue. Leave queue first.");
    return;
}
```

**User Flow**:
1. Player must first leave queue (group-leave-queue.md)
2. Then leave group

## Processing Steps

1. Validate player is member of a group
2. Check group not in queue (QueueStatus = NotQueued)
3. Remove player from group members
4. Clear ChatSession.CurrentGroup reference
5. Send confirmation to leaving player
6. Handle leadership transfer if needed
7. Broadcast departure to remaining members
8. Check if group now empty (disband if so)

## Leadership Transfer Rules

| Scenario | New Leader Selection |
|----------|---------------------|
| **Leader leaves (2+ members remain)** | Transfer to member with earliest join time |
| **Leader leaves (1 member remains)** | Transfer to last remaining member |
| **Non-leader leaves** | No change |
| **Last member leaves** | Disband group |

**Transfer Logic**:
```csharp
if (leavingPlayer.AccountID == group.LeaderAccountID && group.Members.Count > 1)
{
    // Find member with earliest join time (excluding leaver)
    var newLeader = group.Members.Values
        .Where(m => m.AccountID != leavingPlayer.AccountID)
        .OrderBy(m => m.JoinedAt)
        .First();

    group.LeaderAccountID = newLeader.AccountID;

    // Notify all remaining members of leadership change
    BroadcastLeadershipTransfer(group, newLeader.AccountID);
}
```

## Group Disbanding

**Trigger**: Last member leaves

**Cleanup Steps**:
```csharp
// Remove from active groups
ActiveGroups.TryRemove(group.GroupID, out _);

// Clear any pending invitations
group.PendingInvitations.Clear();

// Log disbanding
Logger.LogInformation($"Group {group.GroupID} disbanded (last member left)");
```

## State Management

```csharp
// Remove member
group.Members.TryRemove(session.AccountID, out GroupMember leftMember);
session.CurrentGroup = null;

// Check if empty
if (group.Members.IsEmpty)
{
    ActiveGroups.TryRemove(group.GroupID, out _);
    return; // No broadcast needed
}

// Handle leadership transfer
if (leftMember.AccountID == group.LeaderAccountID)
{
    var newLeader = group.Members.Values.OrderBy(m => m.JoinedAt).First();
    group.LeaderAccountID = newLeader.AccountID;
}

// Broadcast to remaining members
var leaveNotification = BuildGroupMemberLeftMessage(group, leftMember, group.LeaderAccountID);
foreach (var member in group.Members.Values)
{
    member.Session.SendAsync(leaveNotification);
}
```

## Related Commands

- **group-join.md**: Join group before leaving
- **group-create.md**: Create group
- **group-leave-queue.md**: Leave queue before leaving group
- **group-invite.md**: Invite players to group
