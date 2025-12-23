# Leave Channel

**Command**: NET_CHAT_CL_LEAVE_CHANNEL
**Phase**: Phase 1 (Chat Channels)
**Direction**: Client → Server
**Response**: Broadcast to remaining members

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_LEAVE_CHANNEL]
[string: Channel Name]
```

**Fields**:
- **Channel Name** (string): Name of channel to leave

**Example**:
```
Channel Name: "General"
```

## Response Message

**Success Response (to leaving player)**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_LEAVE_CHANNEL_RESPONSE]
[int8: Success = 1]
[string: Channel Name]
```

**Broadcast to Remaining Members**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_CHANNEL_USER_LEFT]
[string: Channel Name]
[int32: Account ID]
[string: Account Name]
```

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Error Cases

| Error | Reason |
|-------|--------|
| "Not in channel" | User not member of specified channel |
| "Cannot leave permanent channel" | Attempting to leave a permanent system channel |

## Processing Steps

1. Validate channel name
2. Check if player is member of channel
3. Check if channel is permanent (prevent leaving if so)
4. Remove player from ChatChannel.Members
5. Broadcast departure to remaining members
6. If channel empty and not permanent, destroy channel
7. Remove channel from player's joined channels list
8. Send confirmation to leaving player

## Channel Cleanup Rules

| Condition | Action |
|-----------|--------|
| **Last member leaves non-permanent channel** | Destroy channel immediately |
| **Last member leaves permanent channel** | Keep channel alive (empty) |
| **Leader leaves** | Transfer leadership to next member by join order |

## State Management

```csharp
// Remove member
ChatChannel channel = ActiveChannels[channelName];
channel.Members.TryRemove(accountID, out _);

// Cleanup if empty and not permanent
if (channel.Members.IsEmpty && !channel.Flags.HasFlag(ChannelFlags.Permanent))
{
    ActiveChannels.TryRemove(channelName, out _);
}
```

## Related Commands

- **join-channel.md**: Join channel
- **channel-message.md**: Send message before leaving
- **kick-from-channel.md**: Forced removal by admin
