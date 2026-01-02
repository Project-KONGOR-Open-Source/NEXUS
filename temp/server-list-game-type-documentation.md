## Server List Response - Server States by Game Type

The `gametype` parameter is a 2-digit code where:
- **First digit** = Match type/server state filter
- **Second digit** = Privacy filter (0=public, 1=private/password, 2=all)

### Game Type First Digit (Server State Filters):

**`0` - All Servers (Browser/Custom Games)**
- `c_state = '2'` (IDLE - lobby state)
- `class = '0'` (custom/unranked games)
- Servers waiting in lobby for players to join

**`1` - Quick Match**
- `c_state = '2'` (IDLE - lobby state)  
- `class = '1'` (quick match type)
- Servers in matchmaking pool waiting for players

**`2` - Ranked/Ladder Match**
- `c_state = '2'` (IDLE - lobby state)
- `class = '2'` (ranked match type)
- Servers for competitive ranked games in lobby

**`3` - Active Games (Spectatable)**
- `c_state = '3'` (ACTIVE - in-game state)
- Any class
- Games currently in progress that can be spectated

**`9` - Zombie Servers (Auto-Join)**
- `c_state = '1'` (SLEEPING state)
- OR `c_state = '0'` (on test servers only)
- Idle servers ready to be woken up for immediate assignment
- Used for rapid auto-join/matchmaking

### Summary Table:

| Game Type | c_state | Server Status | Purpose |
|-----------|---------|---------------|---------|
| 0, 1, 2 | `2` | IDLE (lobby) | Servers waiting for players |
| 3 | `3` | ACTIVE (in-game) | Games in progress (spectating) |
| 9 | `1` | SLEEPING | Dormant servers for instant assignment |

**Key Finding:** For matchmaking/server browser (types 0-2), only include servers in **c_state = 2 (IDLE/lobby)** state. For spectating (type 3), only include **c_state = 3 (ACTIVE/in-game)**. For auto-join (type 9), only include **c_state = 1 (SLEEPING)**.
