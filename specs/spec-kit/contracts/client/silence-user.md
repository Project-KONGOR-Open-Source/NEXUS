# Silence User

**Command**: NET_CHAT_CL_SILENCE_USER
**Phase**: Phase 1 (Chat Channels)
**Direction**: Client → Server (admin only)
**Response**: Notification to silenced user + broadcast to channel

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_SILENCE_USER]
[string: Channel Name]
[string: Target Account Name]
[int32: Duration Seconds]
[string: Reason (optional)]
```

**Fields**:
- **Channel Name** (string): Channel to silence user in
- **Target Account Name** (string): User to silence
- **Duration Seconds** (int32): Silence duration (0 = permanent, >0 = temporary)
- **Reason** (string): Optional reason (max 256 chars)

**Example**:
```
Channel Name: "General"
Target Account Name: "SpammyUser"
Duration Seconds: 300 (5 minutes)
Reason: "Excessive messaging"
```

## Response Messages

**Success Response (to silencing admin)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_SILENCE_USER_RESPONSE]
[int8: Success = 1]
[string: Channel Name]
[string: Target Account Name]
[int64: Silence Until (Unix epoch milliseconds, 0 if permanent)]
```

**Notification to Silenced User**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_SILENCED_NOTIFICATION]
[string: Channel Name]
[string: Admin Account Name]
[int64: Silence Until (Unix epoch milliseconds, 0 if permanent)]
[string: Reason]
```

**Broadcast to Channel** (optional):
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_USER_SILENCED_BROADCAST]
[string: Channel Name]
[string: Silenced Account Name]
[string: Admin Account Name]
[int32: Duration Seconds]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Permission Requirements

| Admin Level | Can Silence |
|-------------|-------------|
| **None (0)** | No |
| **Officer (1)** | Only non-admins |
| **Leader (2)** | Officers and below |
| **Administrator (3)** | Leaders and below |
| **Staff (4)** | Everyone |

## Error Cases

| Error | Reason |
|-------|--------|
| "Insufficient permissions" | Silencer lacks admin rights |
| "Cannot silence higher rank" | Target has equal or higher admin level |
| "User not in channel" | Target not member of channel |
| "User not found" | Target account name invalid |
| "Already silenced" | User already silenced (can extend duration) |

## Duration Presets

| Duration | Seconds | Use Case |
|----------|---------|----------|
| **5 minutes** | 300 | Minor spam |
| **30 minutes** | 1800 | Repeated warnings |
| **1 hour** | 3600 | Disruptive behaviour |
| **24 hours** | 86400 | Serious violation |
| **Permanent** | 0 | Severe/repeated offences |

## Processing Steps

1. Validate silencer is member of channel with admin rights
2. Check silencer's admin level (Officer or higher)
3. Find target user by account name
4. Validate target is member of channel
5. Check admin level hierarchy (silencer > target)
6. Calculate silence expiration (DateTime.UtcNow + duration)
7. Update ChatChannelMember.SilencedUntil field
8. Send notification to silenced user
9. Optionally broadcast to channel (configurable)
10. Send confirmation to silencing admin
11. Log silence event for moderation audit

## Silence Enforcement

**Message Validation**:
```csharp
public bool CanSendMessage(ChatChannelMember member)
{
    if (member.SilencedUntil == null)
        return true;

    if (member.SilencedUntil == DateTime.MaxValue) // Permanent
        return false;

    if (DateTime.UtcNow < member.SilencedUntil)
        return false; // Still silenced

    // Silence expired, clear it
    member.SilencedUntil = null;
    return true;
}
```

## State Management

```csharp
// Apply silence
ChatChannelMember member = channel.Members[targetAccountID];

if (durationSeconds == 0)
{
    member.SilencedUntil = DateTime.MaxValue; // Permanent
}
else
{
    member.SilencedUntil = DateTime.UtcNow.AddSeconds(durationSeconds);
}

// Audit log
AuditLog.LogChannelSilence(channelName, silencerAccountID, targetAccountID,
                           durationSeconds, reason);
```

## Unsilence Command

**Request**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_UNSILENCE_USER]
[string: Channel Name]
[string: Target Account Name]
```

**Processing**:
```csharp
member.SilencedUntil = null; // Clear silence
```

## Related Commands

- **channel-message.md**: Enforces silence check before sending
- **kick-from-channel.md**: Escalation from silence
- **join-channel.md**: Shows silence status in member list
