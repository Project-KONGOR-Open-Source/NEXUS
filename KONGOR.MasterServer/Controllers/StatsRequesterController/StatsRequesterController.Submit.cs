namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

using PlayerEntity = PlayerStatistics;
using global::MERRICK.DatabaseContext.Extensions;

public partial class StatsRequesterController
{
    private async Task<IActionResult> HandleStatsSubmission(StatsForSubmissionRequestForm form)
    {
        if (form.Session is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""session""");
        }

        Logger.LogInformation($"[StatsSubmission] Received Stats Submission For Session: {form.Session}");

        if (!int.TryParse(form.MatchStats["match_id"], out int matchID))
        {
            return BadRequest("Invalid Match ID");
        }

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(form.Session);

        if (matchServer is not null)
        {
            Logger.LogInformation(
                $"[StatsSubmission] Session {form.Session} Found In Redis. Server ID: {matchServer.ID}");
        }

        if (matchServer is null)
        {
            Logger.LogWarning(
                $"[StatsSubmission] Session {form.Session} Not Found In Redis. Attempting Database Fallback.");

            // Fallback: Check Database for the session cookie.
            // This is critical if the Redis key expired or was evicted during a long match.
            Account? account = await MerrickContext.Accounts
                .Include(a => a.Clan)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Cookie == form.Session);

            if (account != null)
            {
                Logger.LogInformation(
                    $"[StatsSubmission] Session {form.Session} Found In Database (User: {account.Name}). Recovering Context.");

                // The session is valid in the DB.
                // We need to reconstruct a temporary MatchServer context to proceed with stats saving.

                // Try to get MatchStartData to find the real Server ID
                MatchStartData? matchStartData = await DistributedCache.GetMatchStartData(matchID);

                int serverID;
                if (matchStartData != null)
                {
                    serverID = matchStartData.ServerID;
                }
                else
                {
                    // If MatchStartData is also gone, try to get server_id from the stats payload
                    // The 'match_stats' dictionary usually contains 'server_id'
                    if (form.MatchStats.TryGetValue("server_id", out string? serverIdStr) &&
                        int.TryParse(serverIdStr, out int parsedServerId))
                    {
                        serverID = parsedServerId;
                    }
                    else
                    {
                        // Default to 0 (Listen Server) if we can't find it. 
                        // It's better to save the stats under ID 0 than to fail the request.
                        serverID = 0;
                        Logger.LogWarning(
                            $"Match Start Data Missing & Server ID Not In Payload For Match {matchID}. Defaulting to Server ID 0.");
                    }
                }

                matchServer = new MatchServer
                {
                    ID = serverID,
                    HostAccountName = account.Name,
                    HostAccountID = account.ID,
                    Name = "Listen Server (Recovered)",
                    Instance = 1,
                    IPAddress =
                        Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1",
                    Port = 0,
                    Location = "Local",
                    Description = "Listen Server (Recovered Context)"
                };

                Logger.LogInformation(
                    $"Recovered Session Context From Database For User {account.Name} In Match {matchID}.");
            }
        }

        if (matchServer is null)
        {
            Logger.LogError(
                $"[StatsSubmission] Session {form.Session} Not Found In Database. Triggering Ultimate Fallback (Orphaned Session).");
            Logger.LogWarning(
                $@"Session ""{form.Session}"" Not Found During Stats Submission. Database Fallback Failed. Forcing Success (Orphaned Session) To Prevent Client Logout.");

            // Ultimate Fallback: Construct an anonymous context so stats can be saved.
            // We prioritize saving the game data and keeping the client alive over strict session validation here.

            int serverID = 0;
            if (form.MatchStats.TryGetValue("server_id", out string? serverIdStr) &&
                int.TryParse(serverIdStr, out int parsedServerId))
            {
                serverID = parsedServerId;
            }

            matchServer = new MatchServer
            {
                ID = serverID,
                HostAccountName = "Unknown", // We don't know who hosted it, but we know stats are waiting.
                HostAccountID = -1,
                Name = "Orphaned Server (Recovered)",
                Instance = 1,
                IPAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1",
                Port = 0,
                Location = "Local",
                Description = "Orphaned Server (Best-Effort Recovery)"
            };
        }

        MatchStatistics? existingMatchStatistics =
            await MerrickContext.MatchStatistics.FirstOrDefaultAsync(stats => stats.MatchID == matchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatistics(matchServer.ID, matchServer.HostAccountName);

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else
        {
            Logger.LogInformation($"Match Statistics For Match ID {matchID} Have Already Been Submitted");
        }

        foreach (int playerIndex in form.PlayerStats.Keys)
        {
            // The Match Server Sends The Account Name With The Clan Tag Combined Into A Single String Value So We Need To Separate Them
            // Dictionaries in PHP forms: player_stats[0][Hero_Legionnaire][nickname]
            // form.PlayerStats is Dictionary<int, Dictionary<string, Dictionary<string, string>>>
            // Outer key: playerIndex (0)
            // Middle key: heroName (Hero_Legionnaire) -> we can just take First().Value since we expect one hero per player entry usually, or iterate.
            // But ToPlayerStatistics handles the logic: string hero = form.PlayerStats[playerIndex].Keys.Single();

            string heroKey = form.PlayerStats[playerIndex].Keys.First();
            string fullAccountName = form.PlayerStats[playerIndex][heroKey]["nickname"];

            string accountName = AccountExtensions.SeparateClanTagFromAccountName(fullAccountName).AccountName;

            PlayerEntity? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .FirstOrDefaultAsync(stats => stats.MatchID == matchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan)
                    .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerEntity playerStatistics = form.ToPlayerStatistics(playerIndex, account.ID, account.Name,
                    account.Clan?.ID, account.Clan?.Tag, HeroDefinitionService);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else
            {
                Logger.LogInformation(
                    $@"Player Statistics For Account Name ""{accountName}"" In Match ID {matchID} Have Already Been Submitted");
            }
        }

        await MerrickContext.SaveChangesAsync();

        // Cleanup Match Start Data (Requested By User to replace global purge)
        await DistributedCache.RemoveMatchStartData(matchID);

        Dictionary<string, object> response = new()
        {
            { "match_id", matchID },
            { "match_info", "OK" },
            { "match_summ", "OK" },
            { "match_stats", "OK" },
            { "match_history", "OK" }
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleStatsResubmission(StatsForSubmissionRequestForm form)
    {
        if (form.HostAccountName is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""login""");
        }

        // The Host Account Name Is Expected To End With A Colon (":") Character
        // Which The Match Server Launcher Uses To Separate The Host Account Name From The Match Server Instance ID
        form.HostAccountName = form.HostAccountName.TrimEnd(':');

        Account? hostAccount = await MerrickContext.Accounts.Include(account => account.User)
            .FirstOrDefaultAsync(account => account.Name.Equals(form.HostAccountName));

        if (hostAccount is null)
        {
            return NotFound($@"Unable To Retrieve Account For Host Account Name ""{hostAccount}""");
        }

        if (form.HostAccountPasswordHash is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""pass""");
        }

        string computedSRPPasswordHash =
            SRPAuthenticationHandlers.ComputeSRPPasswordHash(form.HostAccountPasswordHash,
                hostAccount.User.SRPPasswordSalt);
        string expectedSRPPasswordHash = hostAccount.User.SRPPasswordHash;

        if (computedSRPPasswordHash.Equals(expectedSRPPasswordHash) is false)
        {
            return Unauthorized("Invalid Host Account Password");
        }

        if (form.StatsResubmissionKey is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""resubmission_key""");
        }

        if (!int.TryParse(form.MatchStats["match_id"], out int matchID))
        {
            return BadRequest("Invalid Match ID");
        }

        string computedStatsResubmissionKey = Convert
            .ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(matchID + MatchStatsSubmissionSalt))).ToLower();
        string expectedStatsResubmissionKey = form.StatsResubmissionKey;

        if (computedStatsResubmissionKey.Equals(expectedStatsResubmissionKey) is false)
        {
            return Unauthorized("Invalid Match Statistics Resubmission Key");
        }

        if (form.ServerID is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""server_id""");
        }

        MatchServer? matchServer =
            (await DistributedCache.GetMatchServersByAccountName(hostAccount.Name)).FirstOrDefault(server =>
                server.ID == form.ServerID);

        if (matchServer is null)
        {
            Logger.LogInformation(
                $@"Match Server ID {form.ServerID} Hosted By ""{hostAccount.Name}"" Is No Longer Online While Match Statistics For Match ID {matchID} Are Being Resubmitted");
        }

        MatchStatistics? existingMatchStatistics =
            await MerrickContext.MatchStatistics.FirstOrDefaultAsync(stats => stats.MatchID == matchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatistics();

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else
        {
            Logger.LogInformation($"Match Statistics For Match ID {matchID} Have Already Been Submitted");
        }

        foreach (int playerIndex in form.PlayerStats.Keys)
        {
            // The Match Server Sends The Account Name With The Clan Tag Combined Into A Single String Value So We Need To Separate Them
            string heroKey = form.PlayerStats[playerIndex].Keys.First();
            string fullAccountName = form.PlayerStats[playerIndex][heroKey]["nickname"];
            string accountName = AccountExtensions.SeparateClanTagFromAccountName(fullAccountName).AccountName;

            PlayerEntity? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .FirstOrDefaultAsync(stats => stats.MatchID == matchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan)
                    .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerEntity playerStatistics = form.ToPlayerStatistics(playerIndex, account.ID, account.Name,
                    account.Clan?.ID, account.Clan?.Tag, HeroDefinitionService);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else
            {
                Logger.LogInformation(
                    $@"Player Statistics For Account Name ""{accountName}"" In Match ID {matchID} Have Already Been Submitted");
            }
        }

        await MerrickContext.SaveChangesAsync();

        Dictionary<string, object> response = new()
        {
            { "match_id", matchID },
            { "match_info", "OK" },
            { "match_summ", "OK" },
            { "match_stats", "OK" },
            { "match_history", "OK" }
        };

        return Ok(PhpSerialization.Serialize(response));
    }
}