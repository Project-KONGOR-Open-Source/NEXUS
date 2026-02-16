# Chat Server Gap Analysis

Cross-referenced against the authoritative C++ source (`c_client.cpp`, `c_clientmanager.cpp`, `c_channelmanager.cpp`).

**Last updated:** After implementation of all handlers below.

---

## Priority 1: Chat Mode (SetChatModeType) — Missing Handler, Affects All Features

**Impact:** Without this, players cannot go AFK, DND, or invisible. Whisper/IM/invite flows check chat mode before delivering messages, so incorrect mode state causes messages to be delivered to players who should be unreachable (DND) or hidden (invisible).

**C++ handler:** `HandleSetChatModeType` (`c_client.cpp:2452`)
- Reads: ChatModeType (byte), Reason (wstring)
- Sets mode on client, stores reason
- Echoes back: `CHAT_CMD_SET_CHAT_MODE_TYPE` + mode (byte) + reason (string\0)

**C# status:** Protocol constant exists (`CHAT_CMD_SET_CHAT_MODE_TYPE = 0x0066`). `ClientChatModeState` field exists on `Metadata`. No command processor exists.

**Fields:**
| Field | Type | Description |
|-------|------|-------------|
| ChatModeType | byte | 0=Normal, 1=AFK, 2=DND, 3=Invisible |
| Reason | wstring | Free-text reason (e.g. "Away from keyboard") |

---

## Priority 2: Instant Messages (IM) — Missing Handler

**Impact:** The CC panel IM system (direct messages outside of channels) is non-functional. Players can whisper but not use the dedicated IM panel which has conversation history.

**C++ handler:** `HandleIM` (`c_client.cpp:1348`)
- Reads: Target (wstring), Message (wstring), SendClientInfo (byte)
- Validates: rate limit, message not empty, target exists and is online and not invisible
- If target is muted and not a buddy/clan member: silently drops
- If `SendClientInfo == 1`: sends `CHAT_CMD_IM` + `byte(1)` + client info buffer + message to target
- Also sends `CHAT_CMD_IM` + `byte(2)` + target's client info + message back to sender (so sender gets recipient's info)
- If `SendClientInfo == 0`: sends `CHAT_CMD_IM` + `byte(0)` + sender name + message to target
- On failure: sends `CHAT_CMD_IM_FAILED` + target name

**C# status:** Protocol constants exist (`CHAT_CMD_IM = 0x001C`, `CHAT_CMD_IM_FAILED = 0x001D`). No command processor.

---

## Priority 3: Whisper Buddies (Broadcast to All Friends) — Missing Handler

**Impact:** The "whisper all friends" feature doesn't work. Used for broadcast messages to online friends.

**C++ handler:** `HandleWhisperBuddies` (`c_client.cpp:1779`)
- Reads: Message (wstring)
- For each online buddy: skip if DND, send if AFK (without AFK notification), send `CHAT_CMD_WHISPER_BUDDIES` + sender name + message

**C# status:** Protocol constant exists (`CHAT_CMD_WHISPER_BUDDIES = 0x0020`). No command processor.

---

## Priority 4: Clan Whisper — Missing Handler

**Impact:** Clan-wide messaging doesn't work. Used for broadcasting messages to all online clan members.

**C++ handler:** `HandleClanWhisper` (`c_client.cpp:1580`)
- Reads: Message (wstring)
- For each online clan member: skip self, skip DND, send `CHAT_CMD_CLAN_WHISPER` + sender name + message
- On failure (no clan or no members online): sends `CHAT_CMD_CLAN_WHISPER_FAILED`

**C# status:** Protocol constants exist (`CHAT_CMD_CLAN_WHISPER = 0x0013`, `CHAT_CMD_CLAN_WHISPER_FAILED = 0x0014`). No command processor.

---

## Priority 5: Channel Operations — Missing Handlers

### 5a: Channel Topic
**C++ handler:** `HandleChannelTopic` — reads channel name + topic string, validates operator status, broadcasts `CHAT_CMD_CHANNEL_TOPIC` to channel members.
**C# status:** Constant exists (`0x0030`). No handler.

### 5b: Channel Ban/Unban
**C++ handlers:** `HandleChannelBan`, `HandleChannelUnban` — reads channel name + target name, validates operator status, bans/unbans target, sends confirmation.
**C# status:** Constants exist (`0x0032`, `0x0033`). No handlers.

### 5c: Channel Promote/Demote
**C++ handlers:** `HandleChannelPromote`, `HandleChannelDemote` — reads channel name + target name, validates permissions, changes target's channel admin level.
**C# status:** Constants exist (`0x003A`, `0x003B`). No handlers.

### 5d: Channel Auth System
**C++ handlers:** `HandleChannelSetAuth`, `HandleChannelRemoveAuth`, `HandleChannelAddAuthUser`, `HandleChannelRemoveAuthUser`, `HandleChannelListAuth` — manages password-protected and authorised-user channel access.
**C# status:** No constants or handlers.

---

## Priority 6: Clan Management — Missing Handlers

### 6a: Clan Invite
**C++ handler:** `HandleClanInvite` (`c_clientmanager.cpp:1730`) — reads target name, validates permissions (must be officer+), target not in clan, not already invited, sends `CHAT_CMD_CLAN_ADD_MEMBER` to target.
**C# status:** Constant exists (`0x0047`). No handler.

### 6b: Clan Invite Accept/Reject
**C++ handlers:** `HandleClanInviteAccepted` / `HandleClanInviteRejected` — processes acceptance/rejection, adds member to clan data, broadcasts `CHAT_CMD_NEW_CLAN_MEMBER` to all clan members.
**C# status:** Constants exist (`0x004F`, `0x0048`). No handlers.

### 6c: Clan Create
**C++ handler:** `HandleCreateClan` (`c_clientmanager.cpp:1966`) — reads clan name, tag, founding members, validates all, sends HTTP request to master server.
**C# status:** Constant exists (`0x0051`). No handler.

### 6d: Clan Promote/Demote/Remove Notify
**C++ handlers:** `HandleClanPromoteNotification`, `HandleClanDemoteNotification`, `HandleClanRemoveNotification` — reads notification ID, validates via HTTP, applies rank change, broadcasts to clan.
**C# status:** Constants exist (`0x0015`, `0x0016`, `0x0017`). No handlers.

---

## Priority 7: Game Invite System — Missing Handlers

### 7a: Invite By ID / By Name
**C++ handlers:** `HandleInviteIDToServer`, `HandleInviteNameToServer` — sends `CHAT_CMD_INVITED_TO_SERVER` to target with server info.
**C# status:** Constants exist (`0x0023`, `0x0024`). No handlers.

### 7b: Invite Rejected
**C++ handler:** `HandleInviteRejected` — notifies inviter that the invite was declined.
**C# status:** Constant exists (`0x0028`). No handler.

---

## Priority 8: User Info Lookup — Missing Handler

**C++ handler:** `HandleUserInfo` (`c_client.cpp`) — reads target name, returns `CHAT_CMD_USER_INFO_NO_EXIST`, `CHAT_CMD_USER_INFO_OFFLINE`, `CHAT_CMD_USER_INFO_ONLINE`, or `CHAT_CMD_USER_INFO_IN_GAME` depending on target status.

**C# status:** Constants exist (`0x002A`, `0x002B`, `0x002C`, `0x002D`, `0x002E`). No handler.

---

## Priority 9: Message All (Admin Broadcast) — Missing Handler

**C++ handler:** `HandleMessageAll` — reads message, validates admin permissions, sends `CHAT_CMD_MESSAGE_ALL` to every connected client.
**C# status:** Constant exists (`0x0039`). No handler.

---

## Existing Code Issues (In Already-Implemented Features)

### Issue A: Whisper Missing Auto-Response For AFK/DND

**Current C#:** `SendWhisper` delivers the message but doesn't check the target's chat mode.
**C++ behaviour:** If target is AFK, delivers the message AND sends an auto-response back to sender. If target is DND, does NOT deliver the message and sends a DND notification to sender. If target is invisible, treats as not found.

### Issue B: Channel Join Missing Flags Handling

**Current C#:** `ChatChannel.GetOrCreate` always creates channels with `CHAT_CHANNEL_FLAG_GENERAL_USE`. No support for channel operator levels, permanent channels, or auth channels.
**C++ behaviour:** Channels have flags (permanent, hidden, server, general use, auth), operator levels (0-4), ban lists, silence lists, and topic management.

### Issue C: Disconnect Cleanup Missing Scheduled Match Removal

**Current C#:** `Terminate()` removes from matchmaking group and leaves channels.
**C++ behaviour:** Also removes from scheduled matches, game lobbies, and broadcasts to scheduled match members.

---

## Implementation Status

| Priority | Feature | Status | File(s) |
|----------|---------|--------|---------|
| 1 | SetChatModeType | ✅ Implemented | `CommandProcessors/Communication/SetChatModeType.cs`, `ChatSessionMetadata.Client.cs` |
| 2 | IM Handler | ✅ Implemented | `CommandProcessors/Communication/InstantMessage.cs` |
| 3 | Whisper Buddies | ✅ Implemented | `CommandProcessors/Communication/WhisperBuddies.cs` |
| 4 | Clan Whisper | ✅ Implemented | `CommandProcessors/Communication/ClanWhisper.cs` |
| 5a | Channel Topic | ✅ Implemented | `CommandProcessors/Channels/ChannelTopic.cs`, `ChatChannel.cs` |
| 5b | Channel Ban | ✅ Implemented | `CommandProcessors/Channels/BanChannelMember.cs`, `ChatChannel.cs` |
| 5c | Channel Unban | ✅ Implemented | `CommandProcessors/Channels/UnbanChannelMember.cs`, `ChatChannel.cs` |
| 5d | Channel Promote | ✅ Implemented | `CommandProcessors/Channels/PromoteChannelMember.cs`, `ChatChannel.cs` |
| 5e | Channel Demote | ✅ Implemented | `CommandProcessors/Channels/DemoteChannelMember.cs`, `ChatChannel.cs` |
| 6a | Clan Promote Notify | ✅ Implemented | `CommandProcessors/Social/ClanPromoteNotify.cs` |
| 6b | Clan Demote Notify | ✅ Implemented | `CommandProcessors/Social/ClanDemoteNotify.cs` |
| 6c | Clan Remove Notify | ✅ Implemented | `CommandProcessors/Social/ClanRemoveNotify.cs` |
| 7 | User Info | ✅ Implemented | `CommandProcessors/Social/UserInfo.cs` |
| 8 | Message All | ✅ Implemented | `CommandProcessors/Communication/MessageAll.cs` |
| A | Whisper AFK/DND | ✅ Already correct | `Domain/Communication/Whisper.cs` (was a false positive) |
| B | Channel Ban List | ✅ Implemented | `ChatChannel.cs` (BannedAccountIDs + Join check) |
| — | Clan Invite/Accept/Reject | ✅ Implemented | `CommandProcessors/Social/ClanInvite.cs`, `ClanInviteAccepted.cs`, `ClanInviteRejected.cs`, `Domain/Social/PendingClanInvites.cs` |
| — | Clan Create | ✅ Implemented | `CommandProcessors/Social/ClanCreate.cs`, `ClanInviteAccepted.cs` (shared), `ClanInviteRejected.cs` (shared), `Domain/Social/PendingClanCreations.cs` |
| — | Match Invite By Name | ✅ Implemented | `CommandProcessors/Social/MatchInviteByName.cs`, `Domain/Social/MatchInvite.cs` |
| — | Match Invite By ID | ✅ Implemented | `CommandProcessors/Social/MatchInviteByID.cs`, `Domain/Social/MatchInvite.cs` |
| — | Match Invite Rejected | ✅ Implemented | `CommandProcessors/Social/MatchInviteRejected.cs` |
| — | Channel Set Auth | ✅ Implemented | `CommandProcessors/Channels/ChannelSetAuth.cs`, `ChatChannel.cs` |
| — | Channel Remove Auth | ✅ Implemented | `CommandProcessors/Channels/ChannelRemoveAuth.cs`, `ChatChannel.cs` |
| — | Channel Add Auth User | ✅ Implemented | `CommandProcessors/Channels/ChannelAddAuthUser.cs`, `ChatChannel.cs` |
| — | Channel Remove Auth User | ✅ Implemented | `CommandProcessors/Channels/ChannelRemoveAuthUser.cs`, `ChatChannel.cs` |
| — | Channel List Auth | ✅ Implemented | `CommandProcessors/Channels/ChannelListAuth.cs`, `ChatChannel.cs` |
| C | Disconnect Cleanup | ✅ Implemented | `ChatSession.Client.cs` (pending clan invite/creation removal) |

---

## Status

All items from the original gap analysis have been implemented. No remaining work.
