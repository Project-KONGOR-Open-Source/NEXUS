# Join Channel

**Command**: NET_CHAT_CL_JOIN_CHANNEL
**Phase**: Phase 1 (Chat Channels)
**Direction**: Client → Server
**Response**: Channel member list + channel properties

## Request Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_JOIN_CHANNEL]
[string: Channel Name]
[string: Password (optional, empty string if none)]
```

**Fields**:
- **Channel Name** (string): Name of channel to join (1-64 chars, alphanumeric + underscore)
- **Password** (string): Optional channel password (empty string if no password)

**Example**:
```
Length: 22 bytes
Command: 0x0501 (example)
Channel Name: "General" (7 chars + 2-byte length = 9 bytes)
Password: "" (0 chars + 2-byte length = 2 bytes)
Total payload: 9 + 2 + 2 (command) = 13 bytes
```

## Response Message

**Structure**:
```
[2 bytes: Length]
[2 bytes: Command Code - NET_CHAT_CL_JOIN_CHANNEL_RESPONSE]
[int8: Success (1) or Failure (0)]
[string: Channel Name]
[int32: Channel ID]
[int32: Channel Flags]
[string: Channel Topic]
[int32: Member Count]
[for each member:]
    [int32: Account ID]
    [string: Account Name]
    [int8: Admin Level]
    [int8: Is Silenced]
```

**Success Response Fields**:
- **Success**: 1 (int8)
- **Channel Name**: Joined channel name
- **Channel ID**: Internal channel ID
- **Channel Flags**: Bitfield (Permanent, Hidden, Server, Reserved)
- **Channel Topic**: Current topic string
- **Member Count**: Number of members in channel
- **Member List**: For each member:
  - Account ID (int32)
  - Account Name (string)
  - Admin Level (int8): 0=None, 1=Officer, 2=Leader, 3=Administrator, 4=Staff
  - Is Silenced (int8): 1 if silenced, 0 if not

**Failure Response**:
```
[int8: Success = 0]
[string: Error Message]
```

## Error Cases

| Error | Reason |
|-------|--------|
| "Channel not found" | Channel doesn't exist and creation not allowed |
| "Incorrect password" | Password required but incorrect |
| "Banned from channel" | User banned from channel |
| "Channel full" | Maximum members reached |
| "Already in channel" | User already member of channel |
| "Clan channel restricted" | Non-clan member attempting clan channel join |

## Processing Steps

1. Validate channel name (length, characters)
2. Check if player already in channel (reject duplicate joins)
3. Find or create channel (GetOrAdd pattern)
4. Validate password if channel has one
5. Check clan membership if clan channel
6. Check ban list
7. Add player to channel members (ChatChannelMember)
8. Broadcast join notification to existing members
9. Send channel state + member list to joining player

## Related Commands

- **leave-channel.md**: Leave channel
- **channel-message.md**: Send message to channel
- **kick-from-channel.md**: Kick user from channel (admin)
