namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

using PlayerEntity = MERRICK.DatabaseContext.Entities.Statistics.PlayerStatistics;

public partial class StatsRequesterController
{
    private async Task<IActionResult> HandleStatsSubmission(StatsForSubmissionRequestForm form)
    {
        if (form.Session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        form.Session = form.Session.Replace("-", string.Empty);

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(form.Session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{form.Session}""");

        if (!int.TryParse(form.MatchStats["match_id"], out int matchID))
            return BadRequest("Invalid Match ID");

        MatchStatistics? existingMatchStatistics = await MerrickContext.MatchStatistics.FirstOrDefaultAsync(stats => stats.MatchID == matchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatistics(matchServer.ID, matchServer.HostAccountName);

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else Logger.LogInformation($"Match Statistics For Match ID {matchID} Have Already Been Submitted");

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

            string accountName = Account.SeparateClanTagFromAccountName(fullAccountName).AccountName;

            PlayerEntity? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .FirstOrDefaultAsync(stats => stats.MatchID == matchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan).FirstOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerEntity playerStatistics = form.ToPlayerStatistics(playerIndex, account.ID, account.Name, account.Clan?.ID, account.Clan?.Tag);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else Logger.LogInformation($@"Player Statistics For Account Name ""{accountName}"" In Match ID {matchID} Have Already Been Submitted");
        }

        await MerrickContext.SaveChangesAsync();

        Dictionary<string, string> response = new()
        {
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
            return BadRequest(@"Missing Value For Form Parameter ""login""");

        // The Host Account Name Is Expected To End With A Colon (":") Character
        // Which The Match Server Launcher Uses To Separate The Host Account Name From The Match Server Instance ID
        form.HostAccountName = form.HostAccountName.TrimEnd(':');

        Account? hostAccount = await MerrickContext.Accounts.Include(account => account.User).FirstOrDefaultAsync(account => account.Name.Equals(form.HostAccountName));

        if (hostAccount is null)
            return NotFound($@"Unable To Retrieve Account For Host Account Name ""{hostAccount}""");

        if (form.HostAccountPasswordHash is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        string computedSRPPasswordHash = SRPAuthenticationHandlers.ComputeSRPPasswordHash(form.HostAccountPasswordHash, hostAccount.User.SRPPasswordSalt, passwordIsHashed: true);
        string expectedSRPPasswordHash = hostAccount.User.SRPPasswordHash;

        if (computedSRPPasswordHash.Equals(expectedSRPPasswordHash) is false)
            return Unauthorized("Invalid Host Account Password");

        if (form.StatsResubmissionKey is null)
            return BadRequest(@"Missing Value For Form Parameter ""resubmission_key""");

        if (!int.TryParse(form.MatchStats["match_id"], out int matchID))
            return BadRequest("Invalid Match ID");

        string computedStatsResubmissionKey = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(matchID + MatchStatsSubmissionSalt))).ToLower();
        string expectedStatsResubmissionKey = form.StatsResubmissionKey;

        if (computedStatsResubmissionKey.Equals(expectedStatsResubmissionKey) is false)
            return Unauthorized("Invalid Match Statistics Resubmission Key");

        if (form.ServerID is null)
            return BadRequest(@"Missing Value For Form Parameter ""server_id""");

        MatchServer? matchServer = (await DistributedCache.GetMatchServersByAccountName(hostAccount.Name)).FirstOrDefault(server => server.ID == form.ServerID);

        if (matchServer is null)
            Logger.LogInformation($@"Match Server ID {form.ServerID} Hosted By ""{hostAccount.Name}"" Is No Longer Online While Match Statistics For Match ID {matchID} Are Being Resubmitted");

        MatchStatistics? existingMatchStatistics = await MerrickContext.MatchStatistics.FirstOrDefaultAsync(stats => stats.MatchID == matchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatistics();

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else Logger.LogInformation($"Match Statistics For Match ID {matchID} Have Already Been Submitted");

        foreach (int playerIndex in form.PlayerStats.Keys)
        {
            // The Match Server Sends The Account Name With The Clan Tag Combined Into A Single String Value So We Need To Separate Them
            string heroKey = form.PlayerStats[playerIndex].Keys.First();
            string fullAccountName = form.PlayerStats[playerIndex][heroKey]["nickname"];
            string accountName = Account.SeparateClanTagFromAccountName(fullAccountName).AccountName;

            PlayerEntity? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .FirstOrDefaultAsync(stats => stats.MatchID == matchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan).FirstOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerEntity playerStatistics = form.ToPlayerStatistics(playerIndex, account.ID, account.Name, account.Clan?.ID, account.Clan?.Tag);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else Logger.LogInformation($@"Player Statistics For Account Name ""{accountName}"" In Match ID {matchID} Have Already Been Submitted");
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
