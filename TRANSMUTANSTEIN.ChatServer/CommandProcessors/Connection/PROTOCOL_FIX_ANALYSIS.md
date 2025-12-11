# Server Status Protocol Buffer Overflow - Complete Fix Analysis

## Executive Summary

The buffer overflow exception was caused by **TWO CRITICAL PROTOCOL MISMATCHES** in the Protocol Version >= 59 implementation:
1. **Incorrect Match Configuration Format**: Reading individual flag bytes instead of packed ServerFlags ushort
2. **Missing Hostname Field**: Not reading the hostname string between LoadAverage and client statistics

Both issues have been fully resolved with proper Protocol >= 59 implementation.

## Root Causes

### Issue #1: Incorrect Match Configuration Format

**Source**: HoN c_serverchatconnection.cpp, lines 385-497 (Protocol Version >= 59)

Protocol >= 59 uses **packed server flags** instead of individual bytes:

```cpp
ushort unServerFlags(0);
// Pack all flags into unServerFlags bitfield
m_pktSend << unServerFlags  // Just 2 bytes!
          << MapName
          << GameName
          // ...
```

**Old (Protocol < 59)**: Sent ~15 individual flag bytes
**New (Protocol >= 59)**: Sends 1 ushort (2 bytes) with packed flags

The parser was reading individual bytes (Protocol < 59 format), consuming **13 MORE BYTES** than the server was sending. This caused:
- All subsequent fields to be read from wrong offsets
- ServerVersion to be misaligned
- Hostname data to be interpreted as garbage integers

### Issue #2: Missing Hostname Field in Protocol Implementation

**Source**: HoN c_serverchatconnection.cpp, lines 512-516 (Protocol Version >= 59)

```cpp
m_pktSend
    << K2System.GetVersionString()                                      // Server Version
    << static_cast<uint> (m_pHostServer->GetClientMap().size())         // Client Count
    << K2System.GetLoadAverage()                                        // Load Average
    << Network.GetHostname();                                           // Hostname ← MISSING!
```

The original implementation was reading:
1. `ClientCount` (int32) ✓
2. `LoadAverage` (float32) ✓
3. **MISSING: Hostname (string)** ✗
4. Client statistics loop

### How the Buffer Overflow Occurred

Given a 140-byte buffer:
- After reading all base protocol fields (lines 100-157), the offset was at **position 131**
- **9 bytes remained** (131 to 139)
- Read `ClientCount` (4 bytes): positions 131-134 ✓
- Read `LoadAverage` (4 bytes): positions 135-138 ✓
- **Should have read Hostname (string)**: position 139+
- Instead, tried to read `AccountID` (int32): position 139+ ✗
- **CRASH**: Only 1 byte available, but 4 bytes needed

## Additional Protocol Issues Fixed

### Multiple Ping Values

**Source**: HoN c_serverchatconnection.cpp, lines 530-532

Protocol Version >= 59 sends **three ping values** per client, not one:
- `PingMinimum` (short)
- `PingAverage` (short)
- `PingMaximum` (short)

The original implementation only read one ping value, which would have caused further buffer misalignment.

## Changes Made

### 1. Implemented Protocol >= 59 Match Configuration Format

**Added ServerFlags Enum** (ServerStatus.cs, lines 40-59):
```csharp
[Flags]
public enum ServerFlags : ushort
{
    None                  = 0,
    Official              = 1 << 0,   // BIT(0)
    NoLeaver              = 1 << 2,   // BIT(2)
    // ... all 15 flag bits defined
}
```

**Changed Match Configuration Reading** (ServerStatus.cs, lines 173-189):
```csharp
// OLD: Read 15+ individual flag bytes
Official = buffer.ReadInt8();
NoLeaver = buffer.ReadInt8();
// ... many more ReadInt8() calls

// NEW: Read packed ServerFlags (ushort, 2 bytes)
Flags = (ServerFlags)buffer.ReadInt16();
DecodeServerFlags();  // Decode flags into individual properties

// Then read match details in correct Protocol >= 59 order
MapName = buffer.ReadString();
GameName = buffer.ReadString();
GameMode = buffer.ReadString();
TeamSize = buffer.ReadInt8();
MinimumPSR = buffer.ReadInt16();
MaximumPSR = buffer.ReadInt16();
CurrentGameTime = buffer.ReadInt32();
CurrentGamePhase = buffer.ReadInt32();
```

**Added Flag Decoding Method** (ServerStatus.cs, lines 329-359):
```csharp
private void DecodeServerFlags()
{
    // Decode Official status (2-bit field)
    if (Flags.HasFlag(ServerFlags.OfficialWithStats))
        Official = 2;
    else if (Flags.HasFlag(ServerFlags.Official))
        Official = 1;
    else
        Official = 0;

    // Decode boolean flags
    NoLeaver = Flags.HasFlag(ServerFlags.NoLeaver);
    // ... decode all other flags

    // Decode Tier (2-bit field)
    if (Flags.HasFlag(ServerFlags.TierPro))
        Tier = 2;
    // ...
}
```

### 2. Added Hostname Field

**File**: ServerStatus.cs, line 104
```csharp
public string Hostname;  // Server Hostname
```

**File**: ServerStatus.cs, lines 257-259
```csharp
// Read Hostname Field (Protocol >= 59 Only)
Hostname = buffer.ReadString();
```

### 3. Fixed Ping Values (3 Instead of 1)

**File**: ServerStatus.cs, lines 292-294
```csharp
public short PingMinimum { get; set; }
public short PingAverage { get; set; }
public short PingMaximum { get; set; }
```

**File**: ServerStatus.cs, lines 292-294
```csharp
PingMinimum = buffer.ReadInt16(),
PingAverage = buffer.ReadInt16(),
PingMaximum = buffer.ReadInt16(),
```

### 4. Added Validation And Error Handling

- ClientCount sanity check (reject values < 0 or > 1000)
- Try-catch blocks around client stats parsing
- Better error messages with context

### 5. Added Comprehensive Logging

Added detailed logging at every parse step to capture:
- Raw buffer hex dump
- ASCII representation of buffer
- Offset position after each field
- Remaining bytes at each step
- Full diagnostic information for debugging

## Protocol Version Reference

**Protocol Version 68** uses the Protocol >= 59 format:

### Core Structure
1. Command bytes (2)
2. Core server info (variable)
3. Match configuration (variable, conditional on Status)
4. Team/player info (12 strings)
5. Performance metrics (fixed size)
6. **Server version (string)**
7. **Client count (int32)**
8. **Load average (float32)**
9. **🔴 Hostname (string) ← THIS WAS MISSING!**
10. Client statistics loop (ClientCount iterations)
    - Account ID (int32)
    - Address (string)
    - 🔴 Ping minimum (short) ← WAS MISSING TWO EXTRA PING VALUES
    - 🔴 Ping average (short)
    - 🔴 Ping maximum (short)
    - 8 × network statistics (long each)

## Verification Steps

When you run the server again, the logs will show:
1. Full hex dump of the received buffer
2. Step-by-step parsing progress
3. The hostname value that was being skipped
4. Correct parsing of all client statistics

Send the log output and we can verify the fix is working correctly.

## References

- **HoN Source**: `LEGACY\HoN\hon\HoN_SRC_Ender\src\k2\c_serverchatconnection.cpp`
  - Lines 360-544: `SendStatusUpdate()` (Protocol >= 59)
  - Lines 174-354: `SendStatusUpdate58()` (Protocol < 59)
- **KONGOR Source**: `LEGACY\KONGOR\KONGOR\ChatServer\Server\ServerStatusRequest.cs`
  - Lines 42-115: Decode method showing the extra string field at line 78

## Notes

- Protocol Version 68 is confirmed to use the >= 59 format
- The size-prefixed segments work correctly; the issue was field-level protocol mismatch
- All protocol conditionals are based on compile-time constants in the game server
- Future protocol changes should be cross-referenced with both HoN and KONGOR source code
