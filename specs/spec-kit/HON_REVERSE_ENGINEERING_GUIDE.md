# HoN TCP Message Flow Reverse Engineering Guide

**Date**: 2025-01-16
**Purpose**: Document the methodology for extracting protocol behavior from HoN legacy source code
**Target Audience**: Developers implementing NEXUS chat server features

---

## Overview

This guide explains how to reverse engineer TCP message flows and protocol behaviors from the Heroes of Newerth (HoN) legacy source code located at `C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\`.

**Key Principle**: HoN source is the **absolute source of truth** for protocol behavior. KONGOR (`C:\Users\SADS-810\Source\NEXUS\LEGACY\KONGOR\`) serves as a **practical reference** for working implementations.

---

## Step 1: Locate Protocol Documentation

### Primary Sources

1. **Protocol Header Files** (`chat/HoN_Chat_Server/Chat Server/`)
   - `chatserver_protocol.h` - Command codes, constants, enums
   - `c_client.h` - Client state definitions
   - `c_channel.h` - Channel flags, structures

2. **Protocol Documentation** (`chat/`)
   - `Chatserver Protocol.txt` - Human-readable message format specifications
   - Contains field-by-field breakdown of every command

NOTE: The human-readable message format specification is not on full parity with the code, so always also check the code to make sure

### Example: Finding Command Codes

```cpp
// File: chat/HoN_Chat_Server/Chat Server/chatserver_protocol.h
// Lines: 50-250 (approximate)

#define CHAT_CMD_CHANNEL_MSG         0x0003  // Channel message
#define CHAT_CMD_CHANGED_CHANNEL     0x0004  // Join confirmation
#define CHAT_CMD_JOINED_CHANNEL      0x0005  // Member joined broadcast
#define CHAT_CMD_LEFT_CHANNEL        0x0006  // Member left broadcast
```

**Search Strategy**:
```powershell
# Find all command definitions
Get-ChildItem C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\ -Recurse -Filter "*.h" |
    Select-String "CHAT_CMD_" |
    Select-Object Filename, LineNumber, Line
```

---

## Step 2: Understand Message Formats

### Read the Protocol Documentation

**File**: `LEGACY\HoN\chat\Chatserver Protocol.txt`

Each command is documented with:
- Command code (hex value)
- Description
- Field-by-field format with types
- Directional flow (client→server or server→client)

### Example: CHAT_CMD_CHANGED_CHANNEL

```
0x0004 - CHAT_CMD_CHANGED_CHANNEL - Tells the client they have successfully joined a chat channel.
Format:
    [X] string - channel name
    [4] unsigned long - channel ID
    [1] unsigned char - channel flags (refer to CHAT_CHANNEL_FLAG_*)
    [X] string - channel topic
    [4] unsigned long - number of channel admins
        [4] unsigned long - admin account ID
        [1] EAdminLevel - admin level
    [4] unsigned long - number of clients in the channel
        [X] string - client name
        [4] unsigned long - client account ID
        [1] EChatClientStatus - client status
        [1] unsigned char - client flags (refer to CHAT_CLIENT_* flags)
        [X] string - client chat symbol
        [X] string - client chat name color
        [X] string - client chat icon name
```

**Format Legend**:
- `[X]` = Variable-length, null-terminated string
- `[1]` = 1 byte (unsigned char/byte)
- `[2]` = 2 bytes (unsigned short)
- `[4]` = 4 bytes (unsigned long/int)
- `[8]` = 8 bytes (unsigned long long)

---

## Step 3: Find Implementation Code

### Locate Message Handlers

**Search Pattern**: Look for command code usage in `.cpp` files

```powershell
# Find where CHAT_CMD_CHANGED_CHANNEL is used
Get-ChildItem C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\ -Recurse -Filter "*.cpp" |
    Select-String "CHAT_CMD_CHANGED_CHANNEL" -Context 5,20
```

### Example: Channel Join Implementation

**File**: `chat/HoN_Chat_Server/Chat Server/c_channel.cpp`
**Method**: `CChannel::AddClient()`
**Lines**: 337-360

```cpp
void CChannel::AddClient(CClient* pClient, EAdminLevel eLevel)
{
    if (pClient == NULL || m_pClients->find(pClient) != m_pClients->end())
        return;

    // Add client to channel
    m_pClients->insert(pClient);
    pClient->AddChannel(m_uiChannelID, this);

    if (eLevel != CHAT_CLIENT_ADMIN_NONE)
        m_mapAdmins[pClient->GetAccountID()] = eLevel;

    // Build CHAT_CMD_CHANGED_CHANNEL response
    m_bufferSend.Clear();
    m_bufferSend
        << CHAT_CMD_CHANGED_CHANNEL
        << m_sNameUTF8 << byte('\0')           // Channel name
        << m_uiChannelID                       // Channel ID
        << byte(m_uiFlags & 0xff)              // Channel flags
        << m_sTopicUTF8 << byte('\0')          // Topic
        << INT_SIZE(m_mapAdmins.size());       // Admin count

    // Write admin list
    for (AdminMap_it itAdmin(m_mapAdmins.begin()), itEnd(m_mapAdmins.end());
         itAdmin != itEnd; ++itAdmin)
    {
        m_bufferSend << itAdmin->first << byte(itAdmin->second);
    }

    // Write member count
    m_bufferSend << INT_SIZE(m_pClients->size());

    // Write full member list
    for (ClientSet_it itClient(m_pClients->begin()), itClientEnd(m_pClients->end());
         itClient != itClientEnd; ++itClient)
    {
        CClient* pTarget(*itClient);
        if (pTarget != NULL)
            pTarget->GetClientInfoBuffer(m_bufferSend, true, false, true, false, false);
    }

    // Send to joining client
    pClient->Send(m_bufferSend);

    // Broadcast join notification to existing members
    BroadcastJoin(pClient);
}
```

---

## Step 4: Trace Message Flow

### Understanding Client-Server Interactions

**Question**: Does the leaving player receive CHAT_CMD_LEFT_CHANNEL?

#### Method 1: Check Protocol Documentation

**File**: `Chatserver Protocol.txt`
**Line**: 52

```
0x0006 - CHAT_CMD_LEFT_CHANNEL - Tells a client that a client has left a chat channel.
This is sent to the client that's leaving.
```

✅ **Clear answer**: YES, the leaving player receives it.

#### Method 2: Check Implementation Code

**File**: `c_channel.cpp`
**Method**: `RemoveClient()`
**Lines**: 371-381

```cpp
void CChannel::RemoveClient(CClient* pClient)
{
    if (pClient == NULL || m_pClients->find(pClient) == m_pClients->end())
        return;

    // Build leave message
    m_bufferSend.Clear();
    m_bufferSend << CHAT_CMD_LEFT_CHANNEL << pClient->GetAccountID() << m_uiChannelID;

    // Broadcast to ALL clients (including leaving player)
    Broadcast(m_bufferSend);

    // THEN remove the client from channel
    m_pClients->erase(pClient);
    pClient->RemoveChannel(m_uiChannelID);

    SetActive();
}
```

**Key Observation**: `Broadcast()` is called BEFORE `m_pClients->erase(pClient)`, so the leaving player is still in the member list and receives the message.

#### Method 3: Check Broadcast Implementation

**File**: `c_channel.cpp`
**Method**: `Broadcast()`
**Lines**: 124-138

```cpp
void CChannel::Broadcast(IBuffer& buffer, CClient* pIgnoreClient)
{
    for (ClientSet_it itClient(m_pClients->begin()), itEnd(m_pClients->end());
         itClient != itEnd; ++itClient)
    {
        CClient* pClient(*itClient);

        if (pClient != NULL && pClient != pIgnoreClient)
            pClient->Send(buffer);
    }
}
```

**Analysis**:
- `RemoveClient()` calls `Broadcast(m_bufferSend)` with NO second parameter
- This means `pIgnoreClient` defaults to NULL
- Therefore, ALL clients in `m_pClients` receive the message
- Since removal happens AFTER broadcast, the leaving player is included

✅ **Confirmed**: Leaving player receives CHAT_CMD_LEFT_CHANNEL.

---

## Step 5: Extract Behavioral Logic

### Channel Lifecycle Example

**Question**: When do channels get cleaned up?

#### Step 5.1: Find Channel Status Management

**File**: `c_channel.cpp`
**Method**: `Frame()` (channel processing)
**Lines**: 115-120

```cpp
void CChannel::Frame()
{
    // Check if channel should be marked for deletion
    if (m_pClients->empty() &&
        !HasAllFlags(CHAT_CHANNEL_FLAG_PERMANENT) &&
        !HasAllFlags(CHAT_CHANNEL_FLAG_STREAM_USE))
    {
        m_eStatus = CHANNEL_DEAD;
    }
}
```

**Findings**:
1. Empty channels become `CHANNEL_DEAD` status
2. UNLESS they have `PERMANENT` flag
3. OR they have `STREAM_USE` flag
4. Stream channels have special 12-hour cleanup cycles

#### Step 5.2: Find Channel Removal Logic

**File**: `c_channelmanager.cpp`
**Method**: `Frame()`
**Lines**: 210-215

```cpp
for (ChannelMap_it itChannel = m_pProcessChannels->begin();
     itChannel != m_pProcessChannels->end(); )
{
    CChannel* pChannel(itChannel->second);

    switch (pChannel->GetStatus())
    {
        case CHANNEL_DEAD:
            RemoveChannel(*itChannel);
            STL_ERASE(*m_pProcessChannels, itChannel);
            break;
    }
}
```

**Conclusion**: Channels marked CHANNEL_DEAD are removed from the channel manager during the next frame processing cycle.

---

## Step 6: Identify Constants and Flags

### Channel Flags

**File**: `chatserver_protocol.h`
**Lines**: 893-902

```cpp
// Channel Flags
#define CHAT_CHANNEL_FLAG_PERMANENT     BIT(0)   // 0x0001 - Never clean up
#define CHAT_CHANNEL_FLAG_SERVER        BIT(1)   // 0x0002 - Post-match server channel
#define CHAT_CHANNEL_FLAG_HIDDEN        BIT(2)   // 0x0004 - Hidden from lists
#define CHAT_CHANNEL_FLAG_RESERVED      BIT(3)   // 0x0008 - System-created
#define CHAT_CHANNEL_FLAG_GENERAL_USE   BIT(4)   // 0x0010 - General channels
#define CHAT_CHANNEL_FLAG_UNJOINABLE    BIT(5)   // 0x0020 - Cannot manually join
#define CHAT_CHANNEL_FLAG_AUTH_REQUIRED BIT(6)   // 0x0040 - Requires authorization
#define CHAT_CHANNEL_FLAG_CLAN          BIT(7)   // 0x0080 - Clan channel
#define CHAT_CHANNEL_FLAG_STREAM_USE    BIT(8)   // 0x0100 - Stream channel
```

**Usage Example**:

```cpp
// Create general channel (HoN 1, HoN 2, etc.)
pChannel->SetFlags(CHAT_CHANNEL_FLAG_RESERVED |
                   CHAT_CHANNEL_FLAG_PERMANENT |
                   CHAT_CHANNEL_FLAG_GENERAL_USE);

// Create clan channel
pChannel->SetFlags(CHAT_CHANNEL_FLAG_RESERVED |
                   CHAT_CHANNEL_FLAG_PERMANENT |
                   CHAT_CHANNEL_FLAG_CLAN);

// Create temporary public channel
pChannel->SetFlags(CHAT_CHANNEL_FLAG_RESERVED);  // No PERMANENT flag = cleanup when empty
```

---

## Step 7: Cross-Reference with KONGOR

### Why Check KONGOR?

1. **Practical validation**: KONGOR is production code that works
2. **Modern patterns**: May show better C# implementations
3. **Edge cases**: Might handle issues HoN didn't encounter
4. **Differences**: Helps identify intentional protocol deviations

### Example: Compare Implementations

**HoN C++ (source of truth)**:
```cpp
void CChannel::Broadcast(IBuffer& buffer, CClient* pIgnoreClient)
{
    for (auto client : *m_pClients)
        if (client != pIgnoreClient)
            client->Send(buffer);
}
```

**KONGOR C# (practical reference)**:
```csharp
public void BroadcastMessage(byte[] message, int? excludeAccountId = null)
{
    var recipients = excludeAccountId.HasValue
        ? Members.Values.Where(m => m.AccountId != excludeAccountId.Value)
        : Members.Values;

    foreach (var recipient in recipients)
        recipient.Session.SendAsync(message);
}
```

**Observations**:
- Same pattern: iterate and send
- HoN uses client pointer comparison, KONGOR uses account ID comparison
- Both support optional exclusion
- KONGOR uses async pattern (modern C#)

---

## Common Search Patterns

### Find Command Handler

```powershell
# Search for command code usage
Get-ChildItem "C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\" -Recurse -Filter "*.cpp" |
    Select-String "CHAT_CMD_YOUR_COMMAND" -Context 2,10
```

### Find Message Format

```powershell
# Search protocol documentation
Get-Content "C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\Chatserver Protocol.txt" |
    Select-String "0x00XX" -Context 0,15
```

### Find Flag Definitions

```powershell
# Search header files for flag patterns
Get-ChildItem "C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\" -Recurse -Filter "*.h" |
    Select-String "#define.*FLAG" |
    Select-Object Filename, LineNumber, Line
```

### Trace Function Calls

```powershell
# Find all calls to a specific method
Get-ChildItem "C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN\chat\" -Recurse -Filter "*.cpp" |
    Select-String "->MethodName\(" -Context 3,3
```

---

## Verification Checklist

When implementing a feature from HoN source:

- [ ] ✅ Found command code in `chatserver_protocol.h`
- [ ] ✅ Read message format in `Chatserver Protocol.txt`
- [ ] ✅ Located implementation in `.cpp` files
- [ ] ✅ Traced message flow (who sends to whom, when)
- [ ] ✅ Identified all relevant flags/constants
- [ ] ✅ Checked edge cases (empty states, error conditions)
- [ ] ✅ Verified broadcast/unicast behavior
- [ ] ✅ Cross-referenced with KONGOR for practical validation
- [ ] ✅ Documented any protocol deviations in NEXUS

---

## Example: Full Analysis Workflow

### Task: Implement Channel Message Broadcasting

#### 1. Find Command Code
**File**: `chatserver_protocol.h`
**Result**: `CHAT_CMD_CHANNEL_MSG = 0x0003`

#### 2. Read Protocol Format
**File**: `Chatserver Protocol.txt`
**Result**:
```
0x0003 - CHAT_CMD_CHANNEL_MSG - Channel message from player
Format:
    [4] unsigned long - sender account ID
    [4] unsigned long - channel ID
    [X] string - message text
```

#### 3. Find Implementation
**File**: `c_client.cpp`
**Method**: `HandleChannelMessage()`
**Code**:
```cpp
CChannel* pChannel = HandleChannelMessage(sMessage, uiChannelID);
if (pChannel != NULL)
{
    m_bufferSend.Clear();
    m_bufferSend << CHAT_CMD_CHANNEL_MSG
                 << m_uiAccountID
                 << uiChannelID
                 << sMessage << byte('\0');
    pChannel->Broadcast(m_bufferSend, this);  // Exclude sender
}
```

#### 4. Implement in NEXUS C#
```csharp
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)]
public class ChannelMessageProcessor : IAsynchronousCommandProcessor
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        string channelName = buffer.ReadString();
        string messageText = buffer.ReadString();

        ChatChannel channel = ChatChannel.Get(session, channelName);

        ChatBuffer broadcast = new ChatBuffer();
        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);
        broadcast.WriteInt32(session.Account.ID);        // Sender account ID
        broadcast.WriteInt32(channel.ID);                // Channel ID
        broadcast.WriteString(messageText);              // Message text

        channel.BroadcastMessage(broadcast, excludeAccountID: session.Account.ID);
    }
}
```

---

## Tips and Best Practices

### DO:
✅ Always check protocol documentation first
✅ Trace both send and receive sides of a message
✅ Look for existing patterns (Broadcast, Unicast, etc.)
✅ Verify flag combinations (PERMANENT + CLAN, etc.)
✅ Check edge cases (empty channels, offline targets)
✅ Cross-reference KONGOR for modern C# patterns

### DON'T:
❌ Assume behavior without checking implementation
❌ Ignore flag checks (they affect cleanup, access, etc.)
❌ Skip protocol documentation
❌ Forget to check broadcast exclusion logic
❌ Overlook error handling paths

---

## Quick Reference: Key Files

### Protocol Definition
- `chat/HoN_Chat_Server/Chat Server/chatserver_protocol.h` - Command codes, flags
- `chat/Chatserver Protocol.txt` - Message formats

### Channel Management
- `chat/HoN_Chat_Server/Chat Server/c_channel.h` - Channel class definition
- `chat/HoN_Chat_Server/Chat Server/c_channel.cpp` - Channel implementation
- `chat/HoN_Chat_Server/Chat Server/c_channelmanager.cpp` - Channel lifecycle

### Client Communication
- `chat/HoN_Chat_Server/Chat Server/c_client.h` - Client class definition
- `chat/HoN_Chat_Server/Chat Server/c_client.cpp` - Message handlers

### KONGOR Reference
- Check `LEGACY/KONGOR/` for modern C# equivalents
- Use for patterns, not protocol truth

---

## Conclusion

This methodology allows you to:
1. **Find** command codes and protocols
2. **Understand** message formats and flows
3. **Implement** features correctly in NEXUS
4. **Verify** behavior matches HoN exactly

**Remember**: HoN source = **truth**, KONGOR = **reference**, NEXUS = **implementation**.

---

**Last Updated**: 2025-01-16
**Maintainer**: NEXUS Development Team
