# Kick from Channel

**Command**: NET_CHAT_CL_KICK_FROM_CHANNEL
**Phase**: Phase 1 (Chat Channels)
**Direction**: Client → Server (admin only)
**Response**: Broadcast to channel + notification to kicked user

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_KICK_FROM_CHANNEL]
[string: Channel Name]
[string: Target Account Name]
[string: Reason (optional)]
```

**Fields**:
- **Channel Name** (string): Channel to kick from
- **Target Account Name** (string): User to kick
- **Reason** (string): Optional kick reason (max 256 chars)

**Example**:
```
Channel Name: "General"
Target Account Name: "DisruptiveUser"
Reason: "Spam"
```

## Response Messages

**Success Response (to kicking admin)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_KICK_FROM_CHANNEL_RESPONSE]
[int8: Success = 1]
[string: Channel Name]
[string: Target Account Name]
```

**Notification to Kicked User**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_KICKED_FROM_CHANNEL]
[string: Channel Name]
[string: Kicker Account Name]
[string: Reason]
```

**Broadcast to Channel**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_CHANNEL_USER_KICKED]
[string: Channel Name]
[int32: Kicked Account ID]
[string: Kicked Account Name]
[string: Kicker Account Name]
[string: Reason]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Permission Requirements

| Admin Level | Can Kick |
|-------------|----------|
| **None (0)** | No |
| **Officer (1)** | Only non-admins |
| **Leader (2)** | Officers and below |
| **Administrator (3)** | Leaders and below |
| **Staff (4)** | Everyone |

## Error Cases

| Error | Reason |
|-------|--------|
| "Insufficient permissions" | Kicker lacks admin rights |
| "Cannot kick higher rank" | Target has equal or higher admin level |
| "User not in channel" | Target not member of channel |
| "Cannot kick from permanent channel" | Protected system channel |
| "User not found" | Target account name invalid |

## Processing Steps

1. Validate kicker is member of channel
2. Check kicker's admin level (must be Officer or higher)
3. Find target user by account name
4. Validate target is member of channel
5. Check admin level hierarchy (kicker > target)
6. Remove target from channel members
7. Send kick notification to target
8. Broadcast kick event to remaining channel members
9. Send confirmation to kicking admin
10. Log kick event for moderation audit

## Temporary Ban Option

**Extended Structure** (optional):
```
[string: Channel Name]
[string: Target Account Name]
[string: Reason]
[int32: Ban Duration Seconds (0 = no ban)]
```

**Ban Duration**:
- **0**: No ban (instant rejoin allowed)
- **>0**: Temporary ban in seconds (300 = 5 mins, 3600 = 1 hour, 86400 = 24 hours)

**Ban Enforcement**:
```csharp
if (banDuration > 0)
{
    channel.BannedUsers[targetAccountID] = DateTime.UtcNow.AddSeconds(banDuration);
}
```

## State Management

```csharp
// Remove from channel
channel.Members.TryRemove(targetAccountID, out ChatChannelMember member);

// Apply temporary ban if specified
if (banDuration > 0)
{
    channel.TemporaryBans.TryAdd(targetAccountID, DateTime.UtcNow.AddSeconds(banDuration));
}

// Audit log
AuditLog.LogChannelKick(channelName, kickerAccountID, targetAccountID, reason);
```

## Related Commands

- **join-channel.md**: Rejoin after kick (if not banned)
- **leave-channel.md**: Voluntary departure
- **silence-user.md**: Alternative to kicking (mute instead)
