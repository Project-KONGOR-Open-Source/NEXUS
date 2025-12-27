# Protocol Contracts

**Date**: 2025-01-13
**Feature**: TRANSMUTANSTEIN Chat Server Implementation
**Protocol Version**: 68

## Overview

This directory contains protocol message schemas for the NEXUS chat server. All messages use the HoN-compatible format:

```
[2 bytes: Message Length (excludes length field itself)]
[2 bytes: Command Code]
[Variable: Payload data]
```

**String Format**: 2-byte length prefix + UTF-8 string data
**Integer Types**: int8 (1 byte), int16 (2 bytes), int32 (4 bytes), int64 (8 bytes)
**Floats**: float (4 bytes), double (8 bytes)
**Booleans**: int8 (0 = false, 1 = true)

## Directory Structure

- **client/**: Client-to-server and server-to-client commands
- **server/**: Game server commands (server port)
- **manager/**: Server manager commands (manager port)

## Implementation Phases

**Phase 1: Chat Channels** (client/)
- join-channel
- leave-channel
- channel-message
- kick-from-channel

**Phase 2: Matchmaking Groups** (client/)
- group-create
- group-invite
- group-join
- group-leave
- group-ready-status

**Phase 3: Queue & Match Lobby** (client/, server/, manager/)
- group-join-queue
- match-found
- match-lobby-created
- match-start
- server-allocation

**Phase 4: Communication** (client/)
- whisper
- add-friend
- friend-online-notification

## Command Code Reference

Command codes defined in `ChatProtocol.cs`:
- Channels: 0x0500-0x05FF range
- Matchmaking: 0x0600-0x06FF range
- Communication: 0x0400-0x04FF range
- Server: 0x1000-0x10FF range
- Manager: 0x2000-0x20FF range

(Exact command codes defined in TRANSMUTANSTEIN.ChatServer/Core/ChatProtocol.cs)
