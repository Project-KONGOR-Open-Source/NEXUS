namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

public partial class StatsRequesterController
{
    private async Task<IActionResult> HandleStatsSubmission(StatsForSubmissionRequestForm form)
    {
        if (form.Session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        MatchServer? matchServer = await DistributedCache.GetMatchServerBySessionCookie(form.Session);

        if (matchServer is null)
            return Unauthorized($@"No Match Server Could Be Found For Session Cookie ""{form.Session}""");

        MatchStatistics? existingMatchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(stats => stats.MatchID == form.MatchStats.MatchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatisticsEntity(matchServer.ID, matchServer.HostAccountName);

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else Logger.LogError($"[BUG] Match Statistics For Match ID {form.MatchStats.MatchID} Have Already Been Submitted");

        foreach (int playerIndex in form.PlayerInventory.Keys)
        {
            // The Match Server Sends The Account Name With The Clan Tag Combined Into A Single String Value So We Need To Separate Them
            string accountName = Account.SeparateClanTagFromAccountName(form.PlayerStats[playerIndex].Values.Single().AccountName).AccountName;

            PlayerStatistics? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .SingleOrDefaultAsync(stats => stats.MatchID == form.MatchStats.MatchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan).SingleOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerStatistics playerStatistics = form.ToPlayerStatisticsEntity(playerIndex, account.ID, account.Name, account.Clan?.ID, account.Clan?.Tag);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else Logger.LogError($@"[BUG] Player Statistics For Account Name ""{accountName}"" In Match ID {form.MatchStats.MatchID} Have Already Been Submitted");
        }

        await MerrickContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<IActionResult> HandleStatsResubmission(StatsForSubmissionRequestForm form)
    {
        if (form.HostAccountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""login""");

        // The Host Account Name Is Expected To End With A Colon (":") Character
        // Which The Match Server Launcher Uses To Separate The Host Account Name From The Match Server Instance ID
        form.HostAccountName = form.HostAccountName.TrimEnd(':');

        Account? hostAccount = await MerrickContext.Accounts.Include(account => account.User).SingleOrDefaultAsync(account => account.Name.Equals(form.HostAccountName));

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

        string computedStatsResubmissionKey = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(form.MatchStats.MatchID + MatchStatsSubmissionSalt))).ToLower();
        string expectedStatsResubmissionKey = form.StatsResubmissionKey;

        if (computedStatsResubmissionKey.Equals(expectedStatsResubmissionKey) is false)
            return Unauthorized("Invalid Match Statistics Resubmission Key");

        if (form.ServerID is null)
            return BadRequest(@"Missing Value For Form Parameter ""server_id""");

        MatchServer? matchServer = (await DistributedCache.GetMatchServersByAccountName(hostAccount.Name)).SingleOrDefault(server => server.ID == form.ServerID);

        if (matchServer is null)
            Logger.LogInformation($@"Match Server ID {form.ServerID} Hosted By ""{hostAccount.Name}"" Is No Longer Online While Match Statistics For Match ID {form.MatchStats.MatchID} Are Being Resubmitted");

        MatchStatistics? existingMatchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(stats => stats.MatchID == form.MatchStats.MatchID);

        if (existingMatchStatistics is null)
        {
            MatchStatistics matchStatistics = form.ToMatchStatisticsEntity();

            await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        }

        else Logger.LogError($"[BUG] Match Statistics For Match ID {form.MatchStats.MatchID} Have Already Been Submitted");

        foreach (int playerIndex in form.PlayerInventory.Keys)
        {
            // The Match Server Sends The Account Name With The Clan Tag Combined Into A Single String Value So We Need To Separate Them
            string accountName = Account.SeparateClanTagFromAccountName(form.PlayerStats[playerIndex].Values.Single().AccountName).AccountName;

            PlayerStatistics? existingPlayerStatistics = await MerrickContext.PlayerStatistics
                .SingleOrDefaultAsync(stats => stats.MatchID == form.MatchStats.MatchID && stats.AccountName == accountName);

            if (existingPlayerStatistics is null)
            {
                Account? account = await MerrickContext.Accounts.Include(account => account.Clan).SingleOrDefaultAsync(account => account.Name.Equals(accountName));

                if (account is null)
                {
                    Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                    return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
                }

                PlayerStatistics playerStatistics = form.ToPlayerStatisticsEntity(playerIndex, account.ID, account.Name, account.Clan?.ID, account.Clan?.Tag);

                await MerrickContext.PlayerStatistics.AddAsync(playerStatistics);
            }

            else Logger.LogError($@"[BUG] Player Statistics For Account Name ""{accountName}"" In Match ID {form.MatchStats.MatchID} Have Already Been Submitted");
        }

        await MerrickContext.SaveChangesAsync();

        return Ok();
    }
}
