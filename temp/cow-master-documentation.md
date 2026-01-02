## CoWMaster (Copy-on-Write Master)

### What It Is

**CoWMaster** is a Linux-specific optimization technique used in HON's server architecture to rapidly spawn new game server instances using the OS `fork()` system call. It leverages Linux's Copy-on-Write (CoW) memory management to create new server processes efficiently.

### How It Works

1. **Initial Spawn**: The Server Manager spawns a special "CoW Master" game server instance with the `-cowmaster` flag
   - This master server loads all game data, worlds, and resources into memory
   - Sets `cow_precache` to TRUE to pre-load/precache game worlds
   - Stays in **SERVER_STATUS_IDLE** state, waiting for fork requests
   - Does NOT open a game socket to accept clients

2. **Fork Process**: When a new game server is needed:
   - Server Manager sends `NETCMD_MANAGER_FORK` to the CoW Master
   - CoW Master calls Linux `fork()` system call, creating a child process
   - **Parent process** (CoW Master) returns to IDLE state, waiting for next fork
   - **Child process** becomes the actual game server:
     - Clears the CoWMaster flag
     - Gets assigned unique slave ID, port, and CPU affinity
     - Opens its own game socket on the assigned port
     - Enters SLEEPING state awaiting assignment from Server Manager

3. **Memory Efficiency**: Linux's Copy-on-Write means:
   - Child processes initially share the same physical memory pages as the parent
   - Memory is only duplicated when either process writes to it
   - All pre-loaded game data, textures, models remain shared (read-only)
   - Massive memory savings when running multiple instances

### State Machine

```
COW_DISABLED   → Not using CoW (Windows builds, or disabled)
COW_NEEDSPAWN  → Need to spawn the CoW Master instance
COW_WAIT       → Waiting for CoW Master to finish loading/precaching
COW_READY      → CoW Master ready to fork new instances
```

### Differences from Server Manager

| Aspect | Server Manager | CoW Master |
|--------|---------------|------------|
| **Role** | Orchestration & management | Template for forking |
| **Process Type** | Separate management process | Special game server instance |
| **Purpose** | Monitors, spawns, kills servers | Pre-loaded server ready to clone |
| **Communication** | Commands all servers via sockets | Receives fork requests only |
| **Lifecycle** | Runs entire time | Persistent, forks children |
| **Platform** | Cross-platform | Linux only (`K2_COWMASTER`) |
| **Game Socket** | N/A | Does NOT listen for clients |

**Server Manager responsibilities:**
- Spawns initial servers (including CoW Master)
- Routes game requests to available servers  
- Monitors server health/status
- Handles server crashes and respawns
- Collects statistics and logs
- Manages authentication with master server

**CoW Master responsibilities:**
- Load all game resources once
- Wait in idle state
- Fork child processes on demand
- Report fork success/failure back to manager

### Key Code Locations

- Fork implementation: c_hostserver.cpp
- Fork request handling: c_servermanager.cpp
- State transitions: c_servermanager.cpp

### Benefits

- **Fast spawn times**: ~1 second vs 30+ seconds for cold start
- **Memory efficiency**: Shared read-only pages across all instances
- **CPU efficiency**: No redundant resource loading
- **Scalability**: Can quickly scale to handle demand spikes

This architecture allows HON to efficiently run dozens of game server instances on a single machine while maintaining low memory footprint and near-instant server provisioning.
