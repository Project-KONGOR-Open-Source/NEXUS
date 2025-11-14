# Group Invite

**Command**: NET_CHAT_CL_TMM_GROUP_INVITE
**Phase**: Phase 2 (Matchmaking Groups)
**Direction**: Client → Server
**Response**: Invitation notification to target player

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_INVITE]
[string: Target Account Name]
```

**Fields**:
- **Target Account Name** (string): Player to invite to group

**Example**:
```
Target Account Name: "PlayerTwo"
```

## Response Messages

**Success Response (to inviter)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_INVITE_RESPONSE]
[int8: Success = 1]
[string: Target Account Name]
```

**Notification to Target Player**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_TMM_GROUP_INVITE_NOTIFICATION]
[int32: Group ID]
[int32: Inviter Account ID]
[string: Inviter Account Name]
[int8: Game Type]
[int8: Game Mode]
[int8: Region]
[int32: Current Member Count]
[int64: Invitation Expiry (Unix epoch milliseconds)]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Group size limit** | Max 5 players per group |
| **Inviter must be group leader** | Only leader can invite |
| **Group not in queue** | Cannot invite while queued |
| **Target online** | Target must be connected to chat server |
| **Target not in group** | Target must not already be in a group |
| **Target not in game** | Target must not be in active match |

## Error Cases

| Error | Reason |
|-------|--------|
| "Not group leader" | Only leader can send invitations |
| "Group full" | Already 5 members in group |
| "Group in queue" | Cannot modify group while queued |
| "Player not found" | Target account name invalid or offline |
| "Player already in group" | Target already member of another group |
| "Player in game" | Target currently in active match |
| "Invitation already pending" | Previous invitation not yet accepted/declined |

## Processing Steps

1. Validate sender is group leader
2. Check group size (must be <5)
3. Verify group not in queue (QueueStatus = NotQueued)
4. Find target player session (must be online)
5. Check target not in group (CurrentGroup == null)
6. Check target not in game (InGame == false)
7. Create pending invitation (60 second expiry)
8. Add to PendingInvitations dictionary
9. Send invitation notification to target
10. Send confirmation to inviter

## Invitation Expiry

**Duration**: 60 seconds

**Expiry Handling**:
```csharp
var invitation = new GroupInvitation
{
    GroupID = group.GroupID,
    InviterAccountID = session.AccountID,
    TargetAccountID = targetAccountID,
    ExpiresAt = DateTime.UtcNow.AddSeconds(60)
};

PendingInvitations.TryAdd(invitationID, invitation);

// Auto-cleanup after 60 seconds
_ = Task.Delay(60000).ContinueWith(_ =>
{
    PendingInvitations.TryRemove(invitationID, out _);
});
```

## State Management

```csharp
public class MatchmakingGroup
{
    public ConcurrentDictionary<int, GroupInvitation> PendingInvitations = new();
}

public class GroupInvitation
{
    public int GroupID;
    public int InviterAccountID;
    public int TargetAccountID;
    public DateTime ExpiresAt;
}
```

## Related Commands

- **group-join.md**: Accept invitation
- **group-invite-decline.md**: Decline invitation
- **group-create.md**: Create group before inviting
- **group-leave.md**: Leave group
