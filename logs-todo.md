# Server Log TODOs

Compiled from production logs (KONGOR.MasterServer + TRANSMUTANSTEIN.ChatServer, 2026-06-05 / 2026-06-06).
Frequencies are occurrence counts across the inspected logs.

## Bugs To Fix

### 5. `[BUG] Received Status Update For Unknown Match Server ID "0"`
- **Frequency:** 8+ (recurring across both days)
- **Symptom:** Match-server status update arrives with server ID `0`; handler logs `[BUG]` and drops the update.
- **Location:** `TRANSMUTANSTEIN.ChatServer\CommandProcessors\Connection\ServerStatus.cs` (logged when `GetMatchServerByID` returns null).
- **Root cause:** A status update is sent before the server has a valid registered ID (startup/registration race, or an unregistered/restarted server). ID is assigned at auth via `serverIdentifier.GetDeterministicInt32Hash()`.
- **Done:** The log line now dumps the full status payload (name, address, port, host name, match ID, slave ID, status) to identify the source.
- **Remaining:** Add the remote TCP endpoint to the log line; investigate the registration sequence so status updates cannot precede ID assignment; decide policy — ignore quietly vs. force re-auth.

### 6. `GroupNumber "-1"` on match participants → falls back to solo rewards
- **Frequency:** 28
- **Symptom:** `Unexpected GroupNumber "-1" On Match Participant; Falling Back To Solo Rewards`.
- **Location:** `KONGOR.MasterServer\Helpers\Stats\MatchCompletionRewardsHandler.cs:93-111` (`SelectGroupBuckets` switch default).
- **Root cause:** Valid group numbers are 1–5; `-1` indicates the group size was never set correctly upstream in the match-submission pipeline. The solo fallback is a safe stop-gap but hides a data-integrity defect.
- **Done:** The fallback warning now logs the match ID and account ID for correlation. The solo fallback is intentionally retained.
- **Remaining:** Trace where `-1` originates (match creation/submission) and ensure participants always carry a 1–5 group number.

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
- **Location:** `ClientRequesterController.cs:29-37`. Returns `Unauthorized` (401) — the rejection itself is correct, and these are *not* the cause of the 500s (those are #7).
- **Status:** Monitor only. The cookie is almost always **empty** (`""`). The diagnostics have now been enriched — both forged-cookie sites log a request-context snapshot (requested function, present query/form key names, cookie length, remote endpoint, user-agent) — so the next occurrence is self-diagnosing.

### 10. Server auth rejected — no manager holds the hosting lease
- **Frequency:** 5. `Rejected Server Authentication For Host Account "HOST": No Server Manager Holds The Hosting Lease`.
- **Location:** `ServerRequesterController.Authentication.cs:146-151`. Intentional: manager-less hosting is a bug, so the orphaned server is rejected. Operational concern (ensure the manager is up), not a code fix.

### 11. No available server for match (stale server sessions skipped)
- **Frequency:** 8 (one stale server, `ServerID=294943409`, repeatedly skipped). `Skipping Idle Server With A Stale Chat Session` → `No Idle Servers With Active Sessions For Match GUID` → `No Available Server Found For Match GUID`.
- **Location:** `TRANSMUTANSTEIN.ChatServer\Services\MatchmakingService.cs:510-553` (150s freshness threshold).
- **Note:** The freshness-skip itself is correct by design. A `StaleHostReaper` exists, but it only reaps cache entries that have **no** live chat session — it does **not** cover this case, where the server still has a live (TCP-connected) chat session that has merely stopped sending status heartbeats (`LastStatusUpdate` stale). Such a server is skipped on every selection and lingers until its TCP session actually drops, at which point the reaper finally treats it as an orphan. The original recommendation stands: consider a periodic task that proactively reaps/disconnects sessions whose `LastStatusUpdate` exceeds the freshness threshold. Likely the same root condition as #12.

## Observed In Operation (Not Yet In Logs)

### 12. Match servers become undiscoverable after a match, until manager + servers are restarted
- **Frequency:** Reported manually; reproducible after a match completes.
- **Symptom:** Once a match finishes, no match servers can be allocated for subsequent matches (the symptom surfaces as #11's `No Available Server Found For Match GUID` cluster). The match servers themselves report **no errors**. Restarting the server manager **and** the match servers restores discovery.
- **Suspected area:** The **host-lease** code is the prime suspect, since the regression appears to be recent. The server-list / server-retrieval path is a secondary, less likely suspect (believed not to have changed recently).
- **Likely master-server symptom:** The recurring `Rejected Server Authentication For Host Account "HOST": No Server Manager Holds The Hosting Lease` (#10) — observed 7× across 2026-06-05/06 — fits this: after a match the lease ends up in a state where no manager is seen to hold it, so hosts are rejected at authentication and no server becomes available; a manager restart re-acquires the lease and restores discovery.
- **Not the reaper:** The `StaleHostReaper` (committed several days *before* these logs) does **not** fix this — it only releases the lease for a manager with **no** live session, and the lease rejections recur in logs generated after it shipped. If the post-match manager session stays connected, the reaper never acts.
- **Investigate (against current code, with a fresh repro):** Whether the hosting lease is renewed/retained across a match lifecycle, or released/expired when a match ends and never re-acquired without a manager restart (`IsHostLeaseHeld` gating server auth in `ServerRequesterController.Authentication.cs:146-151`). Reproduce with one manager + one server, complete a match, then watch the lease key and the match-server hash / `LastStatusUpdate` in Redis to confirm which value goes stale. Secondary angle: whether `LastStatusUpdate` stops being refreshed post-match (dropped status updates — see #5 — would make a healthy server look stale; overlaps #11).

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

> General principle: where a warning/error fires but the cause is *not evident from the message alone*, log a JSON-serialised representation of the request — or, if the full request would be too large, the important parts (requested function, present form keys, account/identity, remote endpoint, user-agent). Keep log-property names flat (no dots) so they remain indexable in Seq. The goal is to make the next occurrence self-diagnosing rather than requiring a repro.

This principle has been applied to the forged-cookie sites (see #9). Apply it to the other not-evident warnings as they are touched — e.g. #5 (unknown server ID `0`) and #13 (stats submission failures).
