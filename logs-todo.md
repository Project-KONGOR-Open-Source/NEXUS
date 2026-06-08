# Server Log TODOs

Compiled from production logs (KONGOR.MasterServer + TRANSMUTANSTEIN.ChatServer, 2026-06-05 / 2026-06-06).
Frequencies are occurrence counts across the inspected logs.

## Bugs To Fix

### 1. `GroupLeave` (0x0C0C) throws when the account is not in a matchmaking group
- **Frequency:** ~11 (recurring, many account IDs)
- **Symptom:** `[BUG] Unhandled Exception Processing Command 0x0C0C (GroupLeave)` → `NullReferenceException: No Matchmaking Group Found For Account ID "N"`.
- **Location:** `TRANSMUTANSTEIN.ChatServer\CommandProcessors\Matchmaking\GroupLeave.cs:10` calling `MatchmakingGroup.GetByMemberAccountID` (`...\Domain\Matchmaking\MatchmakingGroup.cs:197`).
- **Root cause:** `GetByMemberAccountID` unconditionally throws when no group is found; `GroupLeave.Process` calls it without a null/membership check, so a leave from a player who is not grouped crashes the command.
- **Fix:** In `GroupLeave.Process`, look the group up via `MatchmakingService.GetMatchmakingGroup(...)` and no-op (or return a benign client response) when it is null, instead of going through the throwing helper.

### 2. Redis cleanup on disconnect fails with `ObjectDisposedException`
- **Frequency:** 5+ (burst on shutdown / mass disconnect)
- **Symptom:** `Failed To Remove Match Server [Manager] ID "N" From The Distributed Cache During Cleanup` → `ObjectDisposedException: ... SafeWaitHandle` (via OpenTelemetry StackExchangeRedis instrumentation).
- **Location:** `...\Domain\Core\ChatSession.MatchServerManager.cs:264,281`; `ChatSession.MatchServer.cs:318,334`; `KONGOR.MasterServer\Extensions\Cache\DistributedCacheExtensions.Matchmaking.cs:92,197`.
- **Root cause:** `OnDisconnected` spawns a fire-and-forget `Task.Run` that resolves the singleton `IDatabase` and issues Redis deletes; on host shutdown the connection multiplexer is disposed before the background task finishes.
- **Fix:** Capture/await cleanup deterministically — e.g. resolve `IDatabase` before spawning, add a timeout/cancellation tied to `IHostApplicationLifetime`, and treat `ObjectDisposedException` during shutdown as benign (downgrade from error) so it does not spam the log.

### 3. `CampaignStatisticsResponse` throws `Sequence contains no matching element`
- **Frequency:** 1 (but every `get_campaign_hero_stats` request risks it)
- **Symptom:** HTTP 500 on `/client_requester.php`; `InvalidOperationException: Sequence contains no matching element`.
- **Location:** `KONGOR.MasterServer\Models\RequestResponse\Stats\CampaignStatisticsResponse.cs:15`, reached from `ClientRequesterController.Stats.cs:304`.
- **Root cause:** `account.User.Accounts.Single(account => account.IsMain)` finds no account flagged as main (or the collection is not loaded).
- **Fix:** Guard the main-account lookup (e.g. `SingleOrDefault` with a sensible fallback to the current account ID), and/or ensure `User.Accounts` is loaded and exactly one account is marked main. Keep `Single*` per project convention but handle the no-match case.

### 4. Production exception handler returns 404 (no `/error` endpoint)
- **Frequency:** 342 (the most frequent exception; a secondary effect of every unhandled 500)
- **Symptom:** `InvalidOperationException: The exception handler configured on ExceptionHandlerOptions produced a 404 status response ... set AllowStatusCode404Response to true.`
- **Location:** `KONGOR.MasterServer\KONGOR.cs:184` — `application.UseExceptionHandler("/error")` with no matching `/error` endpoint.
- **Root cause:** The configured exception-handling path does not resolve to a handler, so the handler itself 404s and masks the original exception with a confusing secondary one.
- **Fix:** Provide a real `/error` endpoint (or use `AddProblemDetails` / lambda-based `UseExceptionHandler`), or set `AllowStatusCode404Response = true`. Resolving bugs #1–#3 removes most of the triggering 500s.

### 5. `[BUG] Received Status Update For Unknown Match Server ID "0"`
- **Frequency:** 8+ (recurring across both days)
- **Symptom:** Match-server status update arrives with server ID `0`; handler logs `[BUG]` and drops the update.
- **Location:** `TRANSMUTANSTEIN.ChatServer\CommandProcessors\Connection\ServerStatus.cs` (logged when `GetMatchServerByID` returns null).
- **Root cause:** A status update is sent before the server has a valid registered ID (startup/registration race, or an unregistered/restarted server). ID is assigned at auth via `serverIdentifier.GetDeterministicInt32Hash()`.
- **Fix:** Add diagnostic context (dump the full status payload + remote endpoint) to identify the source, and decide policy — ignore quietly vs. force re-auth. Investigate the server registration sequence so status updates cannot precede ID assignment.

### 6. `GroupNumber "-1"` on match participants → falls back to solo rewards
- **Frequency:** 28
- **Symptom:** `Unexpected GroupNumber "-1" On Match Participant; Falling Back To Solo Rewards`.
- **Location:** `KONGOR.MasterServer\Helpers\Stats\MatchCompletionRewardsHandler.cs:93-111` (`SelectGroupBuckets` switch default).
- **Root cause:** Valid group numbers are 1–5; `-1` indicates the group size was never set correctly upstream in the match-submission pipeline. The solo fallback is a safe stop-gap but hides a data-integrity defect.
- **Fix:** The fallback is acceptable to keep, but trace where `-1` originates (match creation/submission) and ensure participants always carry a 1–5 group number; consider logging match GUID + participant for correlation.

## Unimplemented Features

### 7. Unimplemented Client Requester form parameters (HTTP 500)
- **Frequency:** 170 total — `get_campaign_hero_stats` (92), `get_hero_stats` (46), `get_hero_usage_list` (26), `get_bundle_contents` (5), `guide_vote` (1).
- **Symptom:** `NotImplementedException: Unsupported Client Requester Controller Form Parameter: f=...` → HTTP 500.
- **Location:** `KONGOR.MasterServer\Controllers\ClientRequesterController\ClientRequesterController.cs` form-`f` switch (default throws ~line 108).
- **Fix:** Implement handler methods and switch cases for the five `f` values above. (Currently implemented: `get_account_mastery`, `get_player_award_summ`, `get_seasons`, `match_history_overview`, `show_stats`, `get_daily_special`, `get_guide_list_filtered`, `get_guide`.)

### 8. Unmapped chat commands (`Missing Type Mapping For Command 0x...`)
- **Frequency:** many; distinct commands observed: `0x0D08`, `0x0F08`, `0x0E07`, `0x00B8`, `0x00B5`, `0x2F00`.
- **Symptom:** `Missing Type Mapping For Command 0xNNNN; Payload Was N Bytes: ...`.
- **Location:** Dispatch via `[ChatCommand(0xNNNN)]` attribute discovery in `...\Domain\Core\ChatSession.cs` (`GetCommandType`).
- **Observed payloads / likely intent (to confirm before implementing):**
  - `0x0D08` — TMM queue request (e.g. `midwars hb|sd USW|USE|EU|AU|RU|`, `caldavar rb ...`).
  - `0x0F08` — small companion command paired with `0x0D08` (likely queue enter/leave toggle).
  - `0x0E07` — bot roster (`ChronosBot`, `ArachnaBot`, `DefilerBot`, `HammerstormBot`, `WitchSlayerBot`).
  - `0x00B8` — account + host endpoint (e.g. `kongor 194.164.93.61:11236`).
  - `0x00B5` — account name only (e.g. `Mabarn`).
  - `0x2F00` — text payload `/*`.
- **Fix:** For each command to be supported, add a processor class decorated with `[ChatCommand(0xNNNN)]` implementing `ISynchronousCommandProcessor<ClientChatSession>` (or the async variant) under `CommandProcessors\`. Decode the payloads against the HON client LUA / packet dumps to confirm semantics first.

## Expected — Monitor Only (no code change unless policy changes)

### 9. Forged / empty cookie → 401
- **Frequency:** 29. `IP Address "X" Has Made A Client Request With Forged Cookie ""`.
- **Location:** `ClientRequesterController.cs:29-37`. Returns `Unauthorized` (401) — the rejection itself is correct, and these are *not* the cause of the 500s (those are #7/#3).
- **Action:** The cookie is almost always **empty** (`""`), and the current log line does not capture enough to explain *why*. Enrich the diagnostics — see **#14**.

### 10. Server auth rejected — no manager holds the hosting lease
- **Frequency:** 5. `Rejected Server Authentication For Host Account "HOST": No Server Manager Holds The Hosting Lease`.
- **Location:** `ServerRequesterController.Authentication.cs:146-151`. Intentional: manager-less hosting is a bug, so the orphaned server is rejected. Operational concern (ensure the manager is up), not a code fix.

### 11. No available server for match (stale server sessions skipped)
- **Frequency:** 8 (one stale server, `ServerID=294943409`, repeatedly skipped). `Skipping Idle Server With A Stale Chat Session` → `No Idle Servers With Active Sessions For Match GUID` → `No Available Server Found For Match GUID`.
- **Location:** `TRANSMUTANSTEIN.ChatServer\Services\MatchmakingService.cs:510-553` (150s freshness threshold).
- **Note:** Correctly skips stale servers, but there is **no active reaper** — stale in-memory sessions linger until TCP timeout / graceful disconnect. Consider (lower priority) a periodic task that disconnects sessions whose `LastStatusUpdate` exceeds the freshness threshold. Likely the same root condition as bug #12 / #2 (cleanup not completing).

## Observed In Operation (Not Yet In Logs)

### 12. Match servers become undiscoverable after a match, until manager + servers are restarted
- **Frequency:** Reported manually; reproducible after a match completes.
- **Symptom:** Once a match finishes, no match servers can be allocated for subsequent matches (the symptom surfaces as #11's `No Available Server Found For Match GUID` cluster). The match servers themselves report **no errors**. Restarting the server manager **and** the match servers restores discovery.
- **Suspected area:** The recently-added **host-lease** code is the prime suspect, since the regression appears to be recent. The server-list / server-retrieval path is a secondary, less likely suspect (believed not to have changed recently).
- **Investigate:**
  - Whether the hosting lease is correctly **renewed/retained** across a match lifecycle, or whether it is released/expired when a match ends and never re-acquired without a manager restart (cross-reference #10 — `IsHostLeaseHeld` gating server auth in `ServerRequesterController.Authentication.cs:146-151`).
  - Whether the per-match status/session state in Redis is left **stale** after a match (cross-reference #2 — `OnDisconnected` cleanup failing — and #11 — stale sessions being skipped with no reaper), such that idle post-match servers are filtered out of `FindAvailableServerWithSession` (`MatchmakingService.cs:510-553`).
  - Whether the server's `LastStatusUpdate` stops being refreshed post-match (status updates dropped — see #5 — would make a healthy server look stale).
- **Note:** Strong likelihood this is the *same underlying defect* as #11 (and possibly #2): a host/server entry that is never refreshed or cleaned up after a match, so allocation silently filters every server out. A manager+server restart works because it re-acquires the lease and re-registers fresh sessions. Reproduce with one manager + one server, complete a match, then watch the lease key and the match-server hash/`LastStatusUpdate` in Redis to confirm which value goes stale.

### 13. Stats submission failure — suspected large-payload / form-limit issue (investigate)
- **Frequency:** Reported manually; not clearly present in the inspected master-server logs (server logs needed to confirm).
- **Symptom:** Match stats submission fails for some matches. Suspected correlation with **large payloads** (long matches / fully-populated stat blobs).
- **Location:** `KONGOR.MasterServer\Controllers\StatsRequesterController\StatsRequesterController.cs` — `submit_stats` / `resubmit_stats` via `StatsRequester` (`:39-50`).
- **Leading suspect:** The form value-count cap `StatsSubmissionFormValueCountLimit = 8192` (`:31`, applied via `[RequestFormLimits(ValueCountLimit = ...)]` at `:40`). The code comment notes a long match with all optional CVARs (`svr_submitMatchStatItems`, `svr_submitMatchStatAbilities`, `svr_submitMatchStatFrags`) can reach ~4,500+ values; an exceptionally long/eventful match could plausibly exceed 8,192, at which point the ASP.NET form reader throws `InvalidDataException: Form value count limit 8192 exceeded` and the submission is lost.
- **Secondary suspects:** Other form limits not overridden by the attribute — default per-value `ValueLengthLimit` (~4 MB), `KeyLengthLimit`, and the Kestrel `MaxRequestBodySize` (~30 MB default; **no override found** anywhere in the project). A very large replay/stat blob could hit the body-size limit before form parsing.
- **Investigate:**
  - Pull the match-server (and master-server) logs for the failing match and capture the exact exception / HTTP status returned by `stats_requester.php`.
  - If it is the value-count limit, measure the actual form value count of a failing submission and raise `StatsSubmissionFormValueCountLimit` accordingly (with headroom), rather than guessing.
  - Confirm whether the failure is a hard reject (4xx/5xx) or a partial/silent loss, and whether `resubmit_stats` recovers it.
- **Note:** User can supply server-side logs to confirm if the cause is not obvious from these master-server logs alone.

## Diagnostics & Logging Improvements

> General principle: where a warning/error fires but the cause is *not evident from the message alone*, log a JSON-serialised representation of the request — or, if the full request would be too large, the important parts (requested function, present form keys, account/identity, remote endpoint, user-agent). The goal is to make the next occurrence self-diagnosing rather than requiring a repro.

### 14. Forged / empty cookie warnings need richer context
- **Frequency:** 29 client (#9) + occurrences on the patcher path.
- **Symptom:** `... Has Made A Client Request With Forged Cookie ""` (the cookie is almost always empty), with no indication of *why* the cookie is missing/invalid.
- **Locations (both forged-cookie log sites):**
  - `KONGOR.MasterServer\Controllers\ClientRequesterController\ClientRequesterController.cs:34`.
  - `KONGOR.MasterServer\Controllers\PatcherController\PatcherController.cs:17`.
- **Current state:** Each log line carries only the remote IP and the (empty) cookie value — insufficient to tell whether the client never set a cookie, sent it under a different key, lost its session, or is a malformed/abusive request.
- **Fix:** When the cookie fails validation, additionally log a JSON-serialised snapshot of the salient request data, e.g.:
  - the requested function (`Request.Query["f"]` / `Request.Form["f"]`),
  - the set of form keys present (names only — avoid dumping secrets/SRP material in full),
  - the account name/identity if available on the request,
  - the remote endpoint and `User-Agent`.
  - If a full serialisation would be too large, include only the important parts above. Keep log-property names flat (no dots) so they remain indexable in Seq.
- **Note:** Treat this as the template for the broader logging principle above — apply the same "serialise the request when the cause isn't evident" approach to other not-evident warnings (e.g. #5 unknown server ID `0`, #13 stats submission failures).
