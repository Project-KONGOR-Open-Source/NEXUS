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

        MatchStatistics matchStatistics = form.ToMatchStatisticsEntity(matchServer.ID, matchServer.HostAccountName);

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

        // TODO: Validate Server By ID (Maybe? Server May No Longer Exist At Stats Resubmission Time)

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

    // TODO: Use This To Populate Clan Tag And ID

    /// <summary>
    ///     Takes a fully qualified account identifier (an optional clan tag enclosed in brackets and a mandatory account name) and returns a tuple composed of a clan tag, if one exists, and an account name.
    /// </summary>
    public static (string ClanTag, string AccountName) SeparateClanTagFromAccountName(string fullyQualifiedAccountIdentifier)
    {
        if (fullyQualifiedAccountIdentifier.Contains('[').Equals(false) && fullyQualifiedAccountIdentifier.Contains(']').Equals(false))
        {
            // If no '[' and ']' characters are found in the fully qualified account identifier, then it is safe to assume that the account is not part of a clan and has no clan tag.
            return (string.Empty, fullyQualifiedAccountIdentifier);
        }

        // Create a pattern that looks for a clan tag (excluding the enclosing brackets) and an account name.
        // For the sake of simplicity and maintenance, the regular expression assumes that both the clan tag and the account name can be any character.
        Regex pattern = new(@"^\[(?<tag>.+)\](?<account>.+)$");

        // A match should always be found, because if the account is not in a clan then this method returns before any regular expression matching happens.
        Match match = pattern.Match(fullyQualifiedAccountIdentifier);

        // There are now two match groups, a "tag" and an "account". The former is the clan tag (excluding the enclosing brackets), while the latter is the account name.
        return (match.Groups["tag"].Value, match.Groups["account"].Value);
    }
}
