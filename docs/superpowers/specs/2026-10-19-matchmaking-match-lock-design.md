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
   - joining another player's matchmaking group, or inviting someone into theirs;
   - searching for a match (joining the queue);
   - asking for the list of public games, or of the match servers on which to create a game;
   - joining any other game: public, custom, or spectating.

   Logging in only tells the player that their match is still in progress, and the solo group which the game client creates by itself is never refused (see "Live Test Amendment (2026-10-21)"). A match server lets back in only the exact account that holds the slot. If the person is logged in on one of their other accounts, nothing can send that account back. Every attempt to play is refused instead, with a message naming the account that holds their match. Logging in on that account lets them return.
3. **No other games while locked.** Every one of the attempts above is refused, with a short message saying why, and no new game of any kind starts. Nobody can invite a locked player into a group either: the invite is not delivered, and the inviter is told why.
4. **Never sent somewhere dead.** A player is only ever sent back to a match server that is alive, to a match that is still in progress, and to a slot they can still reclaim. If any of those fails, the lock lifts at once instead.
5. **Back in from any machine.** Reconnecting works from a different PC or installation, because NEXUS already knows who the player is. While another machine is still connected to the match on the same account, a player who tries to play is told to close the game there, and is sent back as soon as it has left.
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
3. **The trigger:** an attempt to play again, meaning joining another group, inviting someone, a queue, the server list, or a game join. A login only tells the player, and the client's own solo group is never refused (see "Live Test Amendment (2026-10-21)"). Leaving alone triggers nothing.
4. **Whom it covers:** every account the person owns. Guest and staff accounts are exempt.
5. **Another account:** a locked person on an account other than the one holding the slot is refused, with a message naming that account, because only that account can be sent back.
6. **Players already removed:** a player whom the match server has already removed is not locked; the leaver percentage applies.
7. **Crashed servers:** a player is never sent to a server that may have crashed.
8. **Another machine:** reconnecting must work from another machine. Identity is the account, not the installation.
9. **AFK abuse:** left to player reports for now, with ideas kept as follow-ups.
10. **Concurrent logins:** one login per user, and never a kick for a login elsewhere (see "Single Login Per User (Amendment, 2026-10-20)").
11. **The live test redesign:** NEXUS pulls a locked player back only when they really try to play, and leaves a return at login to the game client's own reconnect prompt (see "Live Test Amendment (2026-10-21)").

## The Design

### The lock

- **What it records:** a per-user record in the distributed cache. It holds:
  - the match ID;
  - the arranged match type (Caldavar or MidWars), as the protocol encodes it;
  - the match server's identity (ID, address, and port);
  - which of the person's accounts holds the slot, by ID and by name, and that name with its clan tag for the notices and log lines which name it (see "Live Test Amendment (2026-10-21)");
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

If all three hold, the chat server refuses whatever the player attempted, with a short message sent to that player's session through `CHAT_CMD_MESSAGE_ALL`, which the client displays as free text. (`TMM_GENERIC_RESPONSE` cannot carry text: the client reads only a code from it.) It then sends `CHAT_CMD_AUTO_MATCH_CONNECT` (0x0062) with the real arranged type, match ID, address, and port, and the client connects at once, with no prompt (HON `source/K2.Engine/c_chatmanager.cpp` around 4820–4897). While the slot is still connected from another game, the message says so and asks the player to close that game if it is still open, and `AUTO_MATCH_CONNECT` follows only once the slot is free. That other game is most often one that has just crashed or closed on the same machine, which the match server has yet to drop, rather than one still running on another machine. A refused queue or group join which does not pull the player back at once is answered first with the client's own matchmaking failure reply, and a refused join of a game gets a final `c_conn` reply (see "The triggers").

The client ignores `AUTO_MATCH_CONNECT` while it counts as connected to a server, and it counts connecting and authenticating as connected (`CHost::IsConnected`). So a send-back must reach the client only once it is not on, or joining, any server.

If any check fails, the lock lifts and the attempt proceeds normally.

**When NEXUS cannot confirm,** because the match server does not answer the slot query, the lock lifts so that nobody is sent somewhere that may be dead. There are two exceptions, neither of which is the player's own attempt to play. One is the re-check that a waiting player's return makes by itself between attempts, while it waits for the slot's other connection to leave: nobody has been sent anywhere, so the wait carries on. The other is the check of a player whom someone else invites: the invite is delivered and the lock is kept, so that the player's own group join decides (see "Live Test Amendment (2026-10-21)"). A real attempt to play during the wait still lifts the lock on an unanswered query, which ends the wait (see "Single Login Per User").
- **The player who simply waits, or goes back,** is untouched, and can still reconnect to their match.
- **The player who joins another game has left.** That game gives them a new game cookie, and the old match server's guard removes them at its next account check (the owner's decision), so their team gets the leaver benefits straight away.
- **It is logged as an error,** once per server, naming the address NEXUS tried. If NEXUS could not reach match servers at all, the feature would otherwise quietly enforce nothing.

A match server that is still registered, but whose chat connection has dropped within the grace period the reaper allows, counts as alive, not gone.

### The triggers

A trigger is an attempt to play; a login only tells the player. A locked player is only ever alone in a matchmaking group, apart from a premade party which outlives the match it played together (see "Live Test Amendment (2026-10-21)").
- **Login.** At the login report which follows the chat handshake, once per login cookie, and never while the session reports being in a game, NEXUS verifies the lock and, while it holds, only tells the player: the slot account that it is in a match which is still in progress and that trying to play sends it back, and another account which account holds the match. Nobody is pulled back and no wait starts at login; the client's own reconnect prompt is the way back then, on the machine which played the match.
- **Group create** is never refused. The client creates a solo group by itself shortly after login and whenever it returns to the menu, so refusing it pulled players back without any attempt to play.
- **Group join**, including accepting an invite, is refused: the slot account is pulled back, and another account gets the matchmaking failure reply described under "Queue", then the notice.
- **Invites.** A locked player's invite is refused in the same way, and is not delivered. An invite to a locked player is not delivered either: the inviter is told why, the inviter's group is unchanged, and the invited player is not disturbed.
- **Queue** (the leader's queue request, and the queue which full loading starts). The queue is refused, and every member of the group is enforced, because a premade party outlives the match it played together.
  - A slot account which is pulled back at once gets the notice and the pull, and nothing else: the client leaves a solo group by itself when the pull arrives, which unlocks its screen.
  - Another account, a slot account which waits for its other connection, and a slot account whose attempt continues a pending return or cannot start one, first get the client's own matchmaking failure reply (`NET_CHAT_CL_TMM_FAILED_TO_JOIN`, reason `TMMFTJR_BANNED`, with the seconds left until the lock's safety limit), which shows the client's "Temporary Ban" popup and unlocks the screen, then the notice. The client leaves its group on that reply, so NEXUS removes the member from its group as well.
  - A party which outlives its match is unreadied, with a group update, which unlocks its remaining members' screens, and every member, the leader and the locked members included, is told which members hold a match lock, once per locked member, so that nobody blames the leader.
  - A member that is in a game gets no notice, pull, or popup of its own.
- **The server list** (`client_requester.php?f=server_list`), which the client asks for, with its login cookie, before it lists public games, and before it creates a public or custom game, on a match server which the player picks or on one which the client picks by itself. A locked player gets an empty list, and the master server relays the refusal to the chat server, as for `c_conn`.
- **Joining any other game** (public, custom, or spectating).
  - **Why `c_conn`:** the client connects straight to the game's server, and every match server authenticates each joining player with the master server through `c_conn` (HON `source/K2.Engine/c_clientconnection.cpp`). That is the one place where all game types meet.
  - **The rule:** `c_conn` refuses a locked player when the asking server is registered and is not the locked match's server.
  - **The reply:** an array which names the account (`account_id`) and carries no `game_cookie`. The engine treats it as final: it disconnects the client at once with `DISCONNECT_UNIQUE_LOGIN_FAILED`, with no retry and no blacklisting, and without touching its own session (`CClientConnection::CheckAuthResult`). The reply which NEXUS first used, plain text, made the engine retry `svr_authRequestAttempts` times while the client sat connecting.
  - **The replies to avoid:** an "Invalid cookie." reply would also disconnect at once, but the engine then blacklists that login cookie on that server for the rest of the server process's life (`AddBadHash`), and `auth_failure` makes the match server drop its own session.
  - **The notification:** the master server tells the chat server, which sends the player back once the client has left the other server (see above). A pub/sub channel already carries account logout notifications from the master server to the chat server, so there is precedent. A relayed refusal is ignored while the player's chat session last reported joining the locked match server, because it can only be the late answer to an attempt which the player has abandoned.
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
- **A login elsewhere:** logging in on a second machine retires the first machine's login, but not its connection to the match, which the match server still checks against the first machine's own login cookie, so it plays on undisturbed. At login the second machine is only told that its account is in a match; its first attempt to play tells it to close the game on the first, and sends it back as soon as the first has left the match. NEXUS never makes the match server kick anyone for a login elsewhere (see "Single Login Per User").

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
   - The send-back is aware of the slot. While the slot query says that the account is still connected to the match from another machine, the new machine, once it tries to play, is told to close the game on the other one, and is sent back the moment the slot frees.
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
  - A wait starts only from an attempt to play, never at login (see "Live Test Amendment (2026-10-21)").
  - While the slot account is connected from another game, the player is told so, and asked to close that game if it is still open. No `AUTO_MATCH_CONNECT` is sent, and the slot is checked again at every reminder. The moment it turns disconnected, the player is sent back.
  - A player is told why once per attempt, and is told again while waiting only if the wait runs out.
  - An unanswered slot query at the wait's own re-checks between attempts does not end it, because nobody has been sent anywhere yet. A real attempt to play during the wait still lifts the lock on an unanswered query, which ends the wait, and a match server that is gone, or that no longer hosts the match, lifts the lock at any check.
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
- An unanswered slot query at a wait's own re-checks no longer ends the wait for the other game to leave, although a real attempt to play still lifts the lock on one. So a match server that dies without a word but stays registered holds a waiting player until it is found stale, the player tries to play again, or the wait runs out. A wait still ends after `ConnectedElsewhereWait` in `MatchLockService`, with a short notice, and the player's next attempt starts afresh.

### What Changes In The Earlier Text

- Goals 5 and 6, and the Anti-Goals: a login elsewhere never removes a connection, and the new machine waits for the old one to leave.
- "The Owner's Decisions": a tenth decision points here.
- "Why per user": a login also retires the user's other logins.
- "Sending a player back": the reconnect query's largest values mean connected, a connected slot holds the player back with a different message, and an unanswered query at that wait's own re-checks does not end it.
- "Reconnecting from another machine": only the newest login passes `c_conn`.
- "The duplicate-session guard": `aids2cookie` answers the login cookie recorded for each client rather than the account's latest one, and a login elsewhere no longer kicks.

### Proposed For Phase 2 (Engine)

- Let the newest authenticated connection of an account replace the older one without a strike, which is the engine's own stated intent, defeated by an early return in `CHostServer::AddClient`.
- At minimum, stop duplicate-session kicks counting toward `svr_kickBanCount`.
- Make `Kick` and `Ban` safe for remote use: no fall-back to client number 0 for a name that does not resolve, and a command keyed by account ID.
- Give "logged in elsewhere" its own chat reject text.

Either of the first two would let NEXUS supersede game connections as well.

## Live Test Amendment (2026-10-21)

The feature was played with real game clients on 2026-10-21: match servers started by COMPEL, two clients on one PC, normal accounts (two players and a sub-account), and NEXUS in Development. This section records what the test found, the owner's redesign of the triggers, and the rulings which complete it. It takes precedence over the earlier text, which has been edited to agree with it.

### What Passed

- Every player of an announced match was locked.
- A rage-quit, in the game proper and in hero selection, then an attempt to play, sent the player back into their slot. In hero selection the slot query answered with time remaining, and the lock held.
- A login retired the earlier login, and the old window's logout was refused harmlessly.
- A sub-account's login retired the main account's login, and named the main account without pulling anyone.
- A join on another match server was refused, and relayed into one send-back across the engine's retries.
- A second machine, and a crash followed by a relaunch: once the other connection had left, the waiting return sent the new window in within seconds, and no duplicate-login kick happened.
- A match abandoned by its server, a match which ended, and a player removed as a leaver each lifted the locks they should.

### What It Found

1. **A stale refusal.** The other match server's last retry of an abandoned join arrived after the reminders had brought the player back into their match. Its relay told the player that their slot was still connected from another game (their own connection), and started a wait, which pulled the player back the moment they next left the match, with no new attempt to play.
2. **Public games are refused too late.** Creating a public game took an idle match server into use before `c_conn` refused the player, which tied that server up while the engine retried the refusal, and the client ignored every reminder until its own connection timeout. The engine's last retry produced finding 1.
3. **The client's own reconnect prompt stays open after a pull.** At login the client raises its reconnect prompt from its local reconnect file in the same second as NEXUS's `AUTO_MATCH_CONNECT`. The pull wins, but the prompt stays on screen, and clicking it disconnects and reconnects the player again, at the cost of disconnect allowance and a reload.
4. **Notices name accounts without their clan tags,** which players always see beside a name.
5. **A refused group create or queue locks the client's matchmaking screen** until the client's own timeout, over the chat pane which holds the notice.
6. **The client's reconnect prompt shows a still-connected slot as a countdown of millions of seconds,** because a connected slot's reply is the largest time less the game time. It appears after a crash and relaunch while the dead connection lingers, and on one PC whenever both windows share the reconnect file.

Also: a log line printed a retired session cookie, which the owner has since ruled acceptable, and the chat server logged the match server's expected authentication error after every lock refusal at error level. Practice games which could not open their port were a same-PC clash with a match server instance's ports, not the lock.

### The Owner's Redesign

NEXUS pulls a locked player back only when the player really tries to play. A login and the client's own solo group are not attempts to play: the client creates a group by itself shortly after login and whenever it returns to the menu, which is what pulled players back with no attempt of their own, and what locked the matchmaking screen.

A locked player is only ever alone in a matchmaking group: they cannot join another group, they cannot invite anyone into theirs, and nobody can invite them. The one exception is a premade party which outlives the match it played together (see "Rulings").

| Event | Behaviour |
|---|---|
| Login | A chat notice, with no pull: the slot account is told that a match lock holds it until its match ends and that trying to play sends it back, and another account is told which account holds the match. Once per login, and never while the session reports being in a game. The client's own reconnect prompt handles a return at login. |
| Leaving the match | Nothing. |
| Group create (the client's own solo group) | Allowed, silently. |
| Group join, including accepting an invite | Refused: the pull for the slot account, or, for another account, the client's "Temporary Ban" popup and the notice. |
| A locked player's invite | Refused in the same way, and not delivered. |
| An invite to a locked player | Not delivered: the inviter is told why, and the inviter's group is unchanged. |
| Queue (the leader's request, and the queue which full loading starts) | Refused: the pull for the slot account, or, for another account, the client's "Temporary Ban" popup and the notice. |
| The server list | An empty list, with the refusal relayed to the chat server, which pulls the slot account or tells another account. |
| A refused `c_conn` | A final reply, with the refusal relayed as before. |

A slot account which an attempt pulls back at once gets the pull (`AUTO_MATCH_CONNECT`) and the chat notice from every trigger, with no failure reply and no popup. When a party which outlives its match is refused, every member is also told which members hold a match lock. Another account has no match to return to, and its matchmaking screen covers the chat, so a refused queue or group join shows it the client's "Temporary Ban" popup, which also unlocks the screen, and the chat notice says why (owner).

### Rulings

The owner made the rulings marked "owner", the controller made the others on the owner's redesign, and those marked "planner" were made while planning; where the engine or the client decides, the ruling names the evidence.
- **The pull resets the slot account's own screen.** On `AUTO_MATCH_CONNECT`, a client which is not on a server connects through `CChatManager::MatchmakingConnect` (HON `source/K2.Engine/c_chatmanager.cpp`), which leaves a solo group locally with the reason "foundmatch", as the live test showed. Its interface hides the server-found popup and resets matchmaking (`TMMReset`, HON-Assets `ui/scripts/newui/main_dialogs_v2.lua`), and leaving the group recomputes the ready status (`CChatManager::UpdateReadyStatus`), which hides the full-screen queue loading panel (`Loading_V2:UpdateQueueLoadingPanel`, HON-Assets `ui/scripts/newui/loading_v2.lua`).
- **The "Temporary Ban" popup for everyone else** (owner). A refused queue or group join which does not pull its player back at once (another account, a slot account which waits for its other connection, or a slot account whose attempt continues a pending return or cannot start one) is answered first with `NET_CHAT_CL_TMM_FAILED_TO_JOIN`, reason `TMMFTJR_BANNED`, and the seconds left until the lock's safety limit, because the matchmaking screen covers the chat and a notice alone would go unseen.
  - The layout is the one which `CChatManager::HandleTMMJoinFailed` reads: a byte for the reason, then, for this reason only, a 32-bit number of seconds. The client leaves its group at once, locally and without telling the chat server (`LeaveTMMGroup`, local only), shows its "Temporary Ban" popup with the countdown (HON-Assets `ui/fe2/newui/main_dialogs_v2.package`, `mm_popup_banned`), and resets its matchmaking interface, which unlocks the screen.
  - The countdown is display only. The ban time is read by nothing but the per-frame display, and the next group update clears it (`CChatManager::HandleTMMPlayerUpdates`), so it can never hold a player back after the lock lifts.
  - NEXUS removes the member from its group without sending it a group update (planner), because an update naming it as leaving, or naming its group as disbanded, would make the client leave a second time, with another popup.
  - No other popup fits a match lock better. Every other failure reason shows a text about something else (matchmaking disabled or busy, an option, the version, a full or queued group, statistics, a trial account, an ineligible invitee, or an unknown disconnection), "isleaver" shows only a real leaver's block, the server-not-idle popup asks the player to queue again, and the leave strike warning threatens a punishment. A reset without a popup exists, the group update which answers a cancelled queue, but it would leave the reason unseen.
  - A party which outlives its match (see below) is unreadied, with a full group update, because a pulled member's client keeps a party on the pull and an in-game member gets no frame, so nothing else would unlock the remaining members' screens. Every member which the party had when the queue started, the leader and the locked members included, then gets one notice from the match lock per locked member, naming it with its clan tag (owner).
- **Premade parties outlive their match** (planner). The matchmaking service keeps a group of more than one member through its match, and the client keeps it too, so a locked player can still sit in a party with the teammates of the locked match. Every member of a queueing group is therefore still enforced. The leader's message naming a locked member gives way to the notice which every member receives (owner). A player who leaves the party needs nothing more: a logout, a crash, or a login elsewhere ends the chat session, whose clean-up removes the player from its group (`ClientChatSession.CleanUpSession`, which both `Terminate` and `OnDisconnected` run).
- **Invites** (owner). An invite to a locked player is not delivered, the inviter is told why from the match lock, and the inviter's group is unchanged; the invited player is not disturbed. The "not eligible" failure reply cannot be used, because `CChatManager::HandleTMMJoinFailed` always makes the inviter's client leave its own group. Inviting does not lock the inviter's screen, which only marks the player as invited (HON-Assets `ui/scripts/newui/teammaking_v2.lua`), so the chat notice is all the inviter needs. A player still in their match is locked, so the same rule covers them; the client refuses to join a group while it is in a game anyway (`CChatManager::JoinTMMGroup`), so such an invite could never be accepted. An invite to a player in a game which locks nobody is delivered, as retail did. An invite never lifts the invitee's lock on a slot which NEXUS cannot confirm, because it is not the invitee's own attempt to play: when the match server does not answer the slot query, the invite is delivered and the lock is kept, so that a player still in the match, or a slot account waiting for its other machine, stays locked, and the invitee's own group join decides; a match server which is gone, or has ended the match, still lifts the lock (controller).
- **A player in a game gets no notice of its own** (planner), which removes the in-match notice of the earlier rulings. The client never joins a group while in a game, and it queues only from its menu: its Start button readies the group, and full loading queues it, while no client interface sends the leader's queue request, which only a console command reaches. What remains is a party teammate who is still in the match, who gets the party's notice like every member, and a console command.
- **Stale refusals.** A relayed refusal is ignored, with no notice and no return, while the player's chat session last reported joining the locked match server: the client cannot be connecting elsewhere while it reports being in its own match, so the refusal can only be the late answer to an attempt which it has abandoned. A session which last reported another match server, or none, keeps the full send-back, because a chat reconnect during a refused join reports the server which the client is still connecting to. Nothing else can start a return or a wait for a player in their own match: a login starts none, and a queue from a session in a game is refused without one.
- **The server list.**
  - For a locked account which is not exempt, the master server answers the server list with an empty list, in the shape of the requested list and with a success status, and relays the refusal as `c_conn` does.
  - The client asks for it, with its login cookie, before it lists public games, and before it creates a public or custom game, whether on a match server which the player picks or on one which the client picks by itself (owner, confirmed: "remote_automatic" in `CHost` calls `CHostClient::StartAutoRemoteGame`, which asks for the list of match servers for the region; `CServerList::RequestMasterServerList`, HON `source/K2.Engine/c_hostclient.cpp`).
  - An empty list is safe. The client clears its list, reads the empty `server_list`, and redraws the list (`CServerList::ProcessServerList`), and a game creation then finds no match server and cancels itself (`CHostClient::CheckAutoStartGame`), so no match server is taken into use.
  - The rule which decides whether a lock refuses `c_conn` decides here too, including lifting a lock whose match server is no longer registered, and a lock which cannot be read is served as if there were none.
- **Spectating keeps no earlier refusal** (owner). The client spectates a player through the chat server's spectate request, then connects straight to that player's match server (`CChatManager::SpectatePlayer` and `CChatManager::JoinGame`), as it does to join a friend's game, and the list of ongoing games exists only behind a console command. The final `c_conn` rejection refuses both at once.
- **A final `c_conn` rejection.**
  - The engine retries any reply which it cannot read, and blacklists the login cookie on "Invalid cookie." (`CClientConnection::CheckAuthResult`, HON `source/K2.Engine/c_clientconnection.cpp`).
  - A reply which names the account (`account_id`) but carries no `game_cookie` is final: the engine disconnects the client at once with `DISCONNECT_UNIQUE_LOGIN_FAILED`, which the client shows as a failure to authorise its account. There is no retry, no blacklisting, and no `auth_failure`, so the match server's session is untouched, and the engine ignores the HTTP status of the reply.
  - The engine reports that reply to its manager and to the chat server as a successful authentication before it disconnects, so the chat server no longer logs an error after a lock refusal, and its error level stays for real authentication failures.
- **A refusal which finds the lock lifted tells the player to try again** (planner). Neither the server list nor the final rejection is retried by the client or the engine, so a player whose lock lifts while their refusal is relayed would otherwise see an empty list, or a failed join, with nothing to explain it.
- **Clan tags everywhere** (owner). Notices and log lines name accounts with their clan tags (`Account.NameWithClanTag`), because some names only make sense with their tag. The lock records the slot account's name with its clan tag at announce, and a lock written without it names the bare account. Login takes the bare name only, so the other-account notice tells the player to log in on that account, without asking them to type a name. The refusal relayed by the master server keeps the bare name, which is what finds the chat session.
- **"Match lock" in player-facing text** (owner). Notices call the restriction a match lock, never a ban, and come from the sender "Match Lock". Each text is defined once.
- **Cookies may be logged** (owner): only administrators read the logs, and a cookie is sometimes the only identifier there is.

### What It Costs

- On a machine without the client's reconnect file, nothing returns a player at login: they are told, and are pulled back at their first attempt to play.
- The "Temporary Ban" popup speaks of a matchmaking ban for leaving a match before it started or failing to connect to one, and its countdown runs to the lock's safety limit, an upper bound, while the chat notice from the match lock says why.
- A locked player's Start loads the map before the queue is refused, because the client's own group is not refused.
- A refused join shows the client's generic failure to authorise its account, and the match server's manager and chat server see that authentication as a success followed by a disconnect.
- The first pull after a refused join can reach the client before the other match server's disconnect does, which the client ignores, so the player returns up to one reminder interval later.
- A player whose lock has gone stale is refused once more, and is told to try again.
- Spectating and joining a friend's game are refused only once the client has connected to that match server.
- A party teammate who is still in the match sees the party's notice in its game chat.
- A player in a game who sends a queue request from the console is refused without being told why.

### What Changes In The Earlier Text

- Goals 2, 3, and 5: the attempts which pull a player back, invites, and when a waiting machine is pulled.
- "The Owner's Decisions": the third decision, and an eleventh which points here.
- "The lock": it records the slot account's name with its clan tag, for notices and log lines.
- "Sending a player back": the failure reply to a refused queue or group join, and the final `c_conn` reply.
- "The triggers": rewritten.
- "The duplicate-session guard": a login elsewhere only tells the second machine.
- "Single Login Per User": the new machine waits for the old one only once it tries to play.
- "Facts For The Builder" and "Follow-Ups": the client's own reconnect prompt.

## Facts For The Builder

- **The `AUTO_MATCH_CONNECT` payload:** arranged type (byte), match ID, address, port, and a random integer (HON `source/K2.Shared/chatserver_protocol.h`; NEXUS `ASPIRE.Common/ChatProtocol.cs`). Today NEXUS sends it only at match announce (`TRANSMUTANSTEIN.ChatServer/CommandProcessors/MatchState/MatchAnnounce.cs`).
- **Prior art in KONGOR** (`C:\Users\SADS-810\Source\KONGOR`): `Account.LastPlayedMatchId`, and on chat login, `AUTO_MATCH_CONNECT` for matchmade players. Its weaknesses:
  - it never cleared the match;
  - it sent the wrong arranged type (4, which is MidWars);
  - its phase check missed `GAME_PHASE_ENDED`;
  - its `JOINING_GAME` redirect came too late to matter.
- **The disconnect window:** after `sv_maxDisconnectedTime`, the match server terminates a disconnected player. It reports only the first leaver of a matchmade match (`REPORT_LEAVER`, which NEXUS does not handle).
- **Statistics:** statistics for a match whose server session NEXUS lost are recovered through the manager's resubmission, which NEXUS records (`StatisticsRequesterController.Submit.cs`, `HandleStatsResubmission`).
- **The game client's reconnect prompt:** the client has its own local prompt, from a file written at hero selection that expires after a few hours. It is unchanged. Since a login no longer pulls anyone back, it is the way back at login on the machine which played the match, and its flaws are a HON follow-up (see "Follow-Ups").

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
- **The game client's own reconnect prompt** (HON, client, phase 2), from findings 3 and 6 of "Live Test Amendment (2026-10-21)": it should close once the client connects to or loads a game, so that it cannot be clicked after a pull, and it should show a still-connected slot as such rather than as a countdown of millions of seconds.
