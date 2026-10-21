# Matchmaking Match Lock Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A player who leaves a competitive matchmade match in progress cannot play any other game until it ends, and every attempt to play again sends them straight back into it, without ever breaking a reconnect to that match.

**Architecture:** The chat server writes a per-user lock record to Valkey when it announces a Caldavar, MidWars, or RiftWars match. The lock is enforced for at most one hour from that moment, and every end-of-match signal and server removal lifts it sooner. Every trigger re-verifies the lock against the match server's chat status and the engine's own UDP reconnect query, then either lifts it or refuses the attempt and sends the player back with `AUTO_MATCH_CONNECT` reminders. The triggers are a queue, the server list, and a refused `c_conn`, the last two relayed by the master server over pub/sub, while the first login report of a login cookie only tells the player (amendment of 2026-10-21). Once the lock refuses other games, `c_conn` issues real game cookies and `aids2cookie` answers as the engine asks, which switches on the engine's duplicate-session guard. A player who joins another game once the lock has lifted, for any reason, receives a new game cookie there, so the original server's guard removes them from the match they left.

**Tech Stack:** C# on .NET with ASP.NET Core and Kestrel TCP endpoints, .NET Aspire, StackExchange.Redis against Valkey, Entity Framework Core, TUnit on the Microsoft Testing Platform, and Testcontainers-backed integration hosts.

**Spec:** `docs/superpowers/specs/2026-10-19-matchmaking-match-lock-design.md`

**Amendment (2026-10-20):** The owner settled one login per user, and never a kick for a login elsewhere, in the spec's "Single Login Per User (Amendment, 2026-10-20)", on the evidence in `.superpowers/research/2026-10-20-concurrent-account-sessions.md`. Tasks 13 to 18 build it. They run after Task 11 and before Task 12, which stays the last task and the last in this file. They supersede Task 11's answer for a login elsewhere, Task 10's `SetLatestSessionCookie`, and Task 6's return window, and Task 16 carries two controller rulings from the fix wave's re-review.

**Amendment (2026-10-21):** The first live test with real game clients found six problems, and the owner redesigned the triggers: NEXUS pulls a locked player back only when the player really tries to play. The spec's "Live Test Amendment (2026-10-21)" records the findings, the redesign, and the rulings, including the owner's later rulings on the group model and the wording, which make a locked player's group a solo one. Tasks 19 to 25 build it. They run after Task 18 and before Task 12, which stays the last task and the last in this file, and becomes the re-test of what they change. They supersede Task 6's send-back at login, Task 7's refusal of group create and the leader's group message, Task 8's plain-text `c_conn` refusal, and Task 16's in-match notice with its `groupMember` and `leaderActed` flags, and they narrow Task 16's first ruling, that a refused `c_conn` always gets the full send-back, to sessions which do not report the locked match server. No change affects the arguments or workarounds of the launchers.

## Global Constraints

- Competitive matchmade modes only (Caldavar, MidWars, and RiftWars) create a lock, decided from NEXUS's `QueueType`.
- A lock blocks every match type, co-op included, plus public games, custom games, and spectating.
- One lock per user, with guest and staff accounts exempt.
- Another account gets refused, with a message naming the slot account.
- Players already removed from their match are free.
- Never send a player to a dead server, a finished match, or an unreclaimable slot.
- Never lift a lock during a pick phase.
- The triggers which pull a locked player back are joining another group, inviting someone, a queue, the server list, and any game join (`c_conn`); a login only tells the player, once per login cookie, and never pulls or starts a wait; the client's own solo group (group create) is never refused; and leaving alone triggers nothing (Tasks 20 to 23 and 25).
- A locked player is only ever alone in a group, apart from a premade party which outlives its match: an invite to a locked player is not delivered, the inviter is told why, and the inviter's group is unchanged; an invite never lifts the invitee's lock on a slot which the match server does not confirm: when the match server does not answer the reconnect query, the invite is delivered, the lock is kept, and the invitee's own group join decides (Task 25).
- Account-based reconnect is set by a remote command at server handshake, always.
- NEXUS conventions (`claude.md` and `agents.md` on `next`); commits in imperative Start Case, with no body and no attribution to non-humans.
- The match lock is the default behaviour, with no configuration at all: the safety limit is the constant `MatchLock.Lifetime`, one hour from when the lock begins, never renewed, and tests reference it, never a copied value.
- Reconnecting to the match in progress must never break: `c_conn` from the locked match's own server, or from a server whose session NEXUS does not know, is always served, and every task touching `c_conn`, `aids2cookie`, or game cookies carries a test pinning it.
- A player who joins another game has left their match, and the original server's duplicate-session guard removes them, so that their team shares their gold and can use or sell their items. Locks exist to stop that during the first hour. Once a lock has lifted, for any reason (confirmed, unanswered, unreachable, or the safety limit), joining another game issues a new game cookie there, and the guard removes the player at its next account check.
- Reconnecting to one's own match keeps the same game cookie, including across the server's session renewal, so a real reconnect never trips the guard.
- The duplicate-session guard (Tasks 10 and 11) lands only after the lock and its `c_conn` refusal (Tasks 1 to 8), because before the lock it would terminate rage-quitters instead of sending them back.
- HON stays at retail parity: no client or match server change; only the existing `svr_ignoreConnectionIDForReconnect` setting is switched on remotely.
- One login per user: a login atomically retires the earlier login of its account and, unless it is a guest or staff account, the logins of the user's other accounts, and a login is never refused because another session is live (Task 13).
- NEXUS never causes a duplicate-login kick: `aids2cookie` answers each served account with the login cookie which its own client on the asking match server holds, and the game cookie exactly as before, so a player who joins another game is still removed (Task 15).
- A logout ends only its own session: the logout notification carries its session cookie, the chat server ends only the session which used it, and a logout with a retired cookie publishes nothing (Task 14).
- While the slot account is still connected to the match, from another machine, nobody is sent `AUTO_MATCH_CONNECT`: the player is told to close the game there, once per attempt, and is sent back as soon as the slot is free (Task 17).
- Every refused attempt of a locked player at the menu is told why. A session which reports being in a game gets no notice, pull, or popup of its own, because its client never joins or queues from a game. When a party is refused, every member, the leader and the locked members included, is told which members hold a match lock (Task 21).
- A refusal relayed by the master server (`c_conn` or the server list) is ignored while the chat session last reported joining the locked match server, and otherwise always gets the full send-back, whatever else the session reported (Task 19).
- A slot account which the attempt pulls back at once is never sent a matchmaking failure reply: the trigger gives it the notice and the pull, and the client leaves its solo group by itself on the pull. A refused queue or group join which does not pull its player back at once (another account, a slot account which waits for its other connection, or a slot account whose attempt continues a pending return or cannot start one) is answered first with `NET_CHAT_CL_TMM_FAILED_TO_JOIN`, reason `TMMFTJR_BANNED`, with the seconds left until `MatchLock.TimestampExpires`, because the matchmaking screen covers the chat; the session then leaves its group in NEXUS without a group update. A refused party's remaining members are unreadied, and every member is told which members hold a match lock (Task 21).
- A refused `c_conn` is answered with an array holding only the account's `account_id`, which the engine treats as final; never `game_cookie`, never `auth_failure`, and never `cookie` set to `Invalid cookie.` (Task 22).
- A locked account's server list is a successful, empty list in the requested shape (Task 23).
- Notices and log lines name accounts with their clan tags (`Account.NameWithClanTag`, or the lock's `TaggedAccountName`); only the refusal relayed by the master server keeps the bare name, which finds the chat session, and no notice asks a player to type a tagged name (Task 24).
- Player-facing text calls the restriction a match lock, never a ban, and notices come from the sender "Match Lock"; each text is defined once, in `MatchLockService` (Task 24).
- Work on branch `matchmaking-match-lock`; never push.

## Review Focus

1. A chat reconnect while the player is still in their match (a network blip or a chat server restart) must not message the player or send them back: the login notice fires once per login cookie, and never while the session reports a game (Task 6, `Logging_In_Again_With_The_Same_Session_Cookie_Sends_Nothing`, and `Logging_In_While_Reporting_A_Game_Sends_Nothing`).
2. Repeated attempts within seconds (players click Play twice, the public game list refreshes, a player tries one friend's game after another) must produce one message and one reminder loop (Task 6, `Repeated_Enforcements_Within_The_Return_Window_Message_Once`).
3. A match server NEXUS cannot reach over UDP must not trap anyone: the lock lifts so the attempt proceeds, and the outage is logged at error level once per server. A player who then joins another game has left, and the original server's guard removes them, as intended (Task 3, `Verifying_Against_An_Unanswering_Server_Lifts_The_Lock`; Task 11, `After_A_Lift_Joining_Another_Game_Reports_The_New_Game_Cookie_To_The_Original_Server`).
4. A server that missed an end signal and now hosts a new match must never receive the old match's player (Task 3, `A_Server_Reporting_Another_Match_Has_Ended_The_Match`).
5. A late statistics resubmission for an old match must not lift a newer lock on the same server (Task 1, `Removing_The_Locks_Of_A_Match_Keeps_A_Newer_Match_On_The_Same_Server`).
6. Two logins of one user at the same moment, on one account or on two, must leave exactly one live login, never two and never none (Task 13, `Concurrent_Logins_Of_Two_Accounts_Of_One_User_Leave_Exactly_One_Session_Cookie`).
7. The old machine's logout must never end the new machine's chat session (Task 14, `A_Logout_With_Another_Session_Cookie_Leaves_The_Chat_Session_Alone`).
8. A client which logs in again in place during its match pushes its new cookie to the match server, and must not be kicked for it (Task 15, `Joining_A_Match_Server_With_A_New_Session_Cookie_Refreshes_The_Recorded_Session_Cookie`).
9. A chat reconnect while a refused join is still connecting reports `JOINING_GAME` for the other match server, and must not silence the send-back, while a refusal which arrives after the player is back in their own match must be ignored (Task 16, `A_Published_Refusal_Sends_The_Player_Back_Even_After_The_Client_Reported_Joining_A_Game`; Task 19, `A_Refusal_While_The_Player_Reports_The_Locked_Server_Is_Ignored`).
10. The new machine's own connection reads as connected while it loads into the match, and must never earn it the notice that its slot is still connected from another game (`ConnectedElsewhereMessage`; Task 17's test `Sending_Back_Never_Tells_The_Player_To_Close_Another_Computer`).
11. No trigger may start a return or a wait for a player who is in their own match, so that leaving it afterwards still triggers nothing (Task 19, `Leaving_The_Match_After_An_Ignored_Refusal_Triggers_Nothing`; Task 20, `Logging_In_While_Locked_Tells_The_Player_Without_Sending_Them_Back`; Task 21, `Queueing_While_Still_In_The_Match_Gives_No_Notice_And_No_Return`).
12. A refused queue must unlock the client's matchmaking screen: a slot account's alone in its group through the pull with no popup, another account's, and that of a slot account whose return continues, through the "Temporary Ban" popup, and a party's through a group update which unreadies it (Task 21, `Queueing_While_Locked_Is_Refused_And_Sends_The_Player_Back`, `Queueing_On_Another_Account_Shows_The_Temporary_Ban_Before_The_Notice`, `Queueing_Again_While_The_Return_Continues_Shows_The_Temporary_Ban_And_Leaves_The_Group`, and `Queueing_A_Party_With_A_Locked_Member_Unreadies_It_And_Sends_The_Member_Back`).
13. A refusal which no client or engine retries must never leave a player whose lock has just lifted without an explanation (Task 22, `A_Refusal_Which_Finds_The_Lock_Lifted_Tells_The_Player_To_Try_Again`).
14. The empty server list must be a successful, well-formed list of the requested kind, so that the client clears and redraws its list, and a game creation finds no match server (Task 23, `The_Server_List_Of_A_Locked_Account_Is_Empty_And_Relays_A_Refusal`).
15. An invite must never reach a locked player whose slot its match server confirms, nor change the inviter's group, and must never lift the invitee's lock because the match server does not answer (Task 25, `Inviting_A_Locked_Player_Is_Not_Delivered_And_Tells_The_Inviter` and `Inviting_A_Locked_Player_Whose_Match_Server_Does_Not_Answer_Delivers_The_Invite_And_Keeps_The_Lock`).

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
| `ACCOUNT-LATEST-SESSION-COOKIE:["{accountID}"]` | the account's live login cookie, which the user's next login retires (Task 13) | the session cookie lifetime |
| `ACCOUNT-GAME-COOKIE:["{accountID}"]` | JSON `GameCookie` (cookie, server ID, and the login cookie of the client last served it, Task 15) | the session cookie lifetime, refreshed on each `c_conn` |
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
| no reply after the retries, or only socket errors | any | lift without confirmation (`LiftSlotUnconfirmed`): the lock is removed, so nobody is trapped, and the server is logged at error level once; at the re-checks which a return makes by itself between attempts while it waits for the slot (`WaitForSlot`) and has sent nobody anywhere, the lock is kept and the wait goes on instead (final review ruling); a real attempt to play during the wait still lifts the lock, and a server which is gone or no longer hosts the match lifts it at any check; a third party's invite to a locked player keeps the lock too, and is delivered (Task 25, the amendment's final review) |
| nonzero, at most `ConnectedSlotThreshold` (Task 17) | any | send back |
| above `ConnectedSlotThreshold` (Task 17) | any | wait for the slot (`WaitForSlot`), which keeps the lock: a connected player's value is near `uint.MaxValue`, because its termination time is `INVALID_TIME` |
| `0` | `HERO_LOADING`, `PRE_MATCH`, `ACTIVE`, `POST_MATCH`, or `ENDED` | lift (`LiftSlotGone`): picking phases only ever precede `HERO_LOADING`, so from these phases a `0` can only mean removed or over |
| `0` | unknown, or any other (`IDLE`, `WAITING_FOR_PLAYERS`, `HERO_BAN`, `HERO_SELECT`, or a phase numbered after `ENDED`) | send back: the server answers `0` to everyone while waiting for players and in the pick phases numbered at or above `ENDED`, and a reported `HERO_SELECT` may be up to one status interval stale, so the lock is kept and never lifts during a pick phase |

**The send-back signal (Task 6).** No chat message reaches the chat server when a refused connection ends: the client sends `CHAT_CMD_LEFT_GAME` only after it reported `CHAT_CMD_JOINING_GAME`, which it sends only when loading starts (`CChatManager::LeftGame` and the `CHostClient` connection states), and a refused `c_conn` never reaches loading. So the chat server sends `AUTO_MATCH_CONNECT` at once and re-sends it as a reminder (random integer `-1`) every `ReturnResendInterval`. It stops only when the client's `JOINING_GAME` names the lock's own server, when re-verification no longer gives send-back, when the session ends, or when the return window expires. The client acts on the first reminder it receives while connected to no server, and loading on the match server then reports `JOINING_GAME` with its address. The client's `AC_MATCHMAKING_MATCH_READY` and `AC_MATCHMAKING_MATCH_READY_REMINDER` actions are logged as telemetry only, because acting on a reminder is not reaching the match: the connection can still be refused, or ignored while the match server still holds the slot for another connection which has yet to leave.
- **The return window** is `ReturnWindow` in `MatchLockService`, as Task 17 sizes it (see "The waits" below). It first also waited out the engine's duplicate-session interval and timeout, for the guard to remove the old connection, but no kick is awaited any longer.
- **The trade-off:** the player returns up to one resend interval after the refused connection gives up, and the client silently ignores reminders while it is still connecting.

**The login trigger (Task 6).** It runs on `AC_DAILY_LOGINS`, the last step of `CChatManager::HandleAuthAccepted`, once per login cookie: a marker keyed by the session cookie, living as long as the cookie, stops it from running again on the chat reconnects that send the same action. It also skips a session that reported `JOINING_GAME` after the accept, as the client does when it is already on a server. Task 20 keeps when it runs, but makes it a notice only, with no pull and no wait.

**The `c_conn` notification (Task 8).** The master server refuses with HTTP 409 and a plain-text reason (never `auth_failure`, never `cookie` set to `Invalid cookie.`), and publishes a `MatchLockRefusal` (user ID and account name) as JSON on the pub/sub channel `MATCH-LOCK-REFUSAL`, mirroring `ACCOUNT-LOGOUT`. The chat server runs the trigger on the account's chat session when there is one, and otherwise verifies the user's lock anyway. That frees a removed player whose chat is down before the engine's next `c_conn` retry. Task 22 replaces the plain-text reason with a final reply, still with HTTP 409, so that the engine no longer retries, and Task 23 publishes the same refusal for the server list.

**The roster (Task 4).** `MatchmakingMatch.CommitRoster()` snapshots the members when `CreateMatch` commits them to the match server, and `MatchAnnounce` locks that snapshot, so a player who dropped from chat between spawn and announce is still locked.

**The messages (Tasks 6 and 7).** Sent as `CHAT_CMD_MESSAGE_ALL` from the sender name `Matchmaking`, defined once in `MatchLockService` and compared by tests through those members:
- the slot account: `You left a match which is still in progress, so you cannot play another game until it ends. Sending you back to it.`
- another account: `Your account "{slot account}" is in a match which is still in progress, so you cannot play another game until it ends. Log in as "{slot account}" to return to it.`
- a group leader whose member is locked: `"{member}" is in a match which is still in progress, so your group cannot search for a match until it ends.`
- Task 24 names the slot account with its clan tag, rewords the other account's message, and calls the restriction a match lock; Task 21 removes the group leader's message.

**One login per user (Tasks 13 to 18).**
- **The supersession (Task 13).** The latest-login record (`ACCOUNT-LATEST-SESSION-COOKIE`) is the pointer to each account's live login cookie.
  - One attempt reads the records of the account and of the user's other accounts in one request. It then runs one transaction, conditioned on each record still holding what was read (`Condition.StringEqual`, or `Condition.KeyNotExists` when it was absent). The transaction stores the new cookie's `ACCOUNT-SESSION-COOKIE` key, points the account's record at it, deletes the other accounts' records, and deletes the key of every cookie read.
  - A lost race reads again. Each lost race means that another login of the same user committed, and each such login can defeat an attempt only once, so a login among N concurrent ones needs at most N attempts. `SessionCookieSupersessionAttempts` is 10, and a login which still loses fails with a server error, which the client shows as a failed login, so the player logs in again.
  - A guest or staff login gives no other accounts, because each group shares one user, as `ClientHandshake` already rules.
- **What a retired cookie gets, as checked at the current head.**
  - The client requester answers every function but the login ones with 401 `Unrecognised Cookie` from the controller's common check (`ClientRequesterController.cs`), which a logout never gets past, so a logout with a retired cookie already publishes nothing. The check logs it as a forged cookie, which Task 14 softens for `logout` alone, because every login elsewhere now makes the old machine log out with its retired cookie.
  - `c_conn` answers 401 `No Valid Client Session Cookie Could Be Found`. The engine retries that like any refusal and then disconnects the client with an authentication failure. It is never "Invalid cookie.", so the match server never blacklists the cookie (`CClientConnection::CheckAuthResult`).
  - The engine sends `c_conn` only until a client has authenticated once (`CClientConnection`, `m_uiLastAuthSuccess`), so a retired cookie never disconnects a machine which is already in its match. NEXUS answers the match server's own `get_upgrades` for that machine with 401, which the engine ignores.
  - The chat handshake rejects a retired cookie with `ECR_AUTH_FAILED`.
- **The recorded login cookie (Task 15).**
  - It lives in the `GameCookie` record, beside the game cookie, rather than in a per-server structure. An unrecognised session's `c_conn` keeps the game cookie on the assumption that it is a reconnect to the stored match server, and records its login cookie on the same assumption, so a reconnect from another machine through a match server whose session NEXUS lost is still answered with its own cookie at that server's next check.
  - `CHostClient::UpdateCookie` pushes the client's new cookie to its match server without checking the account, so a client which logs in again in place as another account of the same user pushes that account's cookie. A refresh therefore covers every account of the user whose record is for that match server, the reporting account's own included, each independently (controller rulings: a fall-back which stopped at the first success let a stale record block the account in the match). Guest and staff accounts never refresh another account. This goes beyond "a chat session of that account" in the brief, so that no in-place login causes a kick (my call).
- **The slot-connected boundary (Task 17).** `ConnectedSlotThreshold` is `uint.MaxValue / 2`, and a reply above it means connected.
  - A connected or loading player's termination time is `INVALID_TIME` (`CPlayer::Connected`, `HON.Shared/c_player.cpp`), so the reply is `INVALID_TIME` minus the game time (`CGameServer::GetReconnectInfo`). The game time is the match server's time since its last match ended, or since it started (`CHostServer::EndGame` resets it, `K2.Engine/c_hostserver.cpp`).
  - A disconnected player's reply is at most `sv_maxDisconnectedTime` minus the time they have already spent disconnected (`CPlayer::Disconnected`), which is minutes.
  - A player who never connected is not in the match server's client map, so the reply for them is 0.
  - So half the range separates the two unless an operator sets a disconnect allowance of weeks, or a match server's game time passes about 24.8 days. Either would read a connected slot as disconnected, and the player would be sent back as before this change, where the engine ignores the attempt and the reminders carry on: a benign failure.
- **The waits (Task 17, my call: the owner left them open).**
  - `ReturnWindow` becomes the authentication total timeout, plus `EngineClientConnectionTimeout`, the client's default `cl_connectionTimeout` (`K2.Engine/c_hostclient.cpp`), for one attempt which the match server ignores while an old connection lingers, plus one `ReturnResendInterval` for the next reminder to land. The duplicate-session check's interval and timeout leave it, because no kick is awaited any longer. Since Task 22, a refused `c_conn` is final and never retried, but the authentication total timeout stays (the amendment's final review): during a staggered deployment a master server of the previous version can still answer, whose refusal the engine retries, which keeps the client connecting elsewhere and ignoring reminders until the engine gives up, and a longer window is harmless, because the reminders stop at the client's `JOINING_GAME` for the locked match server. `NoticeRepeatInterval` keeps the same span, so that a player's repeated attempts within seconds (clicking Play again, or asking for the server list and then joining a game), and the retries which a master server of the previous version relays, are explained once.
  - `ConnectedElsewhereWait`, how long a return keeps checking a slot which stays connected, is five minutes. A closed game frees the slot at once, and a crashed or unplugged one after the match server's `svr_clientConnectedTimeout` (`K2.Engine/c_clientconnection.cpp`, without a strike), which five minutes covers many times over, leaving the rest for the player to reach the other computer. A machine left running is never removed by the engine itself: `sv_afkTimeout` only flags the player, which lets the team vote to kick them (`CGameServer`'s idle check and `VOTE_TYPE_KICK_AFK`). So a longer wait would mostly pull a player into the match long after being told, and the player's next attempt to play waits again anyway. The cost while waiting is one reconnect query per `ReturnResendInterval`.
  - Once the slot frees, the return sends the player back at once, and its expiry becomes `ReturnWindow` from then (a plain assignment, controller ruling), so a send-back after a wait lasts exactly as long as an immediate one.
  - An unanswered reconnect query at the wait's own re-checks between attempts does not end it, because nobody has been sent anywhere yet (final review ruling). A real attempt to play during the wait still lifts the lock on an unanswered query, which ends the wait, as the owner ruled, and a server which is gone or no longer hosts the match lifts the lock at any check.
  - A wait which runs out ends with one short notice (`MatchLockService.ConnectedElsewhereWaitExpiredMessage`, final review ruling). It is safe, because a wait which has not sent the player back cannot be waiting on the player's own loading connection.
- **The old machine's notice (Task 18).** The engine shows it.
  - The chat server writes the notice, the reject, and the logout in that order, and its write pump flushes them together. The client handles frames in order, through one switch (`CChatManager`, `K2.Engine/c_chatmanager.cpp`), so the notice is handled before `HandleRejected` disconnects.
  - `HandleMessageAll` and `HandleRejected` both go through `AddIRCChatMessage`, which writes to every chat widget without a channel, the in-game chat included, so the notice is shown wherever the reject's own text is.
  - The logout which follows clears only the message inbox (`CChatManager::ClearMessages`), not the chat widgets.
  - It cannot replace the account-sharing text, which still follows, but it tells the player what happened just before it.
- **The messages (Tasks 16 to 18).** Defined once, beside the others, and compared by tests through those members:
  - a player still in their match (`MatchLockService.InMatchMessage`, controller ruling): `You are in a match which is still in progress, so you cannot start another game until it ends.` Task 21 removes it.
  - the slot account still connected from another game (`MatchLockService.ConnectedElsewhereMessage`, reworded by the final review, because the commonest wait is a crash and relaunch on the same machine): `Your account is still connected to your match from a game which is running or has just closed. Close it if it is still open, and you will be sent back to the match as soon as it has left.`
  - the slot account whose wait has run out (`MatchLockService.ConnectedElsewhereWaitExpiredMessage`, final review): `Your other game has not left the match yet. Try again once it has closed, and you will be sent back.`
  - the old machine (`ClientHandshake.LoggedInElsewhereMessage`, from the sender `Login`): `You have logged in as "{account}" elsewhere, so you have been logged out here. Any game in progress here carries on.`

**The live test amendment (Tasks 19 to 25).** The spec's "Live Test Amendment (2026-10-21)" carries the evidence from the engine and the client; these are the decisions which it leaves to the plan.
- **What a session last reported (Task 19).** `ClientChatSession.ReportedGameServerAddress` holds the address of the latest `CHAT_CMD_JOINING_GAME` on the chat connection, `CHAT_CMD_LEFT_GAME` clears it, and `ReportedJoiningGame` is derived from it. A relayed refusal is stale when that address names the lock's match server by the rule which `EndReturnOnJoiningGame` already uses (`NamesLockedServer`: exactly as the player was sent there, or as the match server is registered).
- **The login notice (Task 20).** `NotifyAtLogin` replaces `EnforceAtLogin`, with the same guards (exempt accounts, sessions which report a game, and the once-per-login-cookie marker). It verifies the lock as before, so an unanswered query still lifts it at login, and it sends one notice while `KeepsLock` accepts the verdict. It never touches the pending return.
- **The group model (Tasks 20, 21, and 25).** Group create stands, silently; group join, a queue, and a locked player's invite go through `Enforce`, as any attempt to play; and an invite to a locked player is not delivered. The client cannot join a group while it is in a game (`CChatManager::JoinTMMGroup`), and inviting changes nothing on the inviter's screen, which only marks the player as invited, so an invite gets a chat notice and nothing more.
- **What the client sees on a refused queue or group join (Task 21).** `Enforce(…, matchmaking: true)` gives a slot account which it pulls back at once the notice and the pull, and nothing else: the client leaves a solo group by itself on the pull (`CChatManager::MatchmakingConnect`, "foundmatch"), which unlocks its screen. Any other refused queue or group join of a session at the menu gets the `TMMFTJR_BANNED` failure reply first, because the matchmaking screen covers the chat (owner); the client then leaves its group without telling the chat server, so NEXUS removes the session from its group with `RemoveMember(…, notifyRemovedMember: false)`, since a group update naming it as leaving, or its group as disbanded, would make the client leave again with another popup. The client has no popup which fits a match lock better: every other failure reason shows a text about something else, "isleaver" shows only a real leaver's block, the server-not-idle popup asks the player to queue again, and the leave strike warning threatens a punishment. The failure reply is not held back by `NoticeRepeatInterval`, and its seconds round up, so that a lock with a fraction of a second left still shows one.
- **What is left of `EnforceForGroup` (Task 21).** It still enforces every member, because a premade party outlives the match it played together (`MatchmakingService` keeps groups of more than one member through their match, and the client keeps them too), so a locked player can still queue with the teammates of the locked match. Beyond that, it only unreadies a refused party's remaining members and sends them one full group update, because a pulled member's client keeps a party on the pull and an in-game member gets no frame, so nothing else would unlock the remaining members' screens; a solo group gets nothing more, so that no group update follows a pull. Every member which the party had when the queue started, the leader and the refused members included, then gets one `GroupMemberLockedMessage` per refused member, so that nobody blames the leader (owner); it replaces the leader's `GroupMemberMessage`. A player who leaves the party needs no handling of its own: a logout, a crash, or a login elsewhere ends the chat session, whose clean-up removes the player from its group (`ClientChatSession.CleanUpSession`, which both `Terminate` and `OnDisconnected` run).
- **What Task 21 removes, and why.** `InMatchMessage`, `TryClaimInMatchNotice` with its ticks field, `Enforce`'s `groupMember`, and the `leaderActed` parameters of `EnforceForGroup` and `JoinQueue` existed only for the in-match notice, which nothing reaches any more: the client never joins a group from a game, group create no longer enforces (Task 20), and no client interface sends `NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE` (the client's Start button readies the group, full loading queues it, and `JoinTMMQueue` is reachable only from the console: HON `K2.Engine/cmd_chatmanager.cpp`, and no caller in HON-Assets `ui`). `GroupMemberMessage` gives way to `GroupMemberLockedMessage`, which every member of a refused party receives.
- **The final reply (Task 22).** `HandleConnectClient` refuses with `Conflict(PhpSerialization.Serialize(new Dictionary<string, object> { { "account_id", account.ID } }))`.
  - The status stays 409. The engine's HTTP manager completes any transfer which cURL finishes, whatever its status (`CHTTPManager`, HON `K2.Engine/c_httpmanager.cpp`), and 409 keeps the refusal visible in the master server's own records.
  - The `account_id` is that of the posted cookie's account, so it never conflicts with the connection's own account ID, which is still unset at the first `c_conn` (`CClientConnection`).
  - The chat server's authentication error log stays as it is: the final reply makes the engine report a success before it disconnects, so the expected error after a lock refusal disappears, while a real failure is still logged at error level.
- **A relayed refusal which finds no lock (Task 22).** `HandleRefusal` tells the session `RefusalLiftedMessage` when `Enforce` lets the attempt through, because the master server refused an attempt which nothing retries.
- **The shared refusal rule (Task 23).** `FindRefusingMatchLock` moves from `ServerRequesterController` into `DistributedCacheExtensions.MatchLock.cs`, with an asking server ID which is NULL for the server list. It also returns the lock which it lifted because its match server is gone, so that each caller logs the lift as `c_conn` did, since the cache extensions do not log.
- **The server list's account (Task 23).** The client requester's common check has already validated the cookie, so `GetServerList` resolves the account from it, with its user and its clan, as `c_conn` does. An unknown game type keeps its 422, before any lock is read. The game type "90" covers the match server which the client picks by itself for a new game (`CHostClient::StartAutoRemoteGame`), as well as one which the player picks.
- **Clan tags (Task 24).** `MatchLock.AccountNameWithClanTag` is optional and nullable, so that a lock stored before it deserialises with NULL, and `MatchLock.TaggedAccountName` falls back to `AccountName`. Every log line and notice of the match lock which names an account uses `Account.NameWithClanTag`, or `TaggedAccountName` for the slot account. `MatchLockRefusal.AccountName` stays bare, because it finds the chat session by name. The accounts of chat sessions already load their clans at the handshake, and `c_conn` already loads the account's clan.
- **The messages (Tasks 20, 21, 22, 24, and 25).** Defined once, in `MatchLockService`, from `MessageSenderName = "Match Lock"` (Task 24), and compared by tests through those members; each says "match lock" where it names the restriction, and never "ban":
  - the slot account sent back (`SlotAccountMessage`, reworded by Task 24): `You left a match which is still in progress, and a match lock keeps you from playing another game until it ends. Sending you back to it.`
  - the slot account at login (`SlotAccountLoginMessage`, Task 20): `You are in a match which is still in progress, and a match lock keeps you from playing another game until it ends. Trying to play another game sends you back to it.`
  - another account (`OtherAccountMessage(string slotAccountNameWithClanTag)`, reworded by Task 24): `Your account "{slot account with clan tag}" is in a match which is still in progress, and a match lock keeps you from playing another game until it ends. Log in on that account to return to it.`
  - a relayed refusal which finds the lock lifted (`RefusalLiftedMessage`, Task 22): `Your match lock has lifted, because you can no longer return to your last match. Please try again.`
  - the inviter of a locked player (`InviteeLockedMessage(string inviteeNameWithClanTag)`, Task 25): `"{invitee with clan tag}" is under a match lock until their match in progress ends, so they cannot join your group.`
  - every member of a refused party, once per locked member (`GroupMemberLockedMessage(string lockedMemberNameWithClanTag)`, Task 21): `"{locked member with clan tag}" has a match lock, because their match is still in progress, so your group cannot search for a match until it ends.`
  - `ConnectedElsewhereMessage` and `ConnectedElsewhereWaitExpiredMessage` keep their wording, which does not name the restriction.

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
    - timings: `internal static readonly TimeSpan ReturnResendInterval = TimeSpan.FromSeconds(5);`, `internal static readonly TimeSpan EngineAccountAuthenticationInterval = TimeSpan.FromMinutes(2);` and `EngineAccountAuthenticationTimeout = TimeSpan.FromSeconds(15);` (the engine's `svr_AccountAuthInterval` and `svr_AccountAuthTimeOut` defaults), and `internal static readonly TimeSpan ReturnWindow` (the authentication total timeout plus those two; Task 17 replaces those two terms and sizes the window again);
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
3. Calls `Verify`, unless an unexpired return for the same match is pending and was verified within `ReturnResendInterval`. Any verdict which `KeepsLock` rejects (Task 17) clears the pending return and returns FALSE.
4. When an unexpired return for the same match is pending, refreshes its `LastVerified` if it verified. It re-sends the slot account or other account notice only when `LastNotified` is at least `NoticeRepeatInterval` old (the fix wave's interval, which replaced `ReturnResendInterval` here), then records `LastNotified`, so every attempt after the interval is told why it was refused. It never starts a second reminder loop, and it returns TRUE.
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
  - `public static Task SetLatestSessionCookie(this IDatabase distributedCacheStore, int accountID, string cookie)` (Task 13 replaces it with `SupersedeSessionCookies`)
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
- Produces: `aids2cookie` reads `aids[]` from the request body. For each account ID the asking server served, it answers `aids2cookie` (the latest login cookie; Task 15 answers the login cookie recorded for the client instead) and `aids2gcookie` (the game cookie) with identical keys in identical order, omitting accounts that lack either cookie.

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
[Test] public async Task Account_IDs_To_Cookie_After_A_Login_Elsewhere_Returns_The_New_Session_Cookie()      // the guard kicks the old connection as a duplicate login (Task 15 replaces it: no kick)
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

### Task 13: Retire Earlier Logins At Login

**Files:**
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs:39-59` (add `SupersedeSessionCookies` and its attempts constant beside the session cookie methods)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs:20-24` (remove `SetLatestSessionCookie`, which `SupersedeSessionCookies` replaces; `GetLatestSessionCookie` stays)
- Modify: `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Authentication.cs:248-250` (log in through `SupersedeSessionCookies`)
- Modify: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs:743-757` (`CreateClientSessionCookie` logs in through `SupersedeSessionCookies` for its own account only, so that the tests which log two accounts of one user in keep both)
- Modify: `ASPIRE.Tests.KONGOR.MasterServer/Services/SRPAuthenticationService.cs` (add `CreateSubAccount`)
- Create: `ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/LoginSessionTests.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/SRPAuthenticationTests.cs:339-357`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs`

**Interfaces:**
- Consumes: Task 10's `ConstructAccountLatestSessionCookieKey` and `GetLatestSessionCookie`; Task 6's `SessionCookieLifetime`.
- Produces:
  - `internal const int SessionCookieSupersessionAttempts = 10;`
  - `public static Task<List<string>> SupersedeSessionCookies(this IDatabase distributedCacheStore, int accountID, string accountName, string cookie, IReadOnlyCollection<int> otherAccountIDs)`. In one transaction, it stores the login cookie for the account name, points the account's latest-login record at it, retires every cookie which the records of the account and of the other accounts point at, and deletes the other accounts' records. It returns the retired cookies, and throws `InvalidOperationException` once `SessionCookieSupersessionAttempts` attempts have lost their races.
  - `public async Task<Account> CreateSubAccount(Account mainAccount, string subAccountName)` on `SRPAuthenticationService`: an account of the main account's user, which logs in with the user's password.

- [ ] **Step 1: Write the failing tests**

```csharp
// LoginSessionTests (KONGORIntegrationWebApplicationFactory with the SQL Server and distributed cache containers, as MatchLockStoreTests starts it)
// Every test draws fresh random account IDs and cookies; the factory also gives each test its own key prefix in the distributed cache, so only the chat server tests share an unprefixed cache
[Test]
public async Task Concurrent_Logins_Of_Two_Accounts_Of_One_User_Leave_Exactly_One_Session_Cookie()
{
    int firstAccountID = NewAccountID();
    int secondAccountID = NewAccountID();

    List<string> cookies = [.. Enumerable.Range(0, DistributedCacheExtensions.SessionCookieSupersessionAttempts).Select(index => Guid.CreateVersion7().ToString())];

    // Half Of The Logins Are Of Each Account, And Each Gives The Other Account As The User's Other Account
    await Task.WhenAll(cookies.Select((cookie, index) => index % 2 is 0
        ? DistributedCache.SupersedeSessionCookies(firstAccountID, "First", cookie, [secondAccountID])
        : DistributedCache.SupersedeSessionCookies(secondAccountID, "Second", cookie, [firstAccountID])));

    List<string> liveCookies = [];

    foreach (string cookie in cookies)
    {
        if (await DistributedCache.GetAccountNameForSessionCookie(cookie) is not null)
            liveCookies.Add(cookie);
    }

    List<string> records = [.. new[] { await DistributedCache.GetLatestSessionCookie(firstAccountID), await DistributedCache.GetLatestSessionCookie(secondAccountID) }.OfType<string>()];

    using (Assert.Multiple())
    {
        await Assert.That(liveCookies.Count).IsEqualTo(1);
        await Assert.That(records).IsEquivalentTo(liveCookies);
    }
}

[Test] public async Task A_Login_Retires_The_Earlier_Session_Cookie_Of_The_Account()
    // two logins of one account: the second returns [first]; the first no longer resolves; the second resolves to the account name and is the latest-login record
[Test] public async Task A_First_Login_Retires_Nothing_And_Lives_For_The_Session_Cookie_Lifetime()
    // returns []; the cookie's key and the latest-login record both expire within SessionCookieLifetime, and later than SessionCookieLifetime minus a minute
[Test] public async Task A_Login_Retires_The_Session_Cookies_Of_The_Other_Given_Accounts()
    // the other account logged in first; the login which gives it returns [its cookie]; its cookie no longer resolves, and its latest-login record is gone
[Test] public async Task A_Login_Leaves_Accounts_Which_Are_Not_Given_Alone()
[Test] public async Task Concurrent_Logins_Of_One_Account_Leave_Exactly_One_Session_Cookie()
    // SessionCookieSupersessionAttempts concurrent logins of one account, none of which throws: exactly one cookie resolves, and it is the latest-login record

// SRPAuthenticationTests
[Test] public async Task Each_Login_Retires_The_Earlier_Session_Cookie_Of_The_Account()
    // renames Each_Login_Records_The_Latest_Session_Cookie_Of_The_Account and keeps its assertions; also, the first cookie no longer resolves and the second resolves to the account name
[Test] public async Task A_Login_Retires_The_Session_Cookies_Of_The_Other_Accounts_Of_The_User()
    // the main account logs in, then a sub-account from CreateSubAccount: the main account's cookie no longer resolves, and GetLatestSessionCookie(main) is NULL
[Test] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task A_Guest_Or_Staff_Login_Leaves_The_Other_Accounts_Of_The_User_Logged_In(AccountType accountType)
    // both accounts set to the type, as the disabled account test sets its type: the main account's cookie still resolves after the sub-account logs in
[Test] public async Task A_Retired_Session_Cookie_Is_Refused_By_The_Client_Requester()
    // after a second login, "client_requester.php?f=get_products" answers 401 with the first cookie, and succeeds with the second

// HostingAuthenticationTests.MatchLock.cs
[Test] public async Task Client_Connection_With_A_Retired_Session_Cookie_Is_Refused_Without_Blacklisting_It()
    // CreateClientSessionCookie twice; c_conn with the first: 401, a body with none of "account_id", "game_cookie", "auth_failure", or "Invalid cookie."; GetGameCookie is NULL

// ClientHandshakeIntegrationTests
[Test] public async Task Handshake_With_A_Retired_Session_Cookie_Is_Rejected()
    // two SupersedeSessionCookies for the account on the host's IDatabase; a handshake with the first: NET_CHAT_CL_REJECT
```

`Authenticate_With_SRP_Same_Account_Multiple_Logins_Generates_Unique_Cookies` stays as it is: a second login while the first is live always succeeds, which keeps crash recovery working.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/LoginSessionTests/*"`
Expected: FAIL to compile, because `SupersedeSessionCookies` does not exist.

- [ ] **Step 3: Implement the supersession**

`SupersedeSessionCookies` follows "The supersession" in Decisions. The login gives `account.User.Accounts` except the account itself as the other accounts, or none for a guest or staff account, and logs the number of logins it retired at information level.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/SRPAuthenticationTests/*"`, `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"`, and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/ClientHandshakeIntegrationTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Authentication.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Services/SRPAuthenticationService.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/LoginSessionTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/SRPAuthenticationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs
git commit -m "Retire Earlier Logins At Login"
```

---

### Task 14: End Only The Logged Out Session

**Files:**
- Create: `KONGOR.MasterServer/Models/ServerManagement/AccountLogout.cs` (beside `MatchLockRefusal`, the other notification from the master server to the chat server)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs:84-95` (the payload becomes a JSON `AccountLogout`)
- Modify: `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Session.cs:9-27` (publish the cookie with the account name)
- Modify: `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.cs:32-44` (a `logout` with an unrecognised cookie is still answered 401, but is logged at information level as the logout of an ended session, not as a forged cookie)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/LogoutMonitor.cs:34-64`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/LoginSessionTests.cs`
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/LogoutMonitorTests.cs`

**Interfaces:**
- Consumes: Task 13's `SupersedeSessionCookies`.
- Produces:
  - `public class AccountLogout { public required string AccountName { get; set; } public required string SessionCookie { get; set; } }`
  - `public static Task PublishAccountLogout(this IDatabase distributedCacheStore, AccountLogout logout)` (a JSON payload on `AccountLogoutChannel`; it replaces the overload which took the account name)

- [ ] **Step 1: Write the failing tests**

```csharp
// LogoutMonitorTests ([NotInParallel(nameof(ChatServerHost))])
[Test]
public async Task A_Logout_With_Another_Session_Cookie_Leaves_The_Chat_Session_Alone()
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

    (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    // The Old Machine Logs Out With Its Own Session Cookie After The New Machine Has Taken Over The Account's Chat Session
    await host.Services.GetRequiredService<IDatabase>().PublishAccountLogout(new AccountLogout { AccountName = accountName, SessionCookie = Guid.CreateVersion7().ToString() });

    bool closed = await ChatTestProtocol.WaitForClose(client.GetStream(), QuietPeriod);

    using (Assert.Multiple())
    {
        await Assert.That(closed).IsFalse();
        await Assert.That(Context.ClientChatSessions.ContainsKey(accountName)).IsTrue();
    }
}

[Test] public async Task A_Logout_Ends_The_Chat_Session_Which_Used_Its_Session_Cookie()   // the session's own cookie: WaitForClose is TRUE, and the account name is no longer registered
[Test] public async Task A_Malformed_Logout_Does_Not_Stop_The_Monitor()
    // a plain account name published raw on the channel, then a valid logout of the session: the session ends

// LoginSessionTests (both carry [NotInParallel(DistributedCacheExtensions.AccountLogoutChannel)])
[Test] public async Task A_Logout_Publishes_The_Account_Name_With_Its_Session_Cookie()
    // a login, then "client_requester.php?f=logout" with its cookie: the AccountLogout read from the channel within five seconds carries both, and the cookie no longer resolves
[Test] public async Task A_Logout_With_A_Retired_Session_Cookie_Publishes_Nothing_And_Ends_No_Session()
    // two logins, then a logout with the first: answered 401, nothing published within two seconds, and the second cookie still resolves
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/LogoutMonitorTests/*"`
Expected: FAIL to compile, because `AccountLogout` does not exist.

- [ ] **Step 3: Implement the scoped logout**

`HandleLogout` publishes `new AccountLogout { AccountName = accountName, SessionCookie = cookie }`. `LogoutMonitor` deserialises the payload, logs a warning and returns for a payload which is not an `AccountLogout`, and terminates the session registered under the account name only when its `Metadata.SessionCookie` equals the logout's (ordinal), logging at debug level otherwise. Both summaries say that a logout ends only the session which used its cookie, because the newer login of the same account must survive the old machine's logout.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/LoginSessionTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/AccountLogout.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.Authentication.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.Session.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.cs TRANSMUTANSTEIN.ChatServer/Services/LogoutMonitor.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/Handlers/SRP/LoginSessionTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/LogoutMonitorTests.cs
git commit -m "End Only The Logged Out Session"
```

---

### Task 15: Answer The Duplicate Session Check With Each Client's Own Login Cookie

**Files:**
- Modify: `KONGOR.MasterServer/Models/ServerManagement/GameCookie.cs` (add the session cookie)
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs:33-106` (`IssueGameCookie` and `TryIssueGameCookie` record the session cookie; add `GetStoredGameCookie` and `RefreshClientSessionCookie`)
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs:392-393` (pass the posted `cookie`)
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.ServerManagement.cs:5-57` (answer the recorded session cookie; stop reading the latest-login record; rewrite the summary)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs:93-121` (`PrepareToJoinMatch` refreshes the recorded session cookie)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs` (add `SeedSubAccount`)
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/JoiningMatchSessionCookieTests.cs`

**Interfaces:**
- Consumes: Task 10's `IssueGameCookie`, `GetGameCookie`, and `GetAccountsConnectedToMatchServer`; Task 13's `CreateClientSessionCookie`, which now retires the earlier login.
- Produces:
  - `public required string SessionCookie { get; set; }` on `GameCookie`: the session cookie of the latest client connection which was served this game cookie, or which has since reported joining its match server.
  - `public static Task<string> IssueGameCookie(this IDatabase distributedCacheStore, int accountID, int? serverID, string sessionCookie)`
  - `public static Task<GameCookie?> GetStoredGameCookie(this IDatabase distributedCacheStore, int accountID)` (`GetGameCookie` stays, returning its `Cookie`)
  - `public static Task<bool> RefreshClientSessionCookie(this IDatabase distributedCacheStore, int accountID, int serverID, string sessionCookie)`: TRUE only when it changed the record, which then carries the session cookie, so that the caller can log the refresh; FALSE, changing nothing, when the record is not for that match server, already carries the session cookie, or was changed first by a newer client connection (final review).
  - `public static Task<(int AccountID, string AccountName)> SeedSubAccount(IServiceProvider services, int mainAccountID, AccountType accountType = AccountType.Normal)` on `ChatTestData`.

- [ ] **Step 1: Write the failing tests**

```csharp
// HostingAuthenticationTests.MatchLock.cs; replaces Account_IDs_To_Cookie_After_A_Login_Elsewhere_Returns_The_New_Session_Cookie
[Test]
public async Task Account_IDs_To_Cookie_After_A_Login_Elsewhere_Still_Answers_The_Session_Cookie_Of_The_Client_On_The_Match_Server()
{
    await InitialiseIn("Development");

    (string session, _) = await RegisterMatchServer(1);

    string firstCookie = await CreateClientSessionCookie(NonHostAccountName);

    int accountID = await GetAccountID(NonHostAccountName);

    string gameCookie = (string) (await DeserialisePHPResponse(await ConnectClient(session, firstCookie)))["game_cookie"];

    // The Player Logs In On Another Machine, Which Retires The First Machine's Login But Not Its Connection To The Match
    string secondCookie = await CreateClientSessionCookie(NonHostAccountName);

    (IDictionary<object, object> sessionCookies, IDictionary<object, object> gameCookies) = await ReadAccountCookies(await RequestAccountCookiesInTheRequestBody(session, accountID));

    // The Match Server Holds The First Machine's Session Cookie, So The Unchanged Answer Never Kicks It, And Never Counts Toward The Engine's Kick Ban
    using (Assert.Multiple())
    {
        await Assert.That(secondCookie).IsNotEqualTo(firstCookie);
        await Assert.That(await webApplicationFactory.Services.GetRequiredService<IDatabase>().GetAccountNameForSessionCookie(firstCookie)).IsNull();
        await Assert.That((string) sessionCookies[accountID.ToString()]).IsEqualTo(firstCookie);
        await Assert.That((string) gameCookies[accountID.ToString()]).IsEqualTo(gameCookie);
    }
}

[Test] public async Task Account_IDs_To_Cookie_Answers_With_The_Session_Cookie_And_The_Game_Cookie_Of_Each_Served_Client()
    // renames Account_IDs_To_Cookie_Answers_With_The_Latest_Session_Cookie_And_The_Game_Cookie_Of_Each_Served_Account, with the same assertions
[Test] public async Task Account_IDs_To_Cookie_Still_Answers_After_The_Session_Cookie_Ends()
    // replaces Account_IDs_To_Cookie_Omits_Accounts_Without_A_Latest_Session_Cookie: with the cookie's key and the latest-login record deleted, as a logout, a login elsewhere, or expiry does, it is still answered with that cookie and the game cookie
[Test] public async Task Account_IDs_To_Cookie_After_A_Reconnect_From_Another_Machine_Answers_Its_Session_Cookie()     // reconnect safety
    // served with the first cookie; a second login; c_conn on the same server with the second: the second cookie, and the same game cookie
[Test] public async Task Account_IDs_To_Cookie_After_A_Reconnect_Through_An_Unrecognised_Session_Answers_Its_Session_Cookie()     // reconnect safety
    // served on server 1 with the first cookie; a second login; c_conn with an unrecognised session and the second: server 1 is answered with the second cookie
[Test] public async Task Refreshing_The_Session_Cookie_For_The_Match_Server_Of_The_Game_Cookie_Is_Answered()
    // RefreshClientSessionCookie(account, server 1, new cookie) is TRUE; server 1 is answered with the new cookie and the unchanged game cookie
[Test] public async Task Refreshing_The_Session_Cookie_For_Another_Match_Server_Changes_Nothing()          // FALSE; the answer is unchanged

// JoiningMatchSessionCookieTests ([NotInParallel(nameof(ChatServerHost))]); each seeds a match server with SeedMatchServer, and the game cookie with IssueGameCookie(accountID, serverID, an earlier cookie)
[Test] public async Task Joining_A_Match_Server_With_A_New_Session_Cookie_Refreshes_The_Recorded_Session_Cookie()
    // the client, authenticated with a new cookie, sends JOINING_GAME naming the server: GetStoredGameCookie's SessionCookie becomes the new cookie within ten seconds, with the same Cookie
[Test] public async Task Joining_Another_Match_Server_Leaves_The_Recorded_Session_Cookie_Alone()
    // JOINING_GAME naming a second seeded server; once Metadata.MatchServerConnectedTo names it, the record is unchanged
[Test] public async Task Joining_A_Match_Server_As_Another_Account_Of_The_User_Refreshes_The_Account_Served_There()
    // the game cookie is the main account's; a sub-account from SeedSubAccount sends JOINING_GAME naming the server: the main account's record carries the sub-account's cookie
[Test] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task Joining_A_Match_Server_As_A_Guest_Or_Staff_Account_Never_Refreshes_Another_Account(AccountType accountType)
    // as above, with both accounts of the type: the main account's record is unchanged
```

`Account_IDs_To_Cookie_Keeps_Both_Lists_Aligned_Around_An_Omitted_Account` now omits its middle account by deleting its game cookie (`ConstructAccountGameCookieKey`) instead of its latest-login record. Every other test of `aids2cookie` and of game cookies stays as it is, the reconnect-safety ones included.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"`
Expected: FAIL to compile, because `RefreshClientSessionCookie` and `GetStoredGameCookie` do not exist.

- [ ] **Step 3: Implement the recorded login cookie**

- `TryIssueGameCookie` keeps the stored game cookie by the same rule as before. When the posted session cookie differs from the stored one, it writes the record again with the same game cookie and server ID and with `SessionCookieLifetime`, in the same conditioned transaction; otherwise it only refreshes the expiry. A new game cookie records the posted session cookie.
- `RefreshClientSessionCookie` writes the record again only when it is for that match server and its session cookie differs, conditioned on the raw value it read; a lost race leaves the newer record, which a newer `c_conn` wrote.
- `aids2cookie` reads each served account's record once with `GetStoredGameCookie`, answers its `SessionCookie` and `Cookie`, and omits an account without a readable record.
- `PrepareToJoinMatch`, once it has resolved the match server, refreshes the session's own account and, unless the account stands alone as a guest or staff account does (`SRPAuthenticationHandlers.LoginAffectsOtherAccounts`), every other account of the user (`Account.User.Accounts`), each independently (controller rulings). A comment says why: the engine pushes the cookie of whatever account the client logged in again with (`CHostClient::UpdateCookie`), and a record which an earlier match on the same match server left cannot be told apart from the record of the account in the match. Each failure is logged and never thrown, so that it stops neither the other refreshes nor the join's status update, and each refresh which changes a record is logged (final review).

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/JoiningMatchSessionCookieTests/*"`, `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`, and the whole master server suite, `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/GameCookie.cs KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.ClientConnections.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.ServerManagement.cs TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/JoiningMatchSessionCookieTests.cs
git commit -m "Answer The Duplicate Session Check With Each Client's Own Login Cookie"
```

---

### Task 16: Tell Players Still In Their Match Why, And Always Send A Refused Join Back

**Controller rulings.** Both items come from the fix wave's re-review, and are the controller's rulings rather than the owner's decisions:
1. A refused `c_conn` never takes `Enforce`'s in-game shortcut. A chat reconnect reports `JOINING_GAME` whenever `Host.IsConnected()` holds (`CChatManager::HandleAuthAccepted`), which includes `CLIENT_STATE_CONNECTING`, so a chat reconnect during a refused join's `c_conn` retries would make that refusal silent, and the player would not be sent back. A refusal proves that the client is connecting elsewhere, not sitting in the locked match, so `HandleRefusal` always runs the full send-back, whatever the flag says.
2. A player still in their match whose own group create, group join, or queue is refused gets a neutral notice, because the spec tells every refused attempt why. It is sent at most once per `NoticeRepeatInterval`, with no reminder loop and no `AUTO_MATCH_CONNECT`. Later rulings narrowed "own": a teammate who is enforced only because the leader queued sees nothing, and so does every member when full loading starts the queue, while a leader who asks to queue (`GroupJoinQueue`) is told why, as for create and join (final review).

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs:126-211,257-274` (the in-match notice, `Enforce`'s new parameter, and `HandleRefusal`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs` (add `TryClaimInMatchNotice`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs:202-250`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`

**Interfaces:**
- Consumes: Task 8's `HandleRefusal`; Task 6's `Enforce`; the fix wave's `NoticeRepeatInterval` and `ChatTestProtocol.ReadSendBackFrames`.
- Produces:
  - `internal const string InMatchMessage = "You are in a match which is still in progress, so you cannot start another game until it ends.";` on `MatchLockService`.
  - `public async Task<bool> Enforce(ClientChatSession session, bool refusedConnection = false, bool groupMember = false)`: TRUE for `refusedConnection` skips the in-game shortcut, and `HandleRefusal` passes TRUE; TRUE for `groupMember` withholds the in-match notice, and `EnforceForGroup` passes it for every member except a leader who asked to queue.
  - `public bool TryClaimInMatchNotice(TimeSpan repeatInterval)` on `ClientChatSession`: records that the in-match notice is sent now, and returns TRUE, only when none was claimed within the interval, atomically (`Interlocked.CompareExchange` on the ticks of the last claim).

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockRefusalMonitorTests
[Test]
public async Task A_Published_Refusal_Sends_The_Player_Back_Even_After_The_Client_Reported_Joining_A_Game()
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    // A Chat Reconnect While The Refused Join Is Still Connecting Reports Joining The Other Game's Server
    await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame("127.0.0.2:11236"));

    bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => Context.ClientChatSessions[accountName].ReportedJoiningGame, FrameTimeout);

    await PublishRefusal(host, userID, accountName);

    (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
    (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

    using (Assert.Multiple())
    {
        await Assert.That(reportedJoiningGame).IsTrue();
        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
        await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
    }
}

// MatchLockMatchmakingTriggerTests; Queueing_A_Group_With_A_Member_Still_In_The_Match_Does_Not_Message_The_Member keeps its name and its expectations: the teammate sees nothing
[Test] public async Task Creating_A_Group_While_Still_In_The_Match_Gives_Only_The_In_Match_Notice()
    // JOINING_GAME naming the lock's server; two group creates; the notices are exactly [MatchLockService.InMatchMessage]; no AUTO_MATCH_CONNECT, no pending return, and no group
[Test] public async Task Queueing_While_Still_In_The_Match_Gives_The_Leader_Only_The_In_Match_Notice()   // final review
[Test] public async Task Reaching_Full_Loading_While_Still_In_The_Match_Gives_No_Notice()               // final review

// MatchLockSendBackTests
[Test] public async Task A_Player_Still_In_The_Match_Is_Told_Why_Once_Per_Notice_Repeat_Interval()
    // JOINING_GAME naming the lock's server; two direct Enforce calls: both TRUE; ReadSendBackFrames over twice ReturnResendInterval holds exactly [InMatchMessage] and no AUTO_MATCH_CONNECT; PendingMatchLockReturn is NULL
```

`Logging_In_While_Reporting_A_Game_Sends_Nothing` stays as it is, because the login trigger still skips a session which reports a game (Review Focus 1).

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockRefusalMonitorTests/*"`
Expected: FAIL to compile, because `InMatchMessage` does not exist.

- [ ] **Step 3: Implement the rulings**

The in-game shortcut applies only when `refusedConnection` is FALSE. It sends `InMatchMessage` when `TryClaimInMatchNotice(NoticeRepeatInterval)` is TRUE, logs the refusal, and returns TRUE, as before without starting a return. `HandleRefusal` calls `Enforce(session, refusedConnection: true)`.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"` and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs
git commit -m "Tell Players Still In Their Match Why And Always Send A Refused Join Back"
```

---

### Task 17: Wait For The Other Machine Before Sending A Player Back

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockVerdict.cs` (add `WaitForSlot`; the summary says that it keeps the lock too)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockReturn.cs` (a settable `Expires`, and `SlotConnected`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (the message and timings at 9-55, `Verify` at 103-124, `Enforce`, `EvaluateSlot` at 340-363, `VerifySlot`'s logging, `Notify` at 480-500, and `RemindToReturn` at 502-563)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs:11-16`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs`

**Interfaces:**
- Consumes: Task 16's `Enforce(session, refusedConnection)` and `InMatchMessage`; Task 3's `EvaluateSlot` and `Verify`.
- Produces, all on `MatchLockService` unless stated:
  - `MatchLockVerdict.WaitForSlot`: the slot account is still connected to the match, so the player waits for that connection to leave.
  - `internal const uint ConnectedSlotThreshold = uint.MaxValue / 2;` (a reply above it is a connected slot's)
  - `internal static bool KeepsLock(MatchLockVerdict verdict)` (TRUE for `SendBack` and `WaitForSlot`), used wherever the code now tests for `SendBack` alone.
  - `internal const string ConnectedElsewhereMessage`: the notice to a slot account which waits for its slot's other connection to leave, worded only in `MatchLockService`, which the tests compare against.
  - `internal static readonly TimeSpan EngineClientConnectionTimeout = TimeSpan.FromSeconds(15);` (the game client's default `cl_connectionTimeout`, a client setting which NEXUS cannot change)
  - `internal static readonly TimeSpan ReturnWindow = TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestTotalTimeoutMilliseconds) + EngineClientConnectionTimeout + ReturnResendInterval;`
  - `internal static readonly TimeSpan ConnectedElsewhereWait = TimeSpan.FromMinutes(5);`
  - `EngineAccountAuthenticationInterval` and `EngineAccountAuthenticationTimeout` are removed, replaced by the terms above.
  - On `MatchLockReturn`: `public DateTimeOffset Expires { get; set; }` and `public bool SlotConnected { get; set; }` (the last verdict was `WaitForSlot`).

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockSendBackTests; the class adds private const uint ConnectedSlotTimeRemaining = uint.MaxValue - 60_000u, a slot connected for a minute of game time
[Test]
public async Task Logging_In_While_Connected_Elsewhere_Tells_The_Player_To_Close_The_Other_Game_And_Sends_Nothing_Back()
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(ConnectedSlotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

    (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2 + QuietPeriod);

    using (Assert.Multiple())
    {
        await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage]);
        await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn?.SlotConnected).IsTrue();
        await Assert.That(gameSocket.Requests.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
    }
}

[Test] public async Task A_Player_Waiting_For_The_Other_Machine_Is_Sent_Back_As_Soon_As_The_Slot_Is_Free()
    // as above, then gameSocket.SlotTimeRemaining = SlotTimeRemaining, a disconnected slot: an AUTO_MATCH_CONNECT whose RandomInteger is not -1 within ReminderTimeout, with no further notice; then reminders of -1
[Test] public async Task Sending_Back_Never_Tells_The_Player_To_Close_Another_Computer()
    // a disconnected slot: the slot notice and the first AUTO_MATCH_CONNECT
    // then the slot answers connected, as the client's own connection does while it loads: no notice and no reminder over twice ReturnResendInterval
    // then disconnected again: a reminder of -1 arrives
[Test] public async Task Repeated_Attempts_While_Connected_Elsewhere_Message_Once()
    // two direct Enforce calls: both TRUE; exactly one ConnectedElsewhereMessage within ReturnResendInterval
[Test] public async Task A_Wait_For_The_Other_Machine_Lasts_The_Connected_Elsewhere_Wait_And_A_Send_Back_The_Return_Window()
    // while waiting, PendingMatchLockReturn.Expires is within five seconds of the start plus ConnectedElsewhereWait
    // once the slot frees and the AUTO_MATCH_CONNECT arrives, Expires is at least that moment plus ReturnWindow, minus a second
[Test] public async Task Another_Account_Is_Named_The_Slot_Account_While_The_Slot_Is_Connected_Elsewhere()
    // a lock for another account of the user, with a connected slot: OtherAccountMessage(lock.AccountName), and no AUTO_MATCH_CONNECT

// MatchLockRefusalMonitorTests
[Test] public async Task A_Published_Refusal_While_Connected_Elsewhere_Tells_The_Player_To_Close_The_Other_Game()
    // a connected slot: ConnectedElsewhereMessage, and no AUTO_MATCH_CONNECT within QuietPeriod

// MatchLockVerificationTests
// A_Slot_With_Time_Remaining_Is_Reclaimable replaces [Arguments(GamePhase.GAME_PHASE_ACTIVE, uint.MaxValue - 1u)] with [Arguments(GamePhase.GAME_PHASE_ACTIVE, MatchLockService.ConnectedSlotThreshold)]
// It also adds [Arguments(GamePhase.GAME_PHASE_ACTIVE, 14_400_000u)], a disconnect allowance raised to four hours
[Test] [Arguments(GamePhase.GAME_PHASE_HERO_SELECT, uint.MaxValue)] [Arguments(GamePhase.GAME_PHASE_ACTIVE, uint.MaxValue - 7_200_000u)] [Arguments(GamePhase.GAME_PHASE_ACTIVE, MatchLockService.ConnectedSlotThreshold + 1u)]
public async Task A_Connected_Slot_Waits_For_Its_Connection_To_Leave(GamePhase gamePhase, uint slotTimeRemaining)     // WaitForSlot; the second is two hours of game time
[Test] public async Task Verifying_A_Connected_Slot_Keeps_The_Lock()                     // the game socket answers uint.MaxValue - 60_000u: Verify is WaitForSlot; the lock remains
```

Every existing send-back test answers `SlotTimeRemaining`, a disconnected slot, so its expectations stand.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockVerificationTests/*"`
Expected: FAIL to compile, because `WaitForSlot` and `ConnectedSlotThreshold` do not exist.

- [ ] **Step 3: Implement the wait**

- `EvaluateSlot` gives `WaitForSlot` for a reply above `ConnectedSlotThreshold`, before its other rules, and `Verify` keeps the lock for both verdicts which `KeepsLock` accepts. `VerifySlot` logs a waiting verdict at debug level, as it logs a kept lock.
- `Enforce` keeps the verdict at hand, which is a fresh one when it verifies, or else the pending return's `SlotConnected`. The other account's notice is unchanged. The slot account's notice is `ConnectedElsewhereMessage` for `WaitForSlot` and `SlotAccountMessage` otherwise, both for a new return and for a repeated notice after `NoticeRepeatInterval`. A new return expires after `ConnectedElsewhereWait` when it waits, and after `ReturnWindow` otherwise, and records `SlotConnected`.
- `RemindToReturn` sends the first `AUTO_MATCH_CONNECT` at once only when the return does not wait. On every resend interval it verifies as before and records `SlotConnected`. For `WaitForSlot`, it sends nothing and carries on. Otherwise, while nothing has been sent yet, it sets `Expires` to `ReturnWindow` from now, sends the first `AUTO_MATCH_CONNECT` (not a reminder), and logs that the slot is free; after that, it sends reminders as before. Once it has sent the player back, it never sends a notice, so the player's own loading connection, which reads as connected, can never earn one; a wait which runs out before then ends with `ConnectedElsewhereWaitExpiredMessage` (final review).

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`, `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockRefusalMonitorTests/*"`, and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockVerdict.cs TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchLockReturn.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs
git commit -m "Wait For The Other Machine Before Sending A Player Back"
```

---

### Task 18: Tell The Old Machine Why Its Chat Ended

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ClientHandshake.cs:73-103` (a notice before each `ECR_ACCOUNT_SHARING` reject)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `ReadCommandsUntil`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs`

**Interfaces:**
- Consumes: Task 15's `SeedSubAccount`; Task 6's `SendNotice`; the existing `ChatTestProtocol.ParseNotice`.
- Produces:
  - `internal const string LoggedInElsewhereSenderName = "Login";` on `ClientHandshake`.
  - `internal static string LoggedInElsewhereMessage(string accountName) => $@"You have logged in as ""{accountName}"" elsewhere, so you have been logged out here. Any game in progress here carries on.";` on `ClientHandshake`, naming the account which has just logged in.
  - `public static Task<List<(ushort Command, ChatBuffer Frame)>> ReadCommandsUntil(NetworkStream stream, ushort finalCommand, TimeSpan timeout)` on `ChatTestProtocol`: every frame in order, up to and including the first with the final command, or those read before the timeout.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test]
public async Task Second_Login_Tells_The_First_Session_Why_Just_Before_Rejecting_It()
{
    // The first client is accepted, then a second handshake for the same account follows, as in Second_Login_Disconnects_The_First_Session
    List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(firstStream, (ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT, TimeSpan.FromSeconds(10));

    using (Assert.Multiple())
    {
        await Assert.That(frames.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(frames[^1].Command).IsEqualTo((ushort) ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT);
        await Assert.That(frames[^2].Command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
        await Assert.That(ChatTestProtocol.ParseNotice(frames[^2].Frame)).IsEqualTo((ClientHandshake.LoggedInElsewhereSenderName, ClientHandshake.LoggedInElsewhereMessage(accountName)));
    }
}

[Test] public async Task A_Login_On_Another_Account_Of_The_User_Names_That_Account_To_The_First_Session()
    // the first client is the main account; the second handshake is a sub-account from SeedSubAccount: the notice before the reject names the sub-account
```

`Second_Login_Disconnects_The_First_Session` stays as it is.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/ClientHandshakeIntegrationTests/*"`
Expected: FAIL to compile, because `LoggedInElsewhereMessage` and `ReadCommandsUntil` do not exist.

- [ ] **Step 3: Implement the notice**

Both rejects, of the user's other accounts and of the same account, become `SendNotice(LoggedInElsewhereSenderName, LoggedInElsewhereMessage(account.Name)).Reject(ChatProtocol.ChatRejectReason.ECR_ACCOUNT_SHARING).Terminate()`. A comment says why: the game client's text for the reject talks about sharing linked sub-accounts, and it handles the notice first and shows it in the same chat panes.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ClientHandshake.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs
git commit -m "Tell The Old Machine Why Its Chat Ended"
```

---

### Task 19: Ignore A Refusal Which Arrives After The Player Is Back In Their Match

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs:41-45` (`ReportedGameServerAddress`, from which `ReportedJoiningGame` is derived)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/JoiningMatch.cs` (record the reported address) and `LeftMatch.cs` (clear it)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs:152-319` (`Enforce`'s `refusedConnection` becomes `relayedRefusal`, and `HandleRefusal` ignores a stale refusal)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs`

**Interfaces:**
- Consumes: Task 8's `HandleRefusal`; Task 16's `Enforce(ClientChatSession session, bool refusedConnection = false, bool groupMember = false)`; the rule of Task 6's `EndReturnOnJoiningGame` (`NamesLockedServer`).
- Produces:
  - `public string? ReportedGameServerAddress { get; set; }` on `ClientChatSession`: the address of the latest `CHAT_CMD_JOINING_GAME` on this chat connection, or NULL before any and after `CHAT_CMD_LEFT_GAME`.
  - `public bool ReportedJoiningGame => ReportedGameServerAddress is not null;` (its setter goes).
  - `public async Task<bool> Enforce(ClientChatSession session, bool relayedRefusal = false, bool groupMember = false)`: `refusedConnection` is renamed, because Task 23 relays the server list's refusals the same way.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockRefusalMonitorTests
[Test]
[Arguments(SlotTimeRemaining)]
[Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
public async Task A_Refusal_While_The_Player_Reports_The_Locked_Server_Is_Ignored(uint slotTimeRemaining)
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    ClientChatSession session = Context.ClientChatSessions[accountName];

    await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

    bool reported = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is not null, FrameTimeout);

    // The Other Match Server's Last Retry Of An Abandoned Join Arrives After The Player Is Back In Their Own Match, Whose Connection Reads As Connected
    await PublishRefusal(host, userID, accountName);

    (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

    using (Assert.Multiple())
    {
        await Assert.That(reported).IsTrue();
        await Assert.That(noticeMessages.Count).IsEqualTo(0);
        await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        await Assert.That(session.PendingMatchLockReturn).IsNull();
        await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
    }
}

[Test] public async Task Leaving_The_Match_After_An_Ignored_Refusal_Triggers_Nothing()
    // a connected slot; JOINING_GAME naming the lock's server, the refusal, then LEFT_GAME: no frame over twice ReturnResendInterval; no pending return; the lock remains
[Test] public async Task A_Refusal_After_The_Player_Left_The_Locked_Server_Sends_Them_Back()
    // JOINING_GAME naming the lock's server, LEFT_GAME, then the refusal: SlotAccountMessage, and an AUTO_MATCH_CONNECT for the lock's match
```

`A_Published_Refusal_Sends_The_Player_Back_Even_After_The_Client_Reported_Joining_A_Game` (another match server reported) and `A_Published_Refusal_Sends_The_Locked_Player_Back` (none reported) stay as they are, because both keep the full send-back.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockRefusalMonitorTests/*"`
Expected: FAIL to compile, because `ReportedGameServerAddress` does not exist.

- [ ] **Step 3: Implement the stale refusal**

`JoiningMatch` sets `ReportedGameServerAddress` to the reported address before it ends the pending return, and `LeftMatch` sets it to NULL. `HandleRefusal`, once it has the account's chat session, reads the user's lock; when there is one and the session's `ReportedGameServerAddress` names its match server (`NamesLockedServer`), it logs at debug level that the refusal is stale and returns, and otherwise it calls `Enforce(session, relayedRefusal: true)`. A comment says why: a client which reports being in its own match cannot be connecting elsewhere, so the refusal is the late answer to an attempt which it has abandoned, while a client which reports another match server may be in a chat reconnect during a refused join. `Enforce`'s summary and parameter documentation follow the rename.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"` and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/JoiningMatch.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/LeftMatch.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs
git commit -m "Ignore A Refusal Which Arrives After The Player Is Back In Their Match"
```

---

### Task 20: Tell A Locked Player At Login And Let The Client's Own Group Form

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs:9-51,287-300` (`SlotAccountLoginMessage`, and `NotifyAtLogin` in place of `EnforceAtLogin`)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Actions/TrackPlayerAction.cs:12-17`
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupCreate.cs` (no `MatchLockService`, and no `Enforce`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs`

**Interfaces:**
- Consumes: Task 3's `Verify`; Task 17's `KeepsLock`; Task 6's `MarkMatchLockLoginChecked`, `SendNotice`, and `OtherAccountMessage`.
- Produces:
  - `internal const string SlotAccountLoginMessage` on `MatchLockService`, worded as in Decisions.
  - `public async Task NotifyAtLogin(ClientChatSession session)`: nothing for an exempt account, for a session which reports a game, or when `MarkMatchLockLoginChecked(session.Metadata.SessionCookie)` is FALSE; otherwise it verifies the user's lock and, while `KeepsLock` accepts the verdict, sends the slot account `SlotAccountLoginMessage` and another account `OtherAccountMessage(lock.AccountName)`, from `MessageSenderName`. It never sends `AUTO_MATCH_CONNECT`, never starts a return or a wait, and never touches the pending return.
  - `EnforceAtLogin` is removed.
  - `public class GroupCreate(MerrickContext merrick)`. `GroupJoin` keeps its enforcement, because joining another player's group is an attempt to play.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockSendBackTests; replaces Logging_In_While_Locked_Sends_The_Player_Back_To_Their_Match and Logging_In_While_Connected_Elsewhere_Tells_The_Player_To_Close_The_Other_Game_And_Sends_Nothing_Back
[Test]
[Arguments(SlotTimeRemaining)]
[Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
public async Task Logging_In_While_Locked_Tells_The_Player_Without_Sending_Them_Back(uint slotTimeRemaining)
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

    // The Game Client's Own Reconnect Prompt Handles A Return At Login, So Neither A Free Nor A Still-Connected Slot Starts A Return
    (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2 + QuietPeriod);

    using (Assert.Multiple())
    {
        await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.SlotAccountLoginMessage]);
        await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn).IsNull();
        await Assert.That(gameSocket.Requests.Count).IsEqualTo(1);
        await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
    }
}

// MatchLockMatchmakingTriggerTests; replaces Creating_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back
[Test] [Arguments(ChatProtocol.TMMType.TMM_TYPE_PVP)] [Arguments(ChatProtocol.TMMType.TMM_TYPE_COOP)]
public async Task Creating_A_Group_While_Locked_Creates_It_And_Sends_Nothing(ChatProtocol.TMMType groupType)
    // GROUP_CREATE of that type: a TMM_GROUP_UPDATE arrives, and the account leads its group alone; no notice and no AUTO_MATCH_CONNECT over twice ReturnResendInterval; no pending return; the lock remains
```

The other tests change as follows:
- `Logging_In_On_Another_Account_Of_A_Locked_User_Names_The_Slot_Account_And_Sends_Nothing_Back` keeps its name and assertions, and adds that `PendingMatchLockReturn` is NULL.
- `Logging_In_Again_With_The_Same_Session_Cookie_Sends_Nothing` reads the first login's notice through a helper `LogInAndReadNotice(NetworkStream stream)`, which asserts `SlotAccountLoginMessage`.
- Every other test which started its return or its wait with `AC_DAILY_LOGINS` starts it with a direct `Enforce` on the session instead, keeping its assertions; `LogInAndReadSendBack` becomes `RefuseAndReadSendBack(ChatServerHost host, string accountName, NetworkStream stream)`, which calls `Enforce` and reads the slot notice and the first `AUTO_MATCH_CONNECT`.
- `Logging_In_While_Reporting_A_Game_Sends_Nothing`, `A_Lock_Whose_Slot_Is_Gone_Lifts_At_Login_And_Sends_Nothing`, `Another_Account_Is_Named_The_Slot_Account_While_The_Slot_Is_Connected_Elsewhere`, and `Guest_And_Staff_Accounts_Are_Never_Sent_Back` stay as they are.
- `Joining_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back` stays as it is, because joining another player's group is still a trigger.
- `Creating_A_Group_While_Still_In_The_Match_Gives_Only_The_In_Match_Notice` is removed: group create no longer enforces, so a player in their match creates a group like anyone else, which `Creating_A_Group_While_Locked_Creates_It_And_Sends_Nothing` covers.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`
Expected: FAIL to compile, because `SlotAccountLoginMessage` does not exist.

- [ ] **Step 3: Implement the login notice and free the client's own group**

`TrackPlayerAction` routes `AC_DAILY_LOGINS` to `NotifyAtLogin`, and its comment says that the action tells a locked player at login. `NotifyAtLogin` logs each notice at information level, naming the account, the match, and the match server. `GroupCreate` acts at once; it loses the comment about refusing groups of any type and gains one saying why the group stands: the game client creates a solo group by itself shortly after login and whenever it returns to the menu, and the lock refuses the queue instead.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Actions/TrackPlayerAction.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupCreate.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs
git commit -m "Tell A Locked Player At Login And Let The Client's Own Group Form"
```

---

### Task 21: Refuse A Locked Player's Queue And Group Join Without Locking Their Screen

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs:9-51,152-285` (`Enforce`'s `matchmaking`, `BanSecondsRemaining`, and `EnforceForGroup`; remove `InMatchMessage`, `GroupMemberMessage`, and `groupMember`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs:381-416` (add `SendMatchmakingBan`; remove `TryClaimInMatchNotice` and `LastInMatchNoticeTicks`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroup.cs:305-322,487-506,691-779` (`SendLoadingStatusUpdate`, `JoinQueue`, and `RemoveMember`)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoin.cs:10` and `GroupJoinQueue.cs:32-33`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `ParseMatchmakingFailure` and `ReadMatchmakingFailure`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs`

**Interfaces:**
- Consumes: Task 19's `Enforce(session, relayedRefusal, groupMember)`; Task 7's `EnforceForGroup`; `MatchmakingGroup.UnloadAndUnreadyMembers` and `MulticastUpdate`; Task 18's `ReadCommandsUntil`.
- Produces:
  - `public async Task<bool> Enforce(ClientChatSession session, bool relayedRefusal = false, bool matchmaking = false)`: for a refused queue or group join (`matchmaking`) of a session which does not report a game, and which this attempt does not pull back at once, it first sends `SendMatchmakingBan(BanSecondsRemaining(matchLock, now))`, then removes the session from its current group with `RemoveMember(…, notifyRemovedMember: false)`, because its client has left that group, and only then sends the notice. That covers another account, and a slot account which waits for its other connection; a slot account pulled back at once gets the notice and the pull alone. A session which reports a game gets no frame and no return.
  - `internal static uint BanSecondsRemaining(MatchLock matchLock, DateTimeOffset now)`: the whole seconds left until `matchLock.TimestampExpires`, rounded up, and 0 once it has passed.
  - `public async Task<bool> EnforceForGroup(MatchmakingGroup group)`: enforces every member concurrently with `matchmaking: true`; when the queue is refused and the group had more than one member, it unreadies the members which remain (`UnloadAndUnreadyMembers`) and sends them one `TMM_FULL_GROUP_UPDATE`, which unlocks a party's screens, and then sends every member which the group had when the queue started, the leader and the refused members included, one `GroupMemberLockedMessage` per refused member, naming it with `Account.NameWithClanTag`; TRUE when any member is refused.
  - `internal static string GroupMemberLockedMessage(string lockedMemberNameWithClanTag)` on `MatchLockService`, worded as in Decisions.
  - `public ClientChatSession SendMatchmakingBan(uint secondsRemaining)` on `ClientChatSession`: `NET_CHAT_CL_TMM_FAILED_TO_JOIN`, the byte `TMMFTJR_BANNED`, then the seconds as a 32-bit integer, the layout which `CChatManager::HandleTMMJoinFailed` reads.
  - `public void RemoveMember(int accountID, bool kick = false, bool notifyRemovedMember = true)`: FALSE sends the removed member no group update, because its client has already left the group, while the remaining members are updated as before.
  - `public async Task JoinQueue(MerrickContext merrick, MatchLockService matchLockService)`.
  - On `ChatTestProtocol`: `public static (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds) ParseMatchmakingFailure(ChatBuffer frame)` (the seconds are read only for `TMMFTJR_BANNED`, as the client reads them, and are 0 otherwise) and `public static async Task<(ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)?> ReadMatchmakingFailure(NetworkStream stream, TimeSpan timeout)`.
  - Removed, for the reasons in Decisions: `MatchLockService.InMatchMessage`, `GroupMemberMessage` (which `GroupMemberLockedMessage` replaces), `ClientChatSession.TryClaimInMatchNotice` and `LastInMatchNoticeTicks`, `Enforce`'s `groupMember`, and the `leaderActed` parameters of `EnforceForGroup` and `JoinQueue`.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockMatchmakingTriggerTests
[Test]
public async Task Queueing_On_Another_Account_Shows_The_Temporary_Ban_Before_The_Notice()
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    try
    {
        MatchmakingGroup group = await CreateGroup(stream, accountID);

        // Another Account Of The Same User Holds The Slot, So This Account Has No Match To Return To, And Its Matchmaking Screen Covers The Chat
        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), "TEST:SlotAccount", gameSocket);

        MakeReadyToQueue(group);

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

        (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure = await ChatTestProtocol.ReadMatchmakingFailure(stream, FrameTimeout);

        // The Notice Follows The Failure Reply, Because Reading Up To The Failure Reply Would Have Skipped A Notice Which Came First
        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
        List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
            await Assert.That(failure?.Seconds).IsLessThanOrEqualTo((uint) MatchLock.Lifetime.TotalSeconds);
            await Assert.That(failure?.Seconds).IsGreaterThan((uint) (MatchLock.Lifetime - TimeSpan.FromMinutes(1)).TotalSeconds);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName));
            await Assert.That(later.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
            await Assert.That(group.QueueStartTime).IsNull();
            await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
        }
    }

    finally
    {
        DisbandGroups(accountID);
    }
}

[Test] public async Task Queueing_While_Connected_Elsewhere_Shows_The_Temporary_Ban_Before_The_Notice()
    // the slot account, with a connected slot: the TMMFTJR_BANNED failure reply, then ConnectedElsewhereMessage; no AUTO_MATCH_CONNECT and no TMM_GROUP_UPDATE within QuietPeriod; the group is gone
[Test] public async Task Joining_A_Group_On_Another_Account_Shows_The_Temporary_Ban_Before_The_Notice()
    // another account of a locked user sends GROUP_JOIN naming an unlocked leader: the failure reply, then OtherAccountMessage; the leader's group keeps one member; the account's own group is gone
[Test] public async Task Queueing_While_Locked_Is_Refused_And_Sends_The_Player_Back()
    // keeps its name and expectations, and adds: the frames up to the first AUTO_MATCH_CONNECT hold no NET_CHAT_CL_TMM_FAILED_TO_JOIN and no TMM_GROUP_UPDATE,
    // and no TMM_GROUP_UPDATE follows within QuietPeriod, because the client leaves its solo group by itself on the pull; the match lock's notices are exactly [SlotAccountMessage], because the party notice is only for a group of more than one member
[Test] public async Task Reaching_Full_Loading_While_Locked_Does_Not_Queue()
    // keeps its name and expectations, and adds the same, which pins that the loading update no longer follows the pull
// Joining_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back keeps its name and expectations, and adds that no failure reply arrives
// replaces Queueing_A_Group_With_A_Locked_Member_Tells_The_Leader_And_Sends_The_Member_Back
[Test] public async Task Queueing_A_Party_With_A_Locked_Member_Unreadies_It_And_Sends_The_Member_Back()
    // the party is formed before the lock, as a premade party outlives its match; the leader queues: the member reads its slot notice and AUTO_MATCH_CONNECT, no failure reply, and stays in the party;
    // the leader reads a TMM_GROUP_UPDATE and no failure reply; the match lock's notices are exactly [GroupMemberLockedMessage(member)] for the leader, and [SlotAccountMessage, GroupMemberLockedMessage(member)] for the member;
    // the leader is not ready, every LoadingPercent is 0, and QueueStartTime is NULL
// replaces Queueing_A_Group_With_A_Member_Still_In_The_Match_Does_Not_Message_The_Member
[Test] public async Task Queueing_A_Party_With_A_Member_Still_In_The_Match_Unreadies_It_And_Tells_Every_Member()
    // the member reports joining the lock's server: the member reads no failure reply and no AUTO_MATCH_CONNECT, and has no pending return; the leader and the member each read exactly [GroupMemberLockedMessage(member)];
    // the leader reads a TMM_GROUP_UPDATE; the party is unreadied
// replaces Queueing_As_A_Locked_Leader_Sends_The_Leader_Back_Without_A_Group_Message
[Test] public async Task Queueing_A_Party_As_Its_Locked_Leader_Unreadies_It_And_Sends_The_Leader_Back()
    // the leader reads its slot notice and AUTO_MATCH_CONNECT and no failure reply; the member reads a TMM_GROUP_UPDATE; each reads GroupMemberLockedMessage(leader) once
[Test] public async Task Queueing_A_Party_With_Two_Locked_Members_Tells_Every_Member_About_Both()
    // with teams of three, two members are locked: each of the three members reads one GroupMemberLockedMessage naming each locked member, two in all
// replaces Queueing_While_Still_In_The_Match_Gives_The_Leader_Only_The_In_Match_Notice
[Test] public async Task Queueing_While_Still_In_The_Match_Gives_No_Notice_And_No_Return()
    // JOINING_GAME naming the lock's server; GROUP_JOIN_QUEUE: no notice, no failure reply, and no AUTO_MATCH_CONNECT over twice ReturnResendInterval; QueueStartTime is NULL; no pending return
// Reaching_Full_Loading_While_Still_In_The_Match_Gives_No_Notice and Reaching_Full_Loading_Without_A_Lock_Still_Queues stay as they are

// MatchLockSendBackTests; replaces A_Player_Still_In_The_Match_Is_Told_Why_Once_Per_Notice_Repeat_Interval
[Test] [Arguments(SlotTimeRemaining)] [Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
public async Task A_Player_Still_In_The_Match_Is_Refused_Without_A_Notice_Or_A_Return(uint slotTimeRemaining)
    // JOINING_GAME naming the lock's server; two direct Enforce calls: both TRUE; no notice and no AUTO_MATCH_CONNECT over twice ReturnResendInterval; PendingMatchLockReturn is NULL

// MatchLockVerificationTests
[Test] public async Task The_Ban_Countdown_Is_The_Time_Left_Until_The_Safety_Limit()
    // a lock started at now: (uint) MatchLock.Lifetime.TotalSeconds; started MatchLock.Lifetime less 1.5 seconds ago: 2; started MatchLock.Lifetime plus a second ago: 0
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: FAIL to compile, because `ReadMatchmakingFailure`, `BanSecondsRemaining`, and `GroupMemberLockedMessage` do not exist.

- [ ] **Step 3: Implement the refusals which the client can see**

- `Enforce`'s in-game shortcut refuses with no frame and logs at debug level, as before but without the notice. After it, a refused `matchmaking` attempt which does not pull the slot account back at once gets the failure reply and leaves its group in NEXUS, in the order which the Interfaces give, including when only a notice follows: another account, a wait for the slot, a continued return, or a lost race to install the return. The failure reply is not held back by `NoticeRepeatInterval`. A comment says why: the matchmaking screen covers the chat, the "Temporary Ban" popup is the client's only refusal whose text fits a match lock, and the client leaves its group on that reply without telling the chat server; a slot account pulled back at once needs none of it, because the client leaves a solo group by itself on the pull (`CChatManager::MatchmakingConnect`, "foundmatch").
- `GroupJoin` calls `Enforce(session, matchmaking: true)`.
- `EnforceForGroup` enforces every member with `matchmaking: true`, with a comment saying why it still does: a premade party outlives the match it played together, so a locked player can queue with its teammates. It unreadies a refused party's remaining members, then tells every member which the party had when the queue started, the leader and the refused members included, about each refused member, as the Interfaces give, with a comment saying why: so that nobody blames the leader.
- `SendLoadingStatusUpdate` sends its partial update before it starts the queue, so that no group update follows a pull, and none goes to a group which the queue's refusal has emptied.
- `JoinQueue` passes the group alone to `EnforceForGroup`, and `GroupJoinQueue` calls `JoinQueue(merrick, matchLockService)`.
- The summaries of `Enforce`, `EnforceForGroup`, and `JoinQueue` lose every mention of the in-match notice, of the leader's own request, and of the leader's message.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`, `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockVerificationTests/*"`, `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchmakingGroupReadinessTests/*"`, and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchmakingGroupLookupTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs TRANSMUTANSTEIN.ChatServer/Domain/Matchmaking/MatchmakingGroup.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoin.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupJoinQueue.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockVerificationTests.cs
git commit -m "Refuse A Locked Player's Queue And Group Join Without Locking Their Screen"
```

---

### Task 22: Reject A Locked Player's Direct Join At Once

**Files:**
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs:350-367` (the final reply)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (`RefusalLiftedMessage`, sent by `HandleRefusal`)
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs`

**Interfaces:**
- Consumes: Task 8's refusal, `HandleRefusal`, and `MasterServerHost.ConnectClient`; Task 19's stale refusal.
- Produces:
  - The refusal's body: a PHP-serialised array whose only entry is `account_id`, the account's ID, with HTTP 409, as Decisions gives it.
  - `internal const string RefusalLiftedMessage` on `MatchLockService`, worded as in Decisions.
  - `AssertClientConnectionIsRejectedAtOnce(HttpResponseMessage response, string accountName)` replaces `AssertClientConnectionIsRefused` in `HostingAuthenticationTests.MatchLock.cs`: 409, and a body which deserialises to exactly one entry, `account_id`, equal to the account's ID, so that it carries no `game_cookie`, `cookie`, `auth_failure`, or `connect`.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockRefusalMonitorTests
[Test]
public async Task A_Refusal_Which_Finds_The_Lock_Lifted_Tells_The_Player_To_Try_Again()
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

    // The Player Was Removed From The Match, Which Its Match Server Answers With No Time Remaining On The Slot
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    await PublishRefusal(host, userID, accountName);

    (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
    (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, QuietPeriod);

    using (Assert.Multiple())
    {
        await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.RefusalLiftedMessage);
        await Assert.That(connect).IsNull();
        await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
    }
}

// replaces A_Removed_Player_Without_Chat_Is_Served_On_The_Engine_Retry
[Test] public async Task A_Removed_Player_Without_Chat_Is_Served_On_Their_Next_Attempt()
    // as before: the first c_conn answers 409 with the account's ID alone; the lock lifts; the second answers 200 with a "game_cookie"

// HostingAuthenticationTests.MatchLock.cs: every test which called AssertClientConnectionIsRefused calls AssertClientConnectionIsRejectedAtOnce with the connecting account's name, so each refusal also pins the final reply
```

`A_Refused_Client_Connection_Publishes_The_User_ID_And_Account_Name` keeps asserting 409. `Client_Connection_With_A_Retired_Session_Cookie_Is_Refused_Without_Blacklisting_It` stays as it is, because a retired cookie is not a lock refusal, and the engine still retries it like any unreadable reply.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"` and `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockRefusalMonitorTests/*"`
Expected: FAIL; the master server's refusals carry no `account_id`, and the chat server's `RefusalLiftedMessage` does not exist, so the chat tests fail to compile.

- [ ] **Step 3: Implement the final reply**

- `HandleConnectClient` refuses with the final reply. A comment says why: the engine retries any reply which it cannot read while the client waits, and blacklists the login cookie on "Invalid cookie.", while a reply which names the account but carries no game cookie disconnects the client at once (`DISCONNECT_UNIQUE_LOGIN_FAILED`), with no retry, no blacklisting, and no harm to the match server's session (`CClientConnection::CheckAuthResult`).
- `HandleRefusal` sends `RefusalLiftedMessage` from `MessageSenderName`, and logs it at information level, when `Enforce` lets the attempt through, with a comment saying why: nothing repeats a refused server list or a finally rejected join.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the commands from Step 2.
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.MatchLock.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockRefusalMonitorTests.cs
git commit -m "Reject A Locked Player's Direct Join At Once"
```

---

### Task 23: Refuse The Server List While Locked

**Files:**
- Modify: `KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs` (add `FindRefusingMatchLock`)
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs:401-427` (call the shared rule; the private method goes)
- Modify: `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.ServerList.cs`
- Modify: `KONGOR.MasterServer/Models/ServerManagement/MatchLockRefusal.cs` and `TRANSMUTANSTEIN.ChatServer/Services/MatchLockRefusalMonitor.cs` (their summaries cover the server list)
- Create: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.ServerList.cs`

**Interfaces:**
- Consumes: Task 8's `PublishMatchLockRefusal` and the partial class's helpers (`RegisterMatchServer`, `LockAccount`, `CreateSubAccount`, `CreateClientSessionCookie`, `DeserialisePHPResponse`, `SubscribeToRefusals`, and `ReadRefusal`); `ServerForJoinListResponse` and `ServerForCreateListResponse`.
- Produces:
  - `public static async Task<(MatchLock? RefusingLock, MatchLock? LiftedLock)> FindRefusingMatchLock(this IDatabase distributedCacheStore, Account account, int? askingServerID)`: no lock for an exempt account; a lock whose match server is no longer registered is removed and returned as `LiftedLock`; otherwise the lock refuses when `askingServerID` is NULL, for a request which names no match server, or names another match server.
  - Test helpers in the new file: `RequestServerList(string cookie, string gameType)` posts `client_requester.php?f=server_list` with the `cookie` and `gametype` form values; `SetMatchServerStatus(int serverID, ServerStatus status)` rewrites the registered match server's status through `SetMatchServer`.

- [ ] **Step 1: Write the failing tests**

```csharp
// HostingAuthenticationTests.ServerList.cs
[Test]
[NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
[Arguments("10")]
[Arguments("90")]
public async Task The_Server_List_Of_A_Locked_Account_Is_Empty_And_Relays_A_Refusal(string gameType)
{
    await InitialiseIn("Development");

    (_, int lockedServerID) = await RegisterMatchServer(1);
    (_, int activeServerID) = await RegisterMatchServer(2);
    (_, int idleServerID) = await RegisterMatchServer(3);

    // One Match Server Is Active And Another Is Idle, So That Neither Kind Of List Would Be Empty Without The Lock
    await SetMatchServerStatus(activeServerID, ServerStatus.SERVER_STATUS_ACTIVE);
    await SetMatchServerStatus(idleServerID, ServerStatus.SERVER_STATUS_IDLE);

    MatchLock matchLock = await LockAccount(NonHostAccountName, lockedServerID);

    string cookie = await CreateClientSessionCookie(NonHostAccountName);

    ChannelMessageQueue refusals = await SubscribeToRefusals();

    try
    {
        HttpResponseMessage response = await RequestServerList(cookie, gameType);

        IDictionary<object, object> body = await DeserialisePHPResponse(response);

        MatchLockRefusal? refusal = await ReadRefusal(refusals, RefusalTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(body["server_list"] is ICollection { Count: 0 }).IsTrue();
            await Assert.That(body.ContainsKey("acc_key")).IsTrue();
            await Assert.That(refusal?.UserID).IsEqualTo(matchLock.UserID);
            await Assert.That(refusal?.AccountName).IsEqualTo(NonHostAccountName);
        }
    }

    finally
    {
        await refusals.UnsubscribeAsync();
    }
}

[Test] [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)] [Arguments("10")] [Arguments("90")]
public async Task The_Server_List_Of_An_Unlocked_Account_Lists_The_Servers(string gameType)
    // the same match servers without a lock: server_list is not empty, and nothing is published within RefusalQuietPeriod
[Test] [NotInParallel(DistributedCacheExtensions.MatchLockRefusalChannel)]
public async Task The_Server_List_Of_Another_Account_Of_A_Locked_User_Is_Empty()
    // the lock is held by the main account; the sub-account's list of "90" is empty, and the refusal names the sub-account
[Test] [Arguments(AccountType.Guest)] [Arguments(AccountType.Staff)]
public async Task The_Server_List_Is_Never_Refused_For_Guest_And_Staff_Accounts(AccountType accountType)
    // the account set to the type, as the client connection test sets it: the list of "90" is not empty
[Test] public async Task The_Server_List_Is_Served_And_The_Lock_Lifted_When_The_Locked_Server_Is_Gone()
    // a lock on an unregistered server ID: the list of "90" is not empty, and GetMatchLock is NULL
[Test] public async Task The_Server_List_Is_Served_When_The_Match_Lock_Cannot_Be_Read()
    // the stored lock overwritten as Client_Connection_Is_Served_When_The_Match_Lock_Cannot_Be_Read does: the list of "90" is not empty
```

In every test which expects a list which is not empty, one registered match server is set idle with `SetMatchServerStatus`, so that no test depends on the status which a newly registered match server starts with.

Every client connection test of the match lock stays as it is, because the shared rule refuses exactly what the private one did.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/HostingAuthenticationTests/*"`
Expected: FAIL, because a locked account's server list still lists the match servers, and nothing is published.

- [ ] **Step 3: Implement the refusal of the server list**

- `GetServerList` keeps rejecting an unknown game type first. It then resolves the account from the cookie, with its user and its clan, and calls `FindRefusingMatchLock(account, askingServerID: null)` in a `try` which logs an error and serves the full list on any exception, as `c_conn` does.
- It logs a lifted lock at information level, as `c_conn` did. For a refusing lock, it publishes the `MatchLockRefusal` (best effort, a failure logged at error level), logs the refusal at information level, naming the account, the match, and the match server, and builds the requested response from an empty list of match servers. A comment says why: the client clears, reads, and redraws its list from any reply (`CServerList::ProcessServerList`), so a successful, empty list leaves it nothing to join and a game creation no match server to take, including the one which the client picks by itself for a new game (`CHostClient::StartAutoRemoteGame` asks for the list of game type "90").
- `HandleConnectClient` calls the shared rule with the asking server's ID, and logs its `LiftedLock` with the line which the private method logged.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer --treenode-filter "/*/*/ServerListRegionFilteringTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Extensions/Cache/DistributedCacheExtensions.MatchLock.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.ServerList.cs KONGOR.MasterServer/Models/ServerManagement/MatchLockRefusal.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockRefusalMonitor.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/HostingAuthenticationTests.ServerList.cs
git commit -m "Refuse The Server List While Locked"
```

---

### Task 24: Use Clan Tags And Match Lock Wording In Notices And Logs

**Files:**
- Modify: `KONGOR.MasterServer/Models/ServerManagement/MatchLock.cs` (`AccountNameWithClanTag` and `TaggedAccountName`)
- Modify: `KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs` and `KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.ServerList.cs` (the match lock's log lines)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (`MessageSenderName`, `SlotAccountMessage`, `OtherAccountMessage`, `LockMatchRoster`, `NotifyAtLogin`, `Notify`, and every log line which names an account)
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ClientHandshake.cs:102,114`
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs` (add `SeedClanMember`; `SeedLockedMatch` takes the clan-tagged name)
- Create: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockMessageTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockCreationTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs`
- Test: `ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs`

**Interfaces:**
- Consumes: Task 20's `NotifyAtLogin` and `SlotAccountLoginMessage`; Task 22's `RefusalLiftedMessage`; Task 23's server list log lines; Task 18's `LoggedInElsewhereMessage`; `Account.NameWithClanTag`.
- Produces:
  - `public string? AccountNameWithClanTag { get; init; }` on `MatchLock`, optional, so that a lock stored without it reads as NULL.
  - `[System.Text.Json.Serialization.JsonIgnore] public string TaggedAccountName => AccountNameWithClanTag ?? AccountName;` on `MatchLock`.
  - `internal const string MessageSenderName = "Match Lock";`, and `SlotAccountMessage` and `internal static string OtherAccountMessage(string slotAccountNameWithClanTag)`, worded as in Decisions; `Notify` and `NotifyAtLogin` pass the lock's `TaggedAccountName`.
  - `public static async Task<string> SeedClanMember(IServiceProvider services, int accountID)` on `ChatTestData`: puts the account in a new clan with a unique tag of four capital letters, and returns the account's name with that tag, as `Account.NameWithClanTag` writes it.
  - `SeedLockedMatch(…, string? accountNameWithClanTag = null)`, which records it on the lock.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockSendBackTests
[Test]
[Arguments(true)]
[Arguments(false)]
public async Task Another_Account_Is_Named_With_The_Slot_Account_Clan_Tag(bool atLogin)
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

    (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

    // The Slot Is Held By Another Account Of The Same User, Which Was In A Clan When The Match Was Announced
    string slotAccountNameWithClanTag = $"[SLOT]{SlotAccountName}";

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket, accountNameWithClanTag: slotAccountNameWithClanTag);

    using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

    NetworkStream stream = client.GetStream();

    if (atLogin)
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

    else
        await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

    (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

    (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

    using (Assert.Multiple())
    {
        await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.OtherAccountMessage(slotAccountNameWithClanTag));
        await Assert.That(noticeMessages.Count).IsEqualTo(0);
        await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
    }
}

// MatchLockMessageTests
[Test] public async Task Notices_Call_The_Restriction_A_Match_Lock_And_Never_A_Ban()
    // SlotAccountMessage, SlotAccountLoginMessage, OtherAccountMessage("[TAG]Slot"), RefusalLiftedMessage, and GroupMemberLockedMessage("[TAG]Member") each contain "match lock",
    // none contains the word "ban" in any letter case, and MessageSenderName is "Match Lock"
// MatchLockCreationTests
[Test] public async Task Locking_A_Competitive_Match_Records_The_Clan_Tagged_Name_Of_Each_Player()
    // one roster member in a clan from SeedClanMember: its lock's AccountNameWithClanTag is that name and its AccountName the bare one; a member without a clan records its bare name in both
// ClientHandshakeIntegrationTests
[Test] public async Task A_Login_Elsewhere_Names_The_Account_With_Its_Clan_Tag()
    // the account joins a clan through SeedClanMember before either handshake: the notice before the reject is LoggedInElsewhereMessage(that name)
// MatchLockStoreTests (KONGOR.MasterServer)
[Test] public async Task A_Lock_Stored_Without_A_Clan_Tagged_Name_Names_The_Bare_Account()
    // the lock's key holds JSON written without the property: GetMatchLock reads AccountNameWithClanTag as NULL, and TaggedAccountName as AccountName
```

Every test which builds `OtherAccountMessage` passes the lock's `TaggedAccountName`; with locks seeded without a clan-tagged name, its expectations are unchanged.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockSendBackTests/*"`
Expected: FAIL to compile, because `SeedLockedMatch` takes no clan-tagged name.

- [ ] **Step 3: Implement the clan tags and the wording**

- `LockMatchRoster` records `member.Account.NameWithClanTag` as `AccountNameWithClanTag`, and both rejects in `ClientHandshake` pass `account.NameWithClanTag` to `LoggedInElsewhereMessage`.
- Every log line of the match lock which names an account, in `MatchLockService`, `HandleConnectClient`, and `GetServerList`, uses `Account.NameWithClanTag`, or the lock's `TaggedAccountName` for the slot account. `MatchLockRefusal.AccountName` stays bare, with a comment saying why: it finds the chat session, which is registered under the bare name.
- `MessageSenderName`, `SlotAccountMessage`, and `OtherAccountMessage` take the wording of Decisions, which calls the restriction a match lock and asks no player to type a tagged name.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `git grep -n -E "(session|matchLock|currentLock)\.(Account\.Name|AccountName)\b" -- TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs`, the whole chat server suite, `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer`, and the whole master server suite, `dotnet test --project ASPIRE.Tests.KONGOR.MasterServer`
Expected: PASS, and no output from `git grep`.

- [ ] **Step 5: Commit**

```bash
git add KONGOR.MasterServer/Models/ServerManagement/MatchLock.cs KONGOR.MasterServer/Controllers/ServerRequester/ServerRequesterController.Authentication.cs KONGOR.MasterServer/Controllers/ClientRequester/ClientRequesterController.ServerList.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ClientHandshake.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestData.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockMessageTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockCreationTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockSendBackTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Integration/ClientHandshakeIntegrationTests.cs ASPIRE.Tests.KONGOR.MasterServer/Tests/ServerManagement/MatchLockStoreTests.cs
git commit -m "Use Clan Tags And Match Lock Wording In Notices And Logs"
```

---

### Task 25: Refuse Invites To And From A Locked Player

**Files:**
- Modify: `TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupInvite.cs` (asynchronous, with `MatchLockService`)
- Modify: `TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs` (`InviteeLockedMessage` and `HoldsLock`)
- Modify: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs` (add `BuildGroupInvite`)
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs`
- Test: `ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockMessageTests.cs`

**Interfaces:**
- Consumes: Task 21's `Enforce`; Task 24's `MessageSenderName` and wording; Task 3's `Verify`; Task 17's `KeepsLock`.
- Produces:
  - `public class GroupInvite(MerrickContext merrick, MatchLockService matchLockService) : IAsynchronousCommandProcessor<ClientChatSession>`.
  - `internal static string InviteeLockedMessage(string inviteeNameWithClanTag)` on `MatchLockService`, worded as in Decisions.
  - `public async Task<bool> HoldsLock(ClientChatSession session)` on `MatchLockService`: TRUE while the session's user, unless exempt, has a lock which `Verify` keeps, with no notice and no return. It verifies with `unconfirmedSlotLifts: false` (the amendment's final review), because a third party's invite is not the invitee's own attempt to play: an unanswered reconnect query gives FALSE, so the invite is delivered, and keeps the lock, so that the invitee's own group join decides, while a match server which is gone, or has ended the match, and a slot which is gone, still lift it.
  - `public static ChatBuffer BuildGroupInvite(string inviteReceiverName)` on `ChatTestProtocol`, in the layout which `GroupInvite` reads.

- [ ] **Step 1: Write the failing tests**

```csharp
// MatchLockMatchmakingTriggerTests
[Test]
[Arguments(false)]
[Arguments(true)]
public async Task Inviting_A_Locked_Player_Is_Not_Delivered_And_Tells_The_Inviter(bool inviteeStillInTheMatch)
{
    await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
    await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(inviteeStillInTheMatch ? FakeMatchServerGameSocket.ConnectedSlotTimeRemaining : SlotTimeRemaining);

    (_, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
    (int inviteeUserID, int inviteeAccountID, string inviteeAccountName, string inviteeCookie) = await ChatTestData.SeedPlayer(host);

    await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, inviteeUserID, inviteeAccountID, inviteeAccountName, gameSocket);

    using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
    using TcpClient inviteeClient = await host.ConnectAndAuthenticateClientAsync(inviteeAccountID, inviteeAccountName, inviteeCookie, RemoteIP);

    NetworkStream inviterStream = inviterClient.GetStream();
    NetworkStream inviteeStream = inviteeClient.GetStream();

    ClientChatSession inviteeSession = Context.ClientChatSessions[inviteeAccountName];

    try
    {
        MatchmakingGroup group = await CreateGroup(inviterStream, inviterAccountID);

        // A Player Still In Their Match Reports Joining Its Match Server, And Is Locked All The Same
        if (inviteeStillInTheMatch)
        {
            await ChatTestProtocol.WriteFrame(inviteeStream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            await ChatTestProtocol.WaitUntil(() => inviteeSession.ReportedJoiningGame, FrameTimeout);
        }

        await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(inviteeAccountName));

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(inviterStream, FrameTimeout);

        ChatBuffer? broadcast = await ChatTestProtocol.ReadUntilCommand(inviterStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST, QuietPeriod);
        ChatBuffer? invite = await ChatTestProtocol.ReadUntilCommand(inviteeStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.InviteeLockedMessage(inviteeAccountName));
            await Assert.That(broadcast).IsNull();
            await Assert.That(invite).IsNull();
            await Assert.That(group.Members.Count).IsEqualTo(1);
            await Assert.That(inviteeSession.PendingMatchLockReturn).IsNull();
        }
    }

    finally
    {
        DisbandGroups(inviterAccountID);
    }
}

[Test] public async Task A_Locked_Player_Inviting_Someone_Is_Sent_Back_And_The_Invite_Is_Not_Delivered()
    // the inviter is the locked slot account: SlotAccountMessage and an AUTO_MATCH_CONNECT; the receiver reads no NET_CHAT_CL_TMM_GROUP_INVITE within QuietPeriod
[Test] public async Task Another_Account_Of_A_Locked_User_Inviting_Someone_Is_Told_Why()
    // OtherAccountMessage; no AUTO_MATCH_CONNECT and no failure reply; the receiver reads no invite
[Test] public async Task Inviting_A_Player_Who_Is_Not_Locked_Still_Delivers_The_Invite()
    // the receiver reads a NET_CHAT_CL_TMM_GROUP_INVITE, and the inviter reads no notice within QuietPeriod
// MatchLockMessageTests: Notices_Call_The_Restriction_A_Match_Lock_And_Never_A_Ban adds InviteeLockedMessage("[TAG]Invitee") to its cases
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMatchmakingTriggerTests/*"`
Expected: FAIL to compile, because `BuildGroupInvite` and `InviteeLockedMessage` do not exist.

- [ ] **Step 3: Implement the refused invites**

`GroupInvite` first calls `Enforce` on the inviter and returns when it is TRUE, because a locked player's invite is an attempt to play. When the receiver has a chat session (`Context.ClientChatSessions`, keyed by the bare name which the client sends) and `HoldsLock(receiver)` is TRUE, it sends the inviter `InviteeLockedMessage(receiver.Account.NameWithClanTag)` from `MessageSenderName`, logs the refusal at information level with both accounts' clan-tagged names, and returns, so that neither the invite nor its broadcast is sent; otherwise it invites as before. A comment says why there is no failure reply: `CChatManager::HandleTMMJoinFailed` would make the inviter's client leave its own group, and inviting leaves nothing to unlock on the inviter's screen.

- [ ] **Step 4: Run the tests to verify they pass**

Run: the command from Step 2, then `dotnet test --project ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer --treenode-filter "/*/*/MatchLockMessageTests/*"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add TRANSMUTANSTEIN.ChatServer/CommandProcessors/Matchmaking/GroupInvite.cs TRANSMUTANSTEIN.ChatServer/Services/MatchLockService.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Infrastructure/ChatTestProtocol.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Matchmaking/MatchLockMatchmakingTriggerTests.cs ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer/Tests/Services/MatchLockMessageTests.cs
git commit -m "Refuse Invites To And From A Locked Player"
```

---

### Task 12: Verify The Match Lock With Real Game Clients (Needs The Owner)

**This task needs the owner, and runs last, after Tasks 19 to 25.** The controller prepares and observes; the owner plays the clients. The first live run (2026-10-21) passed most of the scenarios which it reached, and found what Tasks 19 to 25 change (the spec's "Live Test Amendment (2026-10-21)"). This run re-tests what they change, then carries on with the scenarios which the first run did not reach; the first run's passes are not repeated.

**Files:** none changed. Outcomes are recorded in the branch's working notes.

**Set-up:**
- [ ] Stage the build over the GEMINI game files, with one folder for the match servers and one for each client, and give every folder's path a space, because the launchers' command lines need one until the HON follow-up on Windows command lines lands.
- [ ] Start the match servers through COMPEL only, never by hand, from a local `COMPEL.json` in the match server folder: host mode, the local gateway, no CDN synchronisation, no proxy, and at least two instances (one for the matchmade match, and one for a public or custom game).
- [ ] On a single PC, set COMPEL's `PortRangeOffset` in that file, so that the instances stay off the port which a client's practice game opens. A practice game which cannot open its port shows that clash, not a problem with the lock.
- [ ] Launch each client through WILLOWMAKER, from its own folder.
- [ ] Start NEXUS through the Aspire AppHost in Development before COMPEL. It plays one player per team, as `TRANSMUTANSTEIN.ChatServer/appsettings.Development.json` sets, and the party scenario raises the team size there for its own run.
- [ ] Use normal accounts only, never guest or staff accounts, which are exempt: two players, a sub-account of player A, and a third player, C, for the party, with A in a clan for the clan tag checks.
- [ ] Watch the match server consoles for `Sending info to … - …ms` lines (one per reconnect query), `Cookie failed for` (a duplicate-login kick), `Game Cookie failed for` (a removal for joining another game), and `disconnect_unique_login_failed` (the final rejection of Task 22).
- [ ] Watch NEXUS's logs for lock creation, verdicts, refusals of `c_conn`, of the server list, of groups, and of invites, ignored stale refusals, lifts, and failure replies, and check that each line names accounts with their clan tags.

**Re-test.** Each scenario starts from a fresh Caldavar 1v1 which reached hero selection, unless it says otherwise; the expected outcome follows the arrow.
- [ ] **Leaving, then queueing.** A leaves the match for the menu → nothing happens: no notice, no pull, and the solo group which the client creates by itself stands. A clicks Play → no popup, the matchmaking screen resets as the client leaves its group with "foundmatch", the slot notice from "Match Lock" arrives, and A is pulled back into the match. Repeat once in a pick phase.
- [ ] **A public game through the server list.** After leaving, A opens the public game list → an empty list, the slot notice, and the pull. After leaving again, A creates a public game → no match server leaves idle, the slot notice, and the pull. Back in the match and out of it again → nothing happens until A acts (finding 1).
- [ ] **A custom game.** After leaving, A creates a custom game on a chosen match server → refused through the server list, as above.
- [ ] **A direct join and spectating.** After leaving, A joins C's public game from the friend list, then, after another leave, spectates C → each time the client is disconnected at once, with its message that the game server could not authorise the account and with no retries, the slot notice arrives, and A is pulled back within one reminder interval. The other match server logs `disconnect_unique_login_failed` once, its game carries on, and NEXUS logs no client authentication error for it.
- [ ] **The alt account.** A logs in on the sub-account → the other-account notice, naming A with its clan tag and telling the player to log in on that account, and no pull. The sub-account clicks Play → the "Temporary Ban" popup with a countdown, the matchmaking screen unlocked, and the notice, and no pull. It accepts an invite → the same. It opens the public game list → an empty list and the notice.
- [ ] **Groups and invites.** After leaving, A accepts C's invite → no popup, the slot notice, and the pull, and C's group is unchanged. After leaving again, A invites C → the slot notice and the pull, and C gets no invite. While A is locked, at the menu and again while still in the match, C invites A → A gets nothing, C is told in chat that A, named with its clan tag, is under a match lock, and C's group is unchanged.
- [ ] **A premade party which outlives its match.** With teams of two, A and C play the match as one party; A leaves, and the party leader clicks Play → A is pulled back, the party is unreadied and its screens unlock, nobody gets a popup, and both A and the leader are told in chat that A, named with its clan tag, has a match lock. Repeat while A is still in the match → A gets no pull and no popup, both are told the same, and the leader's screen unlocks. Then A logs out, or its client is killed → A leaves the party, and the leader can queue.
- [ ] **A second machine.** While A plays on machine 1, A logs in on machine 2 → machine 1's chat ends with the logged-in-elsewhere notice naming A with its clan tag, and its game plays on; machine 2 gets the login notice, and no pull and no wait. Machine 2 clicks Play → the "Temporary Ban" popup and the notice to close the other game; closing machine 1's game sends machine 2 back within one reminder interval.
- [ ] **A crash and relaunch.** Kill A's client mid-match, relaunch it, and log in → the login notice, and the client's own reconnect prompt (record its countdown: finding 6). Clicking it once the match server has dropped the dead connection returns A; clicking Play instead pulls A back as soon as the slot frees.
- [ ] **A crashed match server.** Kill the match server process mid-match → at A's next login or attempt to play, the lock lifts, silently or with the try-again notice after a refusal; A plays normally, and is never sent to the dead server.
- [ ] **A chat server restart.** Restart the chat server while A plays → nobody in the match gets a notice. Restart it while A is locked at the menu → A gets the login notice once the client's chat reconnects, and only once.
- [ ] **A lock which has gone stale.** Have A removed from the match (voted out, or terminated after the disconnect window) while A waits at the menu, then open the public game list → one empty list with the try-again notice, and the next refresh lists the games.

**Still to run from the first live test.**
- [ ] **A wait which runs out.** Keep machine 1 in the match past `ConnectedElsewhereWait` while machine 2 waits after clicking Play → machine 2 gets the expiry notice, and is sent back only at its next attempt to play.
- [ ] **An unanswered query during a wait.** Block UDP from NEXUS to the match server's game port for a few seconds while machine 2 waits → the wait carries on, and machine 2 is sent back once the port is unblocked and machine 1's game is closed.
- [ ] **A chat server restart during a wait** → no automatic send-back afterwards, and machine 2's next attempt to play sends it back.
- [ ] **A second switch in the same match,** back from machine 2 to machine 1 → machine 1 is sent back at its attempt to play, with no kick and no ban, so that two switches in one match never lock the player out.
- [ ] **A second switch through the reconnect prompt.** While machine 2 holds the slot, log in on machine 1 and click its reconnect prompt at once → watch for a `Cookie failed for` line for machine 2 (the spec's cost of a connecting chat session's refresh).
- [ ] **An in-place re-login.** On the machine in the match, log in again without leaving it, if the client allows it → it plays on, NEXUS logs the refresh, and no `Cookie failed for` line follows.
- [ ] **A normal logout on the live machine.** With machine 1 retired and still in the match, log out on machine 2 → only machine 2's chat ends, and machine 1 plays on with no kick.
- [ ] **A sub-account spectating the main account's match, if possible** → expect a duplicate-login kick of A (the spec's multi-boxing cost), and record whether a second one bans A from the match.
- [ ] **Removal by a vote.** With teams of two, let A idle until the match server flags A as idle, and have A's team vote A out → A's next attempt to play lifts the lock and proceeds.
- [ ] **Reconnecting to one's own match,** through the client's reconnect prompt and through the pull → never refused, and never kicked or terminated by the duplicate-session guard over the following minutes.
- [ ] **The reconnect query reaching the match server,** with real values: every verification is answered, with one `Sending info to` line per query; record the value for a connected slot, a loading one, and a disconnected one, and check that `ConnectedSlotThreshold` separates the first two from the third.
- [ ] **Remote commands after a session renewal.** Once a match server has finished a match and renewed its session, watch whether it still runs NEXUS's remote commands, or logs `HandleRemoteCommand() - invalid session cookie.` for them (see Follow-Ups).
- [ ] **An unreachable server.** Block UDP from NEXUS to the match server's game port, then have A, after leaving, join a public game → the lock lifts with one error naming the address tried, A plays the public game, and the original match server removes A at its next account check.
- [ ] **A master server restart mid-match** → no `Cookie failed for` line follows, and a player who drops afterwards must log in again before reconnecting.
- [ ] **A match outliving the limit.** Let a match run past `MatchLock.Lifetime` with A out of it → A can then play other games, and the original match server removes A at its next account check.
- [ ] **Controls.** A co-op match locks nobody, and a match which ends by victory, concession, or remake frees both players.
- [ ] **Clean up.** Stop COMPEL, the AppHost, its background tasks, and any orphaned shells when done.

## Follow-Ups (Not In This Plan)

- `RequestGameInfo.BuildGamePhase` labels game phases 7 and above one value off from the engine's `EGamePhase` (7 is `POST_MATCH`, 8 is `ENDED`, 9 is `HERO_BLIND_BAN`); it can use Task 3's `GamePhase`.
- `MatchAborted` passes the matchup (correlation) ID to `RemoveMatchInformation`, which is keyed by match ID, so an aborted match's information is not removed there.
- **Remote commands signed with the stale handshake cookie.** This predates the branch.
  - `MatchServerChatSession` signs every remote command with the session cookie captured at the match server's chat handshake (`TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.MatchServer.cs`, set in `CommandProcessors/Connection/ServerHandshake.cs`).
  - Every `new_session` mints a new session cookie (`ServerRequesterController.Authentication.cs`), and the engine renews its session after every match, but repeats the chat handshake only when the chat address changes (HON `K2.Engine/c_hostserver.cpp`).
  - So once a match server has renewed its session, the engine probably ignores NEXUS's remote commands as signed with an invalid session cookie (HON `K2.Engine/c_serverchatconnection.cpp`, `HandleRemoteCommand`). That includes the `quit` which `MatchServerChatSession.Terminate` sends. Task 9's switch is unaffected, because it is sent at the handshake itself.
  - Any new remote command must take the current cookie from the `MatchServer` record. Unconfirmed; Task 12 watches for it.
- **Engine changes proposed for phase 2,** from the spec's "Single Login Per User (Amendment, 2026-10-20)" and the research report's evidence:
  - **The newest connection takes over without a strike.** `CHostServer::AddClient` ignores any connection attempt for an account which already has a client, before authentication (HON `K2.Engine/c_hostserver.cpp`), which defeats the stated intent of `CGameServer::RemoveDuplicateAccountsInGame` that "precedence is given to the last player connecting" (HON `HON.MatchServer/c_gameserver.cpp`). Keeping the early return only for the same connection would let an authenticated newer connection replace the older one with `DISCONNECT_DUPLICATE_CONNECT`, with no remote command.
  - **Duplicate-session kicks stop counting toward `svr_kickBanCount`.** `CHostServer::KickClient` adds a strike for every kick, and the count refuses every reconnect as banned until `EndGame` clears it (HON `K2.Engine/c_hostserver.cpp` and `K2.Engine/c_clientconnection.cpp`). With this change, or the one above, NEXUS could answer the latest login cookie again, and so supersede game connections as well.
  - **`Kick` and `Ban` become safe for remote use.** Both look the name up among connected clients only, and fall back to `AtoI` of the name, which is 0, so a name which does not resolve removes client number 0, another player (HON `K2.Engine/c_hostserver.cpp` and `K2.Engine/xtoa.h`). They need no fall-back, and a command keyed by account ID.
  - **A proper "logged in elsewhere" reject text.** `ECR_ACCOUNT_SHARING` shows text about sharing linked sub-accounts and contacting support (HON-Assets `stringtables/client_messages_en.str`, `chat_reject_account_sharing`); Task 18's notice only precedes it.
- **The game client's own reconnect prompt (HON, client, phase 2),** from findings 3 and 6 of the spec's "Live Test Amendment (2026-10-21)".
  - It stays open and clickable after NEXUS pulls the player back, and clicking it disconnects and reconnects the player again, at the cost of disconnect allowance and a reload (finding 3). With the pull at login gone (Task 20), it no longer races NEXUS at login, but it can still outlive a later pull. Fix idea: hide `reconnect_dialog` (HON-Assets `ui/fe2/newui/main_dialogs_v2.package`) as soon as the client starts connecting or loading, as `lobby_ack_timeout_dialog` hides on `GamePhase`, and disable its button while the client is connected.
  - It shows a still-connected slot's reply, the largest time less the game time, as a countdown of millions of seconds (finding 6; `CServerList::ProcessReconnectInfoResponse`, HON `K2.Engine/c_hostclient.cpp`). Fix idea: treat a reply above half the range as a slot which is still connected from another game, and say so instead of counting down, in the dialog's `ReconnectTimer` handler or in the engine.
  - Until then, release notes can tell players to close the prompt once they are back in their match.
