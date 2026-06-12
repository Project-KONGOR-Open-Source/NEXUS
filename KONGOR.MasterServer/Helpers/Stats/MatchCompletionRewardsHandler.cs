namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Applies the match-completion side-effects for a single participant.
///     Currency rewards come from <see cref="EconomyConfiguration.MatchRewards"/>.
///     An additive bonus from <see cref="EconomyConfiguration.EventRewards"/> is also paid out while the player is still within the first <see cref="PostSignupBonus.MatchesCount"/> matches.
///     The corresponding <see cref="AccountStatistics"/> row is incremented for the match type, and its skill rating is adjusted by the rating change submitted by the match server.
/// </summary>
public static class MatchCompletionRewardsHandler
{
    /// <summary>
    ///     The floor below which a skill rating cannot drop.
    ///     Mirrors the minimum TMR protected by the chat server's pre-calculated loss values, catching the adjustments the chat server cannot pre-empt (for example the match server doubling the loss value for leavers).
    /// </summary>
    private const double MinimumSkillRating = 1000.0;

    /// <summary>
    ///     Applies match rewards, the post-signup bonus (if applicable), match-counter increments, and the skill rating change for the given participant.
    ///     The caller is responsible for calling <see cref="MerrickContext.SaveChangesAsync"/> after all participants have been processed.
    /// </summary>
    public static async Task Apply(MerrickContext databaseContext, ILogger logger, Account account, MatchInformation? matchInformation, MatchParticipantStatistics matchParticipantStatistics)
    {
        MatchRewards matchRewards = JSONConfiguration.EconomyConfiguration.MatchRewards;

        (Win winPartition, Loss lossPartition) = SelectGroupPartitions(matchRewards, matchParticipantStatistics, logger);

        bool isWin = matchParticipantStatistics.Win is 1;

        int rewardGoldCoins     = isWin ? winPartition.GoldCoins     : lossPartition.GoldCoins;
        int rewardSilverCoins   = isWin ? winPartition.SilverCoins   : lossPartition.SilverCoins;
        int rewardPlinkoTickets = isWin ? winPartition.PlinkoTickets : lossPartition.PlinkoTickets;

        account.User.GoldCoins     += rewardGoldCoins;
        account.User.SilverCoins   += rewardSilverCoins;
        account.User.PlinkoTickets += rewardPlinkoTickets;

        if (account.IsMain)
        {
            int matchesPlayedBeforeThis = await databaseContext.MatchParticipantStatistics
                .CountAsync(stats => stats.AccountID == account.ID);

            PostSignupBonus postSignupBonus = JSONConfiguration.EconomyConfiguration.EventRewards.PostSignupBonus;

            if (matchesPlayedBeforeThis < postSignupBonus.MatchesCount)
            {
                account.User.GoldCoins     += postSignupBonus.GoldCoins;
                account.User.SilverCoins   += postSignupBonus.SilverCoins;
                account.User.PlinkoTickets += postSignupBonus.PlinkoTickets;
            }
        }

        AccountStatisticsType statisticsType = ResolveAccountStatisticsType(matchInformation);

        AccountStatistics? statistics = await databaseContext.AccountStatistics
            .SingleOrDefaultAsync(record => record.AccountID == account.ID && record.Type == statisticsType);

        if (statistics is null)
        {
            // The "AccountStatisticsInterceptor" Seeds A Row For Every Type When An Account Is Created, So Absence Indicates Data Corruption
            logger.LogError($@"[BUG] AccountStatistics Row For Account ID {account.ID} And Type ""{statisticsType}"" Was Not Found");

            return;
        }

        statistics.MatchesPlayed++;

        if (matchParticipantStatistics.Win          is 1) statistics.MatchesWon++;
        if (matchParticipantStatistics.Loss         is 1) statistics.MatchesLost++;
        if (matchParticipantStatistics.Disconnected is 1) statistics.MatchesDisconnected++;
        if (matchParticipantStatistics.Conceded     is 1) statistics.MatchesConceded++;
        if (matchParticipantStatistics.Kicked       is 1) statistics.MatchesKicked++;

        RecordPlacementMatchResult(statistics, matchParticipantStatistics);

        ApplySkillRatingChange(statistics, matchParticipantStatistics);
    }

    /// <summary>
    ///     Records the participant's win or loss in the statistics row's placement data while the placement phase is incomplete.
    ///     Placement data is only maintained for queues that track placements (the row's data is <see langword="null"/> otherwise), and a disconnected participant does not consume a placement match.
    /// </summary>
    private static void RecordPlacementMatchResult(AccountStatistics statistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        if (statistics.PlacementMatchesData is null || statistics.PlacementMatchesData.Length >= AccountStatistics.ExpectedPlacementMatchCount)
            return;

        if (matchParticipantStatistics.Disconnected is 1)
            return;

        statistics.PlacementMatchesData += matchParticipantStatistics.Win is 1 ? "1" : "0";
    }

    /// <summary>
    ///     Applies the rating change submitted by the match server to the resolved statistics row.
    ///     The match server submits a ranked rating change for arranged matches and a public one for public matches, and the change is only applied when it targets the same rating family as the resolved row, so that a fallback resolution (for example a resubmission without match information) cannot mutate an unrelated rating.
    ///     A rating loss never takes the rating below <see cref="MinimumSkillRating"/>.
    /// </summary>
    private static void ApplySkillRatingChange(AccountStatistics statistics, MatchParticipantStatistics matchParticipantStatistics)
    {
        double ratingChange;

        if (statistics.Type is AccountStatisticsType.Public)
            ratingChange = matchParticipantStatistics.PublicMatch is 1 ? matchParticipantStatistics.PublicSkillRatingChange : 0.0;

        else
            ratingChange = matchParticipantStatistics.RankedMatch is 1 ? matchParticipantStatistics.RankedSkillRatingChange : 0.0;

        if (ratingChange is 0.0)
            return;

        statistics.SkillRating = Math.Max(statistics.SkillRating + ratingChange, MinimumSkillRating);
    }

    /// <summary>
    ///     Resolves the <see cref="AccountStatisticsType"/> that the match-count increment should target, based on the cached <see cref="MatchInformation"/>.
    ///     Falls back to <see cref="AccountStatisticsType.Public"/> when no match information is available (e.g. during a resubmission after the Redis entry has been purged).
    /// </summary>
    public static AccountStatisticsType ResolveAccountStatisticsType(MatchInformation? matchInformation)
    {
        if (matchInformation is null)
            return AccountStatisticsType.Public;

        // Reborn Matches Share The MIDWARS Arranged Match Type Purely For The Match Server Binary's Benefit, So The Map Name Distinguishes Actual MidWars Matches
        if (matchInformation.MatchType is MatchType.AM_MATCHMAKING_MIDWARS && matchInformation.Map.Equals("midwars", StringComparison.OrdinalIgnoreCase) is false)
            return matchInformation.IsCasual ? AccountStatisticsType.MatchmakingCasual : AccountStatisticsType.Matchmaking;

        return matchInformation.MatchType switch
        {
            MatchType.AM_MATCHMAKING_BOTMATCH => AccountStatisticsType.Cooperative,
            MatchType.AM_MATCHMAKING_MIDWARS  => AccountStatisticsType.MidWars,
            MatchType.AM_MATCHMAKING_RIFTWARS => AccountStatisticsType.RiftWars,

            MatchType.AM_MATCHMAKING
         or MatchType.AM_MATCHMAKING_CAMPAIGN
         or MatchType.AM_MATCHMAKING_CUSTOM   => matchInformation.IsCasual ? AccountStatisticsType.MatchmakingCasual : AccountStatisticsType.Matchmaking,

            MatchType.AM_UNRANKED_MATCHMAKING => AccountStatisticsType.MatchmakingCasual,

            _ => AccountStatisticsType.Public
        };
    }

    private static (Win Win, Loss Loss) SelectGroupPartitions(MatchRewards matchRewards, MatchParticipantStatistics matchParticipantStatistics, ILogger logger)
    {
        return matchParticipantStatistics.GroupNumber switch
        {
            // A Group Number Of "-1" Is The Match Server's Sentinel For A Participant With No Arranged-Match Roster (A Public Match Participant), Which Is Correctly Rewarded As Solo
            -1 => (matchRewards.Solo.Win,             matchRewards.Solo.Loss),

            // Group Numbers Are 1-5 For Arranged Matches, Corresponding To The Number Of Participants On The Player's Roster (e.g. "2" For A Duo, "3" For A Trio, etc.)
            +1 => (matchRewards.Solo.Win,             matchRewards.Solo.Loss),
            +2 => (matchRewards.TwoPersonGroup.Win,   matchRewards.TwoPersonGroup.Loss),
            +3 => (matchRewards.ThreePersonGroup.Win, matchRewards.ThreePersonGroup.Loss),
            +4 => (matchRewards.FourPersonGroup.Win,  matchRewards.FourPersonGroup.Loss),
            +5 => (matchRewards.FivePersonGroup.Win,  matchRewards.FivePersonGroup.Loss),

            // Any Other Group Number Is Unexpected, So Log A Warning And Fall Back To Solo Rewards (Rather Than Throwing An Exception And Risk Leaving The Player Without Rewards)
            _ => LogAndFallBackToSolo(matchRewards,  matchParticipantStatistics, logger)
        };
    }

    private static (Win Win, Loss Loss) LogAndFallBackToSolo(MatchRewards matchRewards, MatchParticipantStatistics matchParticipantStatistics, ILogger logger)
    {
        logger.LogWarning(@"Unexpected Group Number ""{GroupNumber}"" Encountered For Match Participant With Account ID ""{AccountID}"" (Match ID ""{MatchID}""); Falling Back To Solo Rewards",
            matchParticipantStatistics.GroupNumber, matchParticipantStatistics.AccountID, matchParticipantStatistics.MatchID);

        return (matchRewards.Solo.Win, matchRewards.Solo.Loss);
    }
}
