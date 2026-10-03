namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles the "set_rank" client requester command, which promotes, demotes, or removes a clan member.
    ///     This is the authoritative persistence point for clan rank changes: the game client only notifies the chat server (so it can broadcast the change to online clan members) after this endpoint reports success.
    /// </summary>
    /// <remarks>
    ///     The "rank" form parameter is one of "Officer" (promote), "Member" (demote), or "Remove" (remove from the clan).
    ///     A clan leader may change any same-clan member, an officer may demote or remove a plain member, and any member may remove themselves.
    /// </remarks>
    private async Task<IActionResult> SetClanRank()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        if (int.TryParse(Request.Form["target_id"], out int targetID).Equals(false))
            return BadRequest(@"Invalid Or Missing Value For Form Parameter ""target_id""");

        if (int.TryParse(Request.Form["clan_id"], out int clanID).Equals(false))
            return BadRequest(@"Invalid Or Missing Value For Form Parameter ""clan_id""");

        string rank = Request.Form["rank"].ToString();

        // Validate Session Cookie And Get Requester Account Name
        (bool isValid, string? requesterAccountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || requesterAccountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account requester = await MerrickContext.Accounts
            .Include(account => account.Clan)
            .SingleAsync(account => account.Name.Equals(requesterAccountName));

        // The Requester Must Belong To The Clan They Are Operating On
        if (requester.Clan is null || requester.Clan.ID != clanID)
            return Ok(SerialiseSetClanRankError("clan", "Both Users Must Be Of The Same Clan"));

        // Captured Before Any Mutation, Since A Leader's Self-Removal Clears Their Own Clan Reference
        string clanName = requester.Clan.Name;

        Account? target = await MerrickContext.Accounts
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.ID == targetID);

        if (target is null)
            return Ok(SerialiseSetClanRankError("account", "Invalid Account Given"));

        bool isSelf = targetID == requester.ID;

        // The Target Must Belong To The Same Clan, Unless The Requester Is Removing Themselves
        if (isSelf.Equals(false) && (target.Clan is null || target.Clan.ID != clanID))
            return Ok(SerialiseSetClanRankError("clan", "Both Users Must Be Of The Same Clan"));

        // A Leader May Change Any Same-Clan Member; An Officer May Only Demote Or Remove A Plain Member; Any Member May Remove Themselves
        bool permitted = rank switch
        {
            "Officer" => requester.ClanTier is ClanTier.Leader,
            "Member"  => requester.ClanTier is ClanTier.Leader || (requester.ClanTier is ClanTier.Officer && target.ClanTier is ClanTier.Member),
            "Remove"  => isSelf || requester.ClanTier is ClanTier.Leader || (requester.ClanTier is ClanTier.Officer && target.ClanTier is ClanTier.Member),
            _         => false
        };

        if (permitted.Equals(false))
            return Ok(SerialiseSetClanRankError("rank", "Invalid Options Provided"));

        // When A Leader Removes Themselves, Clan Ownership Is Transferred To The Longest-Serving Remaining Member (Or The Clan Is Disbanded)
        bool leaderIsLeaving = rank == "Remove" && isSelf && target.ClanTier is ClanTier.Leader;

        switch (rank)
        {
            case "Remove":
            {
                target.Clan = null;
                target.ClanTier = ClanTier.None;
                target.TimestampJoinedClan = null;

                break;
            }

            case "Officer":
            {
                target.ClanTier = ClanTier.Officer;

                break;
            }

            case "Member":
            {
                target.ClanTier = ClanTier.Member;

                break;
            }
        }

        Account? newClanLeader = null;

        if (leaderIsLeaving)
            newClanLeader = await TransferClanOwnershipOrDisbandClan(clanID);

        // Send The New Leader A Message Telling Them They Now Own The Clan
        if (newClanLeader is not null)
            await MerrickContext.Messages.AddAsync(BuildClanLeadershipMessage(newClanLeader, clanName));

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{RequesterName}"" Set The Clan Rank Of Account ID {TargetID} To ""{Rank}"" In Clan ID {ClanID}", requester.Name, targetID, rank, clanID);

        /*
            The Game Client Requires This Exact Value In The "set_rank" Response Field To Treat The Rank Change As Successful
            It Is Compared Case-Insensitively But Otherwise Exactly, Including The Trailing Full Stop, So The Wording And Punctuation Must Be Preserved
            A Mismatch Makes The Client Treat The Operation As Failed And Skip Notifying The Chat Server, So The Change Is Never Broadcast To Online Clan Members
        */
        const string ClanRankUpdateSuccessValue = "Member updated.";

        return Ok(PhpSerialization.Serialize(new Dictionary<string, object> { { "set_rank", ClanRankUpdateSuccessValue } }));
    }

    /// <summary>
    ///     Transfers clan ownership when the current leader leaves: the longest-tenured officer inherits, or, if there are no officers, the longest-tenured member.
    ///     Within a rank, the longest clan membership (the earliest <see cref="Account.TimestampJoinedClan"/>) wins.
    ///     If no officers or members remain, the clan is disbanded.
    /// </summary>
    /// <returns>
    ///     The account promoted to leader, or <see langword="null"/> if the clan was disbanded.
    /// </returns>
    private async Task<Account?> TransferClanOwnershipOrDisbandClan(int clanID)
    {
        // Officers Outrank Members, And Within A Rank The Earliest Join Date Wins (The Longest Clan Membership)
        Account? successor = await MerrickContext.Accounts
            .Where(account => account.Clan != null && account.Clan.ID == clanID && (account.ClanTier == ClanTier.Officer || account.ClanTier == ClanTier.Member))
            .OrderByDescending(account => account.ClanTier)
            .ThenBy(account => account.TimestampJoinedClan)
            .ThenBy(account => account.ID)
            .FirstOrDefaultAsync();

        if (successor is not null)
        {
            successor.ClanTier = ClanTier.Leader;

            return successor;
        }

        // No Officers Or Members Remain, So The Clan Is Disbanded
        Clan? clan = await MerrickContext.Clans.SingleOrDefaultAsync(clan => clan.ID == clanID);

        if (clan is not null)
            MerrickContext.Clans.Remove(clan);

        return null;
    }

    /// <summary>
    ///     Builds the inbox message sent to an account when it inherits clan leadership.
    /// </summary>
    private static Message BuildClanLeadershipMessage(Account account, string clanName) => new ()
    {
        Account = account,
        Subject = "You Are Now The Clan Leader",
        Subtitle = clanName,
        BodyTitle = "Congratulations,",
        Body = $"The previous leader of {clanName} has left, and you have inherited leadership of the clan.",
        Footer = "[K]ONGOR"
    };

    private static string SerialiseSetClanRankError(string key, string message)
        => PhpSerialization.Serialize(new Dictionary<string, object> { { "error", new Dictionary<string, string> { { key, message } } } });
}
