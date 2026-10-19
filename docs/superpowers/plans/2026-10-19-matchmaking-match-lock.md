# Matchmaking Match Lock Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A player who leaves a competitive matchmade match in progress cannot play any other game until it ends, and every attempt to play again sends them straight back into it, without ever breaking a reconnect to that match.

**Architecture:** The chat server writes a per-user lock record to Valkey when it announces a Caldavar, MidWars, or RiftWars match. The lock is enforced for at most one hour from that moment, and every end-of-match signal and server removal lifts it sooner. Every trigger re-verifies the lock against the match server's chat status and the engine's own UDP reconnect query, then either lifts it or refuses the attempt and sends the player back with `AUTO_MATCH_CONNECT` reminders. The triggers are the first login report of a login cookie, group create and join, queue, and a refused `c_conn` relayed by the master server over pub/sub. Once the lock refuses other games, `c_conn` issues real game cookies and `aids2cookie` answers as the engine asks, which switches on the engine's duplicate-session guard. A player who joins another game once the lock has lifted, for any reason, receives a new game cookie there, so the original server's guard removes them from the match they left.

**Tech Stack:** C# on .NET with ASP.NET Core and Kestrel TCP endpoints, .NET Aspire, StackExchange.Redis against Valkey, Entity Framework Core, TUnit on the Microsoft Testing Platform, and Testcontainers-backed integration hosts.

**Spec:** `docs/superpowers/specs/2026-10-19-matchmaking-match-lock-design.md`

## Global Constraints

- Competitive matchmade modes only (Caldavar, MidWars, and RiftWars) create a lock, decided from NEXUS's `QueueType`.
- A lock blocks every match type, co-op included, plus public games, custom games, and spectating.
- One lock per user, with guest and staff accounts exempt.
- Another account gets refused, with a message naming the slot account.
- Players already removed from their match are free.
- Never send a player to a dead server, a finished match, or an unreclaimable slot.
- Never lift a lock during a pick phase.
- The triggers are login, group create and join, queue, and any game join, and leaving alone triggers nothing.
- Account-based reconnect is set by a remote command at server handshake, always.
- NEXUS conventions (`claude.md` and `agents.md` on `next`); commits in imperative Start Case, with no body and no attribution to non-humans.
- The match lock is the default behaviour, with no configuration at all: the safety limit is the constant `MatchLock.Lifetime`, one hour from when the lock begins, never renewed, and tests reference it, never a copied value.
- Reconnecting to the match in progress must never break: `c_conn` from the locked match's own server, or from a server whose session NEXUS does not know, is always served, and every task touching `c_conn`, `aids2cookie`, or game cookies carries a test pinning it.
- A player who joins another game has left their match, and the original server's duplicate-session guard removes them, so that their team shares their gold and can use or sell their items. Locks exist to stop that during the first hour. Once a lock has lifted, for any reason (confirmed, unanswered, unreachable, or the safety limit), joining another game issues a new game cookie there, and the guard removes the player at its next account check.
- Reconnecting to one's own match keeps the same game cookie, including across the server's session renewal, so a real reconnect never trips the guard.
- The duplicate-session guard (Tasks 10 and 11) lands only after the lock and its `c_conn` refusal (Tasks 1 to 8), because before the lock it would terminate rage-quitters instead of sending them back.
- HON stays at retail parity: no client or match server change; only the existing `svr_ignoreConnectionIDForReconnect` setting is switched on remotely.
- Work on branch `matchmaking-match-lock`; never push.

## Review Focus

1. A chat reconnect while the player is still in their match (a network blip or a chat server restart) must not message the player or send them back: the login trigger fires once per login cookie (Task 6, `Logging_In_Again_With_The_Same_Session_Cookie_Sends_Nothing`).
2. Repeated attempts within seconds (the engine retries a refused `c_conn`, players click group create twice) must produce one message and one reminder loop (Task 6, `Repeated_Enforcements_Within_The_Return_Window_Message_Once`).
3. A match server NEXUS cannot reach over UDP must not trap anyone: the lock lifts so the attempt proceeds, and the outage is logged at error level once per server. A player who then joins another game has left, and the original server's guard removes them, as intended (Task 3, `Verifying_Against_An_Unanswering_Server_Lifts_The_Lock`; Task 11, `After_A_Lift_Joining_Another_Game_Reports_The_New_Game_Cookie_To_The_Original_Server`).
4. A server that missed an end signal and now hosts a new match must never receive the old match's player (Task 3, `A_Server_Reporting_Another_Match_Has_Ended_The_Match`).
5. A late statistics resubmission for an old match must not lift a newer lock on the same server (Task 1, `Removing_The_Locks_Of_A_Match_Keeps_A_Newer_Match_On_The_Same_Server`).

## Decisions Left To The Builder

**The lock record (Task 1).** `MatchLock` holds the user ID, the slot account's ID and name, the match ID, the correlation ID, the arranged match type as the protocol encodes it, the server's ID, address, and port, and `TimestampStarted`. The correlation ID is the matchup ID the engine echoes in `MATCH_ENDED` and `MATCH_ABORTED`, because `HandleCreateMatch` stores NEXUS's `CREATE_MATCH` correlation ID as `m_uiMatchupID`.
- **The safety limit.** `public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);` is defined once, on `MatchLock`, beside `TimestampExpires => TimestampStarted + Lifetime`, the only place the expiry is set. It is never renewed, and the record's key expires at `TimestampExpires`. Its comment says three things:
  - it is about a long Caldavar match, while MidWars and casual Caldavar average around half that;
  - rage-quitters want a new match right after leaving, so the first minutes matter most, and a match outliving the limit is fine;
  - it is only a backstop, because every trigger re-verifies the lock.
- **Every lift deletes the record**, whether it is confirmed (the slot is gone, the match is over, the server is gone, or an end signal arrives) or not (an unanswered or failed reconnect query, or the safety limit). A player who then joins another game is issued a new game cookie there and is removed from the old match by its guard, as intended.

**The cache records (Tasks 1, 6, and 10).** Keys follow the existing `NAME:["value"]` form; "the session cookie lifetime" is the lifetime `SetAccountNameForSessionCookie` already uses, named in Task 6.

| Key | Value | Expiry |
|---|---|---|
| `MATCH-LOCK:["{userID}"]` | JSON `MatchLock` | the lock's `TimestampExpires` |
| `MATCH-LOCK-SERVER:["{serverID}"]` | set of user IDs locked to that server | reset to the newest lock's expiry on every add |
| `MATCH-LOCK-LOGIN-CHECKED:["{sessionCookie}"]` | marker that a login cookie's login trigger ran | the session cookie's remaining lifetime |
| `ACCOUNT-LATEST-SESSION-COOKIE:["{accountID}"]` | the account's newest login cookie | the session cookie lifetime |
| `ACCOUNT-GAME-COOKIE:["{accountID}"]` | JSON `GameCookie` (cookie and server ID) | the session cookie lifetime, refreshed on each `c_conn` |
| `MATCH-SERVER-ACCOUNTS:["{serverID}"]` | set of account IDs served by `c_conn` for that server | the session cookie lifetime, refreshed on each add |

A lock is written with its server index in one transaction. A lock is removed only by a transaction conditioned on the stored value being the one just read (`Condition.StringEqual`), so a stale release never removes a newer lock. Game cookies are keyed by account and server, not by match, because `c_conn` carries no match ID and a server hosts one match at a time; keying by the stable server ID, not the session cookie, keeps a reconnect's cookie identical when the server renews its session mid-match.

**The reconnect query is feasible from NEXUS (Task 2).** `NETCMD_RECONNECT_INFO_REQUEST` (`0xCC`) is an unauthenticated out-of-band UDP datagram to the match server's game port (`CHostServer::ReadGamePackets`), answered with `NETCMD_RECONNECT_INFO_RESPONSE` (`0x6F`). The request, little-endian, 54 bytes:

| Offset | Bytes | Value |
|---|---|---|
| 0 | 2 | connection ID `0` (the client's server browser socket uses `0`; `GenerateConnectionID` never gives a client `0`, so the server treats the datagram as from a non-client) |
| 2 | 1 | flags `0x01` (`PACKET_NORMAL`, so no sequence number follows) |
| 3 | 1 | `0xCC` |
| 4 | 4 | match ID |
| 8 | 4 | account ID |
| 12 | 2 | connection ID argument `0` (used only by games without a match ID) |
| 14 | 26 | zero padding to the end of the 40-byte watermark area |
| 40 | 14 | the same 14 bytes as offset 0 |

Retail NAEU builds define `K2_NAEU` (`HON.Shell/k2_settings.h`), which defaults `net_enableWatermark` to TRUE (`K2.Engine/c_netdriver_*.cpp`). Such a server skips the first 40 bytes unchecked (`CEnhancedWatermark::handleReceivePacket`) and reads the second copy, while a server with watermarking off reads the first copy; either one processes a single command per non-client datagram. The reply is 48 bytes (40 watermark bytes, connection ID, flags, `0x6F`, and a `uint32`) or 8 bytes without the watermark. The `uint32` is the slot's time remaining in milliseconds (`CGameServer::GetReconnectInfo`). A `SocketException` (an ICMP unreachable surfaces as `ConnectionReset` on Windows and `ConnectionRefused` on Linux) counts as an unanswered attempt.

**Where the query goes.** It goes to the address of the match server's chat connection (mapped to IPv4 when it is an IPv4-mapped IPv6 address) at the lock's game port, because that is the address at which NEXUS demonstrably reaches the server. A registered server whose chat session is missing (it is within the stale host reaper's grace period, so not yet gone) has no chat address left, so its query goes to its registered address, which is also where its players connect.

**Reading the reply with the phase (Task 3).** The phase is the match server's last chat status report (`MatchServerChatSessionMetadata.GamePhase`, raw `EGamePhase`, sent every minute and on every client connection), or unknown without a chat session.

| Reply | Last reported phase | Verdict |
|---|---|---|
| no reply after the retries, or only socket errors | any | lift without confirmation (`LiftSlotUnconfirmed`): the lock is removed, so nobody is trapped, and the server is logged at error level once |
| nonzero | any | send back (a connected player's value is near `uint.MaxValue`, because its termination time is `INVALID_TIME`) |
| `0` | `HERO_LOADING`, `PRE_MATCH`, `ACTIVE`, `POST_MATCH`, or `ENDED` | lift (`LiftSlotGone`): picking phases only ever precede `HERO_LOADING`, so from these phases a `0` can only mean removed or over |
| `0` | unknown, or any other (`IDLE`, `WAITING_FOR_PLAYERS`, `HERO_BAN`, `HERO_SELECT`, or a phase numbered after `ENDED`) | send back: the server answers `0` to everyone while waiting for players and in the pick phases numbered at or above `ENDED`, and a reported `HERO_SELECT` may be up to one status interval stale, so the lock is kept and never lifts during a pick phase |

**The send-back signal (Task 6).** No chat message reaches the chat server when a refused connection ends: the client sends `CHAT_CMD_LEFT_GAME` only after it reported `CHAT_CMD_JOINING_GAME`, which it sends only when loading starts (`CChatManager::LeftGame` and the `CHostClient` connection states), and a refused `c_conn` never reaches loading. So the chat server sends `AUTO_MATCH_CONNECT` at once and re-sends it as a reminder (random integer `-1`) every `ReturnResendInterval`. It stops only when the client's `JOINING_GAME` names the lock's own server, when re-verification no longer gives send-back, when the session ends, or when the return window expires. The client acts on the first reminder it receives while connected to no server, and loading on the match server then reports `JOINING_GAME` with its address. The client's `AC_MATCHMAKING_MATCH_READY` and `AC_MATCHMAKING_MATCH_READY_REMINDER` actions are logged as telemetry only, because acting on a reminder is not reaching the match: the connection can still be refused, for example while the guard has yet to kick the player's other machine.
- **The return window** is the authentication total timeout NEXUS sends in `SendOptions` (the engine's retries of a refused `c_conn`) plus the engine's default duplicate-session interval and timeout (`svr_AccountAuthInterval` and `svr_AccountAuthTimeOut` in `HON.MatchServer/c_gameserver.cpp`, which NEXUS does not change). So a login elsewhere is retried until the guard has removed the old connection.
- **The trade-off:** the player returns up to one resend interval after the refused connection gives up, and the client silently ignores reminders while it is still connecting.

**The login trigger (Task 6).** It runs on `AC_DAILY_LOGINS`, the last step of `CChatManager::HandleAuthAccepted`, once per login cookie: a marker keyed by the session cookie, living as long as the cookie, stops it from running again on the chat reconnects that send the same action. It also skips a session that reported `JOINING_GAME` after the accept, as the client does when it is already on a server.

**The `c_conn` notification (Task 8).** The master server refuses with HTTP 409 and a plain-text reason (never `auth_failure`, never `cookie` set to `Invalid cookie.`), and publishes a `MatchLockRefusal` (user ID and account name) as JSON on the pub/sub channel `MATCH-LOCK-REFUSAL`, mirroring `ACCOUNT-LOGOUT`. The chat server runs the trigger on the account's chat session when there is one, and otherwise verifies the user's lock anyway. That frees a removed player whose chat is down before the engine's next `c_conn` retry.

**The roster (Task 4).** `MatchmakingMatch.CommitRoster()` snapshots the members when `CreateMatch` commits them to the match server, and `MatchAnnounce` locks that snapshot, so a player who dropped from chat between spawn and announce is still locked.

**The messages (Tasks 6 and 7).** Sent as `CHAT_CMD_MESSAGE_ALL` from the sender name `Matchmaking`, defined once in `MatchLockService` and compared by tests through those members:
- the slot account: `You left a match which is still in progress, so you cannot play another game until it ends. Sending you back to it.`
- another account: `Your account "{slot account}" is in a match which is still in progress, so you cannot play another game until it ends. Log in as "{slot account}" to return to it.`
- a group leader whose member is locked: `"{member}" is in a match which is still in progress, so your group cannot search for a match until it ends.`

---

### Task 1: Store Match Locks In The Distributed Cache

**Files:**
- Create: `KONGOR.MasterServer/Models/ServerManagement/MatchLock.cs`
- Create: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs`

**Interfaces:**
- Produces:
  - `public class MatchLock` with:
    - `required` properties `int UserID`, `int AccountID`, `string AccountName`, `int MatchID`, `int CorrelationID`, `MatchType ArrangedMatchType`, `int ServerID`, `string ServerAddress`, `ushort ServerPort`, and `DateTimeOffset TimestampStarted`;
    - `public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);`
    - `public DateTimeOffset TimestampExpires => TimestampStarted + Lifetime;` (computed, not stored).
  - `internal static string ConstructMatchLockKey(int userID)` and `internal static string ConstructMatchLockServerKey(int serverID)`.
  - `public static Task SetMatchLock(this IDatabase distributedCacheStore, MatchLock matchLock)` (the key expires at `TimestampExpires`)
  - `public static Task<MatchLock?> GetMatchLock(this IDatabase distributedCacheStore, int userID)`
  - `public static Task<bool> RemoveMatchLock(this IDatabase distributedCacheStore, int userID, int matchID)` (removes only a lock for that match; returns whether it removed one)
  - `public static Task<int> RemoveMatchLocksForServer(this IDatabase distributedCacheStore, int serverID, Func<MatchLock, bool>? predicate = null)` (removes the server's locks that match the predicate, or all of them; returns the number removed)

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockStoreTests (KONGORIntegrationWebApplicationFactory with the SQL Server and distributed cache containers, as the other master server tests start it)
[Test]
public async Task A_Match_Lock_Is_Stored_Per_User_Until_One_Lifetime_After_It_Began()
{
    MatchLock matchLock = BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow);
    await DistributedCache.SetMatchLock(matchLock);

    MatchLock? stored = await DistributedCache.GetMatchLock(7);
    TimeSpan? timeToLive = await DistributedCache.KeyTimeToLiveAsync(DistributedCacheExtensions.ConstructMatchLockKey(7));

    await Assert.That(stored?.MatchID).IsEqualTo(100);
    await Assert.That(stored?.CorrelationID).IsEqualTo(matchLock.CorrelationID);
    await Assert.That(timeToLive > MatchLock.Lifetime - TimeSpan.FromMinutes(1) && timeToLive <= MatchLock.Lifetime).IsTrue();
}

[Test] public async Task A_Match_Lock_Lifts_At_The_Safety_Limit()
    // a lock started MatchLock.Lifetime minus two seconds ago: present at once; GetMatchLock(7) is NULL within five seconds
[Test] public async Task A_Lock_Already_Past_The_Safety_Limit_Is_Not_Stored()           // started MatchLock.Lifetime plus a minute ago: GetMatchLock(7) is NULL
[Test] public async Task A_Second_Lock_For_The_Same_User_Replaces_The_First()          // GetMatchLock(7).MatchID == 101
[Test] public async Task Removing_A_Lock_For_Another_Match_Keeps_The_Current_Lock()     // RemoveMatchLock(7, 100) is FALSE while the lock is for 101; GetMatchLock(7) is not NULL
[Test] public async Task Removing_The_Locks_Of_A_Server_Removes_Only_The_Locks_On_That_Server()
    // users 7 and 8 on server 1, user 9 on server 2: RemoveMatchLocksForServer(1) == 2; 7 and 8 are NULL; 9 remains
[Test] public async Task Removing_The_Locks_Of_A_Match_Keeps_A_Newer_Match_On_The_Same_Server()
    // user 7 locked to match 100 on server 1, then user 8 to match 101 on server 1:
    // RemoveMatchLocksForServer(1, matchLock => matchLock.MatchID == 100) == 1; GetMatchLock(8).MatchID == 101
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/MatchLockStoreTests/*"`
Expected: FAIL to compile, because `MatchLock` and the cache methods do not exist.

- [ ] **Step 3: Implement the record and the cache extensions**

`SetMatchLock` skips a lock already past `TimestampExpires`, and writes the lock and its server index in one `ITransaction`, both expiring at `TimestampExpires`. `RemoveMatchLock` reads the raw value and acts only when it is present and for the given match, in a transaction conditioned on `Condition.StringEqual` with that raw value. `RemoveMatchLocksForServer` walks the server index: it drops members whose lock is gone or now on another server, and removes the rest that satisfy the predicate, as `RemoveMatchLock` does.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/MatchLock.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs
git commit -m "Store Match Locks In The Distributed Cache"
```

---

### Task 2: Query Match Servers For Reconnectable Slots

**Files:**
- Create: `TRANSMUTANSTEIN.ChatServer/Services/MatchServerReconnectQuery.cs`
- Modify: `TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs` (register as a singleton)
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/FakeMatchServerGameSocket.cs`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs` (add `global using System.Collections.Concurrent;`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchServerReconnectQueryTests.cs`

**Interfaces:**
- Produces:
  - `public class MatchServerReconnectQuery`
  - `internal const int RequestAttempts = 3;` and `internal static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(1);`
  - `internal static byte[] BuildRequest(int matchID, int accountID)`
  - `internal static uint? ParseResponse(ReadOnlySpan<byte> datagram)`
  - `public async Task<uint?> QuerySlotTimeRemaining(IPEndPoint gameEndPoint, int matchID, int accountID, CancellationToken cancellationToken = default)` (NULL when no attempt is answered; never throws a `SocketException`)
  - Test double: `internal sealed class FakeMatchServerGameSocket : IAsyncDisposable` with `static FakeMatchServerGameSocket Start(uint slotTimeRemaining)`, `int Port`, `uint SlotTimeRemaining { get; set; }`, `bool Answers { get; set; }`, `int IgnoredRequestsRemaining { get; set; }`, and `ConcurrentQueue<(int MatchID, int AccountID)> Requests`. It binds `127.0.0.1` on an ephemeral port, reads the copy at offset 40 as a watermarking server does, and answers with the 48-byte layout.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task The_Request_Carries_The_Query_Before_And_After_The_Watermark_Area()
{
    byte[] request = MatchServerReconnectQuery.BuildRequest(matchID: 0x01020304, accountID: 0x0A0B0C0D);
    byte[] query = [0x00, 0x00, 0x01, 0xCC, 0x04, 0x03, 0x02, 0x01, 0x0D, 0x0C, 0x0B, 0x0A, 0x00, 0x00];

    await Assert.That(request.Length).IsEqualTo(54);
    await Assert.That(request[..14]).IsEquivalentTo(query);
    await Assert.That(request[14..40].All(value => value is 0)).IsTrue();
    await Assert.That(request[40..]).IsEquivalentTo(query);
}

[Test] [Arguments(0u)] [Arguments(240000u)] [Arguments(uint.MaxValue - 5000u)]
public async Task A_Watermarked_Response_Is_Read(uint slotTimeRemaining)        // 48 bytes, [43] = 0x6F, [44..48] = value
[Test] public async Task An_Unwatermarked_Response_Is_Read()                     // 8 bytes, [3] = 0x6F
[Test] [Arguments(48, 0x66)] [Arguments(8, 0x66)] [Arguments(47, 0x6F)] [Arguments(9, 0x6F)]
public async Task A_Datagram_Which_Is_Not_A_Reconnect_Response_Is_Ignored(int length, int command)   // ParseResponse is NULL
[Test] public async Task The_Query_Returns_The_Time_The_Match_Server_Reports()
    // FakeMatchServerGameSocket.Start(123456); result == 123456u; Requests contains (matchID, accountID)
[Test] public async Task The_Query_Retries_A_Lost_Request()
    // IgnoredRequestsRemaining = MatchServerReconnectQuery.RequestAttempts - 1; result == the configured value
[Test] public async Task The_Query_Returns_Null_When_The_Match_Server_Never_Answers()
    // Answers = FALSE: result is NULL; Requests.Count == MatchServerReconnectQuery.RequestAttempts
[Test] public async Task The_Query_Returns_Null_Rather_Than_Throwing_For_A_Closed_Port()
    // bind a UdpClient on 127.0.0.1:0, read its port, dispose it, query that port: result is NULL and no exception escapes
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchServerReconnectQueryTests/*"`
Expected: FAIL to compile, because `MatchServerReconnectQuery` does not exist.

- [ ] **Step 3: Implement the query**

Use one `UdpClient` of the end point's address family per query, `Connect` it to the end point so that only that end point's datagrams are received, and for each of `RequestAttempts` send `BuildRequest` and await `ReceiveAsync` under a linked `AttemptTimeout` cancellation. Return the first datagram `ParseResponse` accepts. Treat a timeout or any `SocketException` from the send or the receive as an unanswered attempt, and move on to the next attempt. `ParseResponse` accepts exactly 48 bytes with `0x6F` at offset 43, or exactly 8 bytes with `0x6F` at offset 3, and reads the following little-endian `uint32`. The class summary carries the wire format and why the request holds two copies, naming `CHostServer::ReadGamePackets` and `CEnhancedWatermark::handleReceivePacket`. The query does not log unreachability; `MatchLockService` does, once per server (Task 3).

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Services/MatchServerReconnectQuery.cs TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/FakeMatchServerGameSocket.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchServerReconnectQueryTests.cs
git commit -m "Query Match Servers For Reconnectable Slots"
```

---

### Task 3: Verify Match Locks Before Sending Players Back

**Files:**
- Create: `ASPIRE.Common/Enumerations/Match/GamePhase.cs`
- Create: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockVerdict.cs`
- Create: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs`
- Modify: `TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs` (register `MatchLockService` as a singleton)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs:63-81` (optional `port` and `ipAddress` on `SeedMatchServer`; add `GetUserID` and `SeedLockedMatch`)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatServerHost.cs` (add `ConnectAndAuthenticateMatchServerAsync`)
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/RecordingLogger.cs`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs` (add `global using ASPIRE.Common.Enumerations.Match;`) and `Internals/TypeAliases.cs` (add `global using MatchType = ASPIRE.Common.Enumerations.Match.MatchType;`, as the chat server does, because `System.IO.MatchType` would otherwise be ambiguous)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs`

**Interfaces:**
- Consumes: Task 1's `MatchLock` and `RemoveMatchLock`; Task 2's `QuerySlotTimeRemaining` and `FakeMatchServerGameSocket`.
- Produces:
  - `public enum GamePhase` with the engine's exact values: `GAME_PHASE_INVALID = -1, GAME_PHASE_IDLE = 0, GAME_PHASE_WAITING_FOR_PLAYERS, GAME_PHASE_HERO_BAN, GAME_PHASE_HERO_SELECT, GAME_PHASE_HERO_LOADING, GAME_PHASE_PRE_MATCH, GAME_PHASE_ACTIVE, GAME_PHASE_POST_MATCH, GAME_PHASE_ENDED, GAME_PHASE_HERO_BLIND_BAN, GAME_PHASE_HERO_LOCK, GAME_PHASE_HERO_LOCK_PICK, GAME_PHASE_SHUFFLE_PICK, GAME_PHASE_HERO_VOTE_BLIND_BAN, GAME_PHASE_CUSTOM, GAME_PHASE_WAIT` (summary names `HON.Shared/game_shared_types.h`).
  - `public enum MatchLockVerdict { SendBack, LiftServerGone, LiftMatchOver, LiftSlotGone, LiftSlotUnconfirmed }`
  - `public class MatchLockService(IDatabase distributedCacheStore, MatchServerReconnectQuery reconnectQuery, ILogger<MatchLockService> logger)`
  - `internal static MatchLockVerdict? EvaluateServer(MatchLock matchLock, MatchServer? registeredServer, MatchServerChatSessionMetadata? serverSessionMetadata, DateTimeOffset now)` (NULL means the server does not rule the lock out)
  - `internal static MatchLockVerdict EvaluateSlot(GamePhase? lastReportedGamePhase, uint? slotTimeRemaining)` (NULL phase means unknown)
  - `public async Task<MatchLockVerdict> Verify(MatchLock matchLock)`. Every verdict but `SendBack` removes the lock with `RemoveMatchLock`. Each outcome is logged with its reason; `LiftSlotUnconfirmed` is kept apart from the confirmed lifts for that log.
  - Test helpers:
    - `ChatServerHost.ConnectAndAuthenticateMatchServerAsync(int serverID, string cookie)` returns the `TcpClient` once `Context.MatchServerChatSessions` holds the server.
    - `ChatTestData.GetUserID(IServiceProvider services, int accountID)` reads the account's user ID from the host's `MerrickContext`.
    - `ChatTestData.WaitUntilLockIsGone(ChatServerHost host, int userID, TimeSpan timeout)` polls `GetMatchLock` and returns whether the lock disappeared in time.
    - `internal sealed class RecordingLogger<TCategory> : ILogger<TCategory>` in `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/RecordingLogger.cs`, recording each entry's `LogLevel` and formatted message in a `ConcurrentQueue`, so a test can construct `MatchLockService` directly (as other service tests do with `NullLogger`) and count its error entries.
    - `ChatTestData.SeedLockedMatch(ChatServerHost host, int userID, int accountID, string accountName, FakeMatchServerGameSocket gameSocket, GamePhase gamePhase = GamePhase.GAME_PHASE_ACTIVE, string registeredAddress = "127.0.0.1", bool withChatSession = true)` returns a `LockedMatch : IAsyncDisposable` with `MatchLock Lock` and `TcpClient? MatchServerConnection`. It seeds a `ServerHost` account with `SeedAccount`, and a match server at `registeredAddress` and the game socket's port with `SeedMatchServer`. When `withChatSession` holds, it connects with `ConnectAndAuthenticateMatchServerAsync` and then sets `Context.MatchServerChatSessions[serverID].Metadata.Status = ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE`, `.GamePhase = (int) gamePhase`, and `.MatchID = matchID`. It writes the lock with `SetMatchLock`: random match and correlation IDs, `ServerAddress = registeredAddress`, and `TimestampStarted = now`. On disposal it removes the lock and closes the connection, because `ChatServerHost` databases restart their identifiers while the cache is shared and unprefixed.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] [Arguments(GamePhase.GAME_PHASE_HERO_SELECT, 1u)] [Arguments(GamePhase.GAME_PHASE_ACTIVE, 240000u)] [Arguments(GamePhase.GAME_PHASE_ACTIVE, uint.MaxValue - 1u)]
public async Task A_Slot_With_Time_Remaining_Is_Reclaimable(GamePhase gamePhase, uint slotTimeRemaining)
    => await Assert.That(MatchLockService.EvaluateSlot(gamePhase, slotTimeRemaining)).IsEqualTo(MatchLockVerdict.SendBack);

[Test] [Arguments(GamePhase.GAME_PHASE_HERO_LOADING)] [Arguments(GamePhase.GAME_PHASE_PRE_MATCH)] [Arguments(GamePhase.GAME_PHASE_ACTIVE)] [Arguments(GamePhase.GAME_PHASE_POST_MATCH)] [Arguments(GamePhase.GAME_PHASE_ENDED)]
public async Task No_Time_Remaining_Lifts_The_Lock_Once_No_Pick_Phase_Can_Follow(GamePhase gamePhase)      // LiftSlotGone

[Test] [Arguments(GamePhase.GAME_PHASE_IDLE)] [Arguments(GamePhase.GAME_PHASE_WAITING_FOR_PLAYERS)] [Arguments(GamePhase.GAME_PHASE_HERO_BAN)] [Arguments(GamePhase.GAME_PHASE_HERO_SELECT)]
[Arguments(GamePhase.GAME_PHASE_HERO_BLIND_BAN)] [Arguments(GamePhase.GAME_PHASE_HERO_LOCK)] [Arguments(GamePhase.GAME_PHASE_HERO_LOCK_PICK)] [Arguments(GamePhase.GAME_PHASE_SHUFFLE_PICK)]
[Arguments(GamePhase.GAME_PHASE_HERO_VOTE_BLIND_BAN)] [Arguments(GamePhase.GAME_PHASE_CUSTOM)] [Arguments(GamePhase.GAME_PHASE_WAIT)] [Arguments(null)]
public async Task No_Time_Remaining_Never_Lifts_The_Lock_Before_Or_During_A_Pick_Phase(GamePhase? gamePhase)  // SendBack, including an unknown phase

[Test] public async Task An_Unanswered_Query_Lifts_The_Lock_Without_Confirmation()      // EvaluateSlot(GAME_PHASE_ACTIVE, null) == LiftSlotUnconfirmed
[Test] public async Task A_Server_Which_Is_Not_Registered_Is_Gone()                     // registeredServer NULL: LiftServerGone
[Test] public async Task A_Registered_Server_Without_A_Chat_Session_Does_Not_Rule_The_Lock_Out()   // metadata NULL: NULL
[Test] public async Task A_Server_Whose_Status_Is_Stale_Is_Gone()
    // LastStatusUpdate = now - StaleHostReaper.MatchServerSessionStaleStateThreshold - one second: LiftServerGone
[Test] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_IDLE)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_SLEEPING)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_CRASHED)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_KILLED)]
public async Task A_Server_Without_A_Match_Has_Ended_The_Match(ChatProtocol.ServerStatus status)    // LiftMatchOver
[Test] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_LOADING)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN)]
public async Task A_Server_Reporting_Another_Match_Has_Ended_The_Match(ChatProtocol.ServerStatus status)    // MatchID = matchLock.MatchID + 1: LiftMatchOver
[Test] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE, true)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_LOADING, false)] [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN, false)]
public async Task A_Live_Server_Hosting_The_Match_Does_Not_Rule_The_Lock_Out(ChatProtocol.ServerStatus status, bool reportsTheMatch)   // NULL (MatchID -1 when not reported)

// Integration ([NotInParallel(nameof(ChatServerHost))])
[Test] public async Task Verifying_A_Held_Slot_Queries_The_Chat_Connection_Address_Rather_Than_The_Registered_One()
    // registeredAddress "192.0.2.1" (unroutable), game socket answers 120000: Verify == SendBack; the lock remains; gameSocket.Requests contains (Lock.MatchID, Lock.AccountID)
[Test] public async Task Verifying_A_Lost_Slot_Lifts_The_Lock()                   // answers 0 in GAME_PHASE_ACTIVE: LiftSlotGone; GetMatchLock is NULL
[Test] public async Task Verifying_Against_An_Unanswering_Server_Lifts_The_Lock()  // Answers = FALSE: LiftSlotUnconfirmed; GetMatchLock is NULL
[Test] public async Task An_Unreachable_Server_Is_Logged_As_An_Error_Once()
    // MatchLockService built with the host's IDatabase, a new MatchServerReconnectQuery, and a RecordingLogger; Answers = FALSE;
    // Verify, store the same lock again, Verify again: exactly one Error entry, naming the game socket's address and port
[Test] public async Task Verifying_A_Registered_Server_Without_A_Chat_Session_Queries_Its_Registered_Address_And_Keeps_The_Lock()
    // withChatSession FALSE, registeredAddress "127.0.0.1", answers 0: SendBack (unknown phase); the lock remains; the game socket received the request
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockVerificationTests/*"`
Expected: FAIL to compile, because `MatchLockService`, `MatchLockVerdict`, and `GamePhase` do not exist.

- [ ] **Step 3: Implement the verification**

`EvaluateServer` works through these checks in order:
1. No registered server gives `LiftServerGone`.
2. Without session metadata (the reaper has yet to decide), it gives NULL.
3. `StaleHostReaper.SessionIsStale(metadata.LastStatusUpdate, now, StaleHostReaper.MatchServerSessionStaleStateThreshold)` gives `LiftServerGone`.
4. A positive reported match ID other than the lock's gives `LiftMatchOver`, whatever the status, because the engine reports the new match ID in the status it sends before the announcement.
5. A status of `IDLE`, `SLEEPING`, `CRASHED`, or `KILLED` gives `LiftMatchOver`.
6. Anything else gives NULL.

`EvaluateSlot` implements the reading table in Decisions. `Verify` reads the server with `GetMatchServerByID(matchLock.ServerID)` and the session from `Context.MatchServerChatSessions`. It builds the end point as Decisions describes: the chat connection's address when there is a session, and otherwise the registered server's `IPAddress`, never `matchLock.ServerAddress`, so the two cannot diverge. It queries only when `EvaluateServer` returns NULL, and passes the reported phase, or NULL without a session, to `EvaluateSlot`. When a server leaves a query unanswered, `Verify` logs at error level once per server ID (a `ConcurrentDictionary` of reported IDs, cleared for a server when it next answers), naming the server and the address and port tried. None of this is configurable.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add ASPIRE.Common/Enumerations/Match/GamePhase.cs TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockVerdict.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/RecordingLogger.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatServerHost.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/TypeAliases.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs
git commit -m "Verify Match Locks Before Sending Players Back"
```

---

### Task 4: Lock Players Into Competitive Matchmade Matches

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingMatch.cs` (roster snapshot and queue type)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchmakingService.cs:299` (use `match.QueueType`) and `:417` (commit the roster before `SendCreateMatch`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (add `LockMatchRoster`)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs` (inject `MatchLockService`; lock at the end of `Process`)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `BuildAnnounceMatch(int correlationID, int matchID)` in `MatchAnnounceRequestData`'s layout)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Utilities/Matchmaking/MatchmakingTestBuilder.cs` (add `BuildGroup(IReadOnlyList<ClientChatSession> sessions, MatchmakingGroupInformation information)`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockCreationTests.cs`

**Interfaces:**
- Consumes: Task 1's `SetMatchLock`; Task 3's `MatchLockService` and `ConnectAndAuthenticateMatchServerAsync`.
- Produces:
  - `public IReadOnlyList<MatchmakingGroupMember> CommittedRoster { get; private set; } = [];` and `public void CommitRoster()` on `MatchmakingMatch`.
  - `public QueueType QueueType` on `MatchmakingMatch` (the expression `RecordQueueDuration` uses today).
  - `public async Task LockMatchRoster(MatchmakingMatch match, int matchID, int serverID, string serverAddress, ushort serverPort)`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task Committing_The_Roster_Keeps_Players_Who_Later_Leave_Their_Group()
{
    MatchmakingMatch match = BuildCompetitiveMatch(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL);
    match.CommitRoster();

    match.LegionTeam.Groups.Single().Members.Clear();

    await Assert.That(match.CommittedRoster.Count).IsEqualTo(2);
}

[Test] [NotInParallel(nameof(ChatServerHost))]
[Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL, MatchType.AM_MATCHMAKING_CAMPAIGN)]
[Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS, MatchType.AM_MATCHMAKING_MIDWARS)]
[Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS, MatchType.AM_MATCHMAKING_RIFTWARS)]
public async Task Locking_A_Competitive_Match_Locks_Every_Committed_Player(ChatProtocol.TMMGameType gameType, MatchType arrangedMatchType)
    // for each committed member (users given unique random IDs): the lock has MatchID, CorrelationID == match.CorrelationID, ServerID, ServerAddress, ServerPort,
    // AccountID, AccountName, ArrangedMatchType == arrangedMatchType, and TimestampStarted within five seconds of the call;
    // the locks are removed in a finally block
[Test] [NotInParallel(nameof(ChatServerHost))] public async Task Locking_A_Co_Op_Match_Locks_Nobody()            // MatchmakingMatch.FromBotGroup
[Test] [NotInParallel(nameof(ChatServerHost))] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task Guest_And_Staff_Players_Are_Never_Locked(AccountType accountType)
[Test] [NotInParallel(nameof(ChatServerHost))] public async Task Announcing_A_Match_Locks_Its_Committed_Roster_Including_A_Player_Who_Left()
    // two authenticated clients in solo groups; the match is committed and added to MatchmakingService.ActiveMatches with the handshaked server's ID, address, and port;
    // one member is removed from its group; NET_CHAT_GS_ANNOUNCE_MATCH is sent: both users are locked to the announced match ID, the match's correlation ID,
    // and the announcing server's ID
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockCreationTests/*"`
Expected: FAIL to compile, because `CommitRoster` and `LockMatchRoster` do not exist.

- [ ] **Step 3: Implement the roster snapshot and lock creation**

`CommitRoster` copies `GetAllPlayers()` into a new list. `LockMatchRoster` returns at once unless `match.QueueType` is `Caldavar`, `MidWars`, or `RiftWars`. It then writes one lock per committed member whose account type is neither `Guest` nor `Staff`, with:
- `UserID = member.Account.User.ID`;
- `CorrelationID = match.CorrelationID`;
- `ArrangedMatchType = match.ArrangedMatchType`;
- `TimestampStarted = now`.

`MatchAnnounce` calls it last, after marking members in game, with `session.Metadata.ServerID` and the address and port it sent in `AUTO_MATCH_CONNECT`, so a failure cannot disturb the announcement.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/QueueDurationTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingMatch.cs TRANSMUTANSTEIN.ChatServer/Services/MatchmakingService.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Utilities/Matchmaking/MatchmakingTestBuilder.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockCreationTests.cs
git commit -m "Lock Players Into Competitive Matchmade Matches"
```

---

### Task 5: Lift Match Locks When Matches Or Servers End

**Files:**
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Matchmaking.cs:176-195` (`RemoveMatchServerByID` lifts the server's locks first, unconditionally)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchComplete.cs` (inject `IDatabase`; rename `MatchCompleteRequestData.MatchID` to `MatchupID`, documented as the correlation ID NEXUS sent in `NET_CHAT_GS_CREATE_MATCH`, and log it as such; lift the ended matchup's locks)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAbandoned.cs`, `MatchAborted.cs` (lift every lock on the server)
- Modify: `KONGOR.MasterServer/Controllers/StatisticsRequester/StatisticsRequesterController.Submit.cs` (both handlers, after `SaveChangesAsync`)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `BuildMatchEnded(int matchupID)`, `BuildAbandonMatch(bool failed)`, and `BuildMatchAborted(int matchupID)` in their request data layouts)
- Create: `ASPIRE.Tests.KONGOR.MasterServer/Helpers/StatisticsSubmissionTestsHelper.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockReleaseTests.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/Statistics/StatisticsSubmissionMatchLockTests.cs`

**Interfaces:**
- Consumes: Task 1's `RemoveMatchLocksForServer`; Task 3's `SeedLockedMatch`.
- Produces: `StatisticsSubmissionTestsHelper.BuildSubmission(string session, int serverID, int matchID)`, which returns the minimal `submit_stats` form for a match without player statistics, derived from `StatsForSubmissionRequestForm`.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockReleaseTests ([NotInParallel(nameof(ChatServerHost))])
[Test]
public async Task Match_Ended_Lifts_The_Locks_Of_That_Matchup()
{
    // await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(...); the lock's MatchID and CorrelationID differ
    await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildMatchEnded(matchupID: lockedMatch.Lock.CorrelationID));

    bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, TimeSpan.FromSeconds(10));

    await Assert.That(lifted).IsTrue();
}

[Test] public async Task Match_Ended_For_The_Match_ID_Instead_Of_The_Matchup_Keeps_The_Locks()   // BuildMatchEnded(Lock.MatchID): the lock remains
[Test] public async Task Match_Ended_For_Another_Matchup_Keeps_The_Locks()                 // BuildMatchEnded(Lock.CorrelationID + 1): the lock remains
[Test] public async Task Match_Abandoned_Lifts_Every_Lock_On_The_Server()
[Test] public async Task Match_Aborted_Lifts_Every_Lock_On_The_Server()
[Test] public async Task Terminating_A_Match_Server_Lifts_The_Locks_On_It()                // session.Terminate(distributedCacheStore), as the reaper does

// MatchLockStoreTests
[Test] public async Task Removing_A_Match_Server_Lifts_The_Locks_On_It()          // SetMatchServer, SetMatchLock, RemoveMatchServerByID: GetMatchLock is NULL

// StatisticsSubmissionMatchLockTests
[Test] public async Task Submitting_Match_Statistics_Lifts_The_Locks_Of_That_Match_And_Keeps_Others()
    // locks for match 100 and match 101 on the submitting server; submit match 100: the first is NULL, the second remains
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockReleaseTests/*"` and `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/StatisticsSubmissionMatchLockTests/*"`
Expected: FAIL, because the locks remain after their matches end.

- [ ] **Step 3: Implement the releases**

The release calls:
- `MatchComplete`: `RemoveMatchLocksForServer(session.Metadata.ServerID, matchLock => matchLock.CorrelationID == requestData.MatchupID)`.
- `MatchAbandoned` and `MatchAborted`: `RemoveMatchLocksForServer(session.Metadata.ServerID)`.
- The submission handler: `RemoveMatchLocksForServer(matchServer.ID, matchLock => matchLock.MatchID == form.MatchStats.MatchID)`.
- The resubmission handler: the same, with `form.ServerID.Value`.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the commands from Step 2, then `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/MatchLockStoreTests/*"` and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/StaleHostReaperTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Matchmaking.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchComplete.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAbandoned.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAborted.cs KONGOR.MasterServer/Controllers/StatisticsRequester/StatisticsRequesterController.Submit.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockReleaseTests.cs ASPIRE.Tests.KONGOR.MasterServer/Helpers/StatisticsSubmissionTestsHelper.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/Statistics/StatisticsSubmissionMatchLockTests.cs
git commit -m "Lift Match Locks When Matches Or Servers End"
```

---

### Task 6: Send Locked Players Back To Their Match At Login

**Files:**
- Create: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockReturn.cs`
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs` (pending return, game report flag, two send helpers, and cancellation in `CleanUpSession`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.MatchServer.cs:55-62` (name the three authentication option values as constants and write them from there)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs` (the login marker)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs:44` (name the session cookie lifetime, so that the login marker and Task 10's records share it)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (enforcement, login, the return loop, messages, and timings)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Actions/TrackPlayerAction.cs` (become asynchronous; route `AC_DAILY_LOGINS` and log both ready actions)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/JoiningMatch.cs` (set the flag, end the return when the address names the lock's server, and remove the KONGOR reconnect TODO block, which this feature replaces and which the design rejects as too late)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/LeftMatch.cs` (clear the flag only)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs:73-94` (send through `SendAutoMatchConnect`)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `BuildTrackPlayerAction`, `BuildJoiningGame`, `BuildLeftGame`, `ReadAutoMatchConnect`, and `ReadNotice` returning the parsed fields or NULL on timeout)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`

**Interfaces:**
- Consumes: Task 3's `Verify`, `SeedLockedMatch`, `GetUserID`, and `FakeMatchServerGameSocket`; Task 1's `GetMatchLock`.
- Produces:
  - `internal static readonly TimeSpan SessionCookieLifetime` in `DistributedCacheExtensions`, used by `SetAccountNameForSessionCookie`.
  - `public sealed class MatchLockReturn(int matchID, DateTimeOffset expires)` with `int MatchID`, `DateTimeOffset Expires`, `DateTimeOffset LastVerified { get; set; }`, `DateTimeOffset LastNotified { get; set; }`, `CancellationTokenSource Cancellation`, and `void Cancel()`.
  - On `ClientChatSession`:
    - `public MatchLockReturn? PendingMatchLockReturn` (a `Volatile.Read` of a private field);
    - `public bool TryReplacePendingMatchLockReturn(MatchLockReturn? expected, MatchLockReturn? replacement)` (`Interlocked.CompareExchange`; cancels `expected` when it is replaced);
    - `public bool ReportedJoiningGame { get; set; }`;
    - `public ClientChatSession SendAutoMatchConnect(MatchType arrangedMatchType, int matchID, string serverAddress, ushort serverPort, bool reminder)`;
    - `public ClientChatSession SendNotice(string senderName, string message)`.
  - On `MatchServerChatSession`: `internal const int AuthenticationRequestAttempts`, `AuthenticationRequestTotalTimeoutMilliseconds`, and `AuthenticationRequestIntervalMilliseconds`.
  - `public static Task<bool> MarkMatchLockLoginChecked(this IDatabase distributedCacheStore, string sessionCookie)` (TRUE only the first time for a cookie; `When.NotExists`, expiring with the session cookie's remaining time to live, or `SessionCookieLifetime` when that is unknown).
  - On `MatchLockService`:
    - message members: `internal const string MessageSenderName = "Matchmaking";`, `internal const string SlotAccountMessage`, and `internal static string OtherAccountMessage(string slotAccountName)`;
    - timings: `internal static readonly TimeSpan ReturnResendInterval = TimeSpan.FromSeconds(5);`, `internal static readonly TimeSpan EngineAccountAuthenticationInterval = TimeSpan.FromMinutes(2);` and `EngineAccountAuthenticationTimeout = TimeSpan.FromSeconds(15);` (the engine's `svr_AccountAuthInterval` and `svr_AccountAuthTimeOut` defaults), and `internal static readonly TimeSpan ReturnWindow` (the authentication total timeout plus those two);
    - triggers: `public async Task<bool> Enforce(ClientChatSession session)` (TRUE when the attempt must be refused), `public async Task EnforceAtLogin(ClientChatSession session)`, `public async Task EndReturnOnJoiningGame(ClientChatSession session, string serverAddress)`, and `public void RecordReturnAcknowledgement(ClientChatSession session, ChatProtocol.ActionCampaign action)`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task Logging_In_While_Locked_Sends_The_Player_Back_To_Their_Match()
{
    // authenticated client; userID = await ChatTestData.GetUserID(host.Services, accountID);
    // await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);
    await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

    (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, TimeSpan.FromSeconds(10));
    (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, TimeSpan.FromSeconds(10));

    await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
    await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
    await Assert.That(connect?.ArrangedMatchType).IsEqualTo((byte) lockedMatch.Lock.ArrangedMatchType);
    await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
    await Assert.That(connect?.Address).IsEqualTo(lockedMatch.Lock.ServerAddress);
    await Assert.That((ushort?) connect?.Port).IsEqualTo(lockedMatch.Lock.ServerPort);
    await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
}

[Test] public async Task Logging_In_On_Another_Account_Of_A_Locked_User_Names_The_Slot_Account_And_Sends_Nothing_Back()
    // lock with the session's user ID but another account ID and name: notice == OtherAccountMessage(lock.AccountName); no AUTO_MATCH_CONNECT within two seconds
[Test] public async Task Logging_In_Again_With_The_Same_Session_Cookie_Sends_Nothing()
    // after the first send-back, a second chat connection with the same cookie sends AC_DAILY_LOGINS: neither frame within two seconds
[Test] public async Task Logging_In_While_Reporting_A_Game_Sends_Nothing()                // JOINING_GAME, then AC_DAILY_LOGINS: neither frame within two seconds
[Test] public async Task An_Unanswered_Return_Is_Repeated_As_A_Reminder()                 // a second AUTO_MATCH_CONNECT within ReturnResendInterval plus two seconds, RandomInteger == -1
[Test] public async Task An_Acknowledgement_Alone_Keeps_The_Reminders()                   // AC_MATCHMAKING_MATCH_READY after the first: a reminder still arrives
[Test] public async Task Joining_The_Locked_Server_Stops_The_Reminders()
    // JOINING_GAME with $"{Lock.ServerAddress}:{Lock.ServerPort}" after the first: none within twice ReturnResendInterval
[Test] public async Task Joining_Another_Server_Keeps_The_Reminders()                     // JOINING_GAME with another address: a reminder still arrives
[Test] public async Task A_Lifted_Lock_Stops_The_Reminders()                              // the game socket answers 0 in GAME_PHASE_ACTIVE after the first: no reminder; the lock is gone
[Test] public async Task Repeated_Enforcements_Within_The_Return_Window_Message_Once()
    // two direct MatchLockService.Enforce calls on the session: both TRUE; exactly one notice within ReturnResendInterval; one game socket request (the second skips Verify)
[Test] [Arguments(true)] [Arguments(false)]
public async Task An_Attempt_After_The_Resend_Interval_Gets_The_Notice_Again(bool slotAccount)
    // Enforce, wait ReturnResendInterval plus a second, Enforce again: both TRUE; a second notice (SlotAccountMessage or OtherAccountMessage) arrives;
    // for another account still no AUTO_MATCH_CONNECT, and for the slot account only the one reminder loop's frames
[Test] public async Task Leaving_The_Match_Alone_Triggers_Nothing()                        // LEFT_GAME: neither frame within two seconds; the lock remains
[Test] public async Task A_Lock_Whose_Slot_Is_Gone_Lifts_At_Login_And_Sends_Nothing()      // the game socket answers 0 in GAME_PHASE_ACTIVE: no frames; GetMatchLock is NULL
[Test] public async Task An_Unanswered_Slot_Query_Lifts_The_Lock_And_Lets_The_Attempt_Proceed()
    // Answers = FALSE: Enforce is FALSE; no frames; GetMatchLock is NULL
[Test] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task Guest_And_Staff_Accounts_Are_Never_Sent_Back(AccountType accountType)
```

All tests carry `[NotInParallel(nameof(ChatServerHost))]`, use a fresh session cookie, and read their waits from `MatchLockService.ReturnResendInterval`.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`
Expected: FAIL to compile, because `Enforce`, `MatchLockReturn`, and the protocol helpers do not exist.

- [ ] **Step 3: Implement enforcement and the send-back**

`Enforce`, in order:
1. Returns FALSE when the account is `Guest` or `Staff`.
2. Reads the lock for `session.Account.User.ID`; when there is none, it clears any pending return and returns FALSE.
3. Calls `Verify`, unless an unexpired return for the same match is pending and was verified within `ReturnResendInterval`. Any verdict but `SendBack` clears the pending return and returns FALSE.
4. When an unexpired return for the same match is pending, refreshes its `LastVerified` if it verified. It re-sends the slot account or other account notice only when `LastNotified` is at least `ReturnResendInterval` old, then records `LastNotified`, so every attempt after the interval is told why it was refused. It never starts a second reminder loop, and it returns TRUE.
5. Otherwise sends the notice and installs a new `MatchLockReturn` (with `LastVerified` and `LastNotified` set to now) expiring after `ReturnWindow`, with `TryReplacePendingMatchLockReturn`. For the slot account only, it starts the reminder loop without awaiting it, logging its exceptions. It returns TRUE.

The reminder loop:
- sends `AUTO_MATCH_CONNECT` at once;
- then, after every `ReturnResendInterval`, stops when the return was replaced or cancelled, the window expired, the session disconnected, the user's lock is gone or for another match, or `Verify` no longer gives `SendBack`;
- otherwise records `LastVerified` and sends a reminder.

The other handlers:
- `EnforceAtLogin` acts only when the account is not exempt, `ReportedJoiningGame` is FALSE, and `MarkMatchLockLoginChecked(session.Metadata.SessionCookie)` is TRUE.
- `EndReturnOnJoiningGame` cancels the pending return only when the reported address equals `{ServerAddress}:{ServerPort}`, or resolves through `GetMatchServerByIPAddressAndPort` to the lock's server ID.
- `RecordReturnAcknowledgement` only logs. `CleanUpSession` cancels the pending return.

`SendAutoMatchConnect` keeps the existing field comments from `MatchAnnounce` and writes `-1` as the random integer for a reminder.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchChannelIntegrationTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockReturn.cs TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.MatchServer.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Actions/TrackPlayerAction.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/JoiningMatch.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/LeftMatch.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs
git commit -m "Send Locked Players Back To Their Match At Login"
```

---

### Task 7: Refuse Matchmaking Groups And Queues While Locked

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupCreate.cs`, `GroupJoin.cs` (inject `MatchLockService`; return before acting when `Enforce` is TRUE)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoinQueue.cs`, `GroupPlayerLoadingStatus.cs` (inject and pass `MatchLockService`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroup.cs:305-322,491-499` (thread `MatchLockService` through `SendLoadingStatusUpdate` and `JoinQueue`; check right after the double-queue guard)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (group enforcement and its message)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `BuildGroupCreate`, `BuildGroupJoin`, `BuildGroupJoinQueue`, and `BuildGroupPlayerLoadingStatus` in their request data layouts)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs`

**Interfaces:**
- Consumes: Task 6's `Enforce`, `SendNotice`, and protocol readers.
- Produces:
  - `public async Task<MatchmakingGroup> SendLoadingStatusUpdate(ClientChatSession session, byte loadingPercent, MerrickContext merrick, MatchLockService matchLockService)`
  - `public async Task JoinQueue(MerrickContext merrick, MatchLockService matchLockService)`
  - `public async Task<bool> EnforceForGroup(MatchmakingGroup group)` (enforces every member; when any is refused and the leader is not, sends the leader `GroupMemberMessage` naming the first refused member; TRUE when the queue must be refused)
  - `internal static string GroupMemberMessage(string lockedMemberName)`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] [Arguments(ChatProtocol.TMMType.TMM_TYPE_PVP)] [Arguments(ChatProtocol.TMMType.TMM_TYPE_COOP)]
public async Task Creating_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back(ChatProtocol.TMMType groupType)
    // GROUP_CREATE of that type (a lock blocks co-op too): MatchmakingService.GetMatchmakingGroup(accountID) is NULL; slot notice; AUTO_MATCH_CONNECT
[Test] public async Task Joining_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back()
    // an unlocked client creates a group; the locked client sends GROUP_JOIN naming it: the group still has one member; slot notice; AUTO_MATCH_CONNECT
[Test] public async Task Queueing_While_Locked_Is_Refused_And_Sends_The_Player_Back()
    // solo group created before the lock is seeded; GROUP_JOIN_QUEUE: QueueStartTime is NULL; slot notice; AUTO_MATCH_CONNECT
[Test] public async Task Reaching_Full_Loading_While_Locked_Does_Not_Queue()
    // GROUP_PLAYER_LOADING_STATUS 100 on a solo group: QueueStartTime is NULL; AUTO_MATCH_CONNECT
[Test] public async Task Queueing_A_Group_With_A_Locked_Member_Tells_The_Leader_And_Sends_The_Member_Back()
    // the leader reads GroupMemberMessage(memberName); the member reads the slot notice and AUTO_MATCH_CONNECT; QueueStartTime is NULL
[Test] public async Task Reaching_Full_Loading_Without_A_Lock_Still_Queues()
    // GROUP_PLAYER_LOADING_STATUS 100 on an unlocked solo group: QueueStartTime is not NULL
```

All tests carry `[NotInParallel(nameof(ChatServerHost))]`.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: FAIL, because locked players create, join, and queue.

- [ ] **Step 3: Implement the triggers**

`GroupCreate` and `GroupJoin` call `Enforce` first and return when it is TRUE. `JoinQueue` calls `EnforceForGroup(this)` after the double-queue guard and returns when it is TRUE, before readiness, rating, or queue state change.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchmakingGroupReadinessTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupCreate.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoin.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoinQueue.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupPlayerLoadingStatus.cs TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroup.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs
git commit -m "Refuse Matchmaking Groups And Queues While Locked"
```

---

### Task 8: Refuse Other Games While Locked To A Match

**Files:**
- Create: `KONGOR.MasterServer/Models/ServerManagement/MatchLockRefusal.cs`
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs:284-359` (`HandleConnectClient`)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs` (refusal channel and publisher)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (add `HandleRefusal`)
- Create: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockRefusalMonitor.cs`
- Modify: `TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs:120` (register the monitor beside `LogoutMonitor`)
- Modify: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs:10` (make the class `partial`, so the new file shares its helpers)
- Create: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatServerHost.cs` (expose `DatabaseConnectionString` and `DistributedCacheConnectionString`)
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/MasterServerHost.cs`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.csproj` (a direct `ProjectReference` to `KONGOR.MasterServer`, so `WebApplicationFactory` finds its dependency file)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs` (add `global using KONGOR.MasterServer.Internals;`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs`

**Interfaces:**
- Consumes: Task 1's `GetMatchLock` and `RemoveMatchLock`; Task 3's `Verify` and `SeedLockedMatch`; Task 6's `Enforce`.
- Produces:
  - `public class MatchLockRefusal { public required int UserID { get; set; } public required string AccountName { get; set; } }`
  - `public const string MatchLockRefusalChannel = "MATCH-LOCK-REFUSAL";`
  - `public static Task PublishMatchLockRefusal(this IDatabase distributedCacheStore, MatchLockRefusal refusal)` (JSON payload)
  - `public async Task HandleRefusal(MatchLockRefusal refusal)` (enforces on the account's chat session when there is one; otherwise verifies the user's lock, lifting it if the player was removed)
  - `public class MatchLockRefusalMonitor(IConnectionMultiplexer distributedCacheProvider, MatchLockService matchLockService, ILogger<MatchLockRefusalMonitor> logger) : IHostedService`
  - In `HandleConnectClient`: `MatchServer? askingServer`, resolved from the `session` form value; Task 10 reuses it.
  - Test helpers in the partial file:
    - `RegisterMatchServer(int instance)` returns `(string Session, int ServerID)`; authenticating the same instance again renews its session and keeps its ID.
    - `LockAccount(string accountName, int serverID)` returns the `MatchLock` it stored.
    - `CreateSubAccount(string mainAccountName, string subAccountName)`.
    - `AssertClientConnectionIsRefused(HttpResponseMessage response)` asserts 409 and a body containing none of `account_id`, `game_cookie`, `auth_failure`, or `Invalid cookie.`.
  - `internal sealed class MasterServerHost : WebApplicationFactory<KONGORAssemblyMarker>`, constructed from a `ChatServerHost`. It runs in Development against that host's database and unprefixed cache (`UseSetting` for both connection strings), sets the environment variables the master server reads at start-up, and is started before a test seeds session cookies, because its start-up purges them. Its `ConnectClient(string session, string cookie)` posts `c_conn` with the form fields `HostingAuthenticationTests.ConnectClient` uses.

- [ ] **Step 1: Write the failing tests**

```csharp
// HostingAuthenticationTests.MatchLock.cs; every test that causes or observes a refusal carries [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
[Test] public async Task Client_Connection_From_Another_Registered_Server_Is_Refused_For_A_Locked_Account()
[Test] public async Task Client_Connection_From_The_Locked_Match_Server_Is_Served()                   // reconnect safety: AssertClientConnectionReturnsTheAccount
[Test] public async Task Client_Connection_From_The_Locked_Match_Server_After_It_Renews_Its_Session_Is_Served()   // reconnect safety: RegisterMatchServer(1) twice
[Test] public async Task Client_Connection_With_An_Unrecognised_Session_Is_Served_For_A_Locked_Account()   // reconnect safety
[Test] public async Task Client_Connection_With_A_Copy_On_Write_Master_Session_Is_Served_For_A_Locked_Account()   // reconnect safety: NEXUS cannot tell the server
[Test] public async Task Client_Connection_For_Another_Account_Of_A_Locked_User_Is_Refused()
[Test] public async Task Client_Connection_Is_Served_And_The_Lock_Lifted_When_The_Locked_Server_Is_Gone()   // lock on an unregistered server ID: served; GetMatchLock is NULL
[Test] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task Client_Connection_Is_Never_Refused_For_Guest_And_Staff_Accounts(AccountType accountType)
[Test] public async Task A_Refused_Client_Connection_Publishes_The_User_ID_And_Account_Name()
    // subscribe to MatchLockRefusalChannel on the factory's multiplexer: the refused user's ID and account name arrive within five seconds
[Test] public async Task A_Served_Client_Connection_Publishes_Nothing()                                 // own-server connection: nothing within two seconds

// MatchLockRefusalMonitorTests ([NotInParallel(nameof(ChatServerHost))])
[Test] public async Task A_Published_Refusal_Sends_The_Locked_Player_Back()               // publish the refusal: slot notice and AUTO_MATCH_CONNECT
[Test] public async Task A_Published_Refusal_For_Another_Account_Names_The_Slot_Account()  // OtherAccountMessage; no AUTO_MATCH_CONNECT
[Test] public async Task A_Published_Refusal_Without_A_Chat_Session_Verifies_The_Lock()    // no client connected; the game socket answers 0 in GAME_PHASE_ACTIVE: the lock is lifted
[Test] public async Task A_Removed_Player_Without_Chat_Is_Served_On_The_Engine_Retry()
{
    // ChatServerHost, then MasterServerHost; account and session cookie seeded after the master server starts; no chat client;
    // SeedLockedMatch on server A with the game socket answering 0 in GAME_PHASE_ACTIVE; server B registered with its own session cookie
    HttpResponseMessage refused = await masterServer.ConnectClient(sessionB, cookie);
    bool lifted = await ChatTestData.WaitUntilLockIsGone(host, userID, TimeSpan.FromSeconds(10));
    HttpResponseMessage retried = await masterServer.ConnectClient(sessionB, cookie);

    await Assert.That(refused.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    await Assert.That(lifted).IsTrue();
    await Assert.That(retried.StatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(await retried.Content.ReadAsStringAsync()).Contains("account_id");
}
```

The four reconnect-safety tests (own server, own server after a session renewal, an unrecognised session, and a copy-on-write master session) already pass today, because `c_conn` serves everyone: they are regression guards, not red steps. Each also asserts, against the same lock, that a connection from another registered server is refused, which fails at the red step.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"` and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockRefusalMonitorTests/*"`
Expected: FAIL, because locked accounts are served and nothing is published.

- [ ] **Step 3: Implement the refusal and its relay**

After resolving the account, `HandleConnectClient` looks the session up with `GetMatchServerBySessionCookie` and reads the user's lock. It refuses only when all of these hold:
- the asking server is registered;
- the account is neither `Guest` nor `Staff`;
- the user's lock names another server ID.

When the lock's server is no longer registered, it lifts the lock with `RemoveMatchLock` and serves instead. A refusal publishes the refusal, logs it, and returns `Conflict($@"Account ""{account.Name}"" Is Locked To Match ID ""{matchLock.MatchID}"" On Another Match Server")`. Replace the opening comment so that it says an unknown session is still served, for reconnect safety.

`MatchLockRefusalMonitor` mirrors `LogoutMonitor`: it deserialises the refusal and awaits `HandleRefusal`, logging any exception. `HandleRefusal` enforces on `Context.ClientChatSessions[refusal.AccountName]` when present, and otherwise calls `Verify` on `GetMatchLock(refusal.UserID)` when one exists.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the commands from Step 2.
Expected: PASS, including the existing client connection tests.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/MatchLockRefusal.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockRefusalMonitor.cs TRANSMUTANSTEIN.ChatServer/TRANSMUTANSTEIN.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatServerHost.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/MasterServerHost.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.csproj ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Internals/UsingDirectives.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs
git commit -m "Refuse Other Games While Locked To A Match"
```

---

### Task 9: Allow Reconnecting From Another Machine

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ServerHandshake.cs:147-170` (add the remote command before the confirmation echo)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `ReadRemoteCommandsUntilEcho(NetworkStream stream, TimeSpan timeout)` returning every remote command text up to the first one starting with `echo `)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/MatchServerHandshakeIntegrationTests.cs`

**Interfaces:**
- Produces: the remote command `svr_ignoreConnectionIDForReconnect true` at every match server handshake, always, because identity is the account.

- [ ] **Step 1: Write the failing test**

```csharp
[Test] public async Task The_Handshake_Lets_The_Match_Server_Reconnect_Players_By_Account()
    // ReadRemoteCommandsUntilEcho contains "svr_ignoreConnectionIDForReconnect true"
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchServerHandshakeIntegrationTests/*"`
Expected: FAIL on the new test.

- [ ] **Step 3: Implement the remote command**

Add it unconditionally to the remote command list, with a comment saying why it is safe: `c_conn` has already verified the account before the slot is reclaimed. The comment also notes that the engine accepts remote commands only with its current session cookie, which is why the command is sent at the handshake.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ServerHandshake.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/MatchServerHandshakeIntegrationTests.cs
git commit -m "Allow Reconnecting From Another Machine"
```

---

### Task 10: Issue Game Cookies Per Account And Match

**Files:**
- Create: `KONGOR.MasterServer/Models/ServerManagement/GameCookie.cs`
- Create: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs`
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs:354` (replace the fixed `game_cookie` and its TODO)
- Modify: `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Authentication.cs:248` (record the latest login cookie)
- Modify: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs` (`CreateClientSessionCookie` also records the latest login cookie, as login now does)
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/SRPAuthenticationTests.cs`

**Interfaces:**
- Consumes: Task 8's `askingServer`, refusal, and test helpers; Task 6's `SessionCookieLifetime`.
- Produces:
  - `public class GameCookie { public required string Cookie { get; set; } public required int? ServerID { get; set; } }`
  - `public static Task<string> IssueGameCookie(this IDatabase distributedCacheStore, int accountID, int? serverID)`
  - `public static Task<string?> GetGameCookie(this IDatabase distributedCacheStore, int accountID)`
  - `public static Task<HashSet<int>> GetAccountsConnectedToMatchServer(this IDatabase distributedCacheStore, int serverID)`
  - `public static Task SetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID, string cookie)`
  - `public static Task<string?> GetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID)`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task Reconnecting_To_The_Same_Match_Server_Returns_The_Same_Game_Cookie()   // reconnect safety
{
    (string session, int _) = await RegisterMatchServer(1);
    string cookie = await CreateClientSessionCookie(NonHostAccountName);

    string first = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];
    string second = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];

    await Assert.That(second).IsEqualTo(first);
    await Assert.That(await DistributedCache.GetGameCookie(await GetAccountID(NonHostAccountName))).IsEqualTo(first);
}

[Test] public async Task Reconnecting_After_The_Match_Server_Renews_Its_Session_Returns_The_Same_Game_Cookie()   // reconnect safety
[Test] public async Task Reconnecting_With_An_Unrecognised_Session_Returns_The_Current_Game_Cookie()   // reconnect safety
[Test] public async Task Connecting_To_Another_Match_Server_Returns_A_New_Game_Cookie()
[Test] public async Task Different_Accounts_Receive_Different_Game_Cookies()
[Test] public async Task A_Refused_Client_Connection_Leaves_The_Game_Cookie_Unchanged()                // reconnect safety: GetGameCookie equals the own-server cookie after a refusal elsewhere
[Test] public async Task Connecting_Elsewhere_After_The_Lock_Lifts_Returns_A_New_Game_Cookie()
    // served on server 1; LockAccount on server 1, then RemoveMatchLock, as any lift does; served on server 2: a game_cookie other than server 1's
[Test] public async Task A_Served_Client_Connection_Records_The_Account_On_The_Match_Server()           // GetAccountsConnectedToMatchServer(serverID) contains the account ID

// SRPAuthenticationTests
[Test] public async Task Each_Login_Records_The_Latest_Session_Cookie_Of_The_Account()
    // two PerformFullAuthentication calls: GetLatestSessionCookie(account.ID) equals the second cookie and not the first
```

Tests that cause a refusal carry `[NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]`. The three reconnect tests (same server, after a session renewal, and an unrecognised session) also pass against today's fixed cookie, so their equality of cookies is a regression guard, not a red step. Each also asserts that `GetGameCookie` for the account equals the cookie returned, which fails at the red step, as the first test shows.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"` and `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/SRPAuthenticationTests/*"`
Expected: FAIL, because every account receives the same fixed game cookie and no latest cookie is recorded.

- [ ] **Step 3: Implement the cookies**

`IssueGameCookie` returns the stored cookie, refreshing its expiry, when one exists and either `serverID` is NULL or it equals the stored server ID. Otherwise it writes a new `Guid.CreateVersion7()` cookie with `serverID`, atomically: a transaction conditioned on the raw value it read (or on the key not existing). When the condition fails, it reads again and returns the stored cookie. Whenever `serverID` is not NULL, it also adds the account to `MATCH-SERVER-ACCOUNTS` and refreshes that set's expiry.

`HandleConnectClient` calls it with `askingServer?.ID`, only on the served path and after the refusal check, so a refused attempt never mints a cookie, while a game joined after the lock has lifted always does. Login calls `SetLatestSessionCookie(account.ID, cookie)` beside `SetAccountNameForSessionCookie`. `aids2cookie` is unchanged here, so real match servers still receive empty lists.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the commands from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/GameCookie.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Authentication.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/SRPAuthenticationTests.cs
git commit -m "Issue Game Cookies Per Account And Match"
```

---

### Task 11: Answer The Duplicate Session Check As The Match Server Asks

**Files:**
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.ServerManagement.cs:5-76,143-149` (rewrite `HandleAccountIDsToCookie`; remove the keyspace scan, the query-string reading, both regular expressions, and the comments warning against fixing the parsing alone)
- Modify: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs` (replace the two isolated `aids2cookie` tests and their unprefixed-key workaround, and update the class summary)
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`

**Interfaces:**
- Consumes: Task 10's `GetAccountsConnectedToMatchServer`, `GetLatestSessionCookie`, `GetGameCookie`, and `IssueGameCookie`; Task 8's refusal and helpers.
- Produces: `aids2cookie` reads `aids[]` from the request body. For each account ID the asking server served, it answers `aids2cookie` (the latest login cookie) and `aids2gcookie` (the game cookie) with identical keys in identical order, omitting accounts that lack either cookie.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task Account_IDs_To_Cookie_Answers_With_The_Latest_Session_Cookie_And_The_Game_Cookie_Of_Each_Served_Account()
{
    (string session, int _) = await RegisterMatchServer(1);
    string cookie = await CreateClientSessionCookie(NonHostAccountName);
    string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, cookie)))["game_cookie"];
    int accountID = await GetAccountID(NonHostAccountName);

    IDictionary<object, object> body = await DeserialisePHPResponse(await RequestAccountCookiesInTheRequestBody(session, accountID));

    await Assert.That((string) ((IDictionary<object, object>) body["aids2cookie"])[accountID.ToString()]).IsEqualTo(cookie);
    await Assert.That((string) ((IDictionary<object, object>) body["aids2gcookie"])[accountID.ToString()]).IsEqualTo(gameCookie);
}

[Test] public async Task Account_IDs_To_Cookie_Lists_Both_Cookies_In_The_Same_Account_Order()           // two served accounts: both key sequences are equal (the engine pairs them by index)
[Test] public async Task Account_IDs_To_Cookie_Omits_Accounts_The_Asking_Server_Never_Served()          // served by server 2 only, asked by server 1: both lists empty
[Test] public async Task Account_IDs_To_Cookie_Omits_Accounts_Without_A_Latest_Session_Cookie()        // latest cookie deleted: both lists empty
[Test] public async Task Account_IDs_To_Cookie_After_A_Reconnect_Returns_The_Cookies_The_Reconnect_Used()   // reconnect safety: own server and unrecognised session reconnects
[Test] public async Task Account_IDs_To_Cookie_With_A_Renewed_Session_Still_Answers()                   // reconnect safety: server 1 renews its session; asked with the new session: same cookies
[Test] public async Task Account_IDs_To_Cookie_After_A_Refused_Connection_Elsewhere_Returns_The_Original_Game_Cookie()   // reconnect safety: a rage-quit attempt never trips the guard
[Test] public async Task After_A_Lift_Joining_Another_Game_Reports_The_New_Game_Cookie_To_The_Original_Server()
    // served on server 1 with game cookie G1; LockAccount on server 1, then RemoveMatchLock, as any lift does; served on server 2 with G2;
    // server 1 asks: aids2gcookie holds G2, not G1, so the guard removes the player from the match they left
[Test] public async Task Account_IDs_To_Cookie_After_A_Login_Elsewhere_Returns_The_New_Session_Cookie()      // the guard kicks the old connection as a duplicate login
[Test] public async Task Account_IDs_To_Cookie_Ignores_Account_IDs_In_The_Query_String()                 // RequestAccountCookies (query string): both lists empty
```

Tests that cause a refusal carry `[NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]`.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"`
Expected: FAIL, because the body's `aids[]` are not read.

- [ ] **Step 3: Implement the answer**

Keep the existing session check and its 401. Then:
1. Read the distinct positive integers of `Request.Form["aids[]"]`.
2. Keep those in `GetAccountsConnectedToMatchServer(matchServer.ID)`.
3. Add an account to both dictionaries, in the same iteration, only when both its latest login cookie and its game cookie exist.

The summary states the engine's checks (`CGameServer::GetAccountAuth`): a different game cookie terminates the player as a duplicate connection, and a different login cookie kicks the connection as a duplicate login.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then the whole master server suite, `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.ServerManagement.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs
git commit -m "Answer The Duplicate Session Check As The Match Server Asks"
```

---

### Task 12: Verify The Match Lock With Real Game Clients (Needs The Owner)

**This task needs the owner.** The controller prepares and observes; the owner plays the clients.

**Files:** none changed. Outcomes are recorded in the branch's working notes.

**Prerequisites:**
- [ ] NEXUS running through the Aspire AppHost in Development, which plays one player per team, as `TRANSMUTANSTEIN.ChatServer/appsettings.Development.json` sets.
- [ ] A match server manager with at least two match servers: one for the matchmade match, one for a public or custom game.
- [ ] Normal accounts only, never guest or staff accounts, which are exempt: two players for the 1v1, a sub-account of player A, and a second installation or PC for player A.
- [ ] Watch the match server consoles for `Sending info to … - …ms` lines (one per reconnect query), and NEXUS's logs for lock creation, verdicts, refusals, lifts, and any unreachable-server error.

**Scenarios.** Each starts from a fresh Caldavar 1v1 that reached hero selection unless it says otherwise; the expected outcome follows the arrow.
- [ ] **A rage-quit followed by each trigger.** Player A leaves the match for the menu, and nothing happens until A acts → then, for each trigger in turn, A is refused with the slot message and auto-connected back into the match:
  - [ ] logging in again (restart the client);
  - [ ] creating a group (normal and co-op);
  - [ ] joining player C's group;
  - [ ] queueing (as leader, and with loading complete);
  - [ ] joining a public game (refused through the engine's authentication retries, then sent back once the client is at the menu);
  - [ ] joining a custom game;
  - [ ] spectating a game.
  - Repeat once in a pick phase (before hero loading) and once in the game proper. Observe the group interface after a refused create or join.
- [ ] **Another account.** Player A logs in on the sub-account → the other-account message naming the slot account at login; every trigger refused with it; no auto-connect. Logging in on the slot account → sent back.
- [ ] **Another machine.** Player A logs in on the second installation → sent back, and reclaims the slot (account-based reconnect).
- [ ] **A crashed server.** Kill the match server process mid-match → at A's next attempt (or after the stale host reaper's removal) the lock lifts, A plays normally, and A is never sent to the dead server.
- [ ] **Removal by an AFK kick.** Let A be AFK-kicked → A's next attempt lifts the lock and proceeds at once.
- [ ] **Reconnecting to one's own match.** Through the client's own reconnect prompt and through the send-back → never refused, and never kicked or terminated by the duplicate-session guard over the following minutes.
- [ ] **A login elsewhere.** While A is connected to the match, log in as A on the second installation → the first installation's game connection is kicked as a duplicate login (not terminated, so A's slot is kept), and the second installation is sent back. Its first connection may be refused until the guard's next check removes the old connection; the reminders keep sending it back until it succeeds.
- [ ] **An unreachable server.** Block UDP from NEXUS to the match server's game port, then have A, after a rage-quit, join a public game → the lock lifts with one error log naming the address tried; A plays the public game; and at its next account check the original server removes A from the match they left, so their team shares A's gold and items.
- [ ] **A match outliving the limit.** Let one match run past `MatchLock.Lifetime` with A out of it → A can then play other games, and the original server removes A from that match at its next account check.
- [ ] **Controls.** A co-op match creates no lock; a chat server restart mid-match messages nobody in the match; the match ending (victory, concede, and remake) frees both players.
- [ ] **Clean up.** Stop the AppHost, its background tasks, and any orphaned shells when done.

## Follow-Ups (Not In This Plan)

- `RequestGameInfo.BuildGamePhase` labels game phases 7 and above one value off from the engine's `EGamePhase` (7 is `POST_MATCH`, 8 is `ENDED`, 9 is `HERO_BLIND_BAN`); it can use Task 3's `GamePhase`.
- `MatchAborted` passes the matchup (correlation) ID to `RemoveMatchInformation`, which is keyed by match ID, so an aborted match's information is not removed there.
