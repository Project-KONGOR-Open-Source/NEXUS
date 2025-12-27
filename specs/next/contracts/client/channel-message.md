# Channel Message

**Command**: NET_CHAT_CL_CHANNEL_MESSAGE
**Phase**: Phase 1 (Chat Channels)
**Direction**: Client → Server (broadcast to all members)
**Response**: Broadcast to channel members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_CHANNEL_MESSAGE]
[string: Channel Name]
[string: Message Text]
```

**Fields**:
- **Channel Name** (string): Target channel name
- **Message Text** (string): Message content (max 512 chars)

**Example**:
```
Channel Name: "General"
Message Text: "Hello everyone!"
```

## Broadcast Message (to all channel members except sender)

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_CHANNEL_MESSAGE_BROADCAST]
[string: Channel Name]
[int32: Sender Account ID]
[string: Sender Account Name]
[string: Message Text]
[int64: Timestamp (Unix epoch milliseconds)]
```

**Fields**:
- **Channel Name**: Channel where message sent
- **Sender Account ID**: Message sender's account ID
- **Sender Account Name**: Message sender's display name
- **Message Text**: Message content
- **Timestamp**: When message was sent (server time)

## Validation Rules

| Rule | Enforcement |
|------|-------------|
| **Max message length** | 512 characters (reject longer) |
| **Flood prevention** | Max 5 messages per 3 seconds per user |
| **Silence check** | Reject if user silenced in channel |
| **Membership check** | Reject if user not in channel |
| **Empty message** | Reject empty or whitespace-only messages |

## Error Responses

**Errors sent only to sender**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_ERROR]
[string: Error Message]
```

| Error | Reason |
|-------|--------|
| "Not in channel" | User not member of channel |
| "Silenced" | User silenced in channel |
| "Message too long" | Exceeds 512 char limit |
| "Flood detected" | Too many messages too quickly |

## Processing Steps

1. Validate user is member of channel
2. Check if user silenced in channel (SilencedUntil > DateTime.UtcNow)
3. Validate message length (max 512 chars)
4. Check flood prevention (5 messages per 3 seconds)
5. Trim whitespace, reject if empty
6. Build broadcast message with sender info + timestamp
7. Broadcast to all channel members (except sender)
8. Optionally send echo back to sender for confirmation

## Performance Considerations

- **Target**: <100ms P95 broadcast latency to all members
- **Optimization**: Use ChatChannel.BroadcastToAllMembers() with concurrent sends
- **Buffer reuse**: Serialize once, send same buffer to all recipients

## Related Commands

- **join-channel.md**: Join channel to send messages
- **leave-channel.md**: Leave channel
- **kick-from-channel.md**: Remove disruptive users
