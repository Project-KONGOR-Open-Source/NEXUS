# Matchmaking Match Lock

## Intent

Players should be on their best behaviour in matchmade matches, knowing that walking out of a match they dislike does not get them a fresh one. A player who leaves a matchmade match that is still in progress cannot start or join any other game until that match ends, and every attempt to play again sends them straight back into it. Nobody is pulled back the moment they leave, because people step away for real reasons; the pull happens when they show that they want to play again.

## Goals

1. **One lock per person.**
   - **What creates it:** a competitive matchmade match (Caldavar, MidWars, or RiftWars) that is in progress, in which a person still holds a player slot.
   - **What it covers:** every account the person owns (sub-accounts included), so switching accounts does not dodge it.
   - **What it blocks:** every match type while it lasts, co-op games against bots included, even though a co-op game creates no lock of its own.
   - **Who is exempt:** guest and staff accounts are never locked.
2. **Sent back on the next attempt to play.** While locked, each of these sends the player straight back into their match, automatically and without a prompt:
   - logging in;
   - creating a matchmaking group, or joining one;
   - searching for a match (joining the queue);
   - joining any other game: public, custom, or spectating.

   A match server lets back in only the exact account that holds the slot. If the person is logged in on one of their other accounts, nothing can send that account back. Every attempt to play is refused instead, with a message naming the account that holds their match. Logging in on that account sends them back.
3. **No other games while locked.** Every one of the attempts above is refused, with a short message saying why, and no new game of any kind starts.
4. **Never sent somewhere dead.** A player is only ever sent back to a match server that is alive, to a match that is still in progress, and to a slot they can still reclaim. If any of those fails, the lock lifts at once instead.
5. **Back in from any machine.** Reconnecting works from a different PC or installation, because NEXUS already knows who the player is. While another machine is still connected to the match on the same account, the player is told to close the game there, and is sent back as soon as it has left.
6. **Duplicate sessions handled properly.** The match server's periodic account check (`aids2cookie`) finally works as the engine expects. A connection whose account has joined another game is removed by the match server, without ever disturbing a player reconnecting to their own match. A login elsewhere never removes a connection: each user has one login at a time, and the newest retires the others (see "Single Login Per User").

## Non-Goals

- **Public games, custom games, and co-op games against bots create no lock.** Leaving one carries no restriction. They are only blocked while a lock from a competitive matchmade match exists.
- **Guest and staff accounts are exempt.** Each group shares one user, and NEXUS deliberately lets several of them be logged in at once (`ClientHandshake` skips its one-account-per-user rule for `AccountType.Guest` and `AccountType.Staff`). Locking them would lock every guest or every staff account at once.
- **Players already removed from their match are free.** A player whom the match server has removed (kicked by vote, AFK-kicked, or terminated after the disconnect window) can play again at once. The leaver percentage is the complementary punishment for them.
- **AFK abuse is not addressed here.** That is, players who return just before the AFK kick, move, and leave again. Reports cover it for now; see Follow-Ups.
- **No new leaver penalties or bans.** The existing statistics (disconnects and the leaver percentage) stay as they are.
- **No game client changes, and no match server game-rule changes.** HON stays at retail parity; only an existing engine setting is switched on.
- **Practice and local games are not enforced.** They run without a match server or NEXUS involvement, and harm nobody.

## Anti-Goals

These would make the feature a failure even if it technically worked:

- **Breaking reconnect to the match in progress.** A player must always get back into their own match, even if NEXUS has lost track of that match server's session.
- **Forcing a player back to a crashed or finished server,** or to a match they can no longer rejoin.
- **Pulling a player back immediately after they leave,** rather than on their next attempt to play.
- **Letting the guard terminate a locked player's match for them.** While a lock holds, a rage-quitter who tries another game must be sent back, not removed from their first match: removal would make the leave official instead of preventing it.
- **Keeping a player who has moved on in their old match.** Once a lock has lifted, for any reason, a player who joins another game has left, and their old match server must remove them promptly. A removed player's team shares their gold and can use or sell their items, so keeping them in the match only hurts that team.
- **Making the match server kick a player for logging in elsewhere.** Every kick counts toward the engine's kick ban, which refuses every reconnect until the match ends, so it would lock the player out of the very match the lock sends them back to (see "Single Login Per User").

## Constraints

- **NEXUS's conventions** (`claude.md` and `agents.md`). Work happens on branch `matchmaking-match-lock`, which is based on `server-session-recovery`, whose `c_conn`, `aids2cookie`, and session-recovery changes this feature builds on.
- **Repositories avoid depending on each other.** Everything here is NEXUS, plus one existing HON engine setting that NEXUS switches on remotely. No CI depends on HON or GEMINI.
- **State survives what it must.** The lock outlives the player's chat connection, logouts, and restarts of the chat server, so it lives in the distributed cache (Valkey), not in a chat session's memory, and it always has an expiry.
- **The lock is NEXUS's default behaviour,** with no switch to turn it off (owner's decision). A case that cannot be confirmed fails safe (see "Sending a player back"), so no emergency switch is needed, and a switch would only add a way for the master and chat servers to disagree.
- **There is no configuration** (owner's decision). The safety limit is a constant in code of one hour from when the lock begins, about a long Caldavar match.
  - **A longer match outliving it is fine:** rage-quitters want a new match right after leaving, so the first minutes are the hot zone, and every attempt to play re-verifies the lock anyway.
  - **After it lifts,** a player who joins another game is removed from their old match by the duplicate-session guard, which is intended (see the Anti-Goals).

## The Owner's Decisions

1. **What creates a lock:** competitive matchmade matches only (Caldavar, MidWars, and RiftWars). Co-op games against bots create none.
2. **What a lock does:** a locked player is sent back automatically, and is blocked from every other game until the match ends. That covers every match type (co-op included), public games, custom games, and spectating.
3. **The trigger:** an attempt to play again, meaning login, group create or join, queue, or a game join. Leaving alone triggers nothing.
4. **Whom it covers:** every account the person owns. Guest and staff accounts are exempt.
5. **Another account:** a locked person on an account other than the one holding the slot is refused, with a message naming that account, because only that account can be sent back.
6. **Players already removed:** a player whom the match server has already removed is not locked; the leaver percentage applies.
7. **Crashed servers:** a player is never sent to a server that may have crashed.
8. **Another machine:** reconnecting must work from another machine. Identity is the account, not the installation.
9. **AFK abuse:** left to player reports for now, with ideas kept as follow-ups.
10. **Concurrent logins:** one login per user, and never a kick for a login elsewhere (see "Single Login Per User (Amendment, 2026-10-20)").

## The Design

### The lock

- **What it records:** a per-user record in the distributed cache. It holds:
  - the match ID;
  - the arranged match type (Caldavar or MidWars), as the protocol encodes it;
  - the match server's identity (ID, address, and port);
  - which of the person's accounts holds the slot;
  - when the lock began, and when it expires at the latest.
- **When it begins:** when matchmaking announces the match to the player's group (the same moment NEXUS sends the group to the server). By then matchmaking has committed the player.
  - The roster must be captured at announce. Today the roster lives in live groups, and a player who drops from chat vanishes from it: `TRANSMUTANSTEIN.ChatServer/Domain/Core/ChatSession.Client.cs` removes them from their group (`MatchmakingGroup.cs`).
  - Spectators and mentors never get a lock.
- **Which matches lock:** decide this from NEXUS's own queue type (`QueueType`: Caldavar, MidWars, and RiftWars lock; co-op does not), not from the arranged-type numbers sent to the client. Those are easy to misread: Caldavar goes out as `AM_MATCHMAKING_CAMPAIGN`, and co-op as `AM_MATCHMAKING_BOTMATCH`.
- **Why per user:** the lock is keyed by the user, so every account the person owns sees it. `ClientHandshake` already ends a user's other sub-account chat sessions (`TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ClientHandshake.cs`), and a login retires the logins of the user's other accounts (see "Single Login Per User"), so only one account is ever active at a time.

### When the lock ends

The lock lifts on the first of these:
- **the match ends:**
  - `MATCH_ENDED`, which today only logs;
  - `ABANDON_MATCH`, which also covers remakes;
  - `MATCH_ABORTED`, for example when a player never loaded;
  - statistics submission for the match;
- **the server goes:** the match server disappears from NEXUS (reaped, or silent beyond the stale threshold the reaper already uses);
- **the slot goes:** the player no longer holds a reclaimable slot. That is checked whenever a trigger fires, and from the server's status reports where they say so;
- **the safety limit:** one hour after the lock began, so a missed signal can never lock anyone forever.

### Sending a player back

A trigger first re-checks the lock, as follows:
1. **The match server is alive.** It is registered and its status is fresh.
2. **The match is still in progress.** Its phase is between hero selection or loading and the end of the game. The engine's game phases are in HON `source/HON.Shared/game_shared_types.h`. Note that `GAME_PHASE_ENDED` is not the last value: KONGOR's check missed it.
3. **The player still holds a reclaimable slot.** This is the only check that can show a player was removed (terminated or kicked). The server's status reports never say so.
   - **The source:** the engine's own reconnect query, the same one the game client uses (`NETCMD_RECONNECT_INFO_REQUEST` over UDP; HON `source/K2.Engine/c_hostserver.cpp` around 1196–1222, and `source/HON.MatchServer/c_gameserver.cpp`, `GetReconnectInfo`, around 19244–19302). The match server answers with the time left in the player's slot.
   - **Its 0 has many meanings:** the player was terminated, the game is over, the match ID is wrong, the server is still waiting for players, the account is not on the server, or the phase is numbered at or above `GAME_PHASE_ENDED`. That last case includes the pick phases listed after it, such as blind ban, hero lock, and lock pick.
   - **So read it with the match's phase.** A 0 lifts a lock only when it can only mean that the player was removed or the match is over. A lock must never lift during a pick phase.
   - **Its largest values mean connected.** A player still connected to the match, from any machine, has no termination time (`INVALID_TIME`), so the reply is near the top of its range, while a disconnected player's reply is at most their remaining disconnect allowance. A connected slot cannot be reclaimed until that connection leaves (see "Single Login Per User").

If all three hold, the chat server refuses whatever the player attempted, with a short message sent to that player's session through `CHAT_CMD_MESSAGE_ALL`, which the client displays as free text. (`TMM_GENERIC_RESPONSE` cannot carry text: the client reads only a code from it.) It then sends `CHAT_CMD_AUTO_MATCH_CONNECT` (0x0062) with the real arranged type, match ID, address, and port, and the client connects at once, with no prompt (HON `source/K2.Engine/c_chatmanager.cpp` around 4820–4897). While the slot is still connected from another game, the message says so and asks the player to close that game if it is still open, and `AUTO_MATCH_CONNECT` follows only once the slot is free. That other game is most often one that has just crashed or closed on the same machine, which the match server has yet to drop, rather than one still running on another machine.

The client ignores `AUTO_MATCH_CONNECT` while it counts as connected to a server, and it counts connecting and authenticating as connected (`CHost::IsConnected`). So a send-back must reach the client only once it is not on, or joining, any server.

If any check fails, the lock lifts and the attempt proceeds normally.

**When NEXUS cannot confirm,** because the match server does not answer the slot query, the lock lifts so that nobody is sent somewhere that may be dead. The one exception is a player who is waiting for the slot's other connection to leave: nobody has been sent anywhere, so the wait carries on (see "Single Login Per User").
- **The player who simply waits, or goes back,** is untouched, and can still reconnect to their match.
- **The player who joins another game has left.** That game gives them a new game cookie, and the old match server's guard removes them at its next account check (the owner's decision), so their team gets the leaver benefits straight away.
- **It is logged as an error,** once per server, naming the address NEXUS tried. If NEXUS could not reach match servers at all, the feature would otherwise quietly enforce nothing.

A match server that is still registered, but whose chat connection has dropped within the grace period the reaper allows, counts as alive, not gone.

### The triggers

- **Login.** On the chat handshake, if the person is locked, send them back.
- **Group create, group join, and queue.** These chat-server requests are refused, and the player is sent back. Today, group create and join silently drop the player from a group that is already in a match, and the queue checks only readiness, loading, and rating spread (`MatchmakingGroup.cs`).
- **Joining any other game** (public, custom, or spectating).
  - **Why `c_conn`:** the client connects straight to the game's server, and every match server authenticates each joining player with the master server through `c_conn` (HON `source/K2.Engine/c_clientconnection.cpp`). That is the one place where all game types meet.
  - **The rule:** `c_conn` refuses a locked player when the asking server is registered and is not the locked match's server.
  - **What the engine does with the refusal:** it retries a refused `c_conn` several times before disconnecting the player with a generic authentication failure. The attempts and interval are the authentication settings NEXUS sends in `SendOptions`.
  - **The reply to avoid:** an "Invalid cookie." reply would disconnect at once, but the engine then blacklists that login cookie on that server for the rest of the server process's life (`AddBadHash`). Use an ordinary refusal.
  - **The notification:** the master server tells the chat server, which sends the player back once the client has left the other server (see above). A pub/sub channel already carries account logout notifications from the master server to the chat server, so there is precedent.
  - **Reconnect safety:**
    - `c_conn` from the locked match's own server is always served.
    - So is `c_conn` from a server whose session NEXUS does not know: the reconnect rule from `server-session-recovery` stays, and NEXUS cannot tell which server is asking.
    - Only a known, different server is refused.
  - **Why not `JOINING_GAME`:** the client sends it only after loading has started, so it is too late to refuse anything (HON `source/K2.Engine/c_hostclient.cpp`).

### Reconnecting from another machine

- **Why it fails today:** a match server only lets a player back into their slot when both the account ID and a per-installation connection ID match the original connection (HON `source/K2.Engine/c_hostserver.cpp` around 483–508). A different machine has a different connection ID.
- **The switch:** the engine's `svr_ignoreConnectionIDForReconnect` matches on the account alone. It is an ordinary runtime setting; retail turned it on only for Chinese builds.
- **How it is switched on:** the chat server already sends remote commands when a match server connects (`TRANSMUTANSTEIN.ChatServer/CommandProcessors/Connection/ServerHandshake.cs`, using the helpers in `Domain/Core/ChatSession.MatchServer.cs`), and the engine executes them (HON `source/K2.Engine/c_serverchatconnection.cpp`, `HandleRemoteCommand`). So NEXUS sets this switch on every match server at that handshake.
- **The caveat:** the engine accepts a remote command only with its current session cookie, so it must be sent at handshake time. A later session renewal does not unset it.
- **Why it is safe:** NEXUS has already verified the player's identity through `c_conn` before they reach the slot, and since a login retires the user's earlier logins, only the newest login passes `c_conn`.

### The duplicate-session guard

- **How it works:** the engine periodically sends the master server its players' account IDs (`aids2cookie`). It compares the cookies that come back with each player's (HON `source/HON.MatchServer/c_gameserver.cpp`, `CGameServer::GetAccountAuth`):
  - a different login cookie kicks the connection as a duplicate login;
  - a different game cookie terminates the player as a duplicate connection.
- **Why it never worked:** today NEXUS reads the IDs from the query string, while the engine posts them in the body as `aids[]`. And `c_conn` hands everyone the same fixed `game_cookie`.
  - Fixing `c_conn` alone changes nothing, because the lists stay empty.
  - Fixing `aids2cookie` alone is worse than a kick. The game-cookie check runs first, so every non-spectator would be removed from their match.
- **There was no "current" login cookie.** NEXUS kept every login cookie valid for its full lifetime, and a new login did not retire the old one; only logout did. A login now retires the user's earlier logins (see "Single Login Per User").
- **This design fixes both together:**
  - **`c_conn`** issues a real game cookie per account and match, and stores it in the lock's record or beside it. Reconnecting to the same match returns the same game cookie, so a reconnect never trips the guard.
  - **`c_conn` also records the login cookie that the served client posted,** beside the game cookie. A chat session of the same user that reports joining that match server with a newer login cookie refreshes it, which follows a client that logs in again in place and pushes its new cookie to the match server. So the match server is only ever answered with the login cookie that its own client holds.
  - **`aids2cookie`** reads `aids[]` from the body. It answers only for accounts that connected to the asking server, which also closes the remaining gap where any registered server could ask about any account. It returns the login cookie recorded for each client and its game cookie.
  - **With the lock refusing other games,** a mismatched game cookie arises only from a genuine second connection, never from a rage-quit attempt.
- **A login elsewhere:** logging in on a second machine retires the first machine's login, but not its connection to the match, which the match server still checks against the first machine's own login cookie, so it plays on undisturbed. The login trigger tells the second machine to close the game on the first, and sends it back as soon as the first has left the match. NEXUS never makes the match server kick anyone for a login elsewhere (see "Single Login Per User").

## Single Login Per User (Amendment, 2026-10-20)

The owner settled what NEXUS does when one account, or one user, is live on two machines, on the evidence in `.superpowers/research/2026-10-20-concurrent-account-sessions.md`, which carries the file and line references. This section takes precedence over the earlier text, which has been edited to agree with it.

### Why

- **The two-strike lockout.** As first built, `aids2cookie` answered each account's latest login cookie, so a login elsewhere made the match server kick the first machine's connection as a duplicate login (HON `source/HON.MatchServer/c_gameserver.cpp`, `CGameServer::GetAccountAuth`). Every kick counts toward `svr_kickBanCount` (HON `source/K2.Engine/c_hostserver.cpp`, `KickClient`), whose default the second such kick already reaches, and from then on every reconnect is refused until the match ends.
  - Two switches between machines, or two quick logins after crashes while the dead connection still lingers, would lock a player out of the very match the lock sends them back to.
  - A kick while an arranged match still waits for its players aborts that match for everyone.
- **The logout chain.** A logout ended the chat session of the whole account, found by name, and nothing retired an earlier login. The first machine logs out when its chat is rejected and when it is kicked, and its still valid cookie then logged the second machine out as well, so a login elsewhere could log both machines out.
- **The retail precedent.** Retail's API kept one login cookie per account, which each login overwrote, so the earlier cookie stopped working at once, and each login also logged out the user's other sub-accounts (`clearOtherCookies`). KONGOR superseded logins the same way. Neither ever refused a second login. Retail answered `aids2cookie` with the latest cookie and so kicked too, but it never supported reconnecting from another machine, which NEXUS now does.

### The Owner's Decisions

1. **One login per account, and NEXUS never kicks for a login elsewhere** (the research's option E1).
   - A new login retires the previous login.
   - A logout ends only its own session.
   - `aids2cookie` answers, for each account, the login cookie of the client actually on that match server, and keeps answering the game cookie exactly as before. So NEXUS never causes a duplicate-login kick or a kick strike, while a player who leaves for another game is still removed, through the game cookie.
   - The send-back is aware of the slot. While the slot query says that the account is still connected to the match from another machine, the new machine is told to close the game on the other one, and is sent back the moment the slot frees.
2. **A new login ends the sessions of all of the user's accounts,** not only that account's, as retail did. Guest and staff accounts are the exception, as they already are at the chat handshake: each group shares one user, so their login retires only the same account's earlier login.
3. **It is built on `matchmaking-match-lock`,** before the live check with real game clients.

### The Design

- **Supersession at login.**
  - Each login retires, in one atomic step, the account's previous login cookie, which the account's latest-login record points at, and the login cookies of the user's other accounts. Two logins of one user at the same moment leave exactly one live login.
  - A retired cookie stops resolving everywhere, because the client requester, `c_conn`, and the chat handshake all read the same cache record. At `c_conn`, a retired cookie gets the same ordinary refusal as any unknown cookie, never "Invalid cookie.", so the match server does not blacklist it.
  - A login is never refused because of another live session, so a player can always log in again, after a crash included.
- **A logout ends only its own session.** The logout notification carries the session cookie with the account name, and the chat server ends a chat session only when its session cookie matches. A logout with a retired cookie publishes nothing.
- **The duplicate-session check answers each client's own login cookie.**
  - `c_conn` records the login cookie that each served client posted, beside the game cookie and with the same expiry.
  - A chat session of the same user that reports joining that match server with a different login cookie refreshes it. The engine pushes the new cookie of a client which logs in again in place to its match server (`NETCMD_CLIENT_COOKIE`), and the client reports the game it is in on every chat connection, so NEXUS follows it.
  - The game cookie is unchanged, so a player who joins another game is still removed.
- **The send-back waits for the other machine.**
  - The slot query tells "still connected" apart from "disconnected with time remaining" (see "Sending a player back").
  - While the slot account is connected from another game, the player is told so, and asked to close that game if it is still open. No `AUTO_MATCH_CONNECT` is sent, and the slot is checked again at every reminder. The moment it turns disconnected, the player is sent back.
  - A player is told why once per attempt, and is told again while waiting only if the wait runs out.
  - An unanswered slot query never ends a wait, because nobody has been sent anywhere yet. A match server that is gone, or that no longer hosts the match, still lifts the lock.
  - The wait is bounded, by a constant in `MatchLockService`, so that nobody is pulled into the match long after being told. When it runs out, the player gets a short notice, and the player's next attempt to play waits again, or sends them back if the slot is free by then.
- **The old machine is told why.** Before the chat server rejects the old machine's chat session, it sends a notice that the account has logged in elsewhere, so the player does not read only the client's account-sharing text, which talks about sharing linked sub-accounts and contacting support. The client handles the frames in order, and shows the notice in the same chat panes as that text.

### What It Costs

- The second machine cannot take over while the first is still in the match. The first must be closed, crash, or lose its connection (the engine drops it after its connected timeout, without a strike), or be voted out by its team. A first machine left running where the player cannot reach it holds the slot, because the engine only flags an idle player as AFK, which lets the team vote to kick them, and never removes them by itself.
- A machine whose login was retired plays on, but if it drops, it cannot reconnect without logging in again, which retires the other machine's login in turn. Retail behaved the same.
- NEXUS's `aids2cookie` answer differs from retail's, as NEXUS already differs by switching on account-based reconnect.
- An in-place re-login leaves a short window. The client pushes its new login cookie to its match server on its next frame (HON `source/K2.Engine/c_hostclient.cpp`, `CHostClient::UpdateCookie`), while NEXUS follows only once the client's chat connection is accepted again and reports joining that match server (`source/K2.Engine/c_chatmanager.cpp`, `CChatManager::HandleAuthAccepted`). A duplicate-session check inside that window can still kick the connection, so "never" here means that NEXUS never answers a login cookie other than the one it knows the client holds.
- A chat session that is accepted while its client is only connecting to the locked match server reports joining it, because `CChatManager::HandleAuthAccepted` reports the game whenever `Host.IsConnected()` holds, which includes connecting (HON `source/K2.Engine/c_chatmanager.cpp`). Its refresh then records that client's login cookie, so at the match server's next account check the machine that actually holds the slot is kicked as a duplicate login: one strike. The likeliest trigger is clicking the old machine's reconnect prompt, right after logging in on it, during a second switch while the other machine still holds the slot.
- A `c_conn` through a match server session that NEXUS does not recognise is taken for a reconnect to the stored match server, so it re-records the login cookie it posted for that server (see "The duplicate-session guard"). If the user's other machine is in fact still on the stored match server, that server's next account check kicks it: one strike.
- Multi-boxing costs strikes. A refresh covers every account of the user whose record is for the joined match server, so two of the user's accounts in one match, or a sub-account spectating its main account's match, make each refresh a strike for the other account, whose client is then answered a login cookie that it does not hold. A second strike in that match bans that account from it until the match ends. One login per user exists to stop multi-boxing, so this is accepted.
- An unanswered slot query no longer ends a wait for the other game to leave. So a match server that dies without a word but stays registered holds a waiting player until it is found stale, or until the wait runs out. A wait still ends after `ConnectedElsewhereWait` in `MatchLockService`, with a short notice, and the player's next attempt starts afresh.

### What Changes In The Earlier Text

- Goals 5 and 6, and the Anti-Goals: a login elsewhere never removes a connection, and the new machine waits for the old one to leave.
- "The Owner's Decisions": a tenth decision points here.
- "Why per user": a login also retires the user's other logins.
- "Sending a player back": the reconnect query's largest values mean connected, a connected slot holds the player back with a different message, and an unanswered query does not end that wait.
- "Reconnecting from another machine": only the newest login passes `c_conn`.
- "The duplicate-session guard": `aids2cookie` answers the login cookie recorded for each client rather than the account's latest one, and a login elsewhere no longer kicks.

### Proposed For Phase 2 (Engine)

- Let the newest authenticated connection of an account replace the older one without a strike, which is the engine's own stated intent, defeated by an early return in `CHostServer::AddClient`.
- At minimum, stop duplicate-session kicks counting toward `svr_kickBanCount`.
- Make `Kick` and `Ban` safe for remote use: no fall-back to client number 0 for a name that does not resolve, and a command keyed by account ID.
- Give "logged in elsewhere" its own chat reject text.

Either of the first two would let NEXUS supersede game connections as well.

## Facts For The Builder

- **The `AUTO_MATCH_CONNECT` payload:** arranged type (byte), match ID, address, port, and a random integer (HON `source/K2.Shared/chatserver_protocol.h`; NEXUS `ASPIRE.Common/ChatProtocol.cs`). Today NEXUS sends it only at match announce (`TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs`).
- **Prior art in KONGOR** (`C:\Users\SADS-810\Source\KONGOR`): `Account.LastPlayedMatchId`, and on chat login, `AUTO_MATCH_CONNECT` for matchmade players. Its weaknesses:
  - it never cleared the match;
  - it sent the wrong arranged type (4, which is MidWars);
  - its phase check missed `GAME_PHASE_ENDED`;
  - its `JOINING_GAME` redirect came too late to matter.
- **The disconnect window:** after `sv_maxDisconnectedTime`, the match server terminates a disconnected player. It reports only the first leaver of a matchmade match (`REPORT_LEAVER`, which NEXUS does not handle).
- **Statistics:** statistics for a match whose server session NEXUS lost are recovered through the manager's resubmission, which NEXUS records (`StatisticsRequesterController.Submit.cs`, `HandleStatsResubmission`).
- **The game client's reconnect prompt:** the client has its own local prompt, from a file written at hero selection that expires after a few hours. It is unchanged, and complements this feature on the same machine.

## Left To The Builder

- The cache record's exact shape, keys, and expiry handling.
- The exact source of slot validity: the reconnect query, status reports, or both. The requirement is that no player is ever sent to a slot they cannot reclaim.
- How the master server notifies the chat server after refusing a `c_conn`.
- The wording of the messages.
- Where the safety limit's constant lives.
- Tests: unit and integration tests in NEXUS's test projects for every trigger, every release condition, and every reconnect-safety rule. Live verification needs real game clients in a matchmade match; plan it with the owner.

## Follow-Ups (Not In This Design)

- **AFK abuse.** These belong to the match server's game rules, so they are for after phase 1:
  - count idle time across the whole match instead of one unbroken stretch;
  - count real activity (orders, abilities, and gold or experience gained) rather than any input;
  - have NEXUS flag accounts that repeatedly hover just under the AFK limit, if match statistics carry idle time.
- **Leaver reporting:** handling `REPORT_LEAVER` and `REPORT_MISSING_PLAYERS`, if penalties are wanted later.
- **The engine's unsafe `auth_failure` handling** on `aids2cookie` and `get_upgrades` replies (HON). NEXUS never sends it there.
