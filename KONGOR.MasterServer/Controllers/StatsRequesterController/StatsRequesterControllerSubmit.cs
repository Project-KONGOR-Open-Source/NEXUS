namespace KONGOR.MasterServer.Controllers.StatsRequesterController;

public partial class StatsRequesterController
{
    private async Task<IActionResult> HandleStatsSubmission(StatsForSubmissionRequestForm form)
    {
        if (form.Session is null)
            return BadRequest(@"Missing Value For Form Parameter ""session""");

        // TODO: Validate Session

        MatchStatistics matchStatistics = form.ToMatchStatisticsEntity();

        List<PlayerStatistics> playerStatistics = [];

        foreach (int playerIndex in form.PlayerInventory.Keys)
        {
            string accountName = form.PlayerStats[playerIndex].Values.Single().AccountName;

            Account? account = await MerrickContext.Accounts.SingleOrDefaultAsync(account => account.Name.Equals(accountName));

            if (account is null)
            {
                Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
            }

            playerStatistics.Add(form.ToPlayerStatisticsEntity(playerIndex, account.ID));
        }

        await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        await MerrickContext.PlayerStatistics.AddRangeAsync(playerStatistics);
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

        if (form.HostAccountPasswordHash is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        // TODO: Validate Host Account Password

        if (form.StatsResubmissionKey is null)
            return BadRequest(@"Missing Value For Form Parameter ""resubmission_key""");

        // TODO: Do Something With Stats Resubmission Key

        if (form.ServerID is null)
            return BadRequest(@"Missing Value For Form Parameter ""server_id""");

        // TODO: Do Something With Server ID

        MatchStatistics matchStatistics = form.ToMatchStatisticsEntity();

        List<PlayerStatistics> playerStatistics = [];

        foreach (int playerIndex in form.PlayerInventory.Keys)
        {
            string accountName = form.PlayerStats[playerIndex].Values.Single().AccountName;

            Account? account = await MerrickContext.Accounts.SingleOrDefaultAsync(account => account.Name.Equals(accountName));

            if (account is null)
            {
                Logger.LogError($@"[BUG] Unable To Retrieve Account For Account Name ""{accountName}""");

                return NotFound($@"Unable To Retrieve Account For Account Name ""{accountName}""");
            }

            playerStatistics.Add(form.ToPlayerStatisticsEntity(playerIndex, account.ID));
        }

        await MerrickContext.MatchStatistics.AddAsync(matchStatistics);
        await MerrickContext.PlayerStatistics.AddRangeAsync(playerStatistics);
        await MerrickContext.SaveChangesAsync();

        return Ok();
    }
}
